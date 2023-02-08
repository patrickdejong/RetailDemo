using DataLibrary.Commands;
using DataLibrary.Events;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace RetailServiceBusDemo
{
    public class SalesFunction
    {
        [FunctionName("SalesFunction")]
        [return: ServiceBus("BillingQueue", Connection = "ServiceBusConnectionString")]
        public static string Run([ServiceBusTrigger("SalesQueue", Connection = "ServiceBusConnectionString")] string myQueueItem, ILogger log)
        {
            log.LogInformation($"C# ServiceBus queue trigger function processed message: {myQueueItem}");
            PlaceOrder order = JsonConvert.DeserializeObject<PlaceOrder>(myQueueItem);

            OrderPlaced orderPlaced = new()
            {
                OrderId = order.OrderId
            };
            return JsonConvert.SerializeObject(orderPlaced);
        }
    }
}
