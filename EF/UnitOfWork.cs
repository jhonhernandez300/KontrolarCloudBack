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

        public IBaseRepository<Company> Companies { get; private set; }
        //public IRecetasRepository Recetas { get; private set; }

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;

            Companies = new BaseRepository<Company>(_context);
            //Recetas = new RecetasRepository(_context);
        }

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
