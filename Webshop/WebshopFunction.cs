using DataLibrary.Commands;
using DataLibrary.Events;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Webshop {
    public static class WebshopFunction {
        [FunctionName("WebshopFunction")]
        //[return: ServiceBus("Orders", Connection = "ServiceBusConnectionString")]
        public static void Run([ServiceBus("orders", Connection = "ServiceBusConnectionString")] string myQueueItem, ILogger log) {
            /*log.LogInformation($"C# ServiceBus queue trigger function processed message: {myQueueItem}");
            PlaceOrder order = JsonConvert.DeserializeObject<PlaceOrder>(myQueueItem);

            OrderPlaced orderPlaced = new()
            {
                OrderId = order.OrderId
            };
            return JsonConvert.SerializeObject(orderPlaced);*/
        }
    }
}
