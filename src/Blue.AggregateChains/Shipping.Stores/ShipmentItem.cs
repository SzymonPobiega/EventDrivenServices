using System;

namespace Shipping.Stores
{
    public class ShipmentItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Filling Filling { get; set; }
        public Guid ShipmentId { get; set; }

        public ShipmentItem()
        {
        }

        public ShipmentItem(Filling filling)
        {
            Filling = filling;
        }
    }
}