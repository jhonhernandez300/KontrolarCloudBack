using Core.DTO;
using Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface IProfileRepository : IBaseRepository<Profile>
    {
        Task<List<Profile>> GetProfilesByParam(string param);
        Task<List<OptionProfileDTO>> GetOptionsProfileByIdProfileAsync(int idProfile);
    }
}
