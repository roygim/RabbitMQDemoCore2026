using RabbitMQ.Client;

namespace RabbitMQDemoCore2026.Worker.RabbitMQ;

public class RabbitMqConnection : IRabbitMqConnection
{
    private readonly ConnectionFactory _factory;

    private IConnection? _connection;

    public RabbitMqConnection()
    {
        _factory = new ConnectionFactory
        {
            HostName = "localhost",
            UserName = "admin",
            Password = "admin123"
        };
    }

    public async Task<IConnection> GetConnectionAsync()
    {
        if (_connection == null)
        {
            _connection = await _factory.CreateConnectionAsync();
        }

        return _connection;
    }

    public async Task<IChannel> CreateChannelAsync()
    {
        var connection = await GetConnectionAsync();

        return await connection.CreateChannelAsync();
    }
}