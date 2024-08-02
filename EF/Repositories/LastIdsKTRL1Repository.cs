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
    public class LastIdsKTRL1Repository : BaseRepository<LastIdsKTRL1>, ILastIdsKTRL1Repository
    {
        private readonly ApplicationDbContext _context;

        public LastIdsKTRL1Repository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public LastIdsKTRL1 GetBigger(string tableName)
        {
            return _context.LastIdsKTRL1
                           .Where(x => x.TableName == tableName)
                           .OrderByDescending(x => x.Last)
                           .FirstOrDefault();
        }

        public async Task<LastIdsKTRL1> GetBiggerAsync(string tableName)
        {
            return await _context.LastIdsKTRL1
                                 .Where(x => x.TableName == tableName)
                                 .OrderByDescending(x => x.Last)
                                 .FirstOrDefaultAsync();
        }

        public async Task<bool> DecrementLastAsync(int idLastIds)
        {
            var lastId = await _context.LastIdsKTRL1.FindAsync(idLastIds);

            if (lastId == null)
            {
                return false;
            }

            lastId.Last--;

            _context.LastIdsKTRL1.Update(lastId);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
