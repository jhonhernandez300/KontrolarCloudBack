using Azure.Core;
using EF.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace KontrolarCloud.Middlewares
{
    public class TokenValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;

        public TokenValidationMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var endpoint = context.GetEndpoint();
            if (endpoint != null)
            {
                var allowAnonymous = endpoint.Metadata.GetMetadata<IAllowAnonymous>() != null;

                if (!allowAnonymous)
                {
                    var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

                    if (token != null)
                    {
                        var isValidToken = ValidateToken(token);

                        if (!isValidToken)
                        {
                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            await context.Response.WriteAsync("Invalid Token");
                            return;
                        }
                    }
                    else
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        await context.Response.WriteAsync("Token is missing");
                        return;
                    }
                }
            }

            await _next(context);
        }

        private bool ValidateToken(string token)
        {
            if (string.IsNullOrEmpty(token)) 
            { 
                return false;
            }

            var cleaned = token.Replace("\"", "");
            // Verificar si es cadena Base64 válida
            byte[] encryptedUserBytes;
            try
            {
                encryptedUserBytes = Convert.FromBase64String(cleaned);
            }
            catch (FormatException)
            {
                return false;
            }

            var decryptedParam = CryptoHelper.Decrypt(cleaned);
            var deserialized = JsonConvert.DeserializeObject(decryptedParam);

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);

            try
            {
                tokenHandler.ValidateToken(deserialized.ToString(), new TokenValidationParameters
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

                return true;
            }
            catch
            {
                return false;
            }
        }
    }

}
