using Microsoft.AspNetCore.Mvc;
using Core;
using Core.Models;
using Microsoft.AspNetCore.Cors;
using Newtonsoft.Json;
using EF;
using AutoMapper;
using KontrolarCloud.DTOs;
using System.Threading.Tasks;

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
        public async Task<IActionResult> AddAsync([FromBody] ProfileDTO profileDTO)
        {
            try
            {
                if (profileDTO == null)
                {
                    return BadRequest(Json("Datos inválidos del profile"));
                }

                var profile = _mapper.Map<Core.Models.Profile>(profileDTO); 

                // Consultar el último ID usado para la tabla Profile
                var lastIdRecord = await _unitOfWork.LastIdsKTRL2.GetBiggerAsync("MT_Profiles");

                if (lastIdRecord == null)
                {
                    return StatusCode(500, Json("No se encontró un registro de Last (id) para la tabla Profile"));
                }

                long newUserId = lastIdRecord.Last + 1;
                profile.IdProfile = (int)newUserId;

                var nuevoProfile = _unitOfWork.Profiles.Add(profile);
                _unitOfWork.Complete();

                // Actualizar el modelo LastId con el nuevo ID
                lastIdRecord.Last = newUserId;
                _unitOfWork.LastIdsKTRL2.Update(lastIdRecord);
                _unitOfWork.Complete();

                return Ok(Json(nuevoProfile));
            }
            catch (Exception ex)
            {
                return StatusCode(500, Json($"Error interno del servidor: {ex.Message}"));
            }
        }
    }
}
