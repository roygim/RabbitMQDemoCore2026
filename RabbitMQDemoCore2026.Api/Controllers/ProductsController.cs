using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using RabbitMQDemoCore2026.Api.Interfaces;
using RabbitMQDemoCore2026.Domain.Requests;

namespace RabbitMQDemoCore2026.Controllers
{
    [ApiController]
    [Route("api/products")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductsProducer _productsProducer;
        private readonly IProductService _productService;

        public ProductsController(IProductsProducer productsProducer, IProductService productService)
        {
            _productsProducer = productsProducer;
            _productService = productService;
        }

        [HttpPost("add")]
        public async Task<IActionResult> Create([FromBody] CreateProductRequest request)
        {
            try
            {
                await _productService.CreateAsync(request);

                return Accepted();
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