using Core.DTO;
using Core.Interfaces;
using Core.Models;
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

        public async Task<(
            List<ModuleDTO> moduleOptionDTOs, 
            string message, 
            bool operationExecuted
         )> ProfileGetOptions(int idUser)
        {
            var modules = new List<ModuleDTO>();
            string message = "";
            bool operationExecuted = false;

            var conn = _secondaryContext.Database.GetDbConnection();

            try
            {
                await conn.OpenAsync();

                using (var command = conn.CreateCommand())
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.CommandText = "SP_AdminProfiles";

                    var param1 = command.CreateParameter();
                    param1.ParameterName = "@IdUser";
                    param1.Value = idUser;
                    command.Parameters.Add(param1);

                    var paramOption = command.CreateParameter();
                    paramOption.ParameterName = "@Option";
                    paramOption.Value = "GetOptionsAssigned";
                    command.Parameters.Add(paramOption); 

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
                        // Leer primer resultado (módulos)
                        while (await reader.ReadAsync())
                        {
                            modules.Add(new ModuleDTO
                            {
                                IdModule = reader.GetInt32(0),
                                NameModule = reader.GetString(1),
                                Icon = reader.GetString(2),
                                Color = reader.GetString(3)
                            });
                        }

                        // Leer el siguiente conjunto de resultados (opciones)
                        if (await reader.NextResultAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var module = modules.FirstOrDefault(m => m.IdModule == reader.GetInt32(0));
                                if (module != null)
                                {
                                    module.Options.Add(new OptionDTO
                                    {
                                        IdOption = reader.GetInt32(1),
                                        Icon = reader.GetString(2),
                                        NameOption = reader.GetString(3),
                                        Description = reader.GetString(4),
                                        Controler = reader.GetString(5),
                                        Action = reader.GetString(6),
                                        OrderBy = reader.GetInt32(7),
                                        UserAssigned = reader.GetString(8)
                                    });
                                }
                            }
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

            return (modules, message, operationExecuted);
        }

    }
}
