using Microsoft.EntityFrameworkCore;
using NashunumaApp.Domain.Common.DTOs;
using NashunumaApp.Domain.Entities;
using NashunumaApp.Domain.Interfaces;
using NashunumaApp.Infrastructure.Data;
using NashunumaApp.Infrastructure.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NashunumaApp.Infrastructure.Repositories
{
    public class MotherTrimisterRepository : GenericRepository<NthMotherTrimister>, IMotherTrimisterRepository
    {
        private readonly ApplicationDbContext _context;

        public MotherTrimisterRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        // ------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------
        private static bool IsChild(string type) =>
            string.Equals(type?.Trim(), "child", StringComparison.OrdinalIgnoreCase);

        // Returns the correct DTO projection for the given type
        private IQueryable<MotherTrimisterDto> BuildQuery(IQueryable<string> batches, string type)
        {
            if (IsChild(type))
            {
                return from cd in _context.NthChildVisitDetails.AsNoTracking()
                       join mi in _context.NthMotherInformations.AsNoTracking()
                           on cd.Mothercnic equals mi.Womencnic
                       join st in _context.NthUsers.AsNoTracking()
                           on cd.Updatedby equals st.Username
                       where batches.Contains(cd.Batchnumber)
                       select new MotherTrimisterDto
                       {
                           MotherCnic = cd.Mothercnic,
                           VisitDate = cd.Visitdate,
                           TrimisterNo = cd.Quartercode,
                           SiteName = cd.SiteName,
                           UpdatedBy = cd.Updatedby,
                           PhoneNo = mi.Phoneno,
                           Address = mi.Address,
                           Province = mi.Province,
                           District = mi.District,
                           Tehsil = mi.Tehsil,
                           Email = st.Email,
                           EnteredByCNIC = cd.Enteredby,
                           EnteredByName = st.Personname,
                           BatchNumber = cd.Batchnumber
                       };
            }

            return from mtm in _context.NthMotherTrimisters.AsNoTracking()
                   join mi in _context.NthMotherInformations.AsNoTracking()
                       on mtm.Mothercnic equals mi.Womencnic
                   join st in _context.NthUsers.AsNoTracking()
                       on mtm.Updatedby equals st.Username
                   where batches.Contains(mtm.Batchnumber)
                   select new MotherTrimisterDto
                   {
                       MotherCnic = mtm.Mothercnic,
                       VisitDate = mtm.Visitdate,
                       TrimisterNo = mtm.Trimisterno,
                       SiteName = mtm.SiteName,
                       UpdatedBy = mtm.Updatedby,
                       PhoneNo = mi.Phoneno,
                       Address = mi.Address,
                       Province = mi.Province,
                       District = mi.District,
                       Tehsil = mi.Tehsil,
                       Email = st.Email,
                       EnteredByCNIC = mtm.Enteredby,
                       EnteredByName = st.Personname,
                       BatchNumber = mtm.Batchnumber
                   };
        }

        // Returns the batch-number column from the correct table
        private IQueryable<string> BatchSource(string type) =>
            IsChild(type)
                ? _context.NthChildVisitDetails.AsNoTracking().Select(c => c.Batchnumber)
                : _context.NthMotherTrimisters.AsNoTracking().Select(m => m.Batchnumber);

        private static List<string> Clean(List<string> batches) =>
            (batches ?? new List<string>())
                .Where(b => !string.IsNullOrWhiteSpace(b))
                .Select(b => b.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

        // GetByBatchNumberAsync
        public async Task<(List<MotherTrimisterDto> Items, int TotalCount)> GetByBatchNumberAsync(
            string batchNumber, string type, int pageNumber, int pageSize)
        {
            try
            {
                var query = BuildQuery(new[] { batchNumber }.AsQueryable(), type);

                var totalCount = await query.CountAsync();

                var items = await query
                    .OrderBy(x => x.MotherCnic)
                    .ThenBy(x => x.VisitDate)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return (items, totalCount);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving mother trimister by batch number: {ex.Message}", ex);
            }
        }

        // GetExcelBatchNumbersAsync
        public async Task<(List<MotherTrimisterDto> Items, int TotalCount, List<string> NotFoundBatches)> GetExcelBatchNumbersAsync(
            List<string> batchNumbers, string type, int pageNumber, int pageSize)
        {
            try
            {
                var normalized = Clean(batchNumbers);
                if (normalized.Count == 0)
                    return (new List<MotherTrimisterDto>(), 0, new List<string>());

                // Determine which requested batches actually exist (correct table for the type)
                var existingBatches = await BatchSource(type)
                    .Where(b => normalized.Contains(b))
                    .Distinct()
                    .ToListAsync();

                var existingSet = new HashSet<string>(existingBatches, StringComparer.OrdinalIgnoreCase);
                var notFound = normalized.Where(b => !existingSet.Contains(b)).ToList();

                var query = BuildQuery(normalized.AsQueryable(), type);

                var totalCount = await query.CountAsync();

                var items = await query
                    .OrderBy(x => x.MotherCnic)
                    .ThenBy(x => x.VisitDate)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return (items, totalCount, notFound);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving mother trimister by excel batch numbers: {ex.Message}", ex);
            }
        }

        // GetExistingBatchNumbersAsync
        public async Task<List<string>> GetExistingBatchNumbersAsync(
            List<string> batchNumbers, string type, CancellationToken cancellationToken = default)
        {
            var normalized = Clean(batchNumbers);
            if (normalized.Count == 0) return new List<string>();

            var source = BatchSource(type);
            var existing = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var chunk in normalized.Chunk(BatchUploadLimits.SqlInChunkSize))
            {
                var chunkList = chunk.ToList();
                var hits = await source
                    .Where(b => chunkList.Contains(b))
                    .Distinct()
                    .ToListAsync(cancellationToken);

                foreach (var h in hits) existing.Add(h);
            }

            return existing.ToList();
        }

        // StreamExcelBatchNumbersAsync
        public async IAsyncEnumerable<MotherTrimisterDto> StreamExcelBatchNumbersAsync(
            List<string> batchNumbers, string type,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var normalized = Clean(batchNumbers);
            if (normalized.Count == 0) yield break;

            foreach (var chunk in normalized.Chunk(BatchUploadLimits.SqlInChunkSize))
            {
                var query = BuildQuery(chunk.ToList().AsQueryable(), type);

                var rows = await query
                    .OrderBy(x => x.BatchNumber)
                    .ThenBy(x => x.MotherCnic)
                    .ThenBy(x => x.VisitDate)
                    .ToListAsync(cancellationToken);

                foreach (var row in rows)
                    yield return row;
            }
        }
    }
}