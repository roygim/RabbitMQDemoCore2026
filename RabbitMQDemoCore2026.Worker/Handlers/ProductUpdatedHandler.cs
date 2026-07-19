using Microsoft.Extensions.Logging;
using RabbitMQDemoCore2026.Domain.Events;

namespace RabbitMQDemoCore2026.Worker.Handlers;

public class ProductUpdatedHandler(
    ILogger<ProductUpdatedHandler> logger)
{
    public Task HandleAsync(
        ProductUpdatedEvent productEvent,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Processing ProductUpdatedEvent Id={Id}, Name={Name}, Price={Price}",
            productEvent.ProductId,
            productEvent.Name,
            productEvent.Price);

        /*
         * כאן יהיה עדכון ב-DB
         */

        // סימולציה של תקלה — נשאר בינתיים כדי לראות את זרימת ה-Retry/DLQ
        throw new Exception("DB failed intentionally");
    }
}