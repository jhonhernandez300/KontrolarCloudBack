using Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EF.Models
{
    public class Jwt
    {
        public required string Key { get; set; }
        public required string Issuer { get; set; }
        public required string Audience { get; set; }
        public required string Subject { get; set; }
                
        public static dynamic CheckToken(ClaimsIdentity identity, ApplicationDbContext context)
        {
            try
            {            
                if (identity.Claims.Count() == 0)
                {
                    return new
                    {
                        success = false,
                        message = "Token no válido",
                        result = ""
                    };
                }

                var id = identity.Claims.FirstOrDefault(x => x.Type == "id").Value;
                //Usuario usuario = Usuario.DB().FirstOrDefault(x => x.idUsuario == id);

                User company = context.Users
                    .Where(x => x.IdUser.ToString() == id)
                    .FirstOrDefault();

                return new
                {
                    success = true,
                    message = "exito",                    
                    result = company
                };
            }
            catch (Exception ex) 
            {
                return new
                {
                    success = false,
                    message = "Catch: " + ex.Message,
                    result = ""
                };
            }
        }
    }
}
