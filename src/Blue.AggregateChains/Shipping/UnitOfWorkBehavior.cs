using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using NServiceBus.Pipeline;
using NServiceBus.Transport;

public class UnitOfWorkBehavior : Behavior<ITransportReceiveContext>
{
    public override async Task Invoke(ITransportReceiveContext context, Func<Task> next)
    {
        var connectionHolder = context.Builder.Build<ConnectionHolder>();
        var transportTransaction = context.Extensions.Get<TransportTransaction>();
        connectionHolder.Connection = transportTransaction.Get<SqlConnection>();
        connectionHolder.Transaction = transportTransaction.Get<SqlTransaction>();

        await next().ConfigureAwait(false);

        await connectionHolder.SaveChanges();
    }
}