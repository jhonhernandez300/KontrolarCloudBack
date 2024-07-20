using Core.DTO;
using Core.Interfaces;
using Core.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
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

        public async Task<(List<ModuleOptionDTO> moduleOptionDTOs, string message, bool operationExecuted)> ProfileGetOptions(
        int idUser,
        int idProfile
        )
        {
            var modulesOptions = new List<ModuleOptionDTO>();
            string message = "";
            bool operationExecuted = false;

            var conn = _secondaryContext.Database.GetDbConnection();

            try {
                await conn.OpenAsync();

                using (var command = conn.CreateCommand())
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.CommandText = "SP_ProfileGetOptions";

                    var param = command.CreateParameter();
                    param.ParameterName = "@IdUser";
                    param.Value = idUser;
                    command.Parameters.Add(param);

                    var param2 = command.CreateParameter();
                    param2.ParameterName = "@IdProfile";
                    param2.Value = idProfile;
                    command.Parameters.Add(param2);

                    var messageParam = command.CreateParameter();
                    messageParam.ParameterName = "@Message";
                    messageParam.DbType = DbType.String;
                    messageParam.Size = 500;
                    messageParam.Direction = ParameterDirection.Output;
                    command.Parameters.Add(messageParam);

                    var operationExecutedParam = command.CreateParameter();
                    operationExecutedParam.ParameterName = "@OperationExecuted";
                    operationExecutedParam.DbType = DbType.Boolean;
                    operationExecutedParam.Direction = ParameterDirection.Output;
                    command.Parameters.Add(operationExecutedParam);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            modulesOptions.Add(new ModuleOptionDTO
                            {
                                IdModule = reader.GetInt32(0),
                                NameModule = reader.GetString(1),
                                IdOption = reader.GetInt32(2),
                                NameOption = reader.GetString(3),
                                Description = reader.GetString(4),
                                Icon = reader.GetString(5),
                                Controler = reader.GetString(6),
                                Action = reader.GetString(7),
                                OrderBy = reader.GetInt32(8)
                            });
                        }
                    }

                    operationExecuted = (bool)operationExecutedParam.Value;
                    message = (string)messageParam.Value;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing stored procedure: {ex.Message}");
                throw;
            }
            finally
            {
                await conn.CloseAsync();
                Console.WriteLine("Database connection closed.");
            }

            return (modulesOptions, message, operationExecuted);
        }

    }
}
