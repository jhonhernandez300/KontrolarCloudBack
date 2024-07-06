using Core;
using Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace KontrolarCloud.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CompanyController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpDelete("Delete/{id}")]
        public IActionResult Delete(int id)
        {
            var Receta = _unitOfWork.Companies.GetById(id);
            if (Receta == null)
            {
                return NotFound();
            }

            _unitOfWork.Companies.Delete(Receta);
            _unitOfWork.Complete();

            return NoContent();
        }

        [HttpPut("Update/{id}")]
        public IActionResult Update(int id, [FromBody] Company updatedCompany)
        {
            if (updatedCompany == null || updatedCompany.IdCompany != id)
            {
                return BadRequest("Datos inválidos de la company");
            }

            var existingCompany = _unitOfWork.Companies.GetById(id);

            if (existingCompany == null)
            {
                return NotFound("Receta no encontrada");
            }

            existingCompany.CompanyName = updatedCompany.CompanyName;
            existingCompany.DB = updatedCompany.DB;
            existingCompany.UserName = updatedCompany.UserName;
            existingCompany.Password = updatedCompany.Password;
            existingCompany.LicenseValidDate = updatedCompany.LicenseValidDate;
            existingCompany.NumberSimiltaneousConnection = updatedCompany.NumberSimiltaneousConnection;

            _unitOfWork.Companies.Update(existingCompany);
            _unitOfWork.Complete();

            return Ok(existingCompany);
        }


        [HttpGet]
        public IActionResult GetById()
        {
            return Ok(_unitOfWork.Companies.GetById(1));
        }

        [HttpGet("GetAll")]
        public IActionResult GetAll()
        {
            return Ok(_unitOfWork.Companies.GetAll());
        }

        [HttpPost("Add")]
        public IActionResult Add([FromBody] Company company)
        {
            if (company == null)
            {
                return BadRequest("Datos inválidos de la company");
            }

            var nuevaCompany = _unitOfWork.Companies.Add(company);
            _unitOfWork.Complete();
            return Ok(nuevaCompany);
        }
    }
}
