using Microsoft.AspNetCore.Mvc;
using InventorySys.Models;
using InventorySys.Data;

namespace InventorySys.Controllers
{
    [ApiController]
    [Route("[controller]")]
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
        public IActionResult AddStockRecord(StockRecord record)
        {
            record.Date = DateTime.Now;

            _context.StockRecords.Add(record);
            _context.SaveChanges();
            return Ok(record);
        }

        //查看所有進出貨記錄
        [HttpGet]
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
