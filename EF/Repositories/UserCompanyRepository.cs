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
    public class UserCompanyRepository : BaseRepository<UserCompany>, IUserCompanyRepository
    {
        private readonly ApplicationDbContext _context;

        public UserCompanyRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<User>> GetByCompanyId(int idCompany)
        {
            return await _context.UsersCompanies
                                 .Where(uc => uc.IdCompany == idCompany)
                                 .Select(uc => uc.User)
                                 .ToListAsync();
        }

        public async Task<List<Company>> GetByUserId(int idUser)
        {
            return await _context.UsersCompanies
                                 .Where(uc => uc.IdUser == idUser)
                                 .Select(uc => uc.Company)
                                 .ToListAsync();
        }
    }
}
