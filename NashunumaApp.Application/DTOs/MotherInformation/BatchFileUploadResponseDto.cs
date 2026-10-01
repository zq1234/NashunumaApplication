using NashunumaApp.Application.DTOs.Common;

namespace NashunumaApp.Application.DTOs.MotherInformation
{
    public class BatchExcelResultDto
    {
        public List<string> RequestedBatchNumbers { get; set; } = new();
        public List<string> BatchNumbersNotFound { get; set; } = new();
        public PaginatedResponse<MotherTrimisterResponseDto> Data { get; set; }
    }
}