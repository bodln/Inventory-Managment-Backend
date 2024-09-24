using static System.Runtime.InteropServices.JavaScript.JSType;

namespace InventoryManagment.Models
{
    public class ShipmentOrder
    {
        public int Id { get; set; }
        public Guid GUID { get; set; } = Guid.NewGuid();
        public Item Item { get; set; }
        public int Quantity { get; set; }
        public User Manager { get; set; }
        public DateTime DateOfCreation { get; set; }
        public DateTime DateOfArrival { get; set; }
        public float Price { get; set; }
        public bool Unloaded { get; set; } = false;
    }
}
