using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace RabbitMQDemoCore2026.Controllers
{
    [ApiController]
    [Route("api/products")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductsProducer _productsProducer;
        public ProductsController(IProductsProducer productsProducer)
        {
            _productsProducer = productsProducer;
        }

        [HttpPost("add")]
        public async Task<IActionResult> Create([FromBody] Product product)
        {
            try
            {
                await _productsProducer.PublishAsync(product);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "Failed to process product request",
                    message = ex.Message
                    // Optional: Include ex.StackTrace only in Development environment for security
                });
            }
        }
    }
}