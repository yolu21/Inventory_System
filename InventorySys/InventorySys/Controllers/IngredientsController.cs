using Microsoft.AspNetCore.Mvc;
using InventorySys.Models;
using InventorySys.Data;
namespace InventorySys.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class IngredientsController : ControllerBase
    {
        private readonly InventoryDbContext _context;
        public IngredientsController(InventoryDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public IActionResult GetIngredients()
        {
            return Ok(_context.Ingredients.ToList());
        }

        [HttpPost]
        public IActionResult AddIngredient(Ingredients ingredient)
        {
            _context.Ingredients.Add(ingredient);
            _context.SaveChanges();
            return Ok(ingredient);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteIngredient(int id)
        {
            var ingredient = _context.Ingredients.Find(id);
            if (ingredient == null)
                return NotFound();
            _context.Ingredients.Remove(ingredient);
            _context.SaveChanges();
            return Ok();
        }

    }
}
