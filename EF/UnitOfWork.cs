using Core;
using Core.Interfaces;
using Core.Models;
using EF.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EF
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;

        public UnitOfWork(ApplicationDbContext context,
                          //IBaseRepository<Company> companies,
                          IBaseRepository<User> users,
                          ICompanyRepository companies,
                          IModuleRepository modules,
                          IOptionProfileRepository optionsProfiles,
                          IOptionRepository options,
                          IProfileRepository profiles,
                          ILastIdRepository lastIds,
                          IUserCompanyRepository usersCompanies,
                          IUserProfileRepository usersProfiles)
                          
        {
            _context = context;
            Companies = companies;
            LastIds = lastIds;
            Modules = modules;
            Options = options;
            OptionsProfiles = optionsProfiles;
            Profiles = profiles;
            Users = users;
            UsersCompanies = usersCompanies;
            UsersProfiles = usersProfiles;
        }

        //public IBaseRepository<Company> Companies { get; private set; }
        public IBaseRepository<User> Users { get; private set; }
        public ICompanyRepository Companies { get; private set; }
        public ILastIdRepository LastIds { get; private set; }
        public IModuleRepository Modules { get; private set; }
        public IOptionRepository Options { get; private set; }
        public IOptionProfileRepository OptionsProfiles { get; private set; }
        public IProfileRepository Profiles { get; private set; }
        public IUserProfileRepository UsersProfiles { get; private set; }
        public IUserCompanyRepository UsersCompanies { get; private set; }        

        public int Complete()
        {
            return _context.SaveChanges();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }

}
