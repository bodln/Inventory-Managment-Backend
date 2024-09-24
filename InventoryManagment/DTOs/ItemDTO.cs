namespace InventoryManagment.DTOs
{
    public class ItemDTO
    {
        public Guid GUID { get; set; }
        public string Naziv { get; set; }
        public float Weight { get; set; }
        public string Type { get; set; }
        public float Price { get; set; }
        public int MaxAmount { get; set; }
        public int MinAmount { get; set; }
    }
}
