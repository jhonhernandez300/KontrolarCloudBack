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
        public async Task<List<Company>> GetCompaniesByDocumentNumber(string documentNumber)
        {
            var companies = new List<Company>();

            var conn = _context.Database.GetDbConnection();
            try
            {
                await conn.OpenAsync();
                Console.WriteLine("Database connection opened.");

                using (var command = conn.CreateCommand())
                {
                    command.CommandText = "GetCompaniesByDocumentNumber";
                    command.CommandType = CommandType.StoredProcedure;

                    var param = command.CreateParameter();
                    param.ParameterName = "@DocumentNumber";
                    param.Value = documentNumber;
                    command.Parameters.Add(param);

                    Console.WriteLine($"Executing stored procedure with DocumentNumber: {documentNumber}");

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            companies.Add(new Company
                            {
                                IdCompany = reader.GetInt32(0),
                                CompanyName = reader.GetString(1),
                                DB = reader.GetString(2),
                                UserName = reader.GetString(3),
                                CompanyPassword = reader.GetString(4),
                                LicenseValidDate = reader.GetDateTime(5),
                                ConnectionsSimultaneousNumber = reader.GetInt32(6)
                            });
                        }
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

            return companies;
        }

    }
}
