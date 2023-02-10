using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using DataLibrary.Events;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Billing
{
    public class BillingFunction
    {
        [FunctionName("OrderPlacedHandler")]
        [return: ServiceBus("orders", Connection = "ServiceBusConnectionString")]
        public async static Task<ServiceBusMessage> Run(
            [ServiceBusTrigger(queueName: "BillingQueue", Connection = "ServiceBusConnectionString")]
            string message,
            ILogger logger)
        {
            OrderPlaced orderPlaced = JsonConvert.DeserializeObject<OrderPlaced>(message);

            logger.LogInformation($"Received OrderPlaced, OrderId = {orderPlaced.OrderId} - Charging credit card...");

            bool result = await UploadBlobFile(orderPlaced.OrderId);
            if (result)
            {
                logger.LogInformation($"Uploading invoice succesful!");
            }
            OrderBilled order = new OrderBilled() { OrderId = orderPlaced.OrderId};

            ServiceBusMessage serviceBusMessage = new ServiceBusMessage(JsonConvert.SerializeObject(order));
            serviceBusMessage.ApplicationProperties.Add("Type", "OrderBilled");
            return serviceBusMessage;
        }

        public static async Task<bool> UploadBlobFile(string orderId)
        {
            string Connection = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
            string containerName = Environment.GetEnvironmentVariable("ContainerName");
            string paymentText = ("Betaling succesvol!\nWebshop Cloud Republic");
            Stream myBlob = new MemoryStream(Encoding.UTF8.GetBytes(paymentText));
            var fileName = $@"invoice-{orderId}.txt";
            var blobClient = new BlobContainerClient(Connection, containerName);
            var blob = blobClient.GetBlobClient(fileName);
            await blob.UploadAsync(myBlob);
            return true;
        }
    }
}
