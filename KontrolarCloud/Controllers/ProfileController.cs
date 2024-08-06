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
                        message = "La cadena proporcionada no es válida en Base64."
                    });
                }

                var decryptedProfile = CryptoHelper.Decrypt(encryptedProfile);
                var deserialized = JsonConvert.DeserializeObject<ProfileDTO>(decryptedProfile);
                var profile = _mapper.Map<Core.Models.Profile>(deserialized);

                // Consultar el último ID usado para la tabla Profile
                var lastIdRecord = await _unitOfWork.LastIdsKTRL2.GetBiggerAsync("MT_Profiles");

                if (lastIdRecord == null)
                {
                    return StatusCode(500, new
                    {
                        success = false,
                        message = "No se encontró un registro de Last (id) para la tabla Profile."
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
                    message = "Registro agregado con éxito."                    
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error interno del servidor: {ex.Message}"
                });
            }
        }

    }
}
