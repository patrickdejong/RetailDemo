using Azure.Messaging.ServiceBus;
using DataLibrary.Events;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Shipping {
    public class ShippingFunction {
        [FunctionName("ShippingFunction")]
        //[return: ServiceBus("Orders", Connection = "ServiceBusConnectionString")]
        public static async Task Run(
            [ServiceBusTrigger("OrderBilledQueue", Connection = "ServiceBusConnectionString")] ServiceBusMessage message,
            ILogger log) {
            log.LogInformation($"C# ServiceBus queue trigger function processed message: {message}");

            

            foreach (var prop in message.ApplicationProperties) {
                log.LogInformation($": {prop.Key}: {prop.Value}");
            }





            // Peek incoming 50 messages from the queue
            /*ServiceBusClient serviceBusClient = new ServiceBusClient(Environment.GetEnvironmentVariable("ServiceBusConnectionString"));
            var receiver = serviceBusClient.CreateReceiver("OrderBilledQueue");
            var messagesOfQueue = await receiver.PeekMessagesAsync(50);
*/
            // For every message, check if the order inside contains the same OrderId
            //foreach (var queuedMessage in messagesOfQueue) {
            //    // TODO: Message should not check itself
            //    // Is er een manier om Message ID's bij te houden?

            //    log.LogInformation($"Incoming message body: {queuedMessage.Body}");

            //    var data = (JObject)JsonConvert.DeserializeObject(queuedMessage.Body.ToString());
            //    var orderId = data.SelectToken("OrderId").Value<string>();

            //    // When the message's incoming orderId equals the OrderId, we know it's been activated by Sales & Billing
            //    if (orderId == message.OrderId) {
            //        log.LogWarning($"Dit is het ");
            //    }
            //}
        }
    }
}
