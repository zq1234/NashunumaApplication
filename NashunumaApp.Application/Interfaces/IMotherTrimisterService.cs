using Microsoft.AspNetCore.Http;
using NashunumaApp.Application.DTOs.Common;
using NashunumaApp.Application.DTOs.MotherInformation;
using System.Threading.Tasks;

namespace NashunumaApp.Application.Interfaces
{
    public interface IMotherTrimisterService
    {
        Task<ApiResponse<PaginatedResponse<MotherTrimisterResponseDto>>> GetByBatchNumberAsync(string batchNumber,int pageNumber = 1,int pageSize = 10);
        Task<ApiResponse<BatchExcelResultDto>> GetExcelBatchNumbersAsync(IFormFile file, int pageNumber = 1, int pageSize = 10);
        // — inline JSON, no pagination, capped at BatchUploadLimits.InlineJsonMaxBatches
        Task<ApiResponse<BatchExcelResultDto>> GetExcelBatchNumbersInlineAsync( IFormFile file, CancellationToken cancellationToken = default);
        //  — streams a CSV or XLSX file
        Task<(Stream FileStream, string ContentType, string FileName)> GetExcelBatchNumbersExportAsync( IFormFile file, string format,  CancellationToken cancellationToken = default);

    }
}