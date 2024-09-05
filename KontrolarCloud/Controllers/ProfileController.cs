using Microsoft.AspNetCore.Mvc;
using Core;
using Core.DTOs;
using Core.Models;
using Microsoft.AspNetCore.Cors;
using Newtonsoft.Json;
using EF;
using AutoMapper;
using System.Threading.Tasks;
using EF.Utils;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;

namespace KontrolarCloud.Controllers
{
    [Route("api/[controller]")]
    [EnableCors("AllowOrigins")]
    [ApiController]
    public class ProfileController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        public IConfiguration _configuration;

        public ProfileController(IConfiguration configuration, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _configuration = configuration;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpGet("GetProfileById/{encryptedParam}")]
        public async Task<IActionResult> GetProfileById(string encryptedParam)
        {
            try
            {
                if (string.IsNullOrEmpty(encryptedParam))
                {
                    return CreateErrorResponse.BadRequestResponse(
                        code: "Null or white space",
                        message: "param is null or white space",
                        parameters: new List<string> { "param" },
                        detail: "Check param value"
                    );
                }
                encryptedParam = Uri.UnescapeDataString(encryptedParam);

                // Verificar si encryptedIdUser y encryptedIdProfile son cadenas Base64 válidas
                byte[] encryptedUserBytes;
                try
                {
                    encryptedUserBytes = Convert.FromBase64String(encryptedParam);
                }
                catch (FormatException)
                {
                    return CreateErrorResponse.BadRequestResponse(
                        code: "Base64",
                        message: "encryptedParam is not base 64",
                        parameters: new List<string> { "encryptedParam" },
                        detail: "Check encryptedParam format"
                    );
                }

                var decryptedParam = CryptoHelper.Decrypt(encryptedParam);
                var userList = await _unitOfWork.Profiles.GetProfileById(Convert.ToInt32(decryptedParam.Replace("\"", "")));
                return CreateErrorResponse.OKResponse(
                     code: "Success",
                     message: "Successful operation",
                     parameters: userList,
                     detail: "Profile obtained"
                );
            }
            catch (Exception ex)
            {
                return CreateErrorResponse.InternalServerErrorResponse(
                     code: "Internal Server Error",
                     message: ex.Message,
                     parameters: new List<string> { "userList" },
                     detail: "Check GetProfileById"
                );
            }
        }

        [HttpGet("GetProfileByCod/{encryptedParam}")]
        public async Task<IActionResult> GetProfileByCod(string encryptedParam)
        {
            try
            {
                if (string.IsNullOrEmpty(encryptedParam))
                {
                    return CreateErrorResponse.BadRequestResponse(
                        code: "Null or white space",
                        message: "param is null or white space",
                        parameters: new List<string> { "param" },
                        detail: "Check param value"
                    );
                }
                encryptedParam = Uri.UnescapeDataString(encryptedParam);

                // Verificar si encryptedIdUser y encryptedIdProfile son cadenas Base64 válidas
                byte[] encryptedUserBytes;
                try
                {
                    encryptedUserBytes = Convert.FromBase64String(encryptedParam);
                }
                catch (FormatException)
                {
                    return CreateErrorResponse.BadRequestResponse(
                        code: "Base64",
                        message: "encryptedParam is not base 64",
                        parameters: new List<string> { "encryptedParam" },
                        detail: "Check encryptedParam format"
                    );
                }

                var decryptedParam = CryptoHelper.Decrypt(encryptedParam);
                var userList = await _unitOfWork.Profiles.GetProfileByCod(decryptedParam.Replace("\"", ""));
                return CreateErrorResponse.OKResponse(
                     code: "Success",
                     message: "Successful operation",
                     parameters: userList,
                     detail: "Profile obtained"
                );
            }
            catch (Exception ex)
            {
                return CreateErrorResponse.InternalServerErrorResponse(
                     code: "Internal Server Error",
                     message: ex.Message,
                     parameters: new List<string> { "userList" },
                     detail: "Check GetProfileById"
                );
            }
        }

