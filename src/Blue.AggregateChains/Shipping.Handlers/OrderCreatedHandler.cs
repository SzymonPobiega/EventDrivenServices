using System;
using System.Threading.Tasks;
using Messages;
using NServiceBus;
using NServiceBus.Logging;
using Shipping.Orders;

public class OrderCreatedHandler : IHandleMessages<OrderCreated>
{
    public Task Handle(OrderCreated message, IMessageHandlerContext context)
    {
        var order = new Order(message.OrderId);
        dataContext.Orders.Add(order);
        log.Info($"Order {message.OrderId} created.");
        return Task.CompletedTask;
    }

    public OrderCreatedHandler(OrdersDataContext dataContext)
    {
        this.dataContext = dataContext;
    }

    OrdersDataContext dataContext;
    static readonly ILog log = LogManager.GetLogger<OrderCreatedHandler>();
}