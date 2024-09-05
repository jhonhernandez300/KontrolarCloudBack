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
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly SecondaryDbContext _secondaryContext;

        public UserRepository(ApplicationDbContext context, SecondaryDbContext secondaryContext) : base(context)
        {
            _context = context;
            _secondaryContext = secondaryContext;
        }

        public async Task<User> GetByIdAsync(string id)
        {
            return await _context.Users.FindAsync(id);
        }

        public Task<List<User>> GetUsersByParam(string param)
        {
            return _context.Users
                    .Where(u => u.IdentificationNumber.Contains(param) ||
                                u.Names.Contains(param) ||
                                u.Surnames.Contains(param)).ToListAsync();
        }

        public async Task<(
            List<ModuleDTO> moduleOptionDTOs, 
            string message, 
            bool operationExecuted
         )> ProfileGetOptions(int idUser)
        {
            var userProfile = await _secondaryContext.UsersProfiles.FirstOrDefaultAsync(x => x.IdUser == idUser);

            var messageParam = new SqlParameter("@Message", SqlDbType.VarChar, -1)
            {
                Direction = ParameterDirection.Output
            };

            var operationExecutedParam = new SqlParameter("@OperationExecuted", SqlDbType.Bit)
            {
                Direction = ParameterDirection.Output
            };

            string sqlQuery = "EXEC [dbo].[SP_ProfileGetOptions] @IdUser, @IdProfile, @Message OUTPUT, @OperationExecuted OUTPUT";

            var moduleDtos = new List<ModuleDTO>();
            using (var command = _secondaryContext.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = sqlQuery;
                command.Parameters.Add(new SqlParameter("@IdUser", idUser));
                command.Parameters.Add(new SqlParameter("@IdProfile", userProfile?.IdProfile));
                command.Parameters.Add(messageParam);
                command.Parameters.Add(operationExecutedParam);

                _secondaryContext.Database.OpenConnection();
                using (var result = await command.ExecuteReaderAsync())
                {
                    while (await result.ReadAsync())
                    {
                        var module = moduleDtos.FirstOrDefault(m => m.IdModule == (int)result["IdModule"]);
                        if (module == null)
                        {
                            module = new ModuleDTO
                            {
                                IdModule = (int)result["IdModule"],
                                NameModule = result["NameModule"]?.ToString(),
                                Icon = result["IconModule"]?.ToString(),
                                Color = result["ColorModule"]?.ToString(),
                                Options = new List<OptionDTO>()
                            };
                            moduleDtos.Add(module);
                        }

                        var option = new OptionDTO
                        {
                            IdOption = (int)result["IdOption"],
                            Icon = result["IconOption"]?.ToString(),
                            NameOption = result["NameOption"]?.ToString(),
                            Description = result["Description"]?.ToString(),
                            Controler = result["Controler"]?.ToString(),
                            Action = result["Action"]?.ToString(),
                            OrderBy = (int)result["OrderBy"],
                            UserAssigned = result["UserAssigned"]?.ToString()
                        };

                        module.Options.Add(option);
                    }
                }
            }

            string message = (string)messageParam.Value;
            bool operationExecuted = (bool)operationExecutedParam.Value;

            return (moduleDtos, message, operationExecuted);            
        }

    }
}
