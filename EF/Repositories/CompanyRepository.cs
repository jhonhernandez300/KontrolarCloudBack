using Core.DTO;
using Core.Interfaces;
using Core.Models;
using EF.Utils;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace EF.Repositories
{
    public class CompanyRepository : BaseRepository<Company>, ICompanyRepository
    {
        private readonly ApplicationDbContext _context;

        public CompanyRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        //Este método es distinto por que usa un SP en lugar de EF
        public async Task<(List<Company_UserCompanyDTO> companies_UserCompanies, bool operationExecuted, string message)> GetCompaniesByIdentificationNumber(string identificationNumber)
        {
            var companies_UserCompanies = new List<Company_UserCompanyDTO>();
            bool operationExecuted = false;
            string message = string.Empty;

            var conn = _context.Database.GetDbConnection();
            try
            {
                await conn.OpenAsync();
                Console.WriteLine("Database connection opened.");

                using (var command = conn.CreateCommand())
                {
                    command.CommandText = "SP_AdminUsers2";
                    command.CommandType = CommandType.StoredProcedure;

                    var optionParam = command.CreateParameter();
                    optionParam.ParameterName = "@Option";
                    optionParam.Value = "GetCompaniesAssigned";
                    command.Parameters.Add(optionParam);

                    var param = command.CreateParameter();
                    param.ParameterName = "@IdentificationNumber";
                    param.Value = identificationNumber;
                    command.Parameters.Add(param);

                    var messageParam = command.CreateParameter();
                    messageParam.ParameterName = "@Message";
                    messageParam.DbType = DbType.String;
                    messageParam.Size = -1; // Cambiado a -1 para NVARCHAR(MAX)
                    messageParam.Direction = ParameterDirection.Output;
                    command.Parameters.Add(messageParam);

                    var operationExecutedParam = command.CreateParameter();
                    operationExecutedParam.ParameterName = "@OperationExecuted";
                    operationExecutedParam.DbType = DbType.Boolean;
                    operationExecutedParam.Direction = ParameterDirection.Output;
                    command.Parameters.Add(operationExecutedParam);

                    Console.WriteLine($"Executing stored procedure with IdentificationNumber: {identificationNumber}");

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        // Avanza al segundo conjunto de resultados
                        await reader.NextResultAsync();

                        while (await reader.ReadAsync())
                        {
                            companies_UserCompanies.Add(new Company_UserCompanyDTO
                            {
                                IdCompany = reader.GetInt32(0),
                                CompanyName = reader.GetString(1),
                                DB = reader.GetString(2),
                                LicenseValidDate = reader.GetDateTime(3),
                                NumberSimiltaneousConnection = reader.GetInt32(4),
                                Id = reader.GetInt64(5),
                                Correo = reader.GetString(6),
                                IdUser = reader.GetInt32(7),
                                IsEnabled = reader.GetBoolean(8),
                                //Password = reader.GetString(9)
                                Password = EncryptAndRemoveQuotes(reader.GetString(9))
                            });
                        }
                    }

                    if (messageParam.Value != DBNull.Value)
                    {
                        message = (string)messageParam.Value;
                    }

                    if (operationExecutedParam.Value != DBNull.Value)
                    {
                        operationExecuted = (bool)operationExecutedParam.Value;
                    }                    
                }

                Console.WriteLine("Stored procedure executed successfully.");
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

            return (companies_UserCompanies, operationExecuted, message);
        }

        public string EncryptAndRemoveQuotes(string input)
        {
            string passwordDecrypted = CryptoHelper.Decrypt(input);
            return StringHelper.EliminateFirstAndLast(passwordDecrypted);
        }

    }
}
