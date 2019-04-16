using System;
using System.Collections.Generic;

namespace Shipping.Stores
{
    public class Shipment
    {
        public string StoreId { get; set; }
        public Guid Id { get; set; } = Guid.NewGuid();
        public string OrderId { get; set; }

        public virtual ICollection<ShipmentItem> Items { get; set; }

        public Shipment()
        {
        }

        public Shipment(string orderId, string storeId)
        {
            OrderId = orderId;
            StoreId = storeId;
            Items = new List<ShipmentItem>();
        }
    }
}