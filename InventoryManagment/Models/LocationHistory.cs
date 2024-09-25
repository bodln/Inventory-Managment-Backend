namespace InventoryManagment.Models
{
    public class LocationHistory
    {
        public int Id { get; set; }
        public Guid GUID { get; set; } = Guid.NewGuid();
        public DateTime DateOfStoring { get; set; } = DateTime.UtcNow;
        public User Warehouseman { get; set; }
        public Inventory Inventory { get; set; }
        public string LocationName { get; set; }
        public int Quantity { get; set; } = 0;
    }
}
