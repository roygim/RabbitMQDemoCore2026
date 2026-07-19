namespace RabbitMQDemoCore2026.Domain.Events;

public class ProductDeletedEvent : EventBase
{
    public int ProductId { get; init; }
}