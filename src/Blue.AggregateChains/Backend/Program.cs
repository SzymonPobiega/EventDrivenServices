using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Messages;
using NServiceBus;
using NServiceBus.Logging;
using NServiceBus.Serilog;
using Serilog;
using Serilog.Filters;

class Program
{
    public const string ConnectionString = @"Data Source=(local);Database=MessagesOnTheOutside_AggregateChains;Integrated Security=True;Max Pool Size=100";

    const string letters = "abcdefghijklmnopqrstuvwxyz";
    static Random r = new Random();

    static void Main(string[] args)
    {
        Start().GetAwaiter().GetResult();
    }

    static readonly Regex createExpr = new Regex("create ([A-Za-z]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    static readonly Regex submitExpr = new Regex("submit ([A-Za-z]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    static readonly Regex addExpr = new Regex($"add ({string.Join("|", Enum.GetNames(typeof(Filling)))}) to ([A-Za-z]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    static async Task Start()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .Filter.ByExcluding(Matching.FromSource("NServiceBus.SubscriptionReceiverBehavior"))
            .WriteTo.Console()
            .CreateLogger();

        LogManager.Use<SerilogFactory>();

        Console.Title = "Orders";

        var config = new EndpointConfiguration("Blue.Orders");
        config.UsePersistence<InMemoryPersistence>();
        config.SendFailedMessagesTo("error");

        var transport = config.UseTransport<SqlServerTransport>();
        transport.Transactions(TransportTransactionMode.SendsAtomicWithReceive);
        transport.ConnectionString(ConnectionString);

        config.EnableInstallers();

        //SqlHelper.EnsureDatabaseExists(ConnectionString);

        var endpoint = await Endpoint.Start(config).ConfigureAwait(false);

        Console.WriteLine("'create <order-id>' to create a new order.");
        Console.WriteLine($"'add ({string.Join("|", Enum.GetNames(typeof(Filling)))}) to <order-id>' to add item with selected filling.");
        Console.WriteLine("'submit <order-id>' to submit an order.");

        while (true)
        {
            var command = Console.ReadLine();

            if (string.IsNullOrEmpty(command))
            {
                break;
            }

            var match = createExpr.Match(command);
            if (match.Success)
            {
                var orderId = match.Groups[1].Value;
                var message = new OrderCreated
                {
                    OrderId = orderId,
                };
                await endpoint.Publish(message).ConfigureAwait(false);
                continue;
            }
            match = submitExpr.Match(command);
            if (match.Success)
            {
                var orderId = match.Groups[1].Value;
                //Orders that start with same latter get same addresses and stores
                var rand = new Random(orderId[0].GetHashCode());
                var message = new OrderSubmitted
                {
                    OrderId = orderId,
                    DeliveryAddress = RandomWords(3, 5, 5, rand)
                };
                await endpoint.Publish(message).ConfigureAwait(false);
                continue;
            }
            match = addExpr.Match(command);
            if (match.Success)
            {
                var filling = match.Groups[1].Value;
                var orderId = match.Groups[2].Value;
                var message = new LineAdded
                {
                    OrderId = orderId,
                    Filling = (Filling)Enum.Parse(typeof(Filling), filling)
                };
                await endpoint.Publish(message).ConfigureAwait(false);
                continue;
            }
            Console.WriteLine("Unrecognized command.");
        }

        await endpoint.Stop().ConfigureAwait(false);
    }

    static string RandomWords(int minCount, int maxCount, int maxWordLength, Random random)
    {
        var numberOfWords = random.Next(minCount, maxCount + 1);
        return string.Join(" ", Enumerable.Range(0, numberOfWords).Select(i => RandomWord(maxWordLength, random)));
    }

    static string RandomWord(int maxLength, Random random)
    {
        var length = random.Next(2, maxLength + 1);
        return new string(Enumerable.Range(0, length).Select(i => letters[random.Next(letters.Length)]).ToArray());
    }
}
