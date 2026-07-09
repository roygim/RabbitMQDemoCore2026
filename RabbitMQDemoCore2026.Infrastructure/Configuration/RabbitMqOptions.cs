namespace RabbitMQDemoCore2026.Infrastructure.Configuration;

public class RabbitMqOptions
{
    public const string SectionName = "RabbitMQ";

    public string HostName { get; set; } = string.Empty;

    public int Port { get; set; } = 5672;

    public string UserName { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string VirtualHost { get; set; } = "/";
    public string ProductsQueue { get; set; } = "products_queue";
}