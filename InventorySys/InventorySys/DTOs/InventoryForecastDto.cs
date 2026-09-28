namespace InventorySys.DTOs
{
    public class InventoryForecastDto
    {
        public int IngredientId { get; set; }
        public string IngredientName { get; set; } = string.Empty;

        public string Unit { get; set; } = string.Empty;

        // 目前庫存
        public decimal CurrentStock { get; set; }

        // 最低庫存
        public decimal MinimumStock { get; set; }

        // 單位成本
        public decimal UnitCost { get; set; }

        // 最近幾天的平均每日使用量
        public decimal AverageDailyUsage { get; set; }

        // 預測期間
        public int ForecastDays { get; set; }

        // 預測需求量
        public decimal ForecastDemand { get; set; }

        // 建議補貨量
        public decimal SuggestedPurchase { get; set; }

        // 預估採購成本
        public decimal EstimatedCost { get; set; }
    }
}
