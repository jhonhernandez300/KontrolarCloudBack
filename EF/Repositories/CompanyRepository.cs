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
    public class CompanyRepository : BaseRepository<Company>, ICompanyRepository
    {
        private readonly ApplicationDbContext _context;

        public CompanyRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        //Este método es distinto por que usa un SP en lugar de EF
        public async Task<(List<Company_UserCompanyDTO> companies_UserCompanies, bool userNotFound)> GetCompaniesByIdentificationNumber(string identificationNumber)
        {
            var companies_UserCompanies = new List<Company_UserCompanyDTO>();
            bool userNotFound = false;

            var conn = _context.Database.GetDbConnection();
            try
            {
                await conn.OpenAsync();
                Console.WriteLine("Database connection opened.");

                using (var command = conn.CreateCommand())
                {
                    command.CommandText = "SP_GetCompaniesByIdentificationNumber";
                    command.CommandType = CommandType.StoredProcedure;

                    var param = command.CreateParameter();
                    param.ParameterName = "@IdentificationNumber";
                    param.Value = identificationNumber;
                    command.Parameters.Add(param);

                    var userNotFoundParam = command.CreateParameter();
                    userNotFoundParam.ParameterName = "@UserNotFound";
                    userNotFoundParam.DbType = DbType.Boolean;
                    userNotFoundParam.Direction = ParameterDirection.Output;
                    command.Parameters.Add(userNotFoundParam);

                    Console.WriteLine($"Executing stored procedure with IdentificationNumber: {identificationNumber}");

                    using (var reader = await command.ExecuteReaderAsync())
                    {
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
                                Password = reader.GetString(9)
                            });
                        }
                    }

                    userNotFound = (bool)userNotFoundParam.Value;
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

            return (companies_UserCompanies, userNotFound);
        }

    }
}
