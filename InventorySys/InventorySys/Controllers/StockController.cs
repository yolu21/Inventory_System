using Microsoft.AspNetCore.Mvc;
using InventorySys.Models;
using InventorySys.Data;
using Microsoft.AspNetCore.Authorization;

namespace InventorySys.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class StockController : ControllerBase
    {
        private readonly InventoryDbContext _context;
        public StockController(InventoryDbContext context)
        {
            _context = context;
        }
        //取得單一食材庫存
        [HttpGet("stock/{id}")]
        public IActionResult GetStock(int id)
        {
            var records = _context.StockRecords
                .Where(x => x.IngredientId == id)
                .ToList();

            decimal totalIn = records
                .Where(x => x.Type == "IN")
                .Sum(x => x.Quantity);

            decimal totalOut = records
                .Where(x => x.Type == "OUT")
                .Sum(x => x.Quantity);

            decimal stock = totalIn - totalOut;
            return Ok(new
            {
                IngredientId = id,
                TotalIn = totalIn,
                TotalOut = totalOut,
                Stock = stock
            });
        }
        [HttpPost]
        //新增進出貨紀錄
        [HttpPost]
        public IActionResult AddStockRecord(StockRecord record)
        {
            // 檢查數量
            if (record.Quantity <= 0)
            {
                return BadRequest(new
                {
                    message = "進出貨數量必須大於 0"
                });
            }

            // 檢查異動類型
            if (string.IsNullOrWhiteSpace(record.Type))
            {
                return BadRequest(new
                {
                    message = "請指定進出貨類型"
                });
            }

            record.Type = record.Type.Trim().ToUpperInvariant();

            if (record.Type != "IN" && record.Type != "OUT")
            {
                return BadRequest(new
                {
                    message = "異動類型只能是 IN 或 OUT"
                });
            }

            // 檢查食材是否存在
            var ingredient = _context.Ingredients.Find(record.IngredientId);

            if (ingredient == null)
            {
                return NotFound(new
                {
                    message = "找不到此食材"
                });
            }

            // 出貨前檢查目前庫存
            if (record.Type == "OUT")
            {
                var records = _context.StockRecords
                    .Where(x => x.IngredientId == record.IngredientId)
                    .ToList();

                var totalIn = records
                    .Where(x => x.Type == "IN")
                    .Sum(x => x.Quantity);

                var totalOut = records
                    .Where(x => x.Type == "OUT")
                    .Sum(x => x.Quantity);

                var currentStock = totalIn - totalOut;

                if (record.Quantity > currentStock)
                {
                    return BadRequest(new
                    {
                        message = $"庫存不足，目前庫存為 {currentStock}"
                    });
                }
            }

            // 通過檢核後才建立紀錄
            record.Date = DateTime.Now;

            _context.StockRecords.Add(record);
            _context.SaveChanges();

            return Ok(record);
        }
        //查看所有進出貨記錄
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult GetRecords()
        {
            //LINQ Query Syntax 寫法
            var data = 
                from s in _context.StockRecords
                join i in _context.Ingredients on s.IngredientId equals i.Id

                orderby s.Date descending
                select new
                {
                    s.id,
                    s.IngredientId,
                    IngredientName = i.Name,
                    s.Quantity,
                    s.Type,
                    s.Date
                };
            return Ok(data.ToList());

        }


    }
}
