using Microsoft.EntityFrameworkCore;
using NashunumaApp.Domain.Common.DTOs;
using NashunumaApp.Domain.Entities;
using NashunumaApp.Domain.Interfaces;
using NashunumaApp.Infrastructure.Data;
using NashunumaApp.Infrastructure.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public async Task<(List<MotherTrimisterDto> Items, int TotalCount)> GetByBatchNumberAsync(string batchNumber, int pageNumber, int pageSize)
        {
            try
            {
                var childMothers = _context.NthChildVisitDetails
                    .AsNoTracking()
                    .Where(cd => cd.Batchnumber == batchNumber)
                    .Select(cd => cd.Mothercnic)
                    .Distinct();

                var query =
                    from mtm in _context.NthMotherTrimisters.AsNoTracking()
                    join mi in _context.NthMotherInformations.AsNoTracking()
                        on mtm.Mothercnic equals mi.Womencnic
                    join st in _context.NthUsers.AsNoTracking()
                        on mtm.Updatedby equals st.Username
                    where mtm.Batchnumber == batchNumber
                      // || childMothers.Contains(mtm.Mothercnic)
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
                        Email = st.Email
                    };

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
        public async Task<(List<MotherTrimisterDto> Items, int TotalCount, List<string> NotFoundBatches)>GetExcelBatchNumbersAsync(List<string> batchNumbers, int pageNumber, int pageSize)
        {
            try
            {
                var normalized = (batchNumbers ?? new List<string>())
                    .Where(b => !string.IsNullOrWhiteSpace(b))
                    .Select(b => b.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (normalized.Count == 0)
                    return (new List<MotherTrimisterDto>(), 0, new List<string>());

                // Determine which requested batches actually exist
                var existingBatches = await _context.NthMotherTrimisters
                    .AsNoTracking()
                    .Where(mtm => normalized.Contains(mtm.Batchnumber))
                    .Select(mtm => mtm.Batchnumber)
                    .Distinct()
                    .ToListAsync();

                var existingSet = new HashSet<string>(existingBatches, StringComparer.OrdinalIgnoreCase);
                var notFound = normalized.Where(b => !existingSet.Contains(b)).ToList();

                var query =
                    from mtm in _context.NthMotherTrimisters.AsNoTracking()
                    join mi in _context.NthMotherInformations.AsNoTracking()
                        on mtm.Mothercnic equals mi.Womencnic
                    join st in _context.NthUsers.AsNoTracking()
                        on mtm.Updatedby equals st.Username
                    where normalized.Contains(mtm.Batchnumber)
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
                        Email = st.Email
                    };

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
        public async Task<List<string>> GetExistingBatchNumbersAsync(List<string> batchNumbers,CancellationToken cancellationToken = default)
        {
            var normalized = (batchNumbers ?? new List<string>())
                .Where(b => !string.IsNullOrWhiteSpace(b))
                .Select(b => b.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (normalized.Count == 0) return new List<string>();

            var existing = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var chunk in normalized.Chunk(BatchUploadLimits.SqlInChunkSize))
            {
                var chunkList = chunk.ToList();
                var hits = await _context.NthMotherTrimisters
                    .AsNoTracking()
                    .Where(mtm => chunkList.Contains(mtm.Batchnumber))
                    .Select(mtm => mtm.Batchnumber)
                    .Distinct()
                    .ToListAsync(cancellationToken);

                foreach (var h in hits) existing.Add(h);
            }

            return existing.ToList();
        }

        public async IAsyncEnumerable<MotherTrimisterDto> StreamExcelBatchNumbersAsync(
            List<string> batchNumbers,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var normalized = (batchNumbers ?? new List<string>())
                .Where(b => !string.IsNullOrWhiteSpace(b))
                .Select(b => b.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (normalized.Count == 0) yield break;

            foreach (var chunk in normalized.Chunk(BatchUploadLimits.SqlInChunkSize))
            {
                var chunkList = chunk.ToList();

                var rows = await (
                    from mtm in _context.NthMotherTrimisters.AsNoTracking()
                    join mi in _context.NthMotherInformations.AsNoTracking()
                        on mtm.Mothercnic equals mi.Womencnic
                    join st in _context.NthUsers.AsNoTracking()
                        on mtm.Updatedby equals st.Username
                    where chunkList.Contains(mtm.Batchnumber)
                    orderby mtm.Batchnumber, mtm.Mothercnic, mtm.Visitdate
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
                        Email = st.Email
                    }
                ).ToListAsync(cancellationToken);

                foreach (var row in rows)
                    yield return row;
            }
        }
    }
}