using System;
using System.Data.SqlClient;
using NServiceBus;
using NServiceBus.Router;

namespace StoreGateway
{
    class Program
    {
        static void Main(string[] args)
        {
            var routerConfig = new RouterConfiguration("HQGateway");

            var sqlInterface = routerConfig.AddInterface<SqlServerTransport>("SQL", c =>
            {
                c.ConnectionString("data source=(local)");
            });

            var rabbitInterface = routerConfig.AddInterface<RabbitMQTransport>("Rabbit", c =>
            {
                c.ConnectionString("host=localhost");
            });

            sqlInterface.EnableDeduplication("Rabbit", "StoreGateway", 
                () => new SqlConnection("data source=(local)"), 10000);
        }
    }
}
