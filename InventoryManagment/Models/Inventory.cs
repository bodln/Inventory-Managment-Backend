namespace InventoryManagment.Models
{
    public class Inventory
    {
        public int Id { get; set; }
        public Guid GUID { get; set; } = Guid.NewGuid();
        public Item Item { get; set; }
        public int AvailableAmount { get; set; }
        public DateTime LastShipment { get; set; }
    }
}
