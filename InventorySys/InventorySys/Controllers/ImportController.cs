using Microsoft.AspNetCore.Mvc;
using InventorySys.Models;
using InventorySys.Data;
namespace InventorySys.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ImportController : ControllerBase
    {
        private readonly InventoryDbContext _context;

        public ImportController(InventoryDbContext context) { 
            _context = context;
        }

        [HttpPost]
        public IActionResult ImportInventory(List<ImportInventoryDto> data)
        {
            foreach (var item in data)
            {
                var ingredient = _context.Ingredients.FirstOrDefault(i => i.Name == item.Name);

                if (ingredient == null) {
                    ingredient = new Ingredients
                    {
                        Name = item.Name,
                        Unit = item.Unit
                    };

                    _context.Ingredients.Add(ingredient);

                    
                }
            }
            _context.SaveChanges();
            foreach(var item in data)
            {
                var ingredient = _context.Ingredients.FirstOrDefault(i => i.Name == item.Name);
                var stock = new StockRecord
                {
                    IngredientId = ingredient.Id,
                    Type = "IN",
                    Quantity = item.Stock,
                    Date = DateTime.Now
                };
                
                _context.StockRecords.Add(stock);
            }
            
            _context.SaveChanges();
            return Ok(new { message = "Inventory imported successfully." });
        }
    }
}
