using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQDemoCore2026.Domain.Entities;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;

namespace RabbitMQDemoCore2026.Worker;

public class ProductConsumerWork(
    ILogger<ProductConsumerWork> logger) : BackgroundService
{
    private IConnection? _connection;
    private IChannel? _channel;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = "localhost",
            UserName = "admin",
            Password = "admin123"
        };

        _connection = await factory.CreateConnectionAsync();

        _channel = await _connection.CreateChannelAsync();

        await _channel.QueueDeclareAsync(
            queue: "products_queue",
            durable: true,
            exclusive: false,
            autoDelete: false);

        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.ReceivedAsync += async (sender, args) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(args.Body.ToArray());

                var product = JsonSerializer.Deserialize<Product>(json);

                logger.LogInformation(
                    "Product received: Id={Id}, Name={Name}, Price={Price}",
                    product?.Id,
                    product?.Name,
                    product?.Price);

                // todo - save product to database or perform other processing

                await _channel.BasicAckAsync(
                    deliveryTag: args.DeliveryTag,
                    multiple: false);
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Error processing product message");

                await _channel.BasicNackAsync(
                    deliveryTag: args.DeliveryTag,
                    multiple: false,
                    requeue: true);
            }
        };

        await _channel.BasicConsumeAsync(
            queue: "products_queue",
            autoAck: false,
            consumer: consumer);

        logger.LogInformation(
            "Product consumer started");

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    public override async void Dispose()
    {
        if (_channel != null)
            await _channel.DisposeAsync();

        if (_connection != null)
            await _connection.DisposeAsync();

        base.Dispose();
    }
}