using NashunumaApp.Domain.Common.DTOs;
using NashunumaApp.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NashunumaApp.Domain.Interfaces
{
    public interface IMotherTrimisterRepository : IGenericRepository<NthMotherTrimister>
    {
        Task<(List<MotherTrimisterDto> Items, int TotalCount)> GetByBatchNumberAsync(string batchNumber,int pageNumber, int pageSize);
        Task<(List<MotherTrimisterDto> Items, int TotalCount, List<string> NotFoundBatches)>GetExcelBatchNumbersAsync( List<string> batchNumbers, int pageNumber, int pageSize);
        IAsyncEnumerable<MotherTrimisterDto> StreamExcelBatchNumbersAsync( List<string> batchNumbers, CancellationToken cancellationToken = default);

        //  batch existence check (for "not found" reporting)
        Task<List<string>> GetExistingBatchNumbersAsync(  List<string> batchNumbers,CancellationToken cancellationToken = default);

    }
}