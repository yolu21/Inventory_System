using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using InventorySys.Models;
using InventorySys.Data;

namespace InventorySys.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class IngredientsController : ControllerBase
    {
        private readonly InventoryDbContext _context;

        public IngredientsController(InventoryDbContext context)
        {
            _context = context;
        }

        // 取得所有食材
        [HttpGet]
        public IActionResult GetIngredients()
        {
            return Ok(_context.Ingredients.ToList());
        }

        // 新增食材
        [HttpPost]
        public IActionResult AddIngredient(Ingredients ingredient)
        {
            // 檢查名稱
            if (string.IsNullOrWhiteSpace(ingredient.Name))
            {
                return BadRequest(new
                {
                    message = "食材名稱不可為空白"
                });
            }

            ingredient.Name = ingredient.Name.Trim();

            // 檢查單位
            if (string.IsNullOrWhiteSpace(ingredient.Unit))
            {
                return BadRequest(new
                {
                    message = "食材單位不可為空白"
                });
            }

            ingredient.Unit = ingredient.Unit.Trim();

            // 檢查最低庫存與單價
            if (ingredient.MinimumStock < 0 || ingredient.UnitCost < 0)
            {
                return BadRequest(new
                {
                    message = "最低庫存與單價不可小於 0"
                });
            }

            // 檢查是否已有相同名稱的食材
            var exists = _context.Ingredients.Any(x =>
                x.Name.Trim().ToLower() == ingredient.Name.ToLower()
            );

            if (exists)
            {
                return Conflict(new
                {
                    message = $"食材「{ingredient.Name}」已存在"
                });
            }

            _context.Ingredients.Add(ingredient);
            _context.SaveChanges();

            return Ok(ingredient);
        }

        // 刪除食材
        [HttpDelete("{id}")]
        public IActionResult DeleteIngredient(int id)
        {
            var ingredient = _context.Ingredients.Find(id);

            if (ingredient == null)
            {
                return NotFound(new
                {
                    message = "找不到此食材"
                });
            }

            _context.Ingredients.Remove(ingredient);
            _context.SaveChanges();

            return Ok();
        }
    }
}