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
    public class UserController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public IConfiguration _configuration;

        public UserController(IConfiguration configuration, IUnitOfWork unitOfWork)
        {
            _configuration = configuration;
            _unitOfWork = unitOfWork;
        }

        [HttpGet("GetOptionsByIdUser/{encryptedIdUser}")]
        public async Task<IActionResult> GetOptionsByIdUser(string encryptedIdUser)
        {
            try
            {
                encryptedIdUser = Uri.UnescapeDataString(encryptedIdUser);

                // Verificar si encryptedIdUser y encryptedIdProfile son cadenas Base64 válidas
                byte[] encryptedUserBytes;
                try
                {
                    encryptedUserBytes = Convert.FromBase64String(encryptedIdUser);
                }
                catch (FormatException)
                {
                    return BadRequest("Una o ambas cadenas proporcionadas no son válidas en Base64.");
                }

                var decryptedIdUser = CryptoHelper.Decrypt(encryptedIdUser);
                var trimmedIdUser = StringHelper.EliminateFirstAndLast(decryptedIdUser);
                int idUser = Convert.ToInt32(trimmedIdUser);                

                var (modules, message, operationExecuted) = await _unitOfWork.Users.ProfileGetOptions(idUser);
                
                if (!operationExecuted)
                {
                    return NotFound(message);
                }

                var moduleOptionsJson = JsonConvert.SerializeObject(modules);
                var encryptedData = CryptoHelper.Encrypt(moduleOptionsJson);

                return Ok(encryptedData);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message} - StackTrace: {ex.StackTrace}");
            }
        }


        [HttpGet]  
        [Route("CreateToken/{encryptedIdentificationNumber}/{encryptedIdCompany}")]
        //public dynamic CreateToken()
        public dynamic CreateToken(string encryptedIdentificationNumber, string encryptedIdCompany)
        {
            var IdentificationNumber = CryptoHelper.Decrypt(encryptedIdentificationNumber);
            IdentificationNumber = StringHelper.EliminateFirstAndLast(IdentificationNumber);

            var idCompany = CryptoHelper.Decrypt(encryptedIdCompany);
            idCompany = StringHelper.EliminateFirstAndLast(idCompany);

            try
            {
                var jwt = _configuration.GetSection("Jwt")
                    .Get<Jwt>();

                var claims = new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, jwt.Subject),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()),
                    new Claim("idCompany", idCompany),
                    new Claim("IdentificationNumber", IdentificationNumber)
                };

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key));
                var singIng = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                        jwt.Issuer,
                        jwt.Audience,
                        claims,
                        expires: DateTime.Now.AddMinutes(20),
                        signingCredentials: singIng
                );

                var response = new JwtSecurityTokenHandler().WriteToken(token);

                var tokenJson = JsonConvert.SerializeObject(response);
                var encryptedToken = CryptoHelper.Encrypt(tokenJson);

                return encryptedToken;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        [HttpGet("GetCompaniesByIdentificationNumber/{encryptedIdentificationNumber}")]
        public async Task<IActionResult> GetCompaniesByIdentificationNumber(string encryptedIdentificationNumber)
        {
            try
            {
                encryptedIdentificationNumber = Uri.UnescapeDataString(encryptedIdentificationNumber);
                // Verificar si encryptedIdentificationNumber es una cadena Base64 válida
                byte[] encryptedBytes;
                try
                {
                    encryptedBytes = Convert.FromBase64String(encryptedIdentificationNumber);
                }
                catch (FormatException)
                {
                    return BadRequest("La cadena proporcionada no es una cadena Base64 válida.");
                }

                var IdentificationNumber = CryptoHelper.Decrypt(encryptedIdentificationNumber);
                IdentificationNumber = StringHelper.EliminateFirstAndLast(IdentificationNumber);
                
                var (companies_UserCompanies, userNotFound) = await _unitOfWork.Companies.GetCompaniesByIdentificationNumber(IdentificationNumber);

                if (userNotFound)
                {
                    return NotFound("No se encontraron compañías para el número de documento proporcionado.");
                }

                var userCompaniesJson = JsonConvert.SerializeObject(companies_UserCompanies);
                var encryptedData = CryptoHelper.Encrypt(userCompaniesJson);

                return Ok(encryptedData);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message} - StackTrace: {ex.StackTrace}");
            }
        }

        [HttpDelete("Delete/{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                var user = _unitOfWork.Users.GetById(id);

                if (user == null)
                {
                    return NotFound(Json("El user con el Id especificado no existe o no se pudo eliminar."));
                }

                _unitOfWork.Users.Delete(user);
                _unitOfWork.Complete();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, Json($"Error interno del servidor: {ex.Message}"));
            }
        }

        [HttpPut("Update/{id}")]
        public IActionResult Update(int id, [FromBody] User updatedUser)
        {
            try
            {
                if (updatedUser == null || updatedUser.IdUser != id)
                {
                    return BadRequest(Json("Datos inválidos del user"));
                }

                var existingUser = _unitOfWork.Users.GetById(id);

                if (existingUser == null)
                {
                    return NotFound(Json("User no encontrado"));
                }

                existingUser.IdentificationNumber = updatedUser.IdentificationNumber;
                existingUser.Surnames = updatedUser.Surnames;             

                _unitOfWork.Users.Update(existingUser);
                _unitOfWork.Complete();

                return Ok(Json(existingUser));
            }
            catch (Exception ex)
            {
                return StatusCode(500, Json($"Error interno del servidor: {ex.Message}"));
            }
        }

        [HttpGet("GetById/{id}")]
        public IActionResult GetById(int id)
        {
            try
            {
                var user = _unitOfWork.Users.GetById(id);

                if (user == null)
                {
                    return NotFound(Json("User no encontrado"));
                }
                return Ok(Json(user));
            }
            catch (Exception ex)
            {
                return StatusCode(500, Json($"Error interno del servidor: {ex.Message}"));
            }
        }

        [HttpGet("GetAll")]
        public IActionResult GetAll()
        {
            try
            {
                var users = _unitOfWork.Users.GetAll();
                return Ok(Json(users));
            }
            catch (Exception ex)
            {
                return StatusCode(500, Json($"Error interno del servidor: {ex.Message}"));
            }
        }

        [HttpPost("Add")]
        public IActionResult Add([FromBody] User user)
        {
            try
            {
                if (user == null)
                {
                    return BadRequest(Json("Datos inválidos del user"));
                }

                // Consultar el último ID usado para la tabla User
                var lastIdRecord = _unitOfWork.LastIds.GetBigger("MT_Users");

                if (lastIdRecord == null)
                {
                    return StatusCode(500, Json("No se encontró un registro de Last (id) para la tabla User"));
                }

                int newUserId = lastIdRecord.Last + 1;
                user.IdUser = newUserId;

                var nuevoUser = _unitOfWork.Users.Add(user);
                _unitOfWork.Complete();

                // Actualizar el modelo LastId con el nuevo ID
                lastIdRecord.Last = newUserId;
                _unitOfWork.LastIds.Update(lastIdRecord);
                _unitOfWork.Complete();

                return Ok(Json(nuevoUser));
            }
            catch (Exception ex)
            {
                return StatusCode(500, Json($"Error interno del servidor: {ex.Message}"));
            }
        }


    }
}
