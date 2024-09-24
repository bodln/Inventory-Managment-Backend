using InventoryManagment.Models;

namespace InventoryManagment.DTOs
{
    public class ShipmentOrderDTO
    {
        public Guid ItemGUID { get; set; }
        public int Quantity { get; set; }
        public DateTime DateOfCreation { get; set; }
        public float Price { get; set; }
        public bool Unloaded { get; set; } = false;
    }
}
