using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Data.SqlClient;
using Shipping.Orders;

public class OrdersDataContext :
    DbContext
{
    public OrdersDataContext(ConnectionHolder connectionHolder)
        : base(connectionHolder.Connection, false)
    {
        Database.UseTransaction(connectionHolder.Transaction);
        connectionHolder.Register(this);
    }

    public OrdersDataContext(IDbConnection connection)
        : base((DbConnection)connection, (bool) false)
    {
    }

    public OrdersDataContext(string connectionString)
        : base(new SqlConnection(connectionString), true)
    {

    }

    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderLine> OrderLines { get; set; }
    public DbSet<Store> Stores { get; set; }

    protected override void OnModelCreating(DbModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var orders = modelBuilder.Entity<Order>();
        orders.ToTable("Orders");
        orders.HasKey(x => x.OrderId);
        orders.HasMany(x => x.Lines).WithRequired().HasForeignKey(x => x.OrderId).WillCascadeOnDelete();

        var lines = modelBuilder.Entity<OrderLine>();
        lines.ToTable("OrderLines");
        lines.HasKey(x => x.Id);
        lines.Property(x => x.Filling);

        var stores = modelBuilder.Entity<Store>();
        stores.ToTable("Stores");
        stores.HasKey(x => x.StoreId);
    }
}