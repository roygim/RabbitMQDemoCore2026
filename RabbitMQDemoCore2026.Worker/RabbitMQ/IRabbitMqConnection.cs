using RabbitMQ.Client;

namespace RabbitMQDemoCore2026.Worker.RabbitMQ;

public interface IRabbitMqConnection
{
    Task<IConnection> GetConnectionAsync();
    Task<IChannel> CreateChannelAsync();
}