        [HttpGet("GetProfilesByParam/{encryptedParam}")]
        public async Task<IActionResult> GetProfilesByParam(string encryptedParam)
        //[HttpGet("GetProfilesByParam/{param}")]
        //public async Task<IActionResult> GetProfilesByParam(string param)
        {
            try
            {
                if (string.IsNullOrEmpty(encryptedParam))
                {
                    return CreateErrorResponse.BadRequestResponse(
                        code: "Null or white space",
                        message: "param is null or white space",
                        parameters: new List<string> { "param" },
                        detail: "Check param value"
                    );
                }
                encryptedParam = Uri.UnescapeDataString(encryptedParam);

                // Verificar si encryptedIdUser y encryptedIdProfile son cadenas Base64 válidas
                byte[] encryptedUserBytes;
                try
                {
                    encryptedUserBytes = Convert.FromBase64String(encryptedParam);
                }
                catch (FormatException)
                {
                    return CreateErrorResponse.BadRequestResponse(
                        code: "Base64",
                        message: "encryptedParam is not base 64",
                        parameters: new List<string> { "encryptedParam" },
                        detail: "Check encryptedParam format"
                    );
                }

                var decryptedParam = CryptoHelper.Decrypt(encryptedParam);
                var userList = await _unitOfWork.Profiles.GetListProfiles(decryptedParam.Replace("\"", ""));
                //var userList = await _unitOfWork.Profiles.GetProfilesByParam(param.Replace("\"", ""));
                return CreateErrorResponse.OKResponse(
                     code: "Success",
                     message: "Successful operation",
                     parameters: userList,
                     detail: "Profile obtained"
                );
            }
            catch (Exception ex)
            {
                return CreateErrorResponse.InternalServerErrorResponse(
                     code: "Internal Server Error",
                     message: ex.Message,
                     parameters: new List<string> { "userList" },
                     detail: "Check GetProfilesByParam"
                );
            }
        }


        [HttpGet("GetOptionsProfile/{encryptedIdProfile}")]
        public async Task<IActionResult> GetOptionsProfile(string encryptedIdProfile)
        {
            if (string.IsNullOrEmpty(encryptedIdProfile))
            {
                return CreateErrorResponse.BadRequestResponse(
                    code: "Null or white space",
                    message: "encryptedIdProfile is null or white space",
                    parameters: new List<string> { "encryptedIdProfile" },
                    detail: "Check encryptedIdProfile value"
                );
            }
            try
            {
                encryptedIdProfile = Uri.UnescapeDataString(encryptedIdProfile);

                byte[] encryptedUserBytes;
                try
                {
                    encryptedUserBytes = Convert.FromBase64String(encryptedIdProfile);
                }
                catch (FormatException)
                {
                    return CreateErrorResponse.BadRequestResponse(
                        code: "Base64",
                        message: "Parameter is not base 64",
                        parameters: new List<string> { "encryptedIdProfile" },
                        detail: "Check encryptedIdProfile format"
                    );
                }

                var decryptedParam = CryptoHelper.Decrypt(encryptedIdProfile);
                int idProfile = int.Parse(decryptedParam.Replace("\"", ""));

                var result = await _unitOfWork.Profiles.GetOptionsProfileByIdProfileAsync(idProfile);
                if (!result.Success)
                {
                    return CreateErrorResponse.NotFoundResponse(
                         code: "Not Found",
                         message: "Options for the profile not found",
                         parameters: new List<string> { "idProfile" },
                         detail: "Check idProfile value"
                        );
                }
                return CreateErrorResponse.OKResponse(
                     code: "Success",
                     message: "Successful operation",
                     parameters: result,
                     detail: "Options for the profile obtained"
                );
            }
            catch (Exception ex)
            {
                return CreateErrorResponse.InternalServerErrorResponse(
                     code: "Internal Server Error",
                     message: ex.Message,
                     parameters: new List<string> { "idProfile" },
                     detail: "Check idProfile value"
                );
            }
        }        

