namespace InventorySys.Models
{
    public class Ingredients
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;

        //每單位成本
        public decimal UnitCost { get; set; }

        //最低庫存量
        public decimal MinimumStock { get; set; }
    }
}
