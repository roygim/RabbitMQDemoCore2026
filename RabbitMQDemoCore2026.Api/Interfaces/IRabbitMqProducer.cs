namespace RabbitMQDemoCore2026.Interfaces
{
    public interface IRabbitMqProducer
    {
        Task PublishAsync<T>(string exchange, string routingKey, T message);
    }
}
