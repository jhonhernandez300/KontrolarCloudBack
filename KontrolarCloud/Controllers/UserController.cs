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

        [HttpGet]  
        [Route("CreateToken/{encryptedDocumentNumber}/{encryptedCorreo}")]
        //public dynamic CreateToken()
        public dynamic CreateToken(string encryptedDocumentNumber, string encryptedIdUser)
        {
            var documentNumber = CryptoHelper.Decrypt(encryptedDocumentNumber);
            documentNumber = StringHelper.EliminateFirstAndLast(documentNumber);

            var idUser = CryptoHelper.Decrypt(encryptedIdUser.ToString());
            idUser = StringHelper.EliminateFirstAndLast(idUser);
            //var idUser = "1";
            //var documentNumber = "1234567890";

            try
            {
                var jwt = _configuration.GetSection("Jwt")
                    .Get<Jwt>();

                var claims = new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, jwt.Subject),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()),
                    new Claim("id", idUser),
                    new Claim("documentNumber", documentNumber)
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

                //var tokenJson = JsonConvert.SerializeObject(response);
                var encryptedToken = CryptoHelper.Encrypt(response);

                return new
                {
                    success = true,
                    message = "exito",
                    //result = new JwtSecurityTokenHandler().WriteToken(token)
                    result = encryptedToken
                };

            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        [HttpGet("GetCompaniesByDocumentNumber/{encryptedDocumentNumber}")]
        public async Task<IActionResult> GetCompaniesByDocumentNumber(string encryptedDocumentNumber)
        {
            try
            {
                encryptedDocumentNumber = Uri.UnescapeDataString(encryptedDocumentNumber);
                // Verificar si encryptedDocumentNumber es una cadena Base64 válida
                byte[] encryptedBytes;
                try
                {
                    encryptedBytes = Convert.FromBase64String(encryptedDocumentNumber);
                }
                catch (FormatException)
                {
                    return BadRequest("La cadena proporcionada no es una cadena Base64 válida.");
                }

                var documentNumber = CryptoHelper.Decrypt(encryptedDocumentNumber);
                documentNumber = StringHelper.EliminateFirstAndLast(documentNumber);
                var (companies, userNotFound) = await _unitOfWork.Companies.GetCompaniesByDocumentNumber(documentNumber);

                if (userNotFound)
                {
                    return NotFound("No se encontraron compañías para el número de documento proporcionado.");
                }

                var companiesJson = JsonConvert.SerializeObject(companies);
                var encryptedData = CryptoHelper.Encrypt(companiesJson);

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

                existingUser.DocumentNumber = updatedUser.DocumentNumber;
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
