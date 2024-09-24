using InventoryManagment.Models;

namespace InventoryManagment.DTOs
{
    public class BillOfSaleReturnDTO
    {
        public Guid GUID { get; set; } = Guid.NewGuid();
        public ItemDTO Item { get; set; }
        public int Quantity { get; set; }
        public ReturnUserDTO Warehouseman { get; set; }
        public DateTime DateOfSale { get; set; }
        public string NameOfBuyer { get; set; } = string.Empty;
        public string ContactInfo { get; set; }
        public string DeliveryAddress { get; set; }
    }
}
