using System;
using NServiceBus;

namespace Messages
{
    public class OrderSubmitted : IEvent
    {
        public string OrderId { get; set; }
        public string DeliveryAddress { get; set; }
    }
}
