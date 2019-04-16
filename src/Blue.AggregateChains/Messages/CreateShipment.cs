using System.Collections.Generic;
using NServiceBus;

namespace Messages
{
    public class CreateShipment : ICommand
    {
        public string StoreId { get; set; }
        public string OrderId { get; set; }
        public string DeliveryAddress { get; set; }
        public List<Filling> Items { get; set; }
    }
}