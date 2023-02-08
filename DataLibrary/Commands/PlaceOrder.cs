using DataLibrary.Model;

namespace DataLibrary.Commands
{
    public class PlaceOrder
    {
        public string OrderId { get; set; }
        public Product Product { get; set; }
    }
}
