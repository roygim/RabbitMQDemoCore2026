namespace RabbitMQDemoCore2026.Domain.Events;

public class ProductUpdatedEvent : EventBase
{
    public int ProductId { get; init; }

    public string Name { get; init; } = "";

    public decimal Price { get; init; }
}