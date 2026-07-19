namespace RabbitMQDemoCore2026.Infrastructure.Messaging;

public static class RabbitMqTopology
{
    public const string ProductsExchange = "products.exchange";
    public const string ProductsDbQueue = "products.db.queue";

    public const string RetryExchange = "products.retry.exchange";
    public const string RetryQueue = "products.retry.queue";
    public const string RetryRoutingKey = "product.retry";

    public const string DeadExchange = "products.dlx";
    public const string DeadQueue = "products.dead.queue";
    public const string DeadRoutingKey = "product.failed";
}