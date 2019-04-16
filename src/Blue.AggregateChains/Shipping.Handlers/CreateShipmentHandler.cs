using System.Threading.Tasks;
using Messages;
using NServiceBus;
using NServiceBus.Logging;
using Shipping.Stores;

public class CreateShipmentHandler : IHandleMessages<CreateShipment>
{
    public Task Handle(CreateShipment message, IMessageHandlerContext context)
    {
        var shipment = new Shipment(message.OrderId, message.StoreId);

        foreach (var item in message.Items)
        {
            shipment.Items.Add(new ShipmentItem(Convert(item)));
        }

        dataContext.Shipments.Add(shipment);
        log.Info($"Shipment for order {message.OrderId} created.");
        return Task.CompletedTask;
    }

    static Shipping.Stores.Filling Convert(Messages.Filling filling)
    {
        return (Shipping.Stores.Filling) (int) filling;
    }

    public CreateShipmentHandler(ShipmentsDataContext dataContext)
    {
        this.dataContext = dataContext;
    }

    static readonly ILog log = LogManager.GetLogger<CreateShipmentHandler>();
    ShipmentsDataContext dataContext;
}