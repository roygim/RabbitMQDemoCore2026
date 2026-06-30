namespace RabbitMQDemoCore2026.Interfaces
{
    public interface IRabbitMqProducer
    {
        Task PublishAsync<T>(string queue, T message);
    }
}
