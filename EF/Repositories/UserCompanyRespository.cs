using Core.Interfaces;
using Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EF.Repositories
{
    public class UserCompanyRespository : BaseRepository<UserCompany>, IUserCompanyRepository
    {
        private readonly ApplicationDbContext _context;

        public UserCompanyRespository(ApplicationDbContext context) : base(context)
        {
        }

        //Métodos especiales van aquí
    }
}
