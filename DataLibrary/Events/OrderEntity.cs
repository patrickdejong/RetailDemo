using Microsoft.WindowsAzure.Storage.Table;

namespace DataLibrary.Events {
    public class OrderEntity : TableEntity {
        public OrderEntity(string orderID) {
            this.PartitionKey = "Yolo123";
            this.RowKey = orderID;
        }

        public OrderEntity() {

        }

        public bool isOrderBilled { get; set; }
        public bool isOrderPlaced { get; set; }
        public bool isShipped { get; set; }
    }
}
