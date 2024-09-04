using Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface ILastIdTableCompanyRepository : IBaseRepository<LastIdTablesCompany>
    {
        LastIdTablesCompany GetBigger(string tableName);
        Task<LastIdTablesCompany> GetBiggerAsync(string tableName);
    }
}
