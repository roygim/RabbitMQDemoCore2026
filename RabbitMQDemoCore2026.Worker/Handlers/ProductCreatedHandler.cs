using Microsoft.Extensions.Logging;
using RabbitMQDemoCore2026.Domain.Events;

namespace RabbitMQDemoCore2026.Worker.Handlers;

public class ProductCreatedHandler(ILogger<ProductCreatedHandler> logger)
{
    public Task HandleAsync(
        ProductCreatedEvent productEvent,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Processing ProductCreatedEvent Id={Id}, Name={Name}",
            productEvent.ProductId,
            productEvent.Name);

        /*
         * כאן תהיה שמירה ל-DB
         */

        // סימולציה של תקלה — נשאר בינתיים כדי לראות את זרימת ה-Retry/DLQ
        throw new Exception("DB failed intentionally");
    }
}