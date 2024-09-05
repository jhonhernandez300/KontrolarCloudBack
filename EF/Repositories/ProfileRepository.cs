using Core.Interfaces;
using Core.Models;
using Core.DTO;
using Core.DTOs;
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
using AutoMapper;
using Core.Models;
using System.Reflection.PortableExecutable;

namespace EF.Repositories
{
    public class ProfileRepository : SecondaryBaseRepository<Core.Models.Profile>, IProfileRepository
    {
        private readonly SecondaryDbContext _context;
        private readonly IMapper _mapper;

        public ProfileRepository(SecondaryDbContext context, IMapper mapper) : base(context)
        {
            _context = context;
            _mapper = mapper;
        }

        //public async Task<OperationResult<List<ProfileDTO>>> GetProfileById(int idProfile)
        //{
        //    var result = new OperationResult<List<ProfileDTO>>();

        //    try
        //    {
        //        var profiles = new List<ProfileDTO>();

        //        var command = _context.Database.GetDbConnection().CreateCommand();
        //        command.CommandText = "SP_AdminProfiles";
        //        command.CommandType = CommandType.StoredProcedure;

        //        command.Parameters.Add(new SqlParameter("@Option", SqlDbType.NVarChar) { Value = "GetOptionsProfile" });
        //        command.Parameters.Add(new SqlParameter("@IdProfile", SqlDbType.Int) { Value = idProfile });

        //        await _context.Database.OpenConnectionAsync();

        //        using (var reader = await command.ExecuteReaderAsync())
        //        {
        //            while (await reader.ReadAsync())
        //            {
        //                var profileDTO = new ProfileDTO
        //                {
        //                    IdProfile = reader.GetInt32(reader.GetOrdinal("IdProfile")),
        //                    CodProfile = reader.GetString(reader.GetOrdinal("CodProfile")),
        //                    NameProfile = reader.GetString(reader.GetOrdinal("NameProfile")),
        //                    Description = reader.GetString(reader.GetOrdinal("Description"))
        //                };

        //                profiles.Add(profileDTO);
        //            }
        //        }

        //        result.Data = profiles;
        //        result.Success = true;
        //    }
        //    catch (Exception ex)
        //    {
        //        result.Success = false;
        //        result.ErrorMessage = ex.Message;
        //    }
        //    finally
        //    {
        //        await _context.Database.CloseConnectionAsync();
        //    }

        //    return result;
        //}

        public async Task<OperationResult<List<OptionProfileDTO>>> GetOptionsProfileByIdProfileAsync(int idProfile)
        {
            var result = new OperationResult<List<OptionProfileDTO>>();

            try
            {
                var optionsProfiles = new List<OptionProfileDTO>();

                var command = _context.Database.GetDbConnection().CreateCommand();
                command.CommandText = "SP_AdminProfiles";
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new SqlParameter("@Option", SqlDbType.NVarChar) { Value = "GetOptionsProfile" });
                command.Parameters.Add(new SqlParameter("@IdProfile", SqlDbType.Int) { Value = idProfile });

                await _context.Database.OpenConnectionAsync();

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var optionProfileDTO = new OptionProfileDTO
                        {
                            IdModule = reader.GetInt32(reader.GetOrdinal("IdModule")),
                            IdOption = reader.GetInt32(reader.GetOrdinal("IdOption")),
                            IconOption = reader.GetString(reader.GetOrdinal("IconOption")),
                            NameOption = reader.GetString(reader.GetOrdinal("NameOption")),
                            Description = reader.GetString(reader.GetOrdinal("Description")),
                            Controller = reader.GetString(reader.GetOrdinal("Controler")),
                            Action = reader.GetString(reader.GetOrdinal("Action")),
                            OrderBy = reader.GetInt32(reader.GetOrdinal("OrderBy")),
                            UserAssigned = reader.GetString(reader.GetOrdinal("UserAssigned")) == "True"
                        };
                        optionsProfiles.Add(optionProfileDTO);
                    }
                }

                result.Data = optionsProfiles;
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.Message;
            }
            return result;
        }



        public async Task<List<Core.Models.Profile>> GetListProfiles(string param)
        {
            var profiles = await _context.Profiles
                .FromSqlInterpolated($"EXEC SP_AdminProfiles @Option = 'GetListProfiles', @SearchListBycoincidence = {param}")
                .ToListAsync();

            return profiles;
        }

        public async Task<Core.Models.Profile?> GetProfileById(int idProfile)
        {
            var profiles = await _context.Profiles
                .FromSqlInterpolated($"EXEC SP_AdminProfiles @Option = 'GetProfileById', @IdProfile = {idProfile}")
                .ToListAsync();

            return profiles.FirstOrDefault();
        }

        public async Task<Core.Models.Profile?> GetProfileByCod(string idProfile)
        {
            var profiles = await _context.Profiles
                .FromSqlInterpolated($"EXEC SP_AdminProfiles @Option = 'GetProfileByCod', @CodProfile = {idProfile}")
                .ToListAsync();

            return profiles.FirstOrDefault();
        }

        public async Task<OperationResult<bool>> SetOptionsProfileAsync(int idProfile, List<OptionProfileDTO> options)
        {
            var result = new OperationResult<bool>();
            try
            {
                var dataTable = new DataTable();
                dataTable.Columns.Add("IdOption", typeof(int));
                dataTable.Columns.Add("UserAssigned", typeof(bool));

                foreach (var option in options)
                {
                    dataTable.Rows.Add(option.IdOption, option.UserAssigned);
                }

                var command = _context.Database.GetDbConnection().CreateCommand();
                command.CommandText = "SP_AdminProfiles";
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new SqlParameter("@Option", SqlDbType.NVarChar) { Value = "SetOptionsProfile" });
                command.Parameters.Add(new SqlParameter("@IdProfile", SqlDbType.Int) { Value = idProfile });
                command.Parameters.Add(new SqlParameter("@tblOptionsPerfil", SqlDbType.Structured)
                {
                    TypeName = "typ_OptionsSelec",
                    Value = dataTable
                });

                var messageParameter = new SqlParameter("@Message", SqlDbType.VarChar, -1)
                {
                    Direction = ParameterDirection.Output
                };
                command.Parameters.Add(messageParameter);

                var operationExecutedParameter = new SqlParameter("@OperationExecuted", SqlDbType.Bit)
                {
                    Direction = ParameterDirection.Output
                };
                command.Parameters.Add(operationExecutedParameter);

                await _context.Database.OpenConnectionAsync();
                await command.ExecuteNonQueryAsync();

                result.Success = (bool)operationExecutedParameter.Value;
                result.Data = result.Success; // Set Data to the success status
                result.ErrorMessage = messageParameter.Value.ToString();
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Data = false; // Set Data to false in case of an exception
                result.ErrorMessage = ex.Message;
            }
            finally
            {
                await _context.Database.CloseConnectionAsync();
            }
            return result;
        }

    }
}
