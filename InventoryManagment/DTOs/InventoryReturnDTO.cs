using InventoryManagment.Models;

namespace InventoryManagment.DTOs
{
    public class InventoryReturnDTO
    {
        public Guid GUID { get; set; } = Guid.NewGuid();
        public ItemDTO Item { get; set; }
        public int AvailableAmount { get; set; }
        public DateTime LastShipment { get; set; }
        public string CurrentLocationName { get; set; }
    }
}
