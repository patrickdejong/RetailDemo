using Azure.Messaging.ServiceBus;
using DataLibrary.Enum;
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
    public class ShippingFunction {
        [FunctionName("ShippingFunction")]
        [return: ServiceBus("Orders", Connection = "ServiceBusConnectionString")]
        public static async Task<ServiceBusMessage> Run(
            [ServiceBusTrigger("OrderBilledQueue", Connection = "ServiceBusConnectionString")] ServiceBusReceivedMessage message,
            ILogger log) {
            log.LogInformation($"C# ServiceBus queue trigger function processed message: {message}");

            var order = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(message.Body));

            //Type ophalen
            switch (message.ApplicationProperties.FirstOrDefault(m => m.Key == "Type").Value) {
                case "OrderPlaced":
                    OrderPlaced orderPlaced = JsonConvert.DeserializeObject<OrderPlaced>(Encoding.UTF8.GetString(message.Body));
                    OrderEntity orderEntity = new OrderEntity(orderPlaced.OrderId)
                    {
                        isOrderPlaced = true
                    };
                    return await UpdateOrderInStorage(orderEntity, orderPlaced.OrderId, Operation.OrderPlaced);
                case "OrderBilled":
                    OrderBilled orderBilled = JsonConvert.DeserializeObject<OrderBilled>(Encoding.UTF8.GetString(message.Body));
                    OrderEntity orderEntity2 = new OrderEntity(orderBilled.OrderId)
                    {
                        isOrderBilled = true
                    };
                    return await UpdateOrderInStorage(orderEntity2, orderBilled.OrderId, Operation.OrderBilled);
                default:
                    break;
            }
            return null;
        }

        public async static Task<ServiceBusMessage> UpdateOrderInStorage(OrderEntity orderEntity, string orderId, Operation operation) {
            //Settings
            var storageAccount = CloudStorageAccount.Parse(Environment.GetEnvironmentVariable("StorageAccount"));
            var tableClient = storageAccount.CreateCloudTableClient();
            var table = tableClient.GetTableReference("ordertable");
            await table.CreateIfNotExistsAsync();

            // Insert or Update storage
            try {
                // Get rows
                TableQuery<OrderEntity> retrieveOperation = new TableQuery<OrderEntity>()
                    .Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, orderId));
                var orderList = await table.ExecuteQuerySegmentedAsync(retrieveOperation, null);
                OrderEntity foundInTable = orderList.FirstOrDefault();

                // Add existing data to entity
                if (foundInTable != null) {
                    if (foundInTable != null) {
                        switch (operation) {
                            case Operation.OrderPlaced:
                                orderEntity.isOrderBilled = foundInTable.isOrderBilled;
                                break;
                            case Operation.OrderBilled:
                                orderEntity.isOrderPlaced = foundInTable.isOrderPlaced;
                                break;
                        }
                    }
                }
                if (foundInTable == null) {
                    await table.ExecuteAsync(TableOperation.Insert(orderEntity));
                } else {
                    // Update table
                    orderEntity.ETag = "*";
                    await table.ExecuteAsync(TableOperation.Replace(orderEntity));
                }

                // If placed and billed, ship order!
                if (orderEntity.isOrderPlaced && orderEntity.isOrderBilled) {
                    ServiceBusMessage serviceBusMessage = new ServiceBusMessage(JsonConvert.SerializeObject(orderEntity));
                    serviceBusMessage.ApplicationProperties.Add("Type", "OrderShipped");
                    return serviceBusMessage;
                }
                return null;
            }
            catch (Exception e) {
                Console.WriteLine($"\n!!! Something went wrong in table operation. ({e})!!!");
                throw;
            }
        }
    }
}
