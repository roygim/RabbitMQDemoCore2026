using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQDemoCore2026.Domain.Entities;
using RabbitMQDemoCore2026.Infrastructure.Configuration;
using RabbitMQDemoCore2026.Worker.RabbitMQ;
using System.Text;
using System.Text.Json;
using RabbitMQDemoCore2026.Domain.Events;

namespace RabbitMQDemoCore2026.Worker;

public class ProductConsumerWork(
    ILogger<ProductConsumerWork> logger,
    IOptions<RabbitMqOptions> options,
    IRabbitMqConnection rabbitMq) : BackgroundService
{
    private readonly RabbitMqOptions _rabbitMqOptions = options.Value;

    private const string Exchange = "products.exchange";
    private const string Queue = "products.db.queue";

    private const string RetryExchange = "products.retry.exchange";
    private const string RetryQueue = "products_retry_queue";

    private const string DeadExchange = "products.dlx";
    private const string DeadQueue = "products_dead_queue";

    private const int MaxAttempts = 3; // attempts is the original attempt + 2 retries

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        var channel = await rabbitMq.CreateChannelAsync();

        // Exchanges
         
        await channel.ExchangeDeclareAsync(
            exchange: Exchange,
            type: ExchangeType.Topic,
            durable: true);


        await channel.ExchangeDeclareAsync(
            exchange: RetryExchange,
            type: ExchangeType.Direct,
            durable: true);


        await channel.ExchangeDeclareAsync(
            exchange: DeadExchange,
            type: ExchangeType.Direct,
            durable: true);

        // Dead Letter Queue

        await channel.QueueDeclareAsync(
            queue: DeadQueue,
            durable: true,
            exclusive: false,
            autoDelete: false);

        await channel.QueueBindAsync(
            queue: DeadQueue,
            exchange: DeadExchange,
            routingKey: "product.failed");

        // Retry Queue

        await channel.QueueDeclareAsync(
            queue: RetryQueue,
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
                    Exchange
                },
                {
                    "x-dead-letter-routing-key",
                    "product.retry"
                }
            });

        await channel.QueueBindAsync(
            queue: RetryQueue,
            exchange: RetryExchange,
            routingKey: "product.retry");

        // Main Queue
         
        await channel.QueueDeclareAsync(
            queue: Queue,
            durable: true,
            exclusive: false,
            autoDelete: false);

        await channel.QueueBindAsync(
            queue: Queue,
            exchange: Exchange,
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

                //var product = JsonSerializer.Deserialize<Product>(json);
                var productEvent = JsonSerializer.Deserialize<ProductCreatedEvent>(json);

                logger.LogInformation(
                    "Processing ProductCreatedEvent Id={Id}, Name={Name}",
                    productEvent?.ProductId,
                    productEvent?.Name);

                /*
                 * כאן תהיה שמירה ל DB
                 */

                // סימולציה של תקלה
                throw new Exception(
                    "DB failed intentionally");

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
                        exchange: RetryExchange,
                        routingKey: "product.retry",
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

                    await channel.BasicPublishAsync(
                        exchange: DeadExchange,
                        routingKey: "product.failed",
                        mandatory: false,
                        body: args.Body);

                    await channel.BasicAckAsync(
                        deliveryTag: args.DeliveryTag,
                        multiple: false);
                }
            }
        };

        await channel.BasicConsumeAsync(
            queue: Queue,
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