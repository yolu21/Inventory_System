using InventorySys.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventorySys.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class ForecastController:ControllerBase
    {
        private readonly ForecastService _forecastService;

        public ForecastController(ForecastService forecastService)
        {
            _forecastService = forecastService;
        }
        [HttpGet]
        public async Task<IActionResult> GetForecast(int usageDays = 14, int forecastDays = 7)
        {
            var result = await _forecastService.GetForecast(usageDays, forecastDays);
            return Ok(result);

        }
    }
}
