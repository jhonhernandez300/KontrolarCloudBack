using Core;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace KontrolarCloud.Controllers
{
    [Route("api/[controller]")]
    [EnableCors("AllowOrigins")]
    [ApiController]
    public class UserCompanyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserCompanyController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet("GetByCompanyId/{idCompany}")]
        public async Task<IActionResult> GetByCompanyId(int idCompany)
        {
            try
            {
                var users = await _unitOfWork.UsersCompanies.GetByCompanyId(idCompany);

                if (users == null || users.Count == 0)
                {
                    return NotFound("No se encontraron Users para esa Company");
                }
                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("GetByUserId/{idUser}")]
        public async Task<IActionResult> GetByUserId(int idUser)
        {
            try
            {
                var companies = await _unitOfWork.UsersCompanies.GetByUserId(idUser);

                if (companies == null || companies.Count == 0)
                {
                    return NotFound("No se encontraron Companies para ese User");
                }
                return Ok(companies);
            }
            catch (Exception ex)
            {                
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
