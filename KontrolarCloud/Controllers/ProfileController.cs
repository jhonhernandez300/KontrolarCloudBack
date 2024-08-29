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

        [HttpGet("GetOptionsProfile/{encryptedIdProfile}")]
        public async Task<IActionResult> GetOptionsProfile(string encryptedIdProfile)
        {
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
                    return BadRequest("No es válida en Base64");
                }

                var decryptedParam = CryptoHelper.Decrypt(encryptedIdProfile);
                int idProfile = int.Parse(decryptedParam.Replace("\"", ""));

                var result = await _unitOfWork.Profiles.GetOptionsProfileByIdProfileAsync(idProfile);
                if (!result.Success)
                {
                    return StatusCode(500, $"Error: {result.ErrorMessage}");
                }
                return Ok(result.Data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message} - StackTrace: {ex.StackTrace}");
            }
        }        

        [HttpPut("Update")]
        public IActionResult Update([FromBody] string encryptedProfileDto)
        {
            try
            {
                if (encryptedProfileDto == null)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "El perfil encriptado es nulo"
                    });
                }

                encryptedProfileDto = Uri.UnescapeDataString(encryptedProfileDto);

                // Verificar si es cadena Base64 válida
                byte[] encryptedUserBytes;
                try
                {
                    encryptedUserBytes = Convert.FromBase64String(encryptedProfileDto);
                }
                catch (FormatException)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "No es valida en Base64"
                    });
                }

                var decryptedParam = CryptoHelper.Decrypt(encryptedProfileDto);
                var deserialized = JsonConvert.DeserializeObject<ProfileDTO>(decryptedParam);
                var profile = _mapper.Map<Core.Models.Profile>(deserialized);

                var existingProfile = _unitOfWork.Profiles.GetById(profile.IdProfile);

                if (existingProfile == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "No encontrado"
                    });
                }

                existingProfile.CodProfile = profile.CodProfile;
                existingProfile.NameProfile = profile.NameProfile;
                existingProfile.Description = profile.Description;

                _unitOfWork.Profiles.Update(existingProfile);
                _unitOfWork.Complete();

                return Ok(new
                {
                    success = true,
                    message = "Registro actualizado con exito"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, Json($"Error interno del servidor: {ex.Message}"));
            }
        }

        [HttpDelete("DeleteProfile")]
        public async Task<IActionResult> DeleteProfile([FromBody] string encryptedProfileDto)
        {
            try
            {
                if (encryptedProfileDto == null)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "El profile encriptado es nulo"
                    });
                }

                encryptedProfileDto = Uri.UnescapeDataString(encryptedProfileDto);

                // Verificar si es cadena Base64 válida
                byte[] encryptedUserBytes;
                try
                {
                    encryptedUserBytes = Convert.FromBase64String(encryptedProfileDto);
                }
                catch (FormatException)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "No es valida en Base64"
                    });
                }

                var decryptedParam = CryptoHelper.Decrypt(encryptedProfileDto);
                var deserialized = JsonConvert.DeserializeObject<ProfileDTO>(decryptedParam);
                var profile = _mapper.Map<Core.Models.Profile>(deserialized);

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

        [HttpGet("GetProfilesByParam/{encryptedParam}")]
        public async Task<IActionResult> GetProfilesByParam(string encryptedParam)
        {
            try
            {
                encryptedParam = Uri.UnescapeDataString(encryptedParam);

                // Verificar si encryptedIdUser y encryptedIdProfile son cadenas Base64 válidas
                byte[] encryptedUserBytes;
                try
                {
                    encryptedUserBytes = Convert.FromBase64String(encryptedParam);
                }
                catch (FormatException)
                {
                    return BadRequest("No es valida en Base64");
                }

                var decryptedParam = CryptoHelper.Decrypt(encryptedParam);
                var userList = await _unitOfWork.Profiles.GetProfilesByParam(decryptedParam.Replace("\"", ""));
                return Ok(userList);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message} - StackTrace: {ex.StackTrace}");
            }
        }

        [HttpPost("AddAsync")]
        public async Task<IActionResult> AddAsync([FromBody] string encryptedProfile)
        {
            try
            {
                encryptedProfile = Uri.UnescapeDataString(encryptedProfile);

                // Verificar si encryptedProfile son cadenas Base64 válidas
                byte[] encryptedProfileBytes;
                try
                {
                    encryptedProfileBytes = Convert.FromBase64String(encryptedProfile);
                }
                catch (FormatException)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "No es valida en Base64"
                    });
                }

                var decryptedProfile = CryptoHelper.Decrypt(encryptedProfile);
                var deserialized = JsonConvert.DeserializeObject<ProfileDTO>(decryptedProfile);
                var profile = _mapper.Map<Core.Models.Profile>(deserialized);

                // Verificar si el CodProfile ya existe en la base de datos
                var existing = await _unitOfWork.Profiles.FindAsync(u => u.CodProfile == profile.CodProfile);
                if (existing != null)
                {
                    return StatusCode(500, new
                    {
                        success = false,
                        message = "Ya existe ese Código de Perfil"
                    });
                }

                // Consultar el último ID usado para la tabla Profile
                var lastIdRecord = await _unitOfWork.LastIdsKTRL2.GetBiggerAsync("MT_Profiles");

                if (lastIdRecord == null)
                {
                    return StatusCode(500, new
                    {
                        success = false,
                        message = "Last (id) no encontrado"
                    });
                }

                long newUserId = lastIdRecord.Last + 1;
                profile.IdProfile = (int)newUserId;

                var nuevoProfile = _unitOfWork.Profiles.Add(profile);
                _unitOfWork.Complete();

                // Actualizar el modelo LastId con el nuevo ID
                lastIdRecord.Last = newUserId;
                _unitOfWork.LastIdsKTRL2.Update(lastIdRecord);
                _unitOfWork.Complete();
                
                return Ok(new
                {
                    success = true,
                    message = "Registro agregado con exito"                    
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error interno del servidor {ex.Message}"
                });
            }
        }

    }
}
