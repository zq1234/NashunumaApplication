using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NashunumaApp.Domain.Common.DTOs;
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
                var result = await _locationService.GetAllProvinces();

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
        /// Get districts by province name
        /// </summary>
        [HttpGet("districts/{provinceName}")]
        public async Task<IActionResult> GetDistrictsByProvince(string provinceName)
        {
            try
            {
                var result = await _locationService.GetDistrictsByProvince(provinceName);

                if (!result.IsSuccess)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting districts for province {provinceName}");
                return StatusCode(500, ApiResponse<List<DistrictDto>>.Failure("An error occurred while retrieving districts"));
            }
        }

        /// <summary>
        /// Get tehsils by district name
        /// </summary>
        [HttpGet("tehsils/{districtName}")]
        public async Task<IActionResult> GetTehsilsByDistrict(string districtName)
        {
            try
            {
                var result = await _locationService.GetTehsilsByDistrict(districtName);

                if (!result.IsSuccess)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting tehsils for district {districtName}");
                return StatusCode(500, ApiResponse<List<TehsilDto>>.Failure("An error occurred while retrieving tehsils"));
            }
        }

        /// <summary>
        /// Get UCs by tehsil name
        /// </summary>
        [HttpGet("ucs/{tehsilName}")]
        public async Task<IActionResult> GetUcsByTehsil(string tehsilName)
        {
            try
            {
                var result = await _locationService.GetUcsByTehsil(tehsilName);

                if (!result.IsSuccess)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting UCs for tehsil {tehsilName}");
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
                var result = await _locationService.GetFullLocationHierarchy();

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
        /// Get province by name
        /// </summary>
        [HttpGet("province/{provinceName}")]
        public async Task<IActionResult> GetProvinceByName(string provinceName)
        {
            try
            {
                var result = await _locationService.GetProvinceByName(provinceName);

                if (!result.IsSuccess)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting province {provinceName}");
                return StatusCode(500, ApiResponse<ProvinceDto>.Failure("An error occurred while retrieving province"));
            }
        }

        /// <summary>
        /// Get district by name
        /// </summary>
        [HttpGet("district/{districtName}")]
        public async Task<IActionResult> GetDistrictByName(string districtName)
        {
            try
            {
                var result = await _locationService.GetDistrictByName(districtName);

                if (!result.IsSuccess)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting district {districtName}");
                return StatusCode(500, ApiResponse<DistrictDto>.Failure("An error occurred while retrieving district"));
            }
        }

        /// <summary>
        /// Get tehsil by name
        /// </summary>
        [HttpGet("tehsil/{tehsilName}")]
        public async Task<IActionResult> GetTehsilByName(string tehsilName)
        {
            try
            {
                var result = await _locationService.GetTehsilByName(tehsilName);

                if (!result.IsSuccess)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting tehsil {tehsilName}");
                return StatusCode(500, ApiResponse<TehsilDto>.Failure("An error occurred while retrieving tehsil"));
            }
        }
    }
}