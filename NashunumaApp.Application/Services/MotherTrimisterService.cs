using AutoMapper;
using Microsoft.AspNetCore.Http;
using NashunumaApp.Application.DTOs.Common;
using NashunumaApp.Application.DTOs.MotherInformation;
using NashunumaApp.Application.Helpers;
using NashunumaApp.Application.Interfaces;
using NashunumaApp.Domain.Common.DTOs;
using NashunumaApp.Domain.Interfaces;
using NashunumaApp.Infrastructure.Helpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NashunumaApp.Application.Services
{
    public class MotherTrimisterService : IMotherTrimisterService
    {
        private readonly IMotherTrimisterRepository _repository;
        private readonly IMapper _mapper;
        private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB
        public MotherTrimisterService(IMotherTrimisterRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }
        public async Task<ApiResponse<PaginatedResponse<MotherTrimisterResponseDto>>> GetByBatchNumberAsync(string batchNumber, string type, int pageNumber = 1,int pageSize = 10)
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

                var result = await _repository.GetByBatchNumberAsync(batchNumber.Trim(), type, pageNumber, pageSize);

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
        public async Task<ApiResponse<BatchExcelResultDto>> GetExcelBatchNumbersAsync( IFormFile file, string type, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return ApiResponse<BatchExcelResultDto>.Failure("Please upload a valid file.");

                if (file.Length > MaxFileSizeBytes)
                    return ApiResponse<BatchExcelResultDto>.Failure("File size exceeds the 10 MB limit.");

                if (!BatchFileParser.IsSupported(file.FileName))
                    return ApiResponse<BatchExcelResultDto>
                        .Failure("Unsupported file type. Allowed: .xlsx, .xls, .csv, .txt");

                List<string> batchNumbers;
                using (var stream = file.OpenReadStream())
                {
                    batchNumbers = BatchFileParser.Parse(stream, file.FileName);
                }

                if (batchNumbers == null || batchNumbers.Count == 0)
                    return ApiResponse<BatchExcelResultDto>
                        .Failure("No batch numbers were found in the uploaded file.");

                pageNumber = Math.Max(1, pageNumber);
                pageSize = Math.Max(1, Math.Min(100, pageSize));

                var (items, totalCount, notFound) =
                    await _repository.GetExcelBatchNumbersAsync(batchNumbers, type,pageNumber, pageSize);

                var mappedItems = _mapper.Map<List<MotherTrimisterResponseDto>>(items);

                var resultDto = new BatchExcelResultDto
                {
                    RequestedBatchNumbers = batchNumbers,
                    BatchNumbersNotFound = notFound,
                    Data = new PaginatedResponse<MotherTrimisterResponseDto>
                    {
                        Items = mappedItems,
                        PageNumber = pageNumber,
                        PageSize = pageSize,
                        TotalCount = totalCount
                    }
                };

                var message = notFound.Count > 0
                    ? $"Retrieved records for {batchNumbers.Count - notFound.Count} of {batchNumbers.Count} batch(es). " +
                      $"Not found: {string.Join(", ", notFound)}"
                    : "Mother trimister records retrieved successfully";

                return ApiResponse<BatchExcelResultDto>.Success(resultDto, message);
            }
            catch (NotSupportedException ex)
            {
                return ApiResponse<BatchExcelResultDto>.Failure(ex.Message);
            }
            catch (Exception ex)
            {
                return ApiResponse<BatchExcelResultDto>
                    .Failure($"Failed to retrieve mother trimister records: {ex.Message}");
            }
        }

        // ------------------------------------------------------------
        // — INLINE JSON (no pagination)
        // ------------------------------------------------------------
        public async Task<ApiResponse<BatchExcelResultDto>> GetExcelBatchNumbersInlineAsync( IFormFile file, string type, CancellationToken cancellationToken = default)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return ApiResponse<BatchExcelResultDto>.Failure("Please upload a valid file.");

                if (file.Length > BatchUploadLimits.MaxFileSizeBytes)
                    return ApiResponse<BatchExcelResultDto>.Failure("File size exceeds the 10 MB limit.");

                if (!BatchFileParser.IsSupported(file.FileName))
                    return ApiResponse<BatchExcelResultDto>.Failure( "Unsupported file type. Allowed: .xlsx, .xls, .csv, .txt");

                List<string> batchNumbers;
                using (var s = file.OpenReadStream()) batchNumbers = BatchFileParser.Parse(s, file.FileName);

                if (batchNumbers == null || batchNumbers.Count == 0)
                    return ApiResponse<BatchExcelResultDto>.Failure( "No batch numbers were found in the uploaded file.");

                if (batchNumbers.Count > BatchUploadLimits.MaxBatchesPerFile)
                    return ApiResponse<BatchExcelResultDto>.Failure($"Too many batches ({batchNumbers.Count}). Max allowed per file is {BatchUploadLimits.MaxBatchesPerFile}.");

                if (batchNumbers.Count > BatchUploadLimits.InlineJsonMaxBatches)
                    return ApiResponse<BatchExcelResultDto>.Failure( $"This file has {batchNumbers.Count} batches — too many for inline view. " +
                        $"Use the export endpoint (format=csv or format=xlsx).");

                // --- do not use StreamExcelBatchNumbersAsync paging; just take all rows ---
                var allRows = new List<MotherTrimisterDto>();
                await foreach (var r in _repository.StreamExcelBatchNumbersAsync(batchNumbers,type, cancellationToken))
                    allRows.Add(r);

                var existing = await _repository.GetExistingBatchNumbersAsync(batchNumbers,type, cancellationToken);
                var existingSet = new HashSet<string>(existing, StringComparer.OrdinalIgnoreCase);
                var notFound = batchNumbers
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Where(b => !existingSet.Contains(b))
                    .ToList();

                var mapped = _mapper.Map<List<MotherTrimisterResponseDto>>(allRows);

                var dto = new BatchExcelResultDto
                {
                    RequestedBatchNumbers = batchNumbers,
                    BatchNumbersNotFound = notFound,
                    Data = new PaginatedResponse<MotherTrimisterResponseDto>
                    {
                        Items = mapped,
                        PageNumber = 1,
                        PageSize = mapped.Count,   // full result in one "page"
                        TotalCount = mapped.Count
                    }
                };

                var message = notFound.Count > 0
                    ? $"Retrieved records for {batchNumbers.Count - notFound.Count} of {batchNumbers.Count} batch(es). " +
                      $"Not found: {string.Join(", ", notFound)}" : "Mother trimister records retrieved successfully";

                return ApiResponse<BatchExcelResultDto>.Success(dto, message);
            }
            catch (NotSupportedException ex)
            {
                return ApiResponse<BatchExcelResultDto>.Failure(ex.Message);
            }
            catch (Exception ex)
            {
                return ApiResponse<BatchExcelResultDto>.Failure(
                    $"Failed to retrieve mother trimister records: {ex.Message}");
            }
        }

        // ------------------------------------------------------------
        // — EXPORT (streams csv/xlsx)
        // ------------------------------------------------------------
        public async Task<(Stream FileStream, string ContentType, string FileName)> GetExcelBatchNumbersExportAsync(
            IFormFile file, string type, string format, CancellationToken cancellationToken = default)
        {
            if (file == null || file.Length == 0)
                throw new InvalidOperationException("Please upload a valid file.");

            if (file.Length > BatchUploadLimits.MaxFileSizeBytes)
                throw new InvalidOperationException("File size exceeds the 10 MB limit.");

            if (!BatchFileParser.IsSupported(file.FileName))
                throw new InvalidOperationException(
                    "Unsupported file type. Allowed: .xlsx, .xls, .csv, .txt");

            var normalizedFormat = (format ?? "csv").Trim().ToLowerInvariant();
            if (normalizedFormat != "csv" && normalizedFormat != "xlsx")
                throw new InvalidOperationException("format must be 'csv' or 'xlsx'.");

            List<string> batchNumbers;
            using (var s = file.OpenReadStream())
                batchNumbers = BatchFileParser.Parse(s, file.FileName);

            if (batchNumbers == null || batchNumbers.Count == 0)
                throw new InvalidOperationException("No batch numbers were found in the uploaded file.");

            if (batchNumbers.Count > BatchUploadLimits.MaxBatchesPerFile)
                throw new InvalidOperationException(
                    $"Too many batches ({batchNumbers.Count}). Max allowed per file is {BatchUploadLimits.MaxBatchesPerFile}.");

            // Buffer to a MemoryStream so the controller can return a clean FileStreamResult.
            // For truly huge exports, write to a temp file instead.
            var ms = new MemoryStream();

            var rowStream = _repository.StreamExcelBatchNumbersAsync(batchNumbers,type, cancellationToken);

            if (normalizedFormat == "csv")
                await BatchExportWriter.WriteCsvAsync(ms, rowStream, cancellationToken);
            else
                await BatchExportWriter.WriteXlsxAsync(ms, rowStream, cancellationToken);

            ms.Position = 0;

            var ext = normalizedFormat == "csv" ? "csv" : "xlsx";
            var contentType = normalizedFormat == "csv"
                ? "text/csv"
                : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            var fileName = $"MotherTrimister_BatchExport_{DateTime.UtcNow:yyyyMMdd_HHmmss}.{ext}";

            return (ms, contentType, fileName);
        }
    }
}