        [HttpPut("UpdateProfile")]
        //public IActionResult Update([FromBody] string encryptedProfileDto)
        public IActionResult Update([FromBody] ProfileDTO profileDto)
        {
            try
            {
                //if (encryptedProfileDto == null)
                //{
                //    return CreateErrorResponse.BadRequestResponse(
                //        code: "Null or white space",
                //        message: "encryptedProfileDto is null or white space",
                //        parameters: new List<string> { "encryptedProfileDto" },
                //        detail: "Check encryptedProfileDto value"
                //    );
                //}

                //encryptedProfileDto = Uri.UnescapeDataString(encryptedProfileDto);

                // Verificar si es cadena Base64 válida
                //byte[] encryptedUserBytes;
                //try
                //{
                //    encryptedUserBytes = Convert.FromBase64String(encryptedProfileDto);
                //}
                //catch (FormatException)
                //{
                //    return CreateErrorResponse.BadRequestResponse(
                //        code: "Base64",
                //        message: "Parameter is not base 64",
                //        parameters: new List<string> { "encryptedProfileDto" },
                //        detail: "Check encryptedProfileDto format"
                //    );
                //}

                //var decryptedParam = CryptoHelper.Decrypt(encryptedProfileDto);
                //var deserialized = JsonConvert.DeserializeObject<ProfileDTO>(profileDto);
                var profile = _mapper.Map<Core.Models.Profile>(profileDto);

                var existingProfile = _unitOfWork.Profiles.GetById(profile.IdProfile);

                if (existingProfile == null)
                {
                    return CreateErrorResponse.NotFoundResponse(
                         code: "Not Found",
                         message: "profile.IdProfile is not found",
                         parameters: new List<string> { "profile.IdProfile" },
                         detail: "Check profile.IdProfile value"
                        );
                }

                existingProfile.CodProfile = profile.CodProfile;
                existingProfile.NameProfile = profile.NameProfile;
                existingProfile.Description = profile.Description;

                _unitOfWork.Profiles.Update(existingProfile);
                _unitOfWork.Complete();

                return CreateErrorResponse.OKResponse(
                     code: "Success",
                     message: "Successful operation",
                     parameters: new List<string> { "deserialized" },
                     detail: "Profile updated"
                );
            }
            catch (Exception ex)
            {
                return CreateErrorResponse.InternalServerErrorResponse(
                     code: "Internal Server Error",
                     message: ex.Message,
                     parameters: new List<string> { "deserialized" },
                     detail: "Check deserialized value"
                );
            }
        }

        [HttpDelete("DeleteProfile")]
        //public async Task<IActionResult> DeleteProfile([FromBody] string encryptedProfileDto)
        public async Task<IActionResult> DeleteProfile([FromBody] ProfileDTO profileDTO)
        {
            try
            {
                //if (encryptedProfileDto == null)
                //{
                //    return BadRequest(new
                //    {
                //        success = false,
                //        message = "El profile encriptado es nulo"
                //    });
                //}

                //encryptedProfileDto = Uri.UnescapeDataString(encryptedProfileDto);

                //// Verificar si es cadena Base64 válida
                //byte[] encryptedUserBytes;
                //try
                //{
                //    encryptedUserBytes = Convert.FromBase64String(encryptedProfileDto);
                //}
                //catch (FormatException)
                //{
                //    return BadRequest(new
                //    {
                //        success = false,
                //        message = "No es valida en Base64"
                //    });
                //}

                //var decryptedParam = CryptoHelper.Decrypt(encryptedProfileDto);
                //var deserialized = JsonConvert.DeserializeObject<ProfileDTO>(decryptedParam);
                var profile = _mapper.Map<Core.Models.Profile>(profileDTO);

                var exists = _unitOfWork.Profiles.GetById(profile.IdProfile);

                if (exists == null)
                {
                    //return NotFound(Json("No encontrado"));
                    return NotFound(new
                    {
                        success = false,
                        message = "No encontrado"
                    });
                }
                
                //No funciona con el profile q viene en la petición, por eso se puso el q él encuentra
                _unitOfWork.Profiles.Delete(exists);
                var result = await _unitOfWork.CompleteAsync();

                if (result > 0)
                {
                    return Ok(new
                    {
                        success = true,
                        message = "Registro borrado con exito"
                    });
                }
                else
                    return StatusCode(500, "Error deleting");
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && sqlEx.Number == 547)
            {
                // Código de error SQL 547 indica una violación de la restricción de clave externa
                Console.WriteLine($"Error de integridad referencial: {sqlEx.Message}");
                return Conflict(new
                {
                    success = false,
                    message = "El registro está relacionado"
                });
            }
            catch (Exception ex)
            {
                // Manejo general de excepciones
                Console.WriteLine($"Error interno del servidor: {ex.Message} - StackTrace: {ex.StackTrace}");
                return StatusCode(500, $"Internal server error: {ex.Message} - StackTrace: {ex.StackTrace}");
            }
        }        

