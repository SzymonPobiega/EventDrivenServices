namespace Shipping.Orders
{
    using System.Collections.Generic;

    public class Order
    {
        public Order()
        {
        }

        public Order(string orderId)
        {
            OrderId = orderId;
            Lines = new List<OrderLine>();
        }

        public string OrderId { get; set; }
        public virtual ICollection<OrderLine> Lines { get; set; }
    }
}