using InventoryManagment.Models;

namespace InventoryManagment.DTOs
{
    public class BillOfSaleDTO
    {
        public Guid ItemGUID { get; set; }
        public int Quantity { get; set; }
        public DateTime DateOfSale { get; set; }
        public string NameOfBuyer { get; set; } = string.Empty;
        public string ContactInfo { get; set; }
        public string DeliveryAddress { get; set; }
    }
}
