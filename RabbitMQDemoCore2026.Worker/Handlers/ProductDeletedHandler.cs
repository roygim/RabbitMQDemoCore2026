using Microsoft.Extensions.Logging;
using RabbitMQDemoCore2026.Domain.Events;

namespace RabbitMQDemoCore2026.Worker.Handlers;

public class ProductDeletedHandler(
    ILogger<ProductDeletedHandler> logger)
{
    public Task HandleAsync(
        ProductDeletedEvent productEvent,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Processing ProductDeletedEvent Id={Id}",
            productEvent.ProductId);

        /*
         * כאן יהיה עדכון ב-DB
         */

        // סימולציה של תקלה — נשאר בינתיים כדי לראות את זרימת ה-Retry/DLQ
        throw new Exception("DB failed intentionally");
    }
}