namespace InventoryManagment.DTOs
{
    public class StockDTO
    {
        public ItemDTO Item { get; set; }
        public int AvilableAmount { get; set; }
        public bool OverCapacity { get; set; } = false;
        public bool UnderCapacity { get; set; } = false;
        public bool MoreInStorage { get; set; } = false;
    }
}
