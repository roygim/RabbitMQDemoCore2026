namespace RabbitMQDemoCore2026.Api.Interfaces;

using RabbitMQDemoCore2026.Domain.Requests;

public interface IProductService
{
    Task CreateAsync(CreateProductRequest request);
}