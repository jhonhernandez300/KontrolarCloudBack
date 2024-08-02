using Core.Interfaces;
using Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public interface IUnitOfWork : IDisposable
    {               
        IUserRepository Users { get; }
        ICompanyRepository Companies { get; }
        ILastIdsKTRL1Repository LastIdsKTRL1 { get; }
        IModuleRepository Modules { get; }
        IOptionRepository Options { get; }
        IOptionProfileRepository OptionsProfiles { get; }
        IProfileRepository Profiles { get; }
        IUserCompanyRepository UsersCompanies { get; }
        IUserProfileRepository UsersProfiles { get; }

        int Complete();
    }
}
