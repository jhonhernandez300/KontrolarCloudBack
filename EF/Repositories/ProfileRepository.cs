using Core.Interfaces;
using Core.Models;
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
    }
}
