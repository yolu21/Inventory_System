using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventorySys.Models;
using InventorySys.Data;
using InventorySys.DTOs;
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
        public async Task<IActionResult> ImportInventory(ImportRequestDto request)
        {
            if(request == null || !request.Data.Any())
            {
                return BadRequest(new { message = "No data provided for import." });
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                int newIngredientsCount = 0;
                int StockRecordsCount = 0;
                int importTotalCount = request.Data.Count();
                for(int index=0; index< request.Data.Count; index++)
                {
                    var item = request.Data[index];
                    if (string.IsNullOrEmpty(item.Name))
                    {
                        throw new Exception(
                            $"第 {index + 1} 列: 食材名稱不可空白"
                        );
                    }
                    if (item.Stock <= 0)
                    {
                        throw new Exception(
                            $"第 {index + 1} 列 {item.Name}: 庫存必須大於0"
                        );
                    }
                }

                var names = request.Data.Select(x=>x.Name).ToList();

                var ingredients = await _context.Ingredients
                    .Where(i => request.Data
                        .Select(x => x.Name)
                        .Contains(i.Name))
                    .ToListAsync();

                for (int index=0; index< request.Data.Count; index++)
                {
                    var item = request.Data[index];
                    if(!ingredients.Any(i => i.Name == item.Name.Trim() && i.Unit == item.Unit.Trim()))
                    {
                        var newIngredient = new Ingredients
                        {
                            Name = item.Name.Trim(),
                            Unit = item.Unit.Trim()
                        };
                        _context.Ingredients.Add(newIngredient);
                        newIngredientsCount++;
                    }
                }
                await _context.SaveChangesAsync();

                for(int index=0; index< request.Data.Count; index++)
                {
                    var item = request.Data[index];
                    var ingredient = _context.Ingredients.First(i => i.Name == item.Name.Trim() && i.Unit == item.Unit.Trim());

                    var stock = new StockRecord
                    {
                        IngredientId = ingredient.Id,
                        Type = "IN",
                        Quantity = item.Stock,
                        Date = DateTime.Now
                    };

                    _context.StockRecords.Add(stock);
                    StockRecordsCount++;
                }

                await _context.SaveChangesAsync();
                //20260730 ADD Import Excel LOG
                var importLog = new ImportLog
                {
                    FileName = request.FileName,
                    ImportTime = DateTime.Now,
                    TotalCount = request.Data.Count,
                    SuccessCount = StockRecordsCount,
                    IsSuccess = true
                };
                _context.ImportLog.Add(importLog);
                await _context.SaveChangesAsync();

                await  transaction.CommitAsync();

                return Ok(new
                {
                    message = "Inventory imported successfully.",
                    newIngredients = newIngredientsCount,
                    stockRecords = StockRecordsCount
                });
            }
            catch (Exception ex) {
                await transaction.RollbackAsync();

                var failLog = new ImportLog
                {
                    FileName = request.FileName,
                    ImportTime = DateTime.Now,
                    TotalCount = request.Data.Count,
                    SuccessCount = 0,
                    IsSuccess = false,
                    ErrMsg = ex.Message
                };

                _context.ImportLog.Add(failLog);
                await _context.SaveChangesAsync();


                return BadRequest(new
                {
                    message = "匯入失敗",
                    error = ex.Message
                });            
             }
            
        }
    }
}
