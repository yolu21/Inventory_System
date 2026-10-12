using Microsoft.AspNetCore.Mvc;
using InventorySys.Data;
using Microsoft.AspNetCore.Authorization;

namespace InventorySys.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly InventoryDbContext _context;

        public DashboardController(InventoryDbContext context)
        {
            _context = context;
        }

        [HttpGet("summary")]
        // 所有食材的庫存進出量和庫存不足的食材數量
        public IActionResult GetAllIngredient()
        {
            var totalIngredients = _context.Ingredients.Count();

            var totalIn = _context.StockRecords
                .Where(r => r.Type == "IN")
                .Select(r => (decimal?)r.Quantity)
                .Sum() ?? 0;

            var totalOut = _context.StockRecords
                .Where(r => r.Type == "OUT")
                .Select(r => (decimal?)r.Quantity)
                .Sum() ?? 0;

            var ingredients = _context.Ingredients.ToList();
            var stockRecords = _context.StockRecords.ToList();

            var lowStockIngredients = ingredients.Count(i =>
            {
                var records = stockRecords
                    .Where(r => r.IngredientId == i.Id);

                var stock =
                    records
                        .Where(r => r.Type == "IN")
                        .Sum(r => r.Quantity)
                    -
                    records
                        .Where(r => r.Type == "OUT")
                        .Sum(r => r.Quantity);

                return stock < i.MinimumStock;
            });

            return Ok(new
            {
                totalIngredients,
                totalIn,
                totalOut,
                lowStockIngredients
            });
        }

        [HttpGet("overview")]
        // 目前庫存 + 指定月份的進出貨統計
        public IActionResult GetIngredientsOverview(
            int? year,
            int? month)
        {
            // ============================
            // 1. 決定查詢月份
            // ============================

            var now = DateTime.Now;

            var selectedYear = year ?? now.Year;
            var selectedMonth = month ?? now.Month;

            // 確認月份是否合法
            if (selectedMonth < 1 || selectedMonth > 12)
            {
                return BadRequest("月份必須介於 1 到 12。");
            }

            // ============================
            // 2. 建立月份日期範圍
            // ============================

            var startDate = new DateTime(
                selectedYear,
                selectedMonth,
                1
            );

            var endDate = startDate.AddMonths(1);

            // ============================
            // 3. 取得全部資料
            //    用來計算「目前庫存」
            // ============================

            var stockRecords = _context.StockRecords.ToList();
            var ingredients = _context.Ingredients.ToList();

            // ============================
            // 4. 取得指定月份的進出貨紀錄
            // ============================

            var monthlyRecords = stockRecords
                .Where(r =>
                    r.Date >= startDate &&
                    r.Date < endDate
                )
                .ToList();

            // ============================
            // 5. 每個食材的庫存與月份統計
            // ============================

            var data = ingredients.Select(i =>
            {
                // 全部歷史紀錄
                var records = stockRecords
                    .Where(r => r.IngredientId == i.Id);

                // 指定月份紀錄
                var monthlyIngredientRecords = monthlyRecords
                    .Where(r => r.IngredientId == i.Id);

                // ----------------------------
                // 目前庫存
                // ----------------------------

                var totalIn = records
                    .Where(r => r.Type == "IN")
                    .Sum(r => r.Quantity);

                var totalOut = records
                    .Where(r => r.Type == "OUT")
                    .Sum(r => r.Quantity);

                var stock = totalIn - totalOut;

                // ----------------------------
                // 指定月份進出貨量
                // ----------------------------

                var monthlyIn = monthlyIngredientRecords
                    .Where(r => r.Type == "IN")
                    .Sum(r => r.Quantity);

                var monthlyOut = monthlyIngredientRecords
                    .Where(r => r.Type == "OUT")
                    .Sum(r => r.Quantity);

                return new
                {
                    i.Id,
                    i.Name,
                    i.Unit,
                    i.MinimumStock,

                    // 目前庫存
                    Stock = stock,

                    // 指定月份
                    In = monthlyIn,
                    Out = monthlyOut
                };
            }).ToList();

            // ============================
            // 6. 目前低庫存
            // ============================

            var lowStock = data
                .Where(d => d.Stock < d.MinimumStock)
                .ToList();

            // ============================
            // 7. 指定月份使用量最高
            // ============================

            var topUsage = data
                .OrderByDescending(x => x.Out)
                .Take(5)
                .ToList();

            // ============================
            // 8. 回傳資料
            // ============================

            return Ok(new
            {
                selectedYear,
                selectedMonth,

                Summary = new
                {
                    totalIngredients = ingredients.Count,
                    lowStockCount = lowStock.Count,

                    // 指定月份
                    monthlyIn = data.Sum(x => x.In),
                    monthlyOut = data.Sum(x => x.Out)
                },

                ingredients = data,
                lowStock,
                topUsage
            });
        }
    }
}