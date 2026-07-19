namespace RabbitMQDemoCore2026.Domain.Events;

public abstract class EventBase
{
    public Guid EventId { get; init; }

    public DateTime OccurredAt { get; init; }
}