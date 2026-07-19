using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitMQDemoCore2026.Domain.Events;

public class ProductCreatedEvent: EventBase
{
    public int ProductId { get; init; }

    public string Name { get; init; } = "";

    public decimal Price { get; init; }
}