using Microsoft.AspNetCore.Mvc;
using Core;
using Core.Models;
using Microsoft.AspNetCore.Cors;

namespace KontrolarCloud.Controllers
{
    [Route("api/[controller]")]
    [EnableCors("AllowOrigins")]
    [ApiController]
    public class UserController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
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
