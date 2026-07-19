using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQDemoCore2026.Domain.Entities;
using RabbitMQDemoCore2026.Domain.Events;
using RabbitMQDemoCore2026.Infrastructure.Configuration;
using RabbitMQDemoCore2026.Infrastructure.Messaging;
using RabbitMQDemoCore2026.Worker.Handlers;
using RabbitMQDemoCore2026.Worker.RabbitMQ;
using System.Text;
using System.Text.Json;

namespace RabbitMQDemoCore2026.Worker;

public class ProductConsumerWork(
    ILogger<ProductConsumerWork> logger,
    IOptions<RabbitMqOptions> options,
    IRabbitMqConnection rabbitMq,
    IServiceScopeFactory scopeFactory) : BackgroundService
{
    private readonly RabbitMqOptions _rabbitMqOptions = options.Value;

    private const int MaxAttempts = 3; // attempts is the original attempt + 2 retries

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        var channel = await rabbitMq.CreateChannelAsync();

        // Exchanges
         
        await channel.ExchangeDeclareAsync(
            exchange: RabbitMqTopology.ProductsExchange,
            type: ExchangeType.Topic,
            durable: true);


        await channel.ExchangeDeclareAsync(
            exchange: RabbitMqTopology.RetryExchange,
            type: ExchangeType.Direct,
            durable: true);


        await channel.ExchangeDeclareAsync(
            exchange: RabbitMqTopology.DeadExchange,
            type: ExchangeType.Direct,
            durable: true);

        // Dead Letter Queue

        await channel.QueueDeclareAsync(
            queue: RabbitMqTopology.DeadQueue,
            durable: true,
            exclusive: false,
            autoDelete: false);

        await channel.QueueBindAsync(
            queue: RabbitMqTopology.DeadQueue,
            exchange: RabbitMqTopology.DeadExchange,
            routingKey: RabbitMqTopology.DeadRoutingKey);

        // Retry Queue

        await channel.QueueDeclareAsync(
            queue: RabbitMqTopology.RetryQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: new Dictionary<string, object?>
            {
                {
                    "x-message-ttl",
                    10000 // 10 seconds
                },
                {
                    "x-dead-letter-exchange",
                    RabbitMqTopology.ProductsExchange
                },
                {
                    "x-dead-letter-routing-key",
                    RabbitMqTopology.RetryRoutingKey
                }
            });

        await channel.QueueBindAsync(
            queue: RabbitMqTopology.RetryQueue,
            exchange: RabbitMqTopology.RetryExchange,
            routingKey: RabbitMqTopology.RetryRoutingKey);

        // Main Queue
         
        await channel.QueueDeclareAsync(
            queue: RabbitMqTopology.ProductsDbQueue,
            durable: true,
            exclusive: false,
            autoDelete: false);

        await channel.QueueBindAsync(
            queue: RabbitMqTopology.ProductsDbQueue,
            exchange: RabbitMqTopology.ProductsExchange,
            routingKey: "product.*");

        // Consumer settings

        await channel.BasicQosAsync(
            prefetchSize: 0,
            prefetchCount: 10,
            global: false);

        var consumer =
            new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (_, args) =>
        {
            try
            {
                logger.LogInformation("Message received");

                var json = Encoding.UTF8.GetString(args.Body.ToArray());

                var productEvent = JsonSerializer.Deserialize<ProductCreatedEvent>(json);

                using var scope = scopeFactory.CreateScope();

                var handler = scope.ServiceProvider.GetRequiredService<ProductCreatedHandler>();
                
                await handler.HandleAsync(productEvent, stoppingToken);

                await channel.BasicAckAsync(
                    deliveryTag: args.DeliveryTag,
                    multiple: false);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Product processing failed");

                var retryCount = GetRetryCount(args);

                // logger.LogInformation("Current retry count: {RetryCount}", retryCount);

                var attemptNumber = retryCount + 1; 

                logger.LogInformation(
                    "Attempt {Attempt}/{MaxAttempts}",
                    attemptNumber,
                    MaxAttempts);

                if (attemptNumber < MaxAttempts)
                {
                    //send to retry queue
                    var properties =
                        new BasicProperties
                        {
                            Persistent = true,
                            Headers =
                                new Dictionary<string, object?>()
                        };

                    if (args.BasicProperties.Headers != null)
                    {
                        foreach (var header in args.BasicProperties.Headers)
                        {
                            properties.Headers[header.Key] =
                                header.Value;
                        }
                    }

                    properties.Headers["x-retry-count"] =
                        retryCount + 1;

                    await channel.BasicPublishAsync(
                        exchange: RabbitMqTopology.RetryExchange,
                        routingKey: RabbitMqTopology.RetryRoutingKey,
                        mandatory: false,
                        basicProperties: properties,
                        body: args.Body);

                    await channel.BasicAckAsync(
                        deliveryTag: args.DeliveryTag,
                        multiple: false);
                }
                else
                {
                    //send to dead letter queue
                    logger.LogError("Max retries reached. Sending to DLQ");

                    var deadProperties =
                        new BasicProperties
                        {
                            Persistent = true,
                            Headers =
                                new Dictionary<string, object?>()
                        };

                    if (args.BasicProperties.Headers != null)
                    {
                        foreach (var header in args.BasicProperties.Headers)
                        {
                            deadProperties.Headers[header.Key] =
                                header.Value;
                        }
                    }

                    await channel.BasicPublishAsync(
                        exchange: RabbitMqTopology.DeadExchange,
                        routingKey: RabbitMqTopology.DeadRoutingKey,
                        mandatory: false,
                        basicProperties: deadProperties,
                        body: args.Body);

                    await channel.BasicAckAsync(
                        deliveryTag: args.DeliveryTag,
                        multiple: false);
                }
            }
        };

        await channel.BasicConsumeAsync(
            queue: RabbitMqTopology.ProductsDbQueue,
            autoAck: false,
            consumer: consumer);

        await Task.Delay(
            Timeout.Infinite,
            stoppingToken);
    }

    private int GetRetryCount(
        BasicDeliverEventArgs args)
    {
        if (args.BasicProperties.Headers == null)
            return 0;

        if (!args.BasicProperties.Headers.TryGetValue(
            "x-retry-count",
            out var value))
            return 0;

        return Convert.ToInt32(value);
    }
}