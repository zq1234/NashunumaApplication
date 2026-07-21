using AutoMapper;
using NashunumaApp.Application.DTOs.Common;
using NashunumaApp.Application.DTOs.FoodStock;
using NashunumaApp.Application.Interfaces;
using NashunumaApp.Domain.Entities;
using NashunumaApp.Domain.Interfaces;


namespace NashunumaApp.Application.Services
{
    public class FoodStockService : IFoodStockService
    {
        private readonly IFoodStockRepository _foodStockRepository;
        private readonly IMapper _mapper;

        public FoodStockService(IFoodStockRepository foodStockRepository, IMapper mapper)
        {
            _foodStockRepository = foodStockRepository;
            _mapper = mapper;
        }

        public async Task<ApiResponse<PaginatedResponse<FoodStockDto>>> GetPagedFoodStocksAsync(
            int pageNumber,
            int pageSize,
            string? searchTerm = null)
        {
            try
            {
                // Validate pagination parameters
                pageNumber = Math.Max(1, pageNumber);
                pageSize = Math.Max(1, Math.Min(100, pageSize));

                // Get paged data with site information
                var result = await _foodStockRepository.GetPagedFoodStocksWithSiteAsync(
                    pageNumber,
                    pageSize,
                    searchTerm
                );

                // Extract items and total count
                var items = result.Items;
                var totalCount = result.TotalCount;

                // Map Domain DTO to Application DTO using AutoMapper
                var mappedItems = _mapper.Map<List<FoodStockDto>>(items);

                // Create paginated response
                var paginatedResponse = new PaginatedResponse<FoodStockDto>
                {
                    Items = mappedItems,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCount = totalCount
                };

                return ApiResponse<PaginatedResponse<FoodStockDto>>.Success(
                    paginatedResponse,
                    "Food stocks with site details retrieved successfully"
                );
            }
            catch (Exception ex)
            {
                return ApiResponse<PaginatedResponse<FoodStockDto>>.Failure(
                    $"Failed to retrieve food stocks: {ex.Message}"
                );
            }
        }

        public async Task<ApiResponse<FoodStockDto>> GetFoodStockByIdAsync(decimal id)
        {
            try
            {
                // Validate ID
                if (id <= 0)
                {
                    return ApiResponse<FoodStockDto>.Failure("Invalid ID provided");
                }

                // Get the food stock by ID
                var foodStock = await _foodStockRepository.GetByIdAsync(id);

                // Check if record exists
                if (foodStock == null)
                {
                    return ApiResponse<FoodStockDto>.Failure($"Food stock with ID {id} not found");
                }

                // Map to DTO
                var mappedItem = _mapper.Map<FoodStockDto>(foodStock);

                return ApiResponse<FoodStockDto>.Success(
                    mappedItem,
                    "Food stock retrieved successfully"
                );
            }
            catch (Exception ex)
            {
                return ApiResponse<FoodStockDto>.Failure(
                    $"Failed to retrieve food stock: {ex.Message}"
                );
            }
        }

        //public async Task<ApiResponse<FoodStockDto>> CreateFoodStockAsync(CreateFoodStockDto dto)
        //{
        //    try
        //    {
        //        if (string.IsNullOrEmpty(dto.SiteId))
        //        {
        //            return ApiResponse<FoodStockDto>.Failure("Site ID is required");
        //        }

        //        var entity = _mapper.Map<NthSnfStock>(dto);
        //        //entity.EnteredOn = DateTime.Now;
        //        //entity.ActivityTime = dto.ActivityTime ?? DateTime.Now;

        //        var created = await _foodStockRepository.AddAsync(entity);
        //        await _foodStockRepository.SaveChangesAsync();

        //        var mappedItem = _mapper.Map<FoodStockDto>(created);
        //        return ApiResponse<FoodStockDto>.Success(
        //            mappedItem,
        //            "Food stock created successfully"
        //        );
        //    }
        //    catch (Exception ex)
        //    {
        //        return ApiResponse<FoodStockDto>.Failure(
        //            $"Failed to create food stock: {ex.Message}"
        //        );
        //    }
        //}

        //public async Task<ApiResponse<FoodStockDto>> UpdateFoodStockAsync(int id, UpdateFoodStockDto dto)
        //{
        //    try
        //    {
        //        if (id <= 0)
        //        {
        //            return ApiResponse<FoodStockDto>.Failure("Invalid ID provided");
        //        }

        //        var existing = await _foodStockRepository.GetByIdAsync(id);
        //        if (existing == null)
        //        {
        //            return ApiResponse<FoodStockDto>.Failure($"Food stock with ID {id} not found");
        //        }

        //        _mapper.Map(dto, existing);
        //       // existing.EnteredOn = DateTime.Now;

        //        await _foodStockRepository.UpdateAsync(existing);
        //        await _foodStockRepository.SaveChangesAsync();

        //        var mappedItem = _mapper.Map<FoodStockDto>(existing);
        //        return ApiResponse<FoodStockDto>.Success(
        //            mappedItem,
        //            "Food stock updated successfully"
        //        );
        //    }
        //    catch (Exception ex)
        //    {
        //        return ApiResponse<FoodStockDto>.Failure(
        //            $"Failed to update food stock: {ex.Message}"
        //        );
        //    }
        //}

        public async Task<ApiResponse<bool>> DeleteFoodStockAsync(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return ApiResponse<bool>.Failure("Invalid ID provided");
                }

                var existing = await _foodStockRepository.GetByIdAsync(id);
                if (existing == null)
                {
                    return ApiResponse<bool>.Failure($"Food stock with ID {id} not found");
                }

                await _foodStockRepository.DeleteAsync(existing);
                await _foodStockRepository.SaveChangesAsync();

                return ApiResponse<bool>.Success(true, "Food stock deleted successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.Failure(
                    $"Failed to delete food stock: {ex.Message}"
                );
            }
        }

        public async Task<ApiResponse<Domain.Common.DTOs.FoodStockSummaryDto>> GetSummaryStatsAsync()
        {
            try
            {
                var summary = await _foodStockRepository.GetSummaryStatsAsync();
                return ApiResponse<Domain.Common.DTOs.FoodStockSummaryDto>.Success(
                    summary,
                    "Summary retrieved successfully"
                );
            }
            catch (Exception ex)
            {
                return ApiResponse<Domain.Common.DTOs.FoodStockSummaryDto>.Failure(
                    $"Failed to retrieve summary: {ex.Message}"
                );
            }
        }

        public async Task<ApiResponse<byte[]>> ExportFoodStocksAsync(string? searchTerm, string format)
        {
            try
            {
                var data = await _foodStockRepository.GetExportDataAsync(searchTerm);
                var bytes = await _foodStockRepository.GenerateExportFileAsync(data, format);

                return ApiResponse<byte[]>.Success(
                    bytes,
                    "Export generated successfully"
                );
            }
            catch (Exception ex)
            {
                return ApiResponse<byte[]>.Failure(
                    $"Failed to export: {ex.Message}"
                );
            }
        }
    }
}
