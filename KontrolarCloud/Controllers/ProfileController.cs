using Microsoft.AspNetCore.Mvc;
using Core;
using Core.Models;
using Microsoft.AspNetCore.Cors;
using EF.Utils;
using Newtonsoft.Json;
using EF.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EF;
using Microsoft.EntityFrameworkCore;

namespace KontrolarCloud.Controllers
{
    [Route("api/[controller]")]
    [EnableCors("AllowOrigins")]
    [ApiController]
    public class ProfileController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public IConfiguration _configuration;

        public ProfileController(IConfiguration configuration, IUnitOfWork unitOfWork)
        {
            _configuration = configuration;
            _unitOfWork = unitOfWork;
        }

        [HttpPost("AddAsync")]
        public async Task<IActionResult> AddAsync([FromBody] Profile profile)
        {
            try
            {
                if (profile == null)
                {
                    return BadRequest(Json("Datos inválidos del profile"));
                }                

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
