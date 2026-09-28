using InventorySys.Data;
using InventorySys.DTOs;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.EntityFrameworkCore;

namespace InventorySys.Services
{
    public class ForecastService
    {
        private readonly InventoryDbContext _context;

        public ForecastService(InventoryDbContext context)
        {
            _context = context;
        }

        public async Task<List<InventoryForecastDto>> GetForecast(int usageDays = 14,int forecastDays = 7)
        {
            var ingredients = await _context.Ingredients.ToListAsync();

            var records = await _context.StockRecords.ToListAsync();

            var today = DateTime.Now.Date;
            var startDate = today.AddDays(-usageDays);

            var result = new List<InventoryForecastDto>();

            foreach (var ingredient in ingredients)
            {
                //計算目前庫存
                var currentStock = records.Where(x => x.IngredientId == ingredient.Id)
                                        .Sum(x => x.Type == "IN" ? x.Quantity : -x.Quantity);

                //最近 N 天的OUT使用量
                var usage = records.Where(x => x.IngredientId == ingredient.Id && x.Type == "OUT" &&
                                          x.Date.Date >= startDate && x.Date.Date <= today).Sum(x => x.Quantity);
                //平均每天使用量
                var averageDailyUsage = usageDays > 0 ? usage / usageDays : 0;

                //預測未來需求
                var forecastDemand = averageDailyUsage * forecastDays;

                //建議補貨量
                var suggestedPurchase = forecastDemand + ingredient.MinimumStock - currentStock;

                if(suggestedPurchase < 0)
                {
                    suggestedPurchase = 0;
                }

                //預估採購成本
                var estimatedCost = suggestedPurchase * ingredient.UnitCost;

                result.Add(new InventoryForecastDto
                {
                    IngredientId = ingredient.Id,
                    IngredientName = ingredient.Name,
                    Unit = ingredient.Unit,

                    CurrentStock = currentStock,
                    MinimumStock = ingredient.MinimumStock,
                    UnitCost = ingredient.UnitCost,

                    AverageDailyUsage = averageDailyUsage,
                    ForecastDays = forecastDays,
                    ForecastDemand = forecastDemand,

                    SuggestedPurchase = suggestedPurchase,
                    EstimatedCost = estimatedCost
                });
            }
            return result;

        }
    }
}
