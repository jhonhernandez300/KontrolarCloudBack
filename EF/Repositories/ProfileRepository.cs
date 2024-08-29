using Core.Interfaces;
using Core.Models;
using Core.DTO;
using Core.Interfaces;
using Core.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;


namespace EF.Repositories
{
    public class ProfileRepository : SecondaryBaseRepository<Profile>, IProfileRepository
    {
        private readonly SecondaryDbContext _context;

        public ProfileRepository(SecondaryDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<OptionProfileDTO>> GetOptionsProfileByIdProfileAsync(int idProfile)
        {
            var optionsProfiles = new List<OptionProfileDTO>();

            var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandText = "SP_AdminProfiles";
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add(new SqlParameter("@Option", SqlDbType.Int) { Value = 1 }); // Supongo que siempre usas la opción 1, puedes cambiarlo según tus necesidades.
            command.Parameters.Add(new SqlParameter("@IdProfile", SqlDbType.Int) { Value = idProfile });

            await _context.Database.OpenConnectionAsync();

            using (var reader = await command.ExecuteReaderAsync())
            {
                var mapper = _mapper.ConfigurationProvider.CreateMapper();
                while (await reader.ReadAsync())
                {
                    var optionProfileDTO = mapper.Map<IDataReader, OptionProfileDTO>(reader);
                    optionsProfiles.Add(optionProfileDTO);
                }
            }

            return optionsProfiles;
        }


        public Task<List<Profile>> GetProfilesByParam(string param)
        {
            return _context.Profiles
                    .Where(p => p.CodProfile.Contains(param) ||
                                p.NameProfile.Contains(param) ||
                                p.Description.Contains(param)).ToListAsync();
        }
    }
}
