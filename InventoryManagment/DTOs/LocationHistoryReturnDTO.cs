using InventoryManagment.Models;

namespace InventoryManagment.DTOs
{
    public class LocationHistoryReturnDTO
    {
        public DateTime DateOfStoring { get; set; }
        public ReturnUserDTO Warehouseman { get; set; }
        public InventoryReturnDTO Inventory { get; set; }
        public string LocationName { get; set; }
        public int Quantity { get; set; } = 0;
    }
}
