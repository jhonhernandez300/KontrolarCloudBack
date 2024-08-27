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
using AutoMapper;
using KontrolarCloud.DTOs;
using Microsoft.Data.SqlClient;


namespace KontrolarCloud.Controllers
{
    [Route("api/[controller]")]
    [EnableCors("AllowOrigins")]
    [ApiController]
    public class UserController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        public IConfiguration _configuration;
        private ApplicationDbContext _context;

        public UserController(IConfiguration configuration, IUnitOfWork unitOfWork, IMapper mapper, ApplicationDbContext context)
        {
            _configuration = configuration;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _context = context;
        }

        public bool IsValidToken(string token)
        {
            //if (string.IsNullOrEmpty(encryptedToken)) { return false; }

            try
            {
                //encryptedToken = Uri.UnescapeDataString(encryptedToken);

                // Verificar si es cadena Base64 válida
                //byte[] encryptedUserBytes;
                //try
                //{
                //    encryptedUserBytes = Convert.FromBase64String(encryptedToken);
                //}
                //catch (FormatException)
                //{
                //    return false;
                //}

                //string decrypted = CryptoHelper.Decrypt(encryptedToken);
                //string deserialized = (string)JsonConvert.DeserializeObject(decrypted);

                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);

                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _configuration["Jwt:Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var identity = new ClaimsIdentity(jwtToken.Claims);
                bool valid = Jwt.CheckToken(identity, _context);

                if (!valid)
                {
                    return false;
                }

                var iatClaim = identity.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Iat);
                if (iatClaim != null)
                {
                    var tokenCreationTime = DateTime.Parse(iatClaim.Value);
                    if (DateTime.UtcNow > tokenCreationTime.AddMinutes(20))
                    {
                        return false;
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }


        [HttpPut("Update")]
        public IActionResult Update([FromBody] string encryptedUserDto)        
        {
            try
            {
                var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

                var tokenValid = IsValidToken(token);

                if (encryptedUserDto == null)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "El usuario encriptado es nulo"
                    });
                }

                encryptedUserDto = Uri.UnescapeDataString(encryptedUserDto);

                // Verificar si es cadena Base64 válida
                byte[] encryptedUserBytes;
                try
                {
                    encryptedUserBytes = Convert.FromBase64String(encryptedUserDto);
                }
                catch (FormatException)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "No es valida en Base64"
                    });
                }

                var decryptedParam = CryptoHelper.Decrypt(encryptedUserDto);
                var deserialized = JsonConvert.DeserializeObject<UserDTO>(decryptedParam);
                var user = _mapper.Map<User>(deserialized);

                var existingUser = _unitOfWork.Users.GetById(user.IdUser);

                if (existingUser == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "No encontrado"
                    });                    
                }

                existingUser.IdentificationNumber = user.IdentificationNumber;
                existingUser.Names = user.Names;
                existingUser.Surnames = user.Surnames;                

                _unitOfWork.Users.Update(existingUser);
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
                
        [HttpDelete("DeleteUser")]
        public async Task<IActionResult> DeleteUser([FromBody] string encryptedUserDto) 
        {
            try
            {
                if (encryptedUserDto == null)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "El usuario encriptado es nulo"
                    });
                }

                encryptedUserDto = Uri.UnescapeDataString(encryptedUserDto);

                // Verificar si es cadena Base64 válida
                byte[] encryptedUserBytes;
                try
                {
                    encryptedUserBytes = Convert.FromBase64String(encryptedUserDto);
                }
                catch (FormatException)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "No es valida en Base64"
                    });
                }

                var decryptedParam = CryptoHelper.Decrypt(encryptedUserDto);
                var deserialized = JsonConvert.DeserializeObject<UserDTO>(decryptedParam);
                var user = _mapper.Map<User>(deserialized);

                var exists = _unitOfWork.Users.GetById(user.IdUser);

                if (exists == null)
                {
                    //return NotFound(Json("No encontrado"));
                    return NotFound(new
                    {
                        success = false,
                        message = "No encontrado"
                    });
                }

                //user.IdUser = 63;
                //user.IdUser = (int)user.IdUser;
                //No funciona con el user q viene en la petición, por eso se puso el q él encuentra
                _unitOfWork.Users.Delete(exists);
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


        [HttpGet("GetUserByParam/{encryptedParam}")]
        public async Task<IActionResult> GetUserByParam(string encryptedParam)
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
                var userList = await _unitOfWork.Users.GetUsersByParam(decryptedParam.Replace("\"", ""));
                return Ok(userList);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message} - StackTrace: {ex.StackTrace}");
            }
        }

        [HttpPost("AddAsync")]
        public async Task<IActionResult> AddAsync([FromBody] string encryptedUser)
        {
            try
            {
                if (encryptedUser == null)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "El usuario encriptado es nulo"
                    });
                }

                encryptedUser = Uri.UnescapeDataString(encryptedUser);

                // Verificar si encryptedUser es una cadena Base64 válida
                byte[] encryptedUserBytes;
                try
                {
                    encryptedUserBytes = Convert.FromBase64String(encryptedUser);
                }
                catch (FormatException)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "No es valida en Base64"
                    });
                }

                var decryptedUser = CryptoHelper.Decrypt(encryptedUser);
                var deserialized = JsonConvert.DeserializeObject<UserDTO>(decryptedUser);
                var user = _mapper.Map<Core.Models.User>(deserialized);

                // Verificar si el IdentificationNumber ya existe en la base de datos
                var existingUser = await _unitOfWork.Users.FindAsync(u => u.IdentificationNumber == user.IdentificationNumber);
                if (existingUser != null)
                {
                    return StatusCode(500, new
                    {
                        success = false,
                        message = "Ya existe ese número de identificación"
                    });
                }

                // Consultar el último ID usado para la tabla User
                var lastIdRecord = _unitOfWork.LastIdsKTRL1.GetBigger("MT_Users");

                if (lastIdRecord == null)
                {
                    return StatusCode(500, new
                    {
                        success = false,
                        message = "Last (id) no encontrado"
                    });
                }

                long newUserId = lastIdRecord.Last + 1; // Cambiado a long
                user.IdUser = (int)newUserId; // Convertir a int si es necesario
                user.UserMaster = false;

                var nuevoUser = _unitOfWork.Users.Add(user);
                _unitOfWork.Complete();

                // Actualizar el modelo LastId con el nuevo ID
                lastIdRecord.Last = newUserId;
                _unitOfWork.LastIdsKTRL1.Update(lastIdRecord);
                _unitOfWork.Complete();

                return Ok(new
                {
                    success = true,
                    message = "Registro agregado con exito"
                }); 
            }
            catch (Exception ex)
            {
                return StatusCode(500, Json($"Error interno del servidor {ex.Message}"));
            }
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
                    return BadRequest("No es valida en Base64");
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

                byte[] bytesToEncode = Encoding.UTF8.GetBytes(response);
                string encodedString = Convert.ToBase64String(bytesToEncode);

                var tokenJson = JsonConvert.SerializeObject(response);
                var encryptedToken = CryptoHelper.Encrypt(tokenJson);

                return tokenJson;
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
                    return BadRequest("No es valida en Base64");
                }

                var IdentificationNumber = CryptoHelper.Decrypt(encryptedIdentificationNumber);
                IdentificationNumber = StringHelper.EliminateFirstAndLast(IdentificationNumber);               

                var (companies_UserCompanies, operationExecuted, message) = await _unitOfWork.Companies.GetCompaniesByIdentificationNumber(IdentificationNumber);

                if (!operationExecuted)
                {
                    return NotFound(message);
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
                    return NotFound(Json("No encontrado"));
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

        [HttpGet("GetById/{id}")]
        public IActionResult GetById(int id)
        {
            try
            {
                var user = _unitOfWork.Users.GetById(id);

                if (user == null)
                {
                    return NotFound(Json("No encontrado"));
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

    }
}
