using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Models;
using System.Security.Claims;
//using Microsoft.AspNetCore.Mvc;
//using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Http;
//using Fluent.Infrastructure.FluentModel;
using Microsoft.AspNetCore.Http;
using EF;
using EF.Models;

namespace EF.Utils
{
    public class Security
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor; 

        public Security(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public dynamic GetToken()
        {
            var identity = _httpContextAccessor.HttpContext.User.Identity as ClaimsIdentity; 
            // Accede a HttpContext a través del HttpContextAccessor

            return Jwt.validarToken(identity, _context);
        }

        //public string ObtenerRol(dynamic token)
        //{
        //    Usuario usuario = token.result;
        //    return usuario.Rol != "Administrador" ? "Comprador" : "Administrador";
        //}
    }
}
