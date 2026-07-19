using RabbitMQDemoCore2026.Domain.Events;

namespace RabbitMQDemoCore2026.Interfaces
{
    public interface IProductsProducer
    {
        Task PublishCreatedAsync(ProductCreatedEvent productCreatedEvent);

        Task PublishUpdatedAsync(ProductUpdatedEvent productUpdatedEvent);

        Task PublishDeletedAsync(ProductDeletedEvent productDeletedEvent);
    }
}
