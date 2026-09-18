using Microsoft.EntityFrameworkCore;
using NashunumaApp.Domain.Common.DTOs;
using NashunumaApp.Domain.Entities;
using NashunumaApp.Domain.Interfaces;
using NashunumaApp.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NashunumaApp.Infrastructure.Repositories
{
    public class MotherTrimisterRepository
        : GenericRepository<NthMotherTrimister>, IMotherTrimisterRepository
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
    }
}