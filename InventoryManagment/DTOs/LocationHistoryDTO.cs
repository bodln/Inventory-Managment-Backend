using InventoryManagment.Models;

namespace InventoryManagment.DTOs
{
    public class LocationHistoryDTO
    {
        public Guid InventoryGUID { get; set; }
        public string LocationName { get; set; }
        public int Quantity { get; set; } = 0;
    }
}
