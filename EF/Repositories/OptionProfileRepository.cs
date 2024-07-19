using Core.Interfaces;
using Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EF.Repositories
{
    public class OptionProfileRepository : BaseRepository<OptionProfile>, IOptionProfileRepository
    {
        private readonly ApplicationDbContext _context;

        public OptionProfileRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
    }
}
