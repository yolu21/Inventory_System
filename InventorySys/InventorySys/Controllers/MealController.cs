using Microsoft.AspNetCore.Mvc;
using InventorySys.Data;
using InventorySys.Models;
using InventorySys.DTOs;
using System.Threading.Tasks;
namespace InventorySys.Controllers
{

    [ApiController]
    [Route("controller")]
    public class MealController : ControllerBase
    {
        private readonly InventoryDbContext _context;
        public MealController(InventoryDbContext context)
        {

            _context = context;
        }
        [HttpPost]
        public async Task<IActionResult> CreateMeal(MealDto data)
        {
            if (string.IsNullOrWhiteSpace(data.Name))
            {
                return BadRequest(new
                {
                    message = "餐點名稱不可空白"
                });
            }

            var meal = new Meal
            {
                Name = data.Name.Trim(),
            };
            _context.Meals.Add(meal);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "餐點建立成功",
                id = meal.Id,
                name = meal.Name,
            });
        }

        [HttpPost("{mealId}/ingredients")]
        public async Task<IActionResult> AddIngredientToModel(int mealId, MealIngredientDto data)
        {
            var meal = await _context.Meals.FindAsync(mealId);
            if (meal == null)
            {
                return NotFound(new
                {
                    message = "找不到餐點"
                });
            }

            var ingredient = await _context.Ingredients.FindAsync(data.IngredientId);

            if(ingredient == null)
            {
                return NotFound(new
                {
                    message = "找不到食材"
                });
            }

            if(data.Quantity <= 0)
            {
                return BadRequest(new
                {
                    message = "使用量大於 0"
                });
            }
            var mealIngredient = new MealIngredient
            {
                MealId = mealId,
                IngredientId = data.IngredientId,
                Quantity = data.Quantity
            };

            _context.MealIngredients.Add(mealIngredient);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "BOM 食材新增成功"
            });
        }

    }

}
