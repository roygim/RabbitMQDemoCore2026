using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitMQDemoCore2026.Domain.Requests;

public class CreateProductRequest
{
    public string Name { get; set; } = "";

    public decimal Price { get; set; }
}