namespace RabbitMQDemoCore2026.Api.Services;

using RabbitMQDemoCore2026.Api.Interfaces;
using RabbitMQDemoCore2026.Domain.Events;
using RabbitMQDemoCore2026.Domain.Requests;

public class ProductService : IProductService
{
    private readonly IProductsProducer _producer;


    public ProductService(
        IProductsProducer producer)
    {
        _producer = producer;
    }


    public async Task CreateAsync(CreateProductRequest request)
    {
        var productCreatedEvent = new ProductCreatedEvent
            {
                EventId = Guid.NewGuid(),
                OccurredAt = DateTime.UtcNow,
                ProductId = Random.Shared.Next(),
                Name = request.Name,
                Price = request.Price
            };

        await _producer.PublishCreatedAsync(
            productCreatedEvent);
    }

    public async Task UpdateAsync(int id, UpdateProductRequest request)
    {
        var productUpdatedEvent = new ProductUpdatedEvent
            {
                EventId = Guid.NewGuid(),
                OccurredAt = DateTime.UtcNow,
                ProductId = id,
                Name = request.Name,
                Price = request.Price
            };

        await _producer.PublishUpdatedAsync(productUpdatedEvent);
    }

    public async Task DeleteAsync(int id)
    {
        var productDeletedEvent = new ProductDeletedEvent
        {
            EventId = Guid.NewGuid(),
            OccurredAt = DateTime.UtcNow,
            ProductId = id
        };

        await _producer.PublishDeletedAsync(productDeletedEvent);
    }
}
