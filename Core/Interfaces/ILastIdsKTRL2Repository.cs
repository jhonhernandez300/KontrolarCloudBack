using Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface ILastIdsKTRL2Repository : IBaseRepository<LastIdsKTRL2>
    {
        LastIdsKTRL2 GetBigger(string tableName);
        Task<LastIdsKTRL2> GetBiggerAsync(string tableName);
    }
}
