using Azure.Messaging.ServiceBus;
using DataLibrary.Commands;
using DataLibrary.Events;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Text.Json.Nodes;

namespace RetailServiceBusDemo
{
    public class SalesFunction
    {
        [FunctionName("SalesFunction")]
        [return: ServiceBus("Orders", Connection = "ServiceBusConnectionString")]
        public static ServiceBusMessage Run([ServiceBusTrigger("SalesQueue", Connection = "ServiceBusConnectionString")] string myQueueItem, ILogger log)
        {
            log.LogInformation($"C# ServiceBus queue trigger function processed message: {myQueueItem}");
            PlaceOrder order = JsonConvert.DeserializeObject<PlaceOrder>(myQueueItem);

            OrderPlaced orderPlaced = new()
            {
                OrderId = Guid.NewGuid().ToString()
            };

            ServiceBusMessage message = new ServiceBusMessage(JsonConvert.SerializeObject(orderPlaced));
            message.ApplicationProperties.Add("Type","OrderPlaced");
            message.Body = new BinaryData(JsonConvert.SerializeObject(orderPlaced));
            return message;
        }
    }
}
