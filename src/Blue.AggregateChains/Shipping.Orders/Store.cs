namespace Shipping.Orders
{
    public class Store
    {
        public string StoreId { get; set; }
        public int OutstandingShipments { get; set; }

        public Store(string storeId)
        {
            StoreId = storeId;
        }

        public void AssignShipment()
        {
            OutstandingShipments++;
        }

        public Store()
        {
        }
    }
}
