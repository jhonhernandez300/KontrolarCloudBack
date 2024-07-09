using Core;
using Core.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace KontrolarCloud.Controllers
{
    [Route("api/[controller]")]
    [EnableCors("AllowOrigins")]
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
            try
            {
                var company = _unitOfWork.Companies.GetById(id);
                if (company == null)
                {
                    return NotFound(Json("El company con el Id especificado no existe o no se pudo eliminar."));
                }

                _unitOfWork.Companies.Delete(company);
                _unitOfWork.Complete();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, Json($"Error interno del servidor: {ex.Message}"));
            }
        }

        [HttpPut("Update/{id}")]
        public IActionResult Update(int id, [FromBody] Company updatedCompany)
        {
            try
            {
                if (updatedCompany == null || updatedCompany.IdCompany != id)
                {
                    return BadRequest(Json("Datos inválidos de la company"));
                }

                var existingCompany = _unitOfWork.Companies.GetById(id);

                if (existingCompany == null)
                {
                    return NotFound(Json("Company no encontrada"));
                }

                existingCompany.CompanyName = updatedCompany.CompanyName;
                existingCompany.DB = updatedCompany.DB;
                existingCompany.UserName = updatedCompany.UserName;
                existingCompany.CompanyPassword = updatedCompany.CompanyPassword;
                existingCompany.LicenseValidDate = updatedCompany.LicenseValidDate;
                existingCompany.ConectionsSimultaneousNumber = updatedCompany.ConectionsSimultaneousNumber;

                _unitOfWork.Companies.Update(existingCompany);
                _unitOfWork.Complete();

                return Ok(Json(existingCompany));
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
                var company = _unitOfWork.Companies.GetById(id);
                if (company == null)
                {
                    return NotFound(Json("Company no encontrada"));
                }
                return Ok(Json(company));
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
                var companies = _unitOfWork.Companies.GetAll();
                return Ok(Json(companies));
            }
            catch (Exception ex)
            {
                return StatusCode(500, Json($"Error interno del servidor: {ex.Message}"));
            }
        }

        [HttpPost("Add")]
        public IActionResult Add([FromBody] Company company)
        {
            try
            {
                if (company == null)
                {
                    return BadRequest("Datos inválidos de la company");
                }

                // Obtener el último ID usado para la tabla 'Company'
                var lastIdRecord = _unitOfWork.LastIds.GetBigger("MT_Companies");
                                                    

                if (lastIdRecord == null)
                {
                    return NotFound("No se encontró ningún registro de LastIds para la tabla 'Company'.");
                }

                // Generar el nuevo ID
                int newId = lastIdRecord.Last + 1;

                // Asignar el nuevo ID a la entidad Company
                company.IdCompany = newId;

                // Guardar la nueva entidad Company
                var nuevaCompany = _unitOfWork.Companies.Add(company);
                _unitOfWork.Complete();

                // Actualizar el registro en LastIds con el nuevo ID
                lastIdRecord.Last = newId;
                _unitOfWork.LastIds.Update(lastIdRecord);
                _unitOfWork.Complete();

                return Ok(nuevaCompany);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }
    }
}
