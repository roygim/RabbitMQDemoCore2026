namespace RabbitMQDemoCore2026.Interfaces
{
    public interface IProductsProducer
    {
        Task PublishAsync(Product product);
    }
}
