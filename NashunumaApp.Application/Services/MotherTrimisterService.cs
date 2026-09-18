using AutoMapper;
using NashunumaApp.Application.DTOs.Common;
using NashunumaApp.Application.DTOs.MotherInformation;
using NashunumaApp.Application.Interfaces;
using NashunumaApp.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NashunumaApp.Application.Services
{
    public class MotherTrimisterService : IMotherTrimisterService
    {
        private readonly IMotherTrimisterRepository _repository;
        private readonly IMapper _mapper;

        public MotherTrimisterService(IMotherTrimisterRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<ApiResponse<PaginatedResponse<MotherTrimisterResponseDto>>> GetByBatchNumberAsync(string batchNumber, int pageNumber = 1,int pageSize = 10)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(batchNumber))
                {
                    return ApiResponse<PaginatedResponse<MotherTrimisterResponseDto>>
                        .Failure("Batch number is required");
                }

                pageNumber = Math.Max(1, pageNumber);
                pageSize = Math.Max(1, Math.Min(100, pageSize));

                var result = await _repository.GetByBatchNumberAsync(batchNumber.Trim(), pageNumber, pageSize);

                var mappedItems = _mapper.Map<List<MotherTrimisterResponseDto>>(result.Items);
                var paginatedResponse = new PaginatedResponse<MotherTrimisterResponseDto>
                {
                    Items = mappedItems,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCount = result.TotalCount
                };

                return ApiResponse<PaginatedResponse<MotherTrimisterResponseDto>>
                    .Success(paginatedResponse, "Mother trimister records retrieved successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<PaginatedResponse<MotherTrimisterResponseDto>>
                    .Failure($"Failed to retrieve mother trimister records: {ex.Message}");
            }
        }
    }
}