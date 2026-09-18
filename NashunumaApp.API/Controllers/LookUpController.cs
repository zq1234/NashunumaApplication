using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NashunumaApp.Application.Interfaces;
using System.Threading.Tasks;

namespace NashunumaApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize]
    public class LookUpController : ControllerBase
    {
        private readonly IMotherTrimisterService _motherTrimisterService;

        public LookUpController(IMotherTrimisterService motherTrimisterService)
        {
            _motherTrimisterService = motherTrimisterService;
        }
        [HttpGet("by-batch")]
        public async Task<IActionResult> GetByBatchNumber([FromQuery] string batchNumber, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (string.IsNullOrWhiteSpace(batchNumber))
            {
                return BadRequest(new { isSuccess = false, message = "Batch number is required" });
            }
            var result = await _motherTrimisterService.GetByBatchNumberAsync(batchNumber, pageNumber, pageSize);
            return StatusCode(result.StatusCode, result);
        }
    }
}