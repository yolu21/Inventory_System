using InventorySys.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace InventorySys.Controllers
{
    [Authorize]
    [ApiController]
    [Route("Tools/Inventory")]
    public class ToolController : ControllerBase
    {
        private readonly InventoryToolService _toolService;

        public ToolController(InventoryToolService toolService)
        {
            _toolService = toolService;
        }
        // Tool1: GET: Tools/Inventory/LowStock
        [HttpGet("LowStock")]
        public async Task<IActionResult> GetLowStockItems()
        {
            var result = await _toolService.GetLowStockItems();
            return Ok(result);
        }
        // Tool2: GET: Tools/Inventory/Forecast?usageDays=14&forecastDays=7
        [HttpGet("Forecast")]
        public async Task<IActionResult> GetInventoryForecast(int usageDays = 14, int forecastDays = 7)
        {
            var result = await _toolService.GetInventoryForecast(usageDays, forecastDays);
            return Ok(result);
        }
        // Tool3: GET: Tools/Inventory/Summary
        [HttpGet("Summary")]
        public async Task<IActionResult> GetInventorySummary()
        {
            var result = await _toolService.GetInventorySummary();
            return Ok(result);
        }
    }
}
