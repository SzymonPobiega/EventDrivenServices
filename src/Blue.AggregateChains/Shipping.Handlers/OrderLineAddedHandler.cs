using System.Data.Entity;
using System.Threading.Tasks;
using Messages;
using NServiceBus;
using NServiceBus.Logging;
using Shipping.Orders;
using Filling = Shipping.Orders.Filling;

public class OrderLineAddedHandler : IHandleMessages<LineAdded>
{
    public async Task Handle(LineAdded message, IMessageHandlerContext context)
    {
        var order = await dataContext.Orders
            .SingleAsync(o => o.OrderId == message.OrderId).ConfigureAwait(false);

        var line = new OrderLine(Convert(message.Filling));
        log.Info($"Line {message.Filling} added to order {message.OrderId}.");
        order.Lines.Add(line);
    }

    static Filling Convert(Messages.Filling messageFilling)
    {
        return (Filling) (int) messageFilling;
    }

    public OrderLineAddedHandler(OrdersDataContext dataContext)
    {
        this.dataContext = dataContext;
    }

    OrdersDataContext dataContext;
    static readonly ILog log = LogManager.GetLogger<OrderLineAddedHandler>();
}