using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQDemoCore2026.Infrastructure.Configuration;

namespace RabbitMQDemoCore2026.Worker.RabbitMQ;

public class RabbitMqConnection : IRabbitMqConnection
{
    private readonly ConnectionFactory _factory;

    private IConnection? _connection;

    public RabbitMqConnection(IOptions<RabbitMqOptions> options)
    {
        var settings = options.Value;

        _factory = new ConnectionFactory
        {
            HostName = settings.HostName,
            //Port = settings.Port,
            UserName = settings.UserName,
            Password = settings.Password,
            //VirtualHost = settings.VirtualHost
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