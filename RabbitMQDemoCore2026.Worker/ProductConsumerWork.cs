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
using System.Threading.Channels;

namespace RabbitMQDemoCore2026.Worker;

public class ProductConsumerWork(
    ILogger<ProductConsumerWork> logger,
    IOptions<RabbitMqOptions> options,
    IRabbitMqConnection rabbitMq) : BackgroundService
{
    private readonly RabbitMqOptions _rabbitMqOptions = options.Value;

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        var channel = await rabbitMq.CreateChannelAsync();

        await channel.QueueDeclareAsync(
            _rabbitMqOptions.ProductsQueue,
            durable: true,
            exclusive: false,
            autoDelete: false);

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (sender, args) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(args.Body.ToArray());

                var product = JsonSerializer.Deserialize<Product>(json);

                logger.LogInformation(
                    "Product received: Id={Id}, Name={Name}, Price={Price}, Category={CategoryName}",
                    product?.Id,
                    product?.Name,
                    product?.Price,
                    product?.CategoryName);

                // todo - save product to database or perform other processing

                await channel.BasicAckAsync(
                    deliveryTag: args.DeliveryTag,
                    multiple: false);
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Error processing product message");

                await channel.BasicNackAsync(
                    deliveryTag: args.DeliveryTag,
                    multiple: false,
                    requeue: true);
            }
        };

        await channel.BasicConsumeAsync(
            _rabbitMqOptions.ProductsQueue,
            false,
            consumer);

        await Task.Delay(
            Timeout.Infinite,
            stoppingToken);
    }
}