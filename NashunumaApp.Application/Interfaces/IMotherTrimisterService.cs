using NashunumaApp.Application.DTOs.Common;
using NashunumaApp.Application.DTOs.MotherInformation;
using System.Threading.Tasks;

namespace NashunumaApp.Application.Interfaces
{
    public interface IMotherTrimisterService
    {
        Task<ApiResponse<PaginatedResponse<MotherTrimisterResponseDto>>> GetByBatchNumberAsync(string batchNumber,int pageNumber = 1,int pageSize = 10);
    }
}