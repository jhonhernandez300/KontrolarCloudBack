using Core.DTO;
using Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface IUserRepository : IBaseRepository<User>
    {
        Task<(List<ModuleOptionDTO> moduleOptionDTOs, string message, bool operationExecuted)> ProfileGetOptions(int idUser, int idProfile);
    }
}
