namespace InventoryManagment.Models
{
    public class Item
    {
        public int Id { get; set; }
        public Guid GUID { get; set; } = Guid.NewGuid();
        public string Naziv { get; set; }
        public float Weight { get; set; }
        public string Type { get; set; }
        public float Price { get; set; }
        public int MaxAmount { get; set; }
        public int MinAmount { get; set; }
        //public List<BillOfSale> BillOfSales { get; set; }
        //public List<ShipmentOrder> ShipmentOrders { get; set; }
    }
}
