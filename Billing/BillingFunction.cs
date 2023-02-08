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
        [return: ServiceBus("OrderBilledQueue", Connection = "ServiceBusConnectionString")]
        public async static Task<string> Run(
            [ServiceBusTrigger(queueName: "BillingQueue", Connection = "ServiceBusConnectionString")]
            OrderPlaced message,
            ILogger logger)
        {
            logger.LogInformation($"Received OrderPlaced, OrderId = {message.OrderId} - Charging credit card...");

            bool result = await UploadBlobFile(message.OrderId);
            if (result)
            {
                logger.LogInformation($"Uploading invoice succesful!");
            }

            return JsonConvert.SerializeObject(message);
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
