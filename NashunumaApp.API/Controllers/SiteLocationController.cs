// API/Controllers/SiteLocationController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NashunumaApp.Application.Interfaces;
using NashunumaApp.Application.DTOs.SiteLocation;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace NashunumaApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize]
    public class SiteLocationController : ControllerBase
    {
        private readonly ISiteLocationService _service;
        public SiteLocationController(ISiteLocationService service)
        {
            _service = service;
        }
        private string CurrentUser =>  User?.FindFirstValue(ClaimTypes.Name) ?? User?.Identity?.Name ?? "system";

        // ---------------- Lookups ----------------

        [HttpGet("provinces")]
        public async Task<IActionResult> GetProvinces(CancellationToken ct)
        {
            var result = await _service.GetProvincesAsync(ct);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("districts")]
        public async Task<IActionResult> GetDistricts([FromQuery] decimal provinceId, CancellationToken ct)
        {
            if (provinceId <= 0)
                return BadRequest(new { isSuccess = false, message = "provinceId is required" });

            var result = await _service.GetDistrictsAsync(provinceId, ct);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("tehsils")]
        public async Task<IActionResult> GetTehsils([FromQuery] decimal districtId, CancellationToken ct)
        {
            if (districtId <= 0)
                return BadRequest(new { isSuccess = false, message = "districtId is required" });

            var result = await _service.GetTehsilsAsync(districtId, ct);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("ucs")]
        public async Task<IActionResult> GetUcs([FromQuery] decimal tehsilId, CancellationToken ct)
        {
            if (tehsilId <= 0)
                return BadRequest(new { isSuccess = false, message = "tehsilId is required" });

            var result = await _service.GetUcsAsync(tehsilId, ct);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("sites")]
        public async Task<IActionResult> GetSites([FromQuery] decimal? provinceId = null, [FromQuery] decimal? districtId = null,[FromQuery] decimal? tehsilId = null, CancellationToken ct = default)
        {
            var result = await _service.GetSitesByFiltersAsync(provinceId, districtId, tehsilId, ct);
            return StatusCode(result.StatusCode, result);
        }

        // ---------------- CRUD ----------------

        [HttpGet]
        public async Task<IActionResult> GetAll( [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null,CancellationToken ct = default)
        {
            var result = await _service.GetAllAsync(pageNumber, pageSize, search, ct);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{id:decimal}")]
        public async Task<IActionResult> GetById(decimal id, CancellationToken ct)
        {
            var result = await _service.GetByIdAsync(id, ct);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSiteLocationRequest request, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _service.CreateAsync(request, CurrentUser, ct);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{id:decimal}")]
        public async Task<IActionResult> Update(decimal id, [FromBody] UpdateSiteLocationRequest request, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _service.UpdateAsync(id, request, CurrentUser, ct);
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("{id:decimal}")]
        public async Task<IActionResult> Delete(decimal id, CancellationToken ct)
        {
            var result = await _service.DeleteAsync(id, CurrentUser, ct);
            return StatusCode(result.StatusCode, result);
        }
    }
}