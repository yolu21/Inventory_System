using Microsoft.AspNetCore.Mvc;
using InventorySys.Data;
using InventorySys.Models;
using InventorySys.DTOs;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
namespace InventorySys.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class MealController : ControllerBase
    {
        private readonly InventoryDbContext _context;
        public MealController(InventoryDbContext context)
        {

            _context = context;
        }
        //build Meal
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
        //Build Meal BOM
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

        //Get Meal
        [HttpGet]
        public async Task<IActionResult> GetMeals() {
            var meals = await _context.Meals.Select(m => new
            {
                m.Id,
                m.Name
            }).ToListAsync();

            return Ok(meals);
        }
        //Get Meal BOM
        [HttpGet("{id}")]
        public async Task<IActionResult> GetMeal(int id)
        {
            var meal = await _context.Meals
                .Where(m => m.Id == id)
                .Select(m => new
                {
                    m.Id,
                    m.Name,
                    Ingredients = _context.MealIngredients
                         .Where(mi => mi.MealId == m.Id)
                        .Select(mi => new
                        {
                            IngredientId = mi.IngredientId,
                            Name = _context.Ingredients
                                .Where(i => i.Id == mi.IngredientId)
                                .Select(i => i.Name)
                                .FirstOrDefault(),
                           // 從資料庫取得第一筆資料，找不到就回傳 null。
                            Unit = _context.Ingredients
                            .Where(i => i.Id == mi.IngredientId)
                            .Select(i => i.Unit)
                            .FirstOrDefault(),

                            Quantity = mi.Quantity

                        }).ToList()
                }).FirstOrDefaultAsync();//非同步地從資料庫取得第一筆資料，找不到就回傳 null。

            if (meal == null)
            {
                return NotFound(new
                {
                    message = "找不到餐點。"
                });
            }

            return Ok(meal);
        }

    }


}
