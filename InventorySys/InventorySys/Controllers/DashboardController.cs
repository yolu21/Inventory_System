using Microsoft.AspNetCore.Mvc;
using InventorySys.Data;
using InventorySys.Models;

namespace InventorySys.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly InventoryDbContext _context;
        public DashboardController(InventoryDbContext context)
        {
            _context = context;
        }
        [HttpGet("summary")]//所有食材的庫存進出量和庫存不足的食材數量
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

            var ingredients= _context.Ingredients.ToList();
            var stockRecords = _context.StockRecords.ToList();
            var lowStockIngredients = ingredients.Count(i =>
            {
                var records = stockRecords.Where(r => r.IngredientId == i.Id);
                var stock = records.Where(r => r.Type == "IN").Sum(r => r.Quantity) - records.Where(r => r.Type == "OUT").Sum(r => r.Quantity);
                return stock < 10;
            });

            return Ok(new
            {
                totalIngredients,
                totalIn,
                totalOut,
                lowStockIngredients
            });
        }

        [HttpGet("overview")]//各個食材的庫存概況
        public IActionResult GetIngredientsOverview()
        {
            var stockRecords = _context.StockRecords.ToList();
            var ingredients = _context.Ingredients.ToList();
            var data = ingredients.Select(i =>
            {
                var records = stockRecords.Where(r => r.IngredientId == i.Id);

                var inQty = records
                .Where(r => r.Type == "IN")
                .Sum(r => r.Quantity);

                var OutQty = records
                .Where(r => r.Type == "OUT")
                .Sum(r => r.Quantity);

                var stock = inQty - OutQty;

                return new
                {
                    i.Id,
                    i.Name,
                    i.Unit,
                    In = inQty,
                    Out = OutQty,
                    Stock = stock
                };
            }).ToList();

            var lowStock = data.Where(d => d.Stock < 10).ToList();

            var topUsage = data
            .OrderByDescending(x => x.Out)
            .Take(5)
            .ToList();

            return Ok(new
            {
                Summary = new
                {
                    totalIngredients = ingredients.Count,
                    lowStockCount = lowStock.Count
                },
                ingredients = data,
                lowStock,
                topUsage
            });
        }
    }
}
