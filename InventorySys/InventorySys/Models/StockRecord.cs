namespace InventorySys.Models
{
    public class StockRecord
    {
        public int id { get; set; }
        public int IngredientId { get; set; }
        public string Type { get; set; } = string.Empty;//IN/OUT

        public decimal Quantity { get; set; }
        public DateTime Date { get; set; }

    }
}