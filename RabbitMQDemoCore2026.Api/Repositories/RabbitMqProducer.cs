using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace RabbitMQDemoCore2026.Repositories;

public class RabbitMqProducer : IRabbitMqProducer
{
    private readonly IConnection _connection;

    public RabbitMqProducer(IConnection connection)
    {
        _connection = connection;
    }

    // 3. Fully async publishing method
    public async Task PublishAsync<T>(string queue, T message)
    {
        using var channel = await _connection.CreateChannelAsync();

        await channel.QueueDeclareAsync(
            queue: queue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );

        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);

        var props = new BasicProperties
        {
            Persistent = true
        };

        await channel.BasicPublishAsync(
            exchange: "",
            routingKey: queue,
            mandatory: false,
            basicProperties: props,
            body: body
        );
    }
}