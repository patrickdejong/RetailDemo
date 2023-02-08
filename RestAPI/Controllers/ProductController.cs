using Microsoft.AspNetCore.Mvc;
using DataLibrary.Model;
using DataLibrary.Commands;

namespace RestAPI.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase {
        private static readonly Product[] Products = new[]
        {
            new Product() { Name = "Book", Price = 9.99, Manufacturer = "O'Reilly" },
            new Product() { Name = "Car", Price = 45000, Manufacturer = "Tesla" },
            new Product() { Name = "Starship", Price = 9999999999, Manufacturer = "SpaceX" },
        };

        private readonly ILogger<ProductController> _logger;

        public ProductController(ILogger<ProductController> logger) {
            _logger = logger;
        }

        // GET: api/<ProductController>
        [HttpGet]
        public IEnumerable<Product> Get() {
            return Products;
        }

        // POST: api/<ProductController>/order
        [HttpPost("order")]
        public async Task<IActionResult> OrderProductAsync(string productName) {
            var product = Products.FirstOrDefault(x => x.Name == productName);
            if (product is null) {
                return NotFound();
            }

            var command = new PlaceOrder
            {
                OrderId = Guid.NewGuid().ToString(),
                Product = product,
            };

            return Ok();
        }
    }
}
