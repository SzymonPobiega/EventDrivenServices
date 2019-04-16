using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Data.SqlClient;
using Shipping.Stores;

public class ShipmentsDataContext :
    DbContext
{
    public ShipmentsDataContext(ConnectionHolder connectionHolder)
        : base(connectionHolder.Connection, false)
    {
        Database.UseTransaction(connectionHolder.Transaction);
        connectionHolder.Register(this);
    }

    public ShipmentsDataContext(IDbConnection connection)
        : base((DbConnection)connection, (bool)false)
    {
    }

    public ShipmentsDataContext(string connectionString)
        : base(new SqlConnection(connectionString), true)
    {
    }

    public DbSet<Shipment> Shipments { get; set; }
    public DbSet<ShipmentItem> ShipmentItems { get; set; }

    protected override void OnModelCreating(DbModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var orders = modelBuilder.Entity<Shipment>();
        orders.ToTable("Shipments");
        orders.HasKey(x => x.Id);
        orders.HasMany(x => x.Items).WithRequired().HasForeignKey(x => x.ShipmentId).WillCascadeOnDelete();
        orders.Property(x => x.StoreId);

        var lines = modelBuilder.Entity<ShipmentItem>();
        lines.ToTable("ShipmentItems");
        lines.HasKey(x => x.Id);
        lines.Property(x => x.Filling);
    }
}