using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NashunumaApp.Application.DTOs.Common;
using NashunumaApp.Application.DTOs.MotherInformation;
using NashunumaApp.Application.Helpers;
using NashunumaApp.Application.Interfaces;
using NashunumaApp.Infrastructure.Helpers;
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
        public async Task<IActionResult> GetByBatchNumber([FromQuery] string batchNumber,[FromQuery] string type, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (string.IsNullOrWhiteSpace(batchNumber))
            {
                return BadRequest(new { isSuccess = false, message = "Batch number is required" });
            }
            if (string.IsNullOrWhiteSpace(type))
            {
                return BadRequest(new { isSuccess = false, message = "Type is required. Allowed values: 'mother' or 'child'" });
            }
            var normalizedType = type.Trim().ToLowerInvariant();
            if (normalizedType != "mother" && normalizedType != "child")
            {
                return BadRequest("Invalid type. Allowed values: 'mother' or 'child'");
            }
            var result = await _motherTrimisterService.GetByBatchNumberAsync(batchNumber, normalizedType, pageNumber, pageSize);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("by-batch-excel")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(10 * 1024 * 1024)]
        public async Task<IActionResult> GetExcelBatchNumbers([FromForm] IFormFile file, [FromQuery] string type, [FromQuery] string format = "csv",CancellationToken cancellationToken = default)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { isSuccess = false, message = "File is required." });

            // Peek at the file's batch count to decide the mode.
            int batchCount;
            try
            {
                using var peek = file.OpenReadStream();
                var parsed = BatchFileParser.Parse(peek, file.FileName);
                batchCount = parsed?.Count ?? 0;
            }
            catch (Exception ex)
            {
                return BadRequest(new { isSuccess = false, message = $"Could not read file: {ex.Message}" });
            }

            if (batchCount == 0)
                return BadRequest(new { isSuccess = false, message = "No batch numbers found in file." });

            // -------- Small file: JSON ----------
            if (batchCount <= BatchUploadLimits.InlineJsonMaxBatches)
            {
                var jsonResult = await _motherTrimisterService.GetExcelBatchNumbersInlineAsync(file,type, cancellationToken);
                return StatusCode(jsonResult.StatusCode, jsonResult);
            }

            // -------- Large file: streaming download ----------
            try
            {
                var (stream, contentType, fileName) = await _motherTrimisterService.GetExcelBatchNumbersExportAsync(file, type, format, cancellationToken);

                // File() will dispose the stream after sending
                return File(stream, contentType, fileName);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { isSuccess = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { isSuccess = false, message = $"Export failed: {ex.Message}" });
            }
        }
    }
}