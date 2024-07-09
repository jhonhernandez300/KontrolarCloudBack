using Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface IUserCompanyRepository : IBaseRepository<UserCompany>
    {
        Task<List<User>> GetByCompanyId(int idCompany);
        Task<List<Company>> GetByUserId(int idUser);        
    }
}
