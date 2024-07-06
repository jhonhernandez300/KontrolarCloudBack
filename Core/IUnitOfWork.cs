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
        IBaseRepository<Company> Companies { get; }
        //ICompaniessRepository Recetas { get; }

        int Complete();
    }
}
