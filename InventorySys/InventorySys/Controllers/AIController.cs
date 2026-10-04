using InventorySys.DTOs;
using InventorySys.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventorySys.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class AIController : ControllerBase
    {
        private readonly InventoryAgentService _agentService;

        public AIController(InventoryAgentService agentService)
        {
            _agentService = agentService;
        }

        [HttpPost("Chat")]
        public async Task<IActionResult> Chat([FromBody] AIChatRequestDto request)
        {
            if(string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest("Message 不可為空。");
            }
            var response = await _agentService.Chat(request.Message);
            return Ok(response);
        }
    }
}
