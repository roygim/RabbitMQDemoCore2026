using Microsoft.Extensions.Options;
using RabbitMQDemoCore2026.Domain.Constants;
using RabbitMQDemoCore2026.Domain.Events;
using RabbitMQDemoCore2026.Infrastructure.Configuration;

namespace RabbitMQDemoCore2026.Repositories
{
    public class ProductsProducer : IProductsProducer
    {
        private readonly IRabbitMqProducer _producer;
        private readonly IOptions<RabbitMqOptions> _options;
        
        public ProductsProducer(IRabbitMqProducer producer, IOptions<RabbitMqOptions> options)
        {
            _producer = producer;
            _options = options;
        }

        public async Task PublishCreatedAsync(ProductCreatedEvent productCreatedEvent)
        {
            await _producer.PublishAsync(
                "products.exchange",
                ProductEventNames.Created,
                productCreatedEvent);
        }
    }
}
