﻿using Microsoft.AspNetCore.Mvc;
using Core;
using Core.Models;
using Microsoft.AspNetCore.Cors;
using Newtonsoft.Json;
using EF;
using AutoMapper;
using KontrolarCloud.DTOs;
using System.Threading.Tasks;
using EF.Utils;

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
                        message = "La cadena no es válida en Base64"
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

        [HttpPut("DisableProfile")]
        public async Task<IActionResult> DisableProfile([FromBody] string encryptedProfileDto)
        {
            try
            {
                if (encryptedProfileDto == null)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "El usuario encriptado es nulo"
                    });
                }

                encryptedProfileDto = Uri.UnescapeDataString(encryptedProfileDto);

                // Verificar si es cadena Base64 válida
                byte[] encryptedProfileBytes;
                try
                {
                    encryptedProfileBytes = Convert.FromBase64String(encryptedProfileDto);
                }
                catch (FormatException)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "La cadena proporcionada no es válida en Base64"
                    });
                }

                var decryptedParam = CryptoHelper.Decrypt(encryptedProfileDto);
                var deserialized = JsonConvert.DeserializeObject<ProfileDTO>(decryptedParam);
                var profile = _mapper.Map<Core.Models.Profile>(deserialized);                

                _unitOfWork.Profiles.Update(profile);
                var result = await _unitOfWork.CompleteAsync();

                if (result > 0)
                {
                    return Ok(new
                    {
                        success = true,
                        message = "Registro borrado con exito" //En realidad se desactiva
                    });
                }
                else
                    return StatusCode(500, "An error occurred while updating the user");
            }
            catch (Exception ex)
            {
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
                    return BadRequest("Una o ambas cadenas proporcionadas no son válidas en Base64");
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
                        message = "La cadena proporcionada no es válida en Base64"
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
                        message = "No se encontró un registro de Last (id) para la tabla Profile"
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
