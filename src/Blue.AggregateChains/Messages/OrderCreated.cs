using NServiceBus;

namespace Messages
{
    public class OrderCreated : IEvent
    {
        public string OrderId { get; set; }
    }
}