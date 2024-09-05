using Core.DTO;
using Core.DTOs;
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
        Task<Profile> GetProfileById(int idProfile);
        Task<Core.Models.Profile?> GetProfileByCod(string idProfile);
        Task<List<Profile>> GetListProfiles(string param);
        //Task<List<Profile>> GetProfileByCod(string param);
        Task<OperationResult<List<OptionProfileDTO>>> GetOptionsProfileByIdProfileAsync(int idProfile);
        Task<OperationResult<bool>> SetOptionsProfileAsync(int idProfile, List<OptionProfileDTO> options);
    }
}
