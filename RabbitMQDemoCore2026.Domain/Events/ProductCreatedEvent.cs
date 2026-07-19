using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitMQDemoCore2026.Domain.Events;

public class ProductCreatedEvent: EventBase
{
    public int ProductId { get; set; }

    public string Name { get; set; } = "";

    public decimal Price { get; set; }
}