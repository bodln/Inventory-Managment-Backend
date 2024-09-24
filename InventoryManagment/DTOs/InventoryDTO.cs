using InventoryManagment.Models;

namespace InventoryManagment.DTOs
{
    public class InventoryDTO
    {
        public Guid ItemGUID { get; set; }
        public int AvailableAmount { get; set; }
        public DateTime LastShipment { get; set; }
    }
}
