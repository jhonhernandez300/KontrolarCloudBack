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
    public class ProfileRepository : SecondaryBaseRepository<Profile>, IProfileRepository
    {
        private readonly SecondaryDbContext _context;

        public ProfileRepository(SecondaryDbContext context) : base(context)
        {
            _context = context;
        }

        public Task<List<Profile>> GetProfilesByParam(string param)
        {
            return _context.Profiles
                    .Where(p => p.CodProfile.Contains(param) ||
                                p.NameProfile.Contains(param) ||
                                p.Description.Contains(param)).ToListAsync();
        }
    }
}
