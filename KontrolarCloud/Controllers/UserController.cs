using Microsoft.AspNetCore.Mvc;
using Core;
using Core.DTOs;
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
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Authorization;


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

        [HttpPut("Update")]
        public IActionResult Update([FromBody] string encryptedUserDto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(encryptedUserDto))
                {
                    return CreateErrorResponse.BadRequestResponse(
                        code: "Null or white space",
                        message: "encryptedUserDto is null or white space",
                        parameters: new List<string> { "encryptedUserDto" },
                        detail: "Check encryptedUserDto value"
                    );
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
                    return CreateErrorResponse.BadRequestResponse(
                      code: "Base64",
                      message: "Parameter is not base 64",
                      parameters: new List<string> { "encryptedUserDto" },
                      detail: "Check encryptedUserDto format"
                  );
                }

                var decryptedParam = CryptoHelper.Decrypt(encryptedUserDto);
                var deserialized = JsonConvert.DeserializeObject<UserDTO>(decryptedParam);
                var user = _mapper.Map<User>(deserialized);

                var existingUser = _unitOfWork.Users.GetById(user.IdUser);

                if (existingUser == null)
                {
                    return CreateErrorResponse.NotFoundResponse(
                     code: "Not Found",
                     message: "existingUser is not found",
                     parameters: new List<string> { "existingUser" },
                     detail: "Check existingUser value"
                    );
                }

                existingUser.IdentificationNumber = user.IdentificationNumber;
                existingUser.Names = user.Names;
                existingUser.Surnames = user.Surnames;

                _unitOfWork.Users.Update(existingUser);
                _unitOfWork.Complete();

                return CreateErrorResponse.OKResponse(
                     code: "Success",
                     message: "Successful operation",
                     parameters: new List<string> { "User Update" },
                     detail: "User successfully updated"
                );
            }
            catch (Exception ex)
            {
                return CreateErrorResponse.InternalServerErrorResponse(
                     code: "Internal Server Error",
                     message: ex.Message,
                     parameters: new List<string> { "existingUser" },
                     detail: "Check existingUser updated"
                );
            }
        }

        [HttpDelete("DeleteUser")]
        public async Task<IActionResult> DeleteUser([FromBody] string encryptedUserDto) 
        {
            try
            {
                if (encryptedUserDto == null)
                {
                    return CreateErrorResponse.BadRequestResponse(
                       code: "Not Found",
                       message: "encryptedUserDto not found",
                       parameters: new List<string> { "encryptedUserDto" },
                       detail: "Check encryptedUserDto value"
                     );
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
                    return CreateErrorResponse.BadRequestResponse(
                      code: "Base64",
                      message: "Parameter is not base 64",
                      parameters: new List<string> { "encryptedUserDto" },
                      detail: "Check encryptedUserDto format"
                    );
                }

                var decryptedParam = CryptoHelper.Decrypt(encryptedUserDto);
                var deserialized = JsonConvert.DeserializeObject<UserDTO>(decryptedParam);
                var user = _mapper.Map<User>(deserialized);

                var exists = _unitOfWork.Users.GetById(user.IdUser);

                if (exists == null)
                {
                    //return NotFound(Json("No encontrado"));
                    return CreateErrorResponse.NotFoundResponse(
                    code: "Not Found",
                    message: "existingUser is not found",
                    parameters: new List<string> { "existingUser" },
                    detail: "Check existingUser value"
                   );
                }
                
                //No funciona con el user q viene en la petición, por eso se puso el q él encuentra
                _unitOfWork.Users.Delete(exists);
                var result = await _unitOfWork.CompleteAsync();

                if (result > 0)
                {
                    return CreateErrorResponse.OKResponse(
                     code: "Success",
                     message: "Successful operation",
                     parameters: new List<string> { "User Deleted" },
                     detail: "User successfully deleted"
                );
                }
                else {
                    return CreateErrorResponse.InternalServerErrorResponse(
                         code: "Internal Server Error",
                         message: "Error at deleting user",
                         parameters: new List<string> { "exists" },
                         detail: "Check exists value"
                    );
                }                    
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && sqlEx.Number == 547)
            {
                // Código de error SQL 547 indica una violación de la restricción de clave externa
                Console.WriteLine($"Error de integridad referencial: {sqlEx.Message}");
                return CreateErrorResponse.ConflictErrorResponse(
                         code: "Related",
                         message: "Foreign key violation",
                         parameters: new List<string> { "exists" },
                         detail: "Check exists value"
                    );
            }
            catch (Exception ex)
            {
                return CreateErrorResponse.InternalServerErrorResponse(
                         code: "Internal Server Error",
                         message: "Error at deleting user",
                         parameters: new List<string> { "exists" },
                         detail: "Check exists value"
                    );
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
                    return CreateErrorResponse.BadRequestResponse(
                      code: "Base64",
                      message: "Parameter is not base 64",
                      parameters: new List<string> { "encryptedUserDto" },
                      detail: "Check encryptedUserDto format"
                    );
                }

                var decryptedParam = CryptoHelper.Decrypt(encryptedParam);
                var userList = await _unitOfWork.Users.GetUsersByParam(decryptedParam.Replace("\"", ""));
                return CreateErrorResponse.OKResponse(
                     code: "Success",
                     message: "Successful operation",
                     parameters: new List<string> { "decryptedParam" },
                     detail: "User obtained"
                );
            }
            catch (Exception ex)
            {
                return CreateErrorResponse.InternalServerErrorResponse(
                     code: "Internal Server Error",
                     message: ex.Message,
                     parameters: new List<string> { "decryptedParam" },
                     detail: "Check decryptedParam value"
                );
            }
        }

        [HttpPost("AddAsync")]
        public async Task<IActionResult> AddAsync([FromBody] string encryptedUser)
        {
            try
            {
                if (encryptedUser == null)
                {
                    return CreateErrorResponse.BadRequestResponse(
                        code: "Null or white space",
                        message: "encryptedUser is null or white space",
                        parameters: new List<string> { "encryptedUser" },
                        detail: "Check encryptedUser value"
                    );
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
                    return CreateErrorResponse.BadRequestResponse(
                       code: "Base64",
                       message: "Parameter is not base 64",
                       parameters: new List<string> { "encryptedUser" },
                       detail: "Check encryptedUser format"
                    );
                }

                var decryptedUser = CryptoHelper.Decrypt(encryptedUser);
                var deserialized = JsonConvert.DeserializeObject<UserDTO>(decryptedUser);
                var user = _mapper.Map<Core.Models.User>(deserialized);

                // Verificar si el IdentificationNumber ya existe en la base de datos
                var existingUser = await _unitOfWork.Users.FindAsync(u => u.IdentificationNumber == user.IdentificationNumber);
                if (existingUser != null)
                {
                    return CreateErrorResponse.InternalServerErrorResponse(
                         code: "Internal Server Error",
                         message: "IdentificationNumber alreay exists",
                         parameters: new List<string> { "existingUser" },
                         detail: "Check existingUser value"
                    );
                }

                // Consultar el último ID usado para la tabla User
                var lastIdRecord = _unitOfWork.LastIdsKTRL1.GetBigger("MT_Users");

                if (lastIdRecord == null)
                {
                    return CreateErrorResponse.InternalServerErrorResponse(
                         code: "Internal Server Error",
                         message: "lastIdRecord not found",
                         parameters: new List<string> { "lastIdRecord" },
                         detail: "Check lastIdRecord value"
                    );
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

                return CreateErrorResponse.OKResponse(
                     code: "Success",
                     message: "Successful operation",
                     parameters: new List<string> { "user Updated" },
                     detail: "User Added"
                );
            }
            catch (Exception ex)
            {
                return CreateErrorResponse.InternalServerErrorResponse(
                     code: "Internal Server Error",
                     message: ex.Message,
                     parameters: new List<string> { "user" },
                     detail: "Check user value"
                );
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
                    return CreateErrorResponse.BadRequestResponse(
                      code: "Base64",
                      message: "Parameter is not base 64",
                      parameters: new List<string> { "encryptedIdUser" },
                      detail: "Check encryptedIdUser format"
                    );
                }

                var decryptedIdUser = CryptoHelper.Decrypt(encryptedIdUser);
                var trimmedIdUser = StringHelper.EliminateFirstAndLast(decryptedIdUser);
                int idUser = Convert.ToInt32(trimmedIdUser);                

                var (modules, message, operationExecuted) = await _unitOfWork.Users.ProfileGetOptions(idUser);
                
                if (!operationExecuted)
                {
                    return CreateErrorResponse.NotFoundResponse(
                     code: "Not Found",
                     message: "idUser is not found",
                     parameters: new List<string> { "idUser" },
                     detail: "Check idUser value"
                    );
                }

                var moduleOptionsJson = JsonConvert.SerializeObject(modules);
                var encryptedData = CryptoHelper.Encrypt(moduleOptionsJson);

                return CreateErrorResponse.OKResponse(
                     code: "Success",
                     message: "Successful operation",
                     parameters: new List<string> { "idUser" },
                     detail: "Options obtained"
                );
            }
            catch (Exception ex)
            {
                return CreateErrorResponse.InternalServerErrorResponse(
                     code: "Internal Server Error",
                     message: ex.Message,
                     parameters: new List<string> { "idUser" },
                     detail: "Check idUser value"
                );
            }
        }

        [HttpPost("CreateToken")]
        [AllowAnonymous]
        public async Task<IActionResult> CreateToken([FromBody] LoginRequestDTOs loginRequestDTO)
        {
            try
            {
                if(loginRequestDTO == null 
                    || string.IsNullOrWhiteSpace(loginRequestDTO.IdentificationNumber) 
                    || string.IsNullOrWhiteSpace(loginRequestDTO.Company)
                    || string.IsNullOrWhiteSpace(loginRequestDTO.AccessKey)
                )
                {
                    return CreateErrorResponse.BadRequestResponse(
                        code: "Null or white space",
                        message: "loginRequestDTO has null or white space values",
                        parameters: new List<string> { "loginRequestDTO" },
                        detail: "Check loginRequestDTO values"
                    );
                }
                if (!loginRequestDTO.IsKontrolarCloud && loginRequestDTO.IsKontrolarCloud != false)
                {
                    return CreateErrorResponse.BadRequestResponse(
                        code: "Boolean Required",
                        message: "A boolean value is required",
                        parameters: new List<string> { "loginRequestDTO.IsKontrolarCloud" },
                        detail: "Check loginRequestDTO.IsKontrolarCloud value"
                    );
                }

                 string idCompany, identificationNumber;
                
                if (loginRequestDTO.IsKontrolarCloud == true)
                {                  
                    (idCompany, identificationNumber) = DecryptAndCleanLoginData(loginRequestDTO);
                    var accessKey = CryptoHelper.Decrypt(loginRequestDTO.AccessKey);

                    if (!await IsAuthorizedUser(idCompany, identificationNumber, accessKey))
                    {
                        return CreateErrorResponse.UnauthorizedErrorResponse(
                            code: "Unauthorized",
                            message: "Unauthorized with this parameters",
                            parameters: new List<string> { "idCompany, identificationNumber, accessKey" },
                            detail: "Check idCompany, identificationNumber, accessKey values"
                        );
                    }
                }
                else                
                {
                    var accessKey = CryptoHelper.Decrypt(loginRequestDTO.AccessKey);

                    if (string.IsNullOrWhiteSpace(accessKey))                    
                    {
                        return CreateErrorResponse.BadRequestResponse(
                            code: "Null or white space",
                            message: "loginRequestDTO is null or white space",
                            parameters: new List<string> { "accessKey" },
                            detail: "Check accessKey value"
                        );
                    }
                    (idCompany, identificationNumber) = ExtractLoginData(loginRequestDTO);

                    if (!await IsAuthorizedCompany(idCompany, identificationNumber, loginRequestDTO, accessKey))
                    {
                        return CreateErrorResponse.UnauthorizedErrorResponse(
                            code: "Unauthorized",
                            message: "Unauthorized with this parameters",
                            parameters: new List<string> { "idCompany, identificationNumber, loginRequestDTO, accessKey" },
                            detail: "Check idCompany, identificationNumber, loginRequestDTO, accessKey values"
                        );
                    }
                }

                var token = GenerateJwtToken(idCompany, identificationNumber);
                var encryptedToken = EncryptToken(token);

                return CreateErrorResponse.OKResponse(
                     code: "Success",
                     message: "Successful operation",
                     parameters: new List<string> { "token" },
                     detail: "Token generated"
                );
            }
            catch (Exception ex)
            {
                return CreateErrorResponse.InternalServerErrorResponse(
                     code: "Internal Server Error",
                     message: ex.Message,
                     parameters: new List<string> { "idCompany, identificationNumber, loginRequestDTO, accessKey" },
                     detail: "Check idCompany, identificationNumber, loginRequestDTO, accessKey values"
                );
            }
        }

        private (string idCompany, string identificationNumber) DecryptAndCleanLoginData(LoginRequestDTOs loginRequestDTO)
        {
            string identificationNumber = CryptoHelper.Decrypt(loginRequestDTO.IdentificationNumber);
            identificationNumber = StringHelper.EliminateFirstAndLast(identificationNumber);

            string idCompany = CryptoHelper.Decrypt(loginRequestDTO.Company);
            idCompany = StringHelper.EliminateFirstAndLast(idCompany);

            return (idCompany, identificationNumber);
        }

        private async Task<bool> IsAuthorizedUser(string idCompany, string identificationNumber, string encryptedPassword)
        {
            var password = StringHelper.EliminateFirstAndLast(CryptoHelper.Decrypt(encryptedPassword));
            var (companies_UserCompanies, _, _) = await _unitOfWork.Companies.GetCompaniesByIdentificationNumber(identificationNumber);

            return companies_UserCompanies.Any(x => x.IdCompany == Convert.ToInt32(idCompany) && x.Password == password);
        }

        private (string idCompany, string identificationNumber) ExtractLoginData(LoginRequestDTOs loginRequestDTO)
        {
            return (loginRequestDTO.Company, loginRequestDTO.IdentificationNumber);
        }

        private async Task<bool> IsAuthorizedCompany(string idCompany, string identificationNumber, LoginRequestDTOs loginRequestDTO, string key)
        {
            var (companies_UserCompanies, _, _) = await _unitOfWork.Companies.GetCompaniesByIdentificationNumber(identificationNumber);
            if (!companies_UserCompanies.Any(x => x.IdCompany == Convert.ToInt32(idCompany)))
            {
                return false;
            }

            var company = await _unitOfWork.Companies.GetByIdAsync(Convert.ToInt32(idCompany));
            if (company == null || company.AcessKey != key || !company.ApisActive || company.LicenseValidDate < DateTime.Now)
            {
                return false;
            }

            return true;
        }

        private string GenerateJwtToken(string idCompany, string identificationNumber)
        {
            var jwt = _configuration.GetSection("Jwt").Get<Jwt>();

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, jwt.Subject),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()),
                new Claim("idCompany", idCompany),
                new Claim("IdentificationNumber", identificationNumber)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key));
            var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                jwt.Issuer,
                jwt.Audience,
                claims,
                expires: DateTime.Now.AddMinutes(20),
                signingCredentials: signingCredentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string EncryptToken(string token)
        {
            var tokenJson = JsonConvert.SerializeObject(token);
            return CryptoHelper.Encrypt(tokenJson);
        }

        [HttpGet("GetCompaniesByIdentificationNumber/{encryptedIdentificationNumber}")]
        [AllowAnonymous]
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
                    return CreateErrorResponse.BadRequestResponse(
                         code: "Base64",
                         message: "Parameter is not base 64",
                         parameters: new List<string> { "encryptedIdentificationNumber" },
                         detail: "Check encryptedIdentificationNumber format"
                     );
                }

                var IdentificationNumber = CryptoHelper.Decrypt(encryptedIdentificationNumber);
                IdentificationNumber = StringHelper.EliminateFirstAndLast(IdentificationNumber);               

                var (companies_UserCompanies, operationExecuted, message) = await _unitOfWork.Companies.GetCompaniesByIdentificationNumber(IdentificationNumber);

                if (!operationExecuted)
                {
                    return CreateErrorResponse.NotFoundResponse(
                     code: "Not Found",
                     message: "Company not found",
                     parameters: new List<string> { "existingUser" },
                     detail: "Check existingUser value"
                    );
                }

                companies_UserCompanies.ForEach(x => x.Password = "");
                var userCompaniesJson = JsonConvert.SerializeObject(companies_UserCompanies);
                var encryptedData = CryptoHelper.Encrypt(userCompaniesJson);

                return CreateErrorResponse.OKResponse(
                     code: "Success",
                     message: "Successful operation",
                     parameters: new List<string> { "IdentificationNumber" },
                     detail: "Companies obtained for the identification number"
                );
            }
            catch (Exception ex)
            {
                return CreateErrorResponse.InternalServerErrorResponse(
                     code: "Internal Server Error",
                     message: ex.Message,
                     parameters: new List<string> { "IdentificationNumber" },
                     detail: "Check IdentificationNumber value"
                );
            }
        }       
    }
}