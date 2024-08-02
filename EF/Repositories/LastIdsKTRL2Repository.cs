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
    public class LastIdsKTRL2Repository : SecondaryBaseRepository<LastIdsKTRL2>, ILastIdsKTRL2Repository
    {
        private readonly SecondaryDbContext _context;
                
        public LastIdsKTRL2Repository(SecondaryDbContext context) : base(context)
        {
            _context = context;
        }

        public LastIdsKTRL2 GetBigger(string tableName)
        {
            return _context.LastIdsKTRL2
                           .Where(x => x.TableName == tableName)
                           .OrderByDescending(x => x.Last)
                           .FirstOrDefault();
        }

        public async Task<LastIdsKTRL2> GetBiggerAsync(string tableName)
        {
            return await _context.LastIdsKTRL2
                                 .Where(x => x.TableName == tableName)
                                 .OrderByDescending(x => x.Last)
                                 .FirstOrDefaultAsync();
        }

        public async Task<bool> DecrementLastAsync(int idLastIds)
        {
            var lastId = await _context.LastIdsKTRL2.FindAsync(idLastIds);

            if (lastId == null)
            {
                return false;
            }

            lastId.Last--;

            _context.LastIdsKTRL2.Update(lastId);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
