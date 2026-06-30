namespace RabbitMQDemoCore2026.Repositories
{
    public class ProductsProducer : IProductsProducer
    {
        private readonly IRabbitMqProducer _producer;
        private const string QueueName = "products_queue";

        public ProductsProducer(IRabbitMqProducer producer)
        {
            _producer = producer;
        }

        public async Task PublishAsync(Product product)
        {
            await _producer.PublishAsync(QueueName, product);
        }
    }
}
