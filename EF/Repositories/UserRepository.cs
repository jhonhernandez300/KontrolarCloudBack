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
     int idProfile)
        {
            var modulesOptions = new List<ModuleOptionDTO>();
            string message = "";
            bool operationExecuted = false;

            var conn = _secondaryContext.Database.GetDbConnection();

            try
            {
                await conn.OpenAsync();

                using (var command = conn.CreateCommand())
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.CommandText = "SP_ProfileGetOptions";

                    var param1 = command.CreateParameter();
                    param1.ParameterName = "@IdUser";
                    param1.Value = idUser;
                    command.Parameters.Add(param1);

                    var param2 = command.CreateParameter();
                    param2.ParameterName = "@IdProfile";
                    param2.Value = idProfile;
                    command.Parameters.Add(param2);

                    var messageParam = command.CreateParameter();
                    messageParam.ParameterName = "@Message";
                    messageParam.DbType = System.Data.DbType.String;
                    messageParam.Size = int.MaxValue;
                    messageParam.Direction = System.Data.ParameterDirection.Output;
                    command.Parameters.Add(messageParam);

                    var operationExecutedParam = command.CreateParameter();
                    operationExecutedParam.ParameterName = "@OperationExecuted";
                    operationExecutedParam.DbType = System.Data.DbType.Boolean;
                    operationExecutedParam.Direction = System.Data.ParameterDirection.Output;
                    command.Parameters.Add(operationExecutedParam);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            modulesOptions.Add(new ModuleOptionDTO
                            {
                                IdModule = reader.GetInt32(0),
                                NameModule = reader.GetString(1),
                                IconModule = reader.GetString(2),
                                colorModule = reader.GetString(3),
                                IdOption = reader.GetInt32(4),
                                IconOption = reader.GetString(5),
                                NameOption = reader.GetString(6),
                                Description = reader.GetString(7),
                                Controler = reader.GetString(8),
                                Action = reader.GetString(9),
                                OrderBy = reader.GetInt32(10),
                                UserAssigned = reader.GetString(11)
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
