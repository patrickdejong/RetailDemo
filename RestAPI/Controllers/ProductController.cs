using Azure.Messaging.ServiceBus;
using DataLibrary.Model;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

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

        // GET: api/<ProductController>
        [HttpGet]
        public IEnumerable<Product> Get() {
            return Products;
        }

        // POST: api/<ProductController>/order
        [HttpPost("order")]
        public async Task OrderProductAsync(string productName) {
            var product = Products.FirstOrDefault(x => x.Name == productName);
            if (product is null) {
                throw new ArgumentException("Dit product bestaat niet");
            }

            // Create serialised ServiceBusMessage object
            OrderInfo orderInfo = new OrderInfo
            {
                OrderId = Guid.NewGuid().ToString(),
                Product = product,
                Buyer = "Patrick"
            };
            var serialisedOrder = JsonConvert.SerializeObject(orderInfo);
            ServiceBusMessage serviceBusMessage = new ServiceBusMessage(serialisedOrder);
            serviceBusMessage.ApplicationProperties.Add("Type", "PlaceOrder");

            // Setup ServiceBus communication
            ServiceBusClient client;
            ServiceBusSender sender;
            var configuration = new ConfigurationBuilder().AddJsonFile($"appsettings.json");
            var config = configuration.Build();
            var connectionString = config.GetConnectionString("ServiceBusConnectionString");
            client = new ServiceBusClient(connectionString);
            sender = client.CreateSender("orders");

            // Create a batch and send message
            using ServiceBusMessageBatch messageBatch = await sender.CreateMessageBatchAsync();
            messageBatch.TryAddMessage(serviceBusMessage);

            try {
                await sender.SendMessagesAsync(messageBatch);
            }
            finally {
                // Calling DisposeAsync on client types is required to ensure that network
                // resources and other unmanaged objects are properly cleaned up.
                await sender.DisposeAsync();
                await client.DisposeAsync();
            }
        }
    }
}
