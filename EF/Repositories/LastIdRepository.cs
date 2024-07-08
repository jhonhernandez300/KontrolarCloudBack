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
    public class LastIdRepository : BaseRepository<LastId>, ILastIdRepository
    {
        private readonly ApplicationDbContext _context;

        public LastIdRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public LastId GetBigger(string tableName)
        {
            return _context.LastIds
                           .Where(x => x.TableName == tableName)
                           .OrderByDescending(x => x.Last)
                           .FirstOrDefault();
        }

        public async Task<LastId> GetBiggerAsync(string tableName)
        {
            return await _context.LastIds
                                 .Where(x => x.TableName == tableName)
                                 .OrderByDescending(x => x.Last)
                                 .FirstOrDefaultAsync();
        }
    }
}
