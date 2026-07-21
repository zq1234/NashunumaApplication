using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NashunumaApp.Application.DTOs.Common;
using NashunumaApp.Application.Interfaces;
namespace NashunumaApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize]
    public class LocationController : ControllerBase
    {
        private readonly ILocationService _locationService;
        private readonly ILogger<LocationController> _logger;
        public LocationController(ILocationService locationService, ILogger<LocationController> logger)
        {
            _locationService = locationService;
            _logger = logger;
        }

        /// <summary>
        /// Get all provinces
        /// </summary>
        [HttpGet("provinces")]
        public async Task<IActionResult> GetAllProvinces()
        {
            try
            {
                var result = await _locationService.GetAllProvincesAsync();

                if (!result.IsSuccess)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all provinces");
                return StatusCode(500, ApiResponse<List<ProvinceDto>>.Failure("An error occurred while retrieving provinces"));
            }
        }

        /// <summary>
        /// Get districts by province code
        /// </summary>
        [HttpGet("districts/{provinceCode}")]
        public async Task<IActionResult> GetDistrictsByProvince(decimal provinceCode)
        {
            try
            {
                var result = await _locationService.GetDistrictsByProvinceAsync(provinceCode);

                if (!result.IsSuccess)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting districts for province {provinceCode}");
                return StatusCode(500, ApiResponse<List<DistrictDto>>.Failure("An error occurred while retrieving districts"));
            }
        }

        /// <summary>
        /// Get tehsils by district code
        /// </summary>
        [HttpGet("tehsils/{districtCode}")]
        public async Task<IActionResult> GetTehsilsByDistrict(decimal districtCode)
        {
            try
            {
                var result = await _locationService.GetTehsilsByDistrictAsync(districtCode);

                if (!result.IsSuccess)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting tehsils for district {districtCode}");
                return StatusCode(500, ApiResponse<List<TehsilDto>>.Failure("An error occurred while retrieving tehsils"));
            }
        }

        /// <summary>
        /// Get UCs by tehsil code
        /// </summary>
        [HttpGet("ucs/{tehsilCode}")]
        public async Task<IActionResult> GetUcsByTehsil(decimal tehsilCode)
        {
            try
            {
                var result = await _locationService.GetUcsByTehsilAsync(tehsilCode);

                if (!result.IsSuccess)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting UCs for tehsil {tehsilCode}");
                return StatusCode(500, ApiResponse<List<UcDto>>.Failure("An error occurred while retrieving UCs"));
            }
        }

        /// <summary>
        /// Get full location hierarchy (Provinces -> Districts -> Tehsils -> UCs)
        /// </summary>
        [HttpGet("hierarchy")]
        public async Task<IActionResult> GetFullLocationHierarchy()
        {
            try
            {
                var result = await _locationService.GetFullLocationHierarchyAsync();

                if (!result.IsSuccess)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting full location hierarchy");
                return StatusCode(500, ApiResponse<LocationHierarchyDto>.Failure("An error occurred while retrieving location hierarchy"));
            }
        }

        /// <summary>
        /// Get province by code
        /// </summary>
        [HttpGet("province/{provinceCode}")]
        public async Task<IActionResult> GetProvinceByCode(decimal provinceCode)
        {
            try
            {
                var result = await _locationService.GetProvinceByCodeAsync(provinceCode);

                if (!result.IsSuccess)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting province {provinceCode}");
                return StatusCode(500, ApiResponse<ProvinceDto>.Failure("An error occurred while retrieving province"));
            }
        }

        /// <summary>
        /// Get district by code
        /// </summary>
        [HttpGet("district/{districtCode}")]
        public async Task<IActionResult> GetDistrictByCode(decimal districtCode)
        {
            try
            {
                var result = await _locationService.GetDistrictByCodeAsync(districtCode);

                if (!result.IsSuccess)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting district {districtCode}");
                return StatusCode(500, ApiResponse<DistrictDto>.Failure("An error occurred while retrieving district"));
            }
        }

        /// <summary>
        /// Get tehsil by code
        /// </summary>
        [HttpGet("tehsil/{tehsilCode}")]
        public async Task<IActionResult> GetTehsilByCode(decimal tehsilCode)
        {
            try
            {
                var result = await _locationService.GetTehsilByCodeAsync(tehsilCode);

                if (!result.IsSuccess)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting tehsil {tehsilCode}");
                return StatusCode(500, ApiResponse<TehsilDto>.Failure("An error occurred while retrieving tehsil"));
            }
        }
    }
}