using Core.Interfaces;
using Core.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EF.Repositories
{
    public class LastIdTableCompanyRepository : SecondaryBaseRepository<LastIdTablesCompany>, ILastIdTableCompanyRepository
    {
        private readonly SecondaryDbContext _context;
                
        public LastIdTableCompanyRepository(SecondaryDbContext context) : base(context)
        {
            _context = context;
        }

        public LastIdTablesCompany GetBigger(string tableName)
        {
            return _context.LastIdTablesCompanies
                           .Where(x => x.TableName == tableName)
                           .OrderByDescending(x => x.Last)
                           .FirstOrDefault();
        }

        public async Task<LastIdTablesCompany> GetBiggerAsync(string tableName)
        {
            return await _context.LastIdTablesCompanies
                                 .Where(x => x.TableName == tableName)
                                 .OrderByDescending(x => x.Last)
                                 .FirstOrDefaultAsync();
        }

        public async Task<bool> DecrementLastAsync(int idLastIds)
        {
            var lastId = await _context.LastIdTablesCompanies.FindAsync(idLastIds);

            if (lastId == null)
            {
                return false;
            }

            lastId.Last--;

            _context.LastIdTablesCompanies.Update(lastId);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
