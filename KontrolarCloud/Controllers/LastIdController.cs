using Core;
using Core.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace KontrolarCloud.Controllers
{
    [Route("api/[controller]")]
    [EnableCors("AllowOrigins")]
    [ApiController]
    public class LastIdController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public LastIdController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet("GetBigger/{tableName}")]
        public IActionResult GetBigger(string tableName)
        {
            try
            {
                if (string.IsNullOrEmpty(tableName))
                {
                    return BadRequest("El parámetro tableName es requerido.");
                }

                var lastId = _unitOfWork.LastIds.GetAll()
                                                .FirstOrDefault(l => l.TableName == tableName);

                if (lastId == null)
                {
                    return NotFound($"No se encontró ningún registro para la tabla '{tableName}'.");
                }

                return Ok(lastId);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }        

        [HttpPut("Update/{id}")]
        public IActionResult Update(int id, [FromBody] LastId updatedLastId)
        {
            try
            {
                if (updatedLastId == null || updatedLastId.IdLastIds != id)
                {
                    return BadRequest(Json("Datos inválidos del LastId"));
                }

                var existingLastId = _unitOfWork.LastIds.GetById(id);

                if (existingLastId == null)
                {
                    return NotFound(Json("User no encontrado"));
                }

                existingLastId.TableName = updatedLastId.TableName;
                existingLastId.Last = updatedLastId.Last;

                _unitOfWork.LastIds.Update(existingLastId);
                _unitOfWork.Complete();

                return Ok(Json(existingLastId));
            }
            catch (Exception ex)
            {
                return StatusCode(500, Json($"Error interno del servidor: {ex.Message}"));
            }
        }
    }
}