        [HttpPost("SetOptionsProfile/{idProfile}")]
        public async Task<IActionResult> SetOptionsProfile(int idProfile, [FromBody] List<OptionProfileDTO> optionProfiles)
        {
            try
            {
                var userList = await _unitOfWork.Profiles.SetOptionsProfileAsync(idProfile, optionProfiles);
                return Ok(userList);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message} - StackTrace: {ex.StackTrace}");
            }
        }

        [HttpPost("CreateProfile")]
        //public async Task<IActionResult> CreateProfile([FromBody] string encryptedProfile)
        public async Task<IActionResult> CreateProfile([FromBody] ProfileDTO profileDto)
        {
            //if (string.IsNullOrEmpty(encryptedProfile))
            //{7
            //    return BadRequest("Profile nulo o vacío");
            //}
            try
            {
                //encryptedProfile = Uri.UnescapeDataString(encryptedProfile);

                //// Verificar si encryptedProfile son cadenas Base64 válidas
                //byte[] encryptedProfileBytes;
                //try
                //{
                //    encryptedProfileBytes = Convert.FromBase64String(encryptedProfile);
                //}
                //catch (FormatException)
                //{
                //    return CreateErrorResponse.BadRequestResponse(
                //      code: "Base64",
                //      message: "Parameter is not base 64",
                //      parameters: new List<string> { "encryptedProfile" },
                //      detail: "Check encryptedProfile format"
                //    );
                //}

                //var decryptedProfile = CryptoHelper.Decrypt(encryptedProfile);
                //var deserialized = JsonConvert.DeserializeObject<ProfileDTO>(decryptedProfile);
                var profile = _mapper.Map<Core.Models.Profile>(profileDto);

                // Verificar si el CodProfile ya existe en la base de datos
                var existing = await _unitOfWork.Profiles.FindAsync(u => u.CodProfile == profile.CodProfile);
                if (existing != null)
                {
                    return CreateErrorResponse.InternalServerErrorResponse(
                         code: "Internal Server Error",
                         message: "Parameter already exists",
                         parameters: new List<string> { "profile.CodProfile" },
                         detail: "Check Profiles.FindAsync"
                    );
                }

                // Consultar el último ID usado para la tabla Profile
                var lastIdRecord = await _unitOfWork.LastIdsKTRL2.GetBiggerAsync("MT_Profiles");

                if (lastIdRecord == null)
                {
                    return CreateErrorResponse.InternalServerErrorResponse(
                         code: "Internal Server Error",
                         message: "Las id not found",
                         parameters: new List<string> { "LastIdsKTRL2.GetBiggerAsync(\"MT_Profiles\")" },
                         detail: "Check LastIdsKTRL2.GetBiggerAsync(\"MT_Profiles\")"
                    );
                }

                long newUserId = lastIdRecord.Last + 1;
                profile.IdProfile = (int)newUserId;

                var nuevoProfile = _unitOfWork.Profiles.Add(profile);
                _unitOfWork.Complete();

                // Actualizar el modelo LastId con el nuevo ID
                lastIdRecord.Last = newUserId;
                _unitOfWork.LastIdsKTRL2.Update(lastIdRecord);
                _unitOfWork.Complete();

                return CreateErrorResponse.OKResponse(
                     code: "Success",
                     message: "Successful operation",
                     parameters: new List<string> { "profile" },
                     detail: "Profile added"
                );
            }
            catch (Exception ex)
            {
                return CreateErrorResponse.InternalServerErrorResponse(
                     code: "Internal Server Error",
                     message: ex.Message,
                     parameters: new List<string> { "existingUser" },
                     detail: "Check existingUser value"
                );
            }
        }
    }
}
