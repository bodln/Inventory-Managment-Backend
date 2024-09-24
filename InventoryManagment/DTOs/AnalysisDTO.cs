namespace InventoryManagment.DTOs
{
    public class AnalysisDTO
    {
        public ItemDTO Item{ get; set; }
        public int UnitsSold { get; set; }
        public float TotalProfit { get; set; }
        public List<LocationHistoryReturnDTO> Locations { get; set; }
    }
}
