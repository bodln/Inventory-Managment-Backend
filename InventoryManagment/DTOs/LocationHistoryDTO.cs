using InventoryManagment.Models;

namespace InventoryManagment.DTOs
{
    public class LocationHistoryDTO
    {
        public Guid InventoryGUID { get; set; }
        public string LocationName { get; set; }
    }
}
