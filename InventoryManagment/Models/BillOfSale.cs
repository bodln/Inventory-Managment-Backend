using static System.Runtime.InteropServices.JavaScript.JSType;

namespace InventoryManagment.Models
{
    public class BillOfSale
    {
        public int Id { get; set; }
        public Guid GUID { get; set; } = Guid.NewGuid();
        public Item Item { get; set; }
        public int Quantity { get; set; }
        public User Warehouseman { get; set; }
        public DateTime DateOfSale { get; set; }
        public string NameOfBuyer { get; set; } = string.Empty;
        public string ContactInfo { get; set; }
        public string DeliveryAddress { get; set; }
    }
}
