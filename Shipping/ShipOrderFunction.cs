using Azure.Messaging.ServiceBus;
using DataLibrary.Events;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shipping {
    public class ShipOrderFunction {
        [FunctionName("ShipOrderFunction")]
        public async Task Run(
            [ServiceBusTrigger("ShippingQueue", Connection = "ServiceBusConnectionString")] ServiceBusReceivedMessage message,
            ILogger log) {
            log.LogInformation($"Let's ship this order!");
            OrderEntity order = JsonConvert.DeserializeObject<OrderEntity>(Encoding.UTF8.GetString(message.Body));
            log.LogInformation($"Shipping order: {order.PartitionKey}");

            //Settings
            var storageAccount = CloudStorageAccount.Parse(Environment.GetEnvironmentVariable("StorageAccount"));
            var tableClient = storageAccount.CreateCloudTableClient();
            var table = tableClient.GetTableReference("ordertable");
            await table.CreateIfNotExistsAsync();

            //Update table
            TableQuery<OrderEntity> retrieveOperation = new TableQuery<OrderEntity>()
                    .Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, order.RowKey));
            var orderList = await table.ExecuteQuerySegmentedAsync(retrieveOperation, null);
            OrderEntity foundInTable = orderList.FirstOrDefault();
            foundInTable.isShipped = true;
            var result = await table.ExecuteAsync(TableOperation.Merge(foundInTable));
            if (result.HttpStatusCode >= 200 && result.HttpStatusCode < 300) {
                log.LogInformation($"Shipping order succesful!");
            }
        }
    }
}
