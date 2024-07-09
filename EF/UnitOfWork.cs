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
                          IBaseRepository<Company> companies,
                          IBaseRepository<User> users,
                          //IBaseRepository<UserCompany> usersCompanies,
                          IUserCompanyRepository usersCompanies,
                          ILastIdRepository lastIds)
        {
            _context = context;
            Companies = companies;
            Users = users;
            UsersCompanies = usersCompanies;
            LastIds = lastIds;
        }

        public IBaseRepository<Company> Companies { get; private set; }
        public IBaseRepository<User> Users { get; private set; }
        //public IBaseRepository<UserCompany> UsersCompanies { get; private set; }
        public IUserCompanyRepository UsersCompanies { get; private set; }
        public ILastIdRepository LastIds { get; private set; }

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
