using Core.DTO;
using Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface ICompanyRepository : IBaseRepository<Company>
    {
        Task<(List<Company_UserCompanyDTO> companies_UserCompanies, bool operationExecuted, string message)> GetCompaniesByIdentificationNumber(string documentNumber);
    }
}
