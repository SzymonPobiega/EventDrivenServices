using System;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Transactions;
using Messages;
using NServiceBus;
using NServiceBus.Logging;
using NServiceBus.Serilog;
using NServiceBus.Transport.SQLServer;
using Serilog;
using Shipping.Orders;

class Program
{
    public const string ConnectionString = @"Data Source=(local);Database=MessagesOnTheOutside_AggregateChains;Integrated Security=True;Max Pool Size=100";

    static void Main(string[] args)
    {
        Start().GetAwaiter().GetResult();
    }

    static async Task Start()
    {
        Console.Title = "Shipping";

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .Enrich.With(new ExceptionMessageEnricher())
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{ExceptionMessage}{NewLine}")
            .CreateLogger();

        LogManager.Use<SerilogFactory>();
        var consoleController = new ConsoleController();

        var config = new EndpointConfiguration("Blue.Shipping");
        config.UsePersistence<InMemoryPersistence>();

        var transport = config.UseTransport<SqlServerTransport>();
        transport.Transactions(TransportTransactionMode.SendsAtomicWithReceive);
        transport.TransactionScopeOptions(null, IsolationLevel.RepeatableRead);
        transport.UseCustomSqlConnectionFactory(async () =>
        {
            var connection = new SqlConnection(ConnectionString);
            await connection.OpenAsync().ConfigureAwait(false);

            using (var cmd = new SqlCommand("SET XACT_ABORT ON", connection))
            {
                await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            }

            return connection;
        });

        var routing = transport.Routing();
        routing.RegisterPublisher(typeof(OrderSubmitted), "Blue.Orders");
        routing.RegisterPublisher(typeof(OrderCreated), "Blue.Orders");
        routing.RegisterPublisher(typeof(LineAdded), "Blue.Orders");
        routing.RouteToEndpoint(typeof(CreateShipment), "Blue.Shipping");

        config.Pipeline.Register(new UnitOfWorkBehavior(), "Unit of work");
        config.RegisterComponents(c =>
        {
            c.ConfigureComponent<ConnectionHolder>(DependencyLifecycle.InstancePerUnitOfWork);
            c.ConfigureComponent<OrdersDataContext>(DependencyLifecycle.InstancePerUnitOfWork);
            c.ConfigureComponent<ShipmentsDataContext>(DependencyLifecycle.InstancePerUnitOfWork);
            c.RegisterSingleton(typeof(IDelayGenerator), consoleController);
        });

        config.Recoverability().Immediate(x => x.NumberOfRetries(5));
        config.Recoverability().Delayed(x => x.NumberOfRetries(0));
        config.SendFailedMessagesTo("error");
        config.EnableInstallers();

        //SqlHelper.EnsureDatabaseExists(ConnectionString);

        using (var ordersDataContext = new OrdersDataContext(ConnectionString))
        {
            ordersDataContext.Database.Initialize(true);

            await EnsureStoreExists(ordersDataContext, "0").ConfigureAwait(false);
            await EnsureStoreExists(ordersDataContext, "1").ConfigureAwait(false);
            await EnsureStoreExists(ordersDataContext, "2").ConfigureAwait(false);
            await EnsureStoreExists(ordersDataContext, "3").ConfigureAwait(false);
            await EnsureStoreExists(ordersDataContext, "4").ConfigureAwait(false);

            await ordersDataContext.SaveChangesAsync().ConfigureAwait(false);
        }

        using (var storesDataContext = new ShipmentsDataContext(ConnectionString))
        {
            storesDataContext.Database.Initialize(true);
        }

        var endpoint = await Endpoint.Start(config).ConfigureAwait(false);

        await endpoint.Subscribe(typeof(OrderSubmitted));

        //Console.WriteLine("Press <enter> to exit.");
        //Console.ReadLine();

        while (true)
        {
            var sequence = Console.ReadLine();
            consoleController.OnTyped(sequence);
        }

        await endpoint.Stop().ConfigureAwait(false);
    }

    static async Task EnsureStoreExists(OrdersDataContext ordersDataContext, string storeId)
    {
        var s = await ordersDataContext.Stores.FirstOrDefaultAsync(x => x.StoreId == storeId);
        if (s == null)
        {
            s = new Store(storeId);
            ordersDataContext.Stores.Add(s);
        }
    }
}