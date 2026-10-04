using InventorySys.DTOs;

namespace InventorySys.Services
{
    public class InventoryToolService
    {
        private readonly ForecastService _forecastService;

        public InventoryToolService(ForecastService forecastService)
        {
            _forecastService = forecastService;
        }

        //Tool1 : 取得低庫存食材清單
        public async Task<List<InventoryForecastDto>> GetLowStockItems()
        {
            //使用既有庫存預測邏輯
            var forecasts = await _forecastService.GetForecast();

            //只留下需捕獲的食材
            return forecasts.Where(x => x.SuggestedPurchase > 0).ToList();
        }
        //Tool2 : 取得庫存預測清單
        public async Task<List<InventoryForecastDto>> GetInventoryForecast(int usageDays = 14, int forecastDays = 7)
        {
            return await _forecastService.GetForecast(usageDays, forecastDays);
        }

        //Tool3 : 取得庫存摘要資訊
        public async Task<InventorySummaryDto> GetInventorySummary()
        {
            var forecasts = await _forecastService.GetForecast();
            var summary = new InventorySummaryDto
            {
                TotalIngredients = forecasts.Count,
                NeedReplenishment = forecasts.Count(x => x.SuggestedPurchase > 0),
                TotalEstimatedCost = forecasts.Sum(x => x.EstimatedCost)
            };
            return summary;
        }

    }
}
