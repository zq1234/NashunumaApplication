using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NashunumaApp.Application.Interfaces;

namespace NashunumaApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FoodStockController : ControllerBase
    {
        private readonly IFoodStockService _foodStockService;

        public FoodStockController(IFoodStockService foodStockService)
        {
            _foodStockService = foodStockService;
        }

        [HttpGet]
        public async Task<IActionResult> GetFoodStocks([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10,[FromQuery] string? searchTerm = null)
        {
            
            var result = await _foodStockService.GetPagedFoodStocksAsync(pageNumber, pageSize, searchTerm);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetFoodStockById(decimal id)
        {
            var result = await _foodStockService.GetFoodStockByIdAsync(id);

            if (!result.IsSuccess)
            {
                if (result.Message.Contains("not found"))
                {
                    return NotFound(result);
                }
                if (result.Message.Contains("Invalid ID"))
                {
                    return BadRequest(result);
                }
            }

            return Ok(result);
        }

        // Add summary endpoint
        [HttpGet("summary")]
        public async Task<IActionResult> GetSummaryStats()
        {
            var result = await _foodStockService.GetSummaryStatsAsync();
            return StatusCode(result.StatusCode, result);
        }

         
        [HttpGet("export")]
        public async Task<IActionResult> ExportFoodStocks(
            [FromQuery] string? searchTerm = null,
            [FromQuery] string format = "excel")
        {
            var result = await _foodStockService.ExportFoodStocksAsync(searchTerm, format);
            return File(result.Data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"FoodStock_{DateTime.Now:yyyy-MM-dd}.xlsx");
        }

      
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFoodStock(int id)
        {
            var result = await _foodStockService.DeleteFoodStockAsync(id);

            if (!result.IsSuccess)
            {
                if (result.Message.Contains("not found"))
                {
                    return NotFound(result);
                }
                return BadRequest(result);
            }

            return Ok(result);
        }
    }

}