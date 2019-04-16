using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Messages;
using NServiceBus;
using NServiceBus.Logging;
using Filling = Shipping.Orders.Filling;

public class OrderSubmittedHandler : IHandleMessages<OrderSubmitted>
{
    public async Task Handle(OrderSubmitted message, IMessageHandlerContext context)
    {
        var order = await dataContext.Orders
            .SingleAsync(o => o.OrderId == message.OrderId).ConfigureAwait(false);

        var items = order.Lines.Select(x => Convert(x.Filling)).ToList();

        var storeId = FindClosestStore(message.DeliveryAddress);

        var store = await dataContext.Stores
            .SingleAsync(s => s.StoreId == storeId);

        store.AssignShipment();

        var createShipmentMessage = new CreateShipment
        {
            OrderId = order.OrderId,
            DeliveryAddress = message.DeliveryAddress,
            StoreId = storeId,
            Items = items
        };

        await context.SendLocal(createShipmentMessage).ConfigureAwait(false);

        log.Info($"Order {message.OrderId} with delivery to {message.DeliveryAddress} submitted.");

        await delayGenerator.WaitFor(message.OrderId).ConfigureAwait(false);
    }

    static string FindClosestStore(string deliveryAddress)
    {
        //Complex geo-location logic follows
        return (Math.Abs(deliveryAddress.GetHashCode()) % 5).ToString();
    }

    static Messages.Filling Convert(Filling filling)
    {
        return (Messages.Filling) (int) filling;
    }

    public OrderSubmittedHandler(OrdersDataContext dataContext, IDelayGenerator delayGenerator)
    {
        this.dataContext = dataContext;
        this.delayGenerator = delayGenerator;
    }

    OrdersDataContext dataContext;
    IDelayGenerator delayGenerator;
    static readonly ILog log = LogManager.GetLogger<OrderLineAddedHandler>();
}