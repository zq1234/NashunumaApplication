using NashunumaApp.Domain.Common.DTOs;
using NashunumaApp.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NashunumaApp.Domain.Interfaces
{
    public interface IMotherTrimisterRepository : IGenericRepository<NthMotherTrimister>
    {
        Task<(List<MotherTrimisterDto> Items, int TotalCount)> GetByBatchNumberAsync(string batchNumber,int pageNumber, int pageSize);
    }
}