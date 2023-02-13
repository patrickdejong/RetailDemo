using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
