using InventoryManagment.Models;

namespace InventoryManagment.DTOs
{
    public class ShipmentOrderReturnDTO
    {
        public Guid GUID { get; set; }
        public ItemDTO Item { get; set; }
        public int Quantity { get; set; }
        public ReturnUserDTO Manager { get; set; }
        public DateTime DateOfCreation { get; set; }
        public DateTime DateOfArrival { get; set; }
        public float Price { get; set; }
        public bool Unloaded { get; set; } = false;
    }
}
