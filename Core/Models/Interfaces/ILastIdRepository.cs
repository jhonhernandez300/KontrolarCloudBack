using Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface ILastIdsKTRL1Repository : IBaseRepository<LastIdsKTRL1>
    {
        LastIdsKTRL1 GetBigger(string tableName);
        Task<LastIdsKTRL1> GetBiggerAsync(string tableName);
    }
}
