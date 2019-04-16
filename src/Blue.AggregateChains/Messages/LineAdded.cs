using NServiceBus;

namespace Messages
{
    public class LineAdded : IEvent
    {
        public LineAdded()
        {
        }

        public LineAdded(string orderId, Filling filling)
        {
            OrderId = orderId;
            Filling = filling;
        }

        public string OrderId { get; set; }
        public Filling Filling { get; set; }
    }
}