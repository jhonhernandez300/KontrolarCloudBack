using Core.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EF.Utils
{
    public static class CreateErrorResponse
    {
        public static IActionResult BadRequestResponse(string code, string message, List<string> parameters, string detail, int status = 400)
        {
            var errorResponse = new ErrorResponse
            {
                Status = status,
                Results = new List<ErrorDetail>
                {
                    new ErrorDetail
                    {
                        Code = code,
                        Message = message,
                        Params = parameters,
                        Detail = detail
                    }
                }
            };

            return new ObjectResult(errorResponse) { StatusCode = status };
        }

        public static IActionResult NotFoundResponse(string code, string message, List<string> parameters, string detail, int status = 404)
        {
            var errorResponse = new ErrorResponse
            {
                Status = status,
                Results = new List<ErrorDetail>
                {
                    new ErrorDetail
                    {
                        Code = code,
                        Message = message,
                        Params = parameters,
                        Detail = detail
                    }
                }
            };

            return new ObjectResult(errorResponse) { StatusCode = status };
        }

        public static IActionResult OKResponse(string code, string message, object parameters, string detail, int status = 200)
        {
            var errorResponse = new ErrorResponse
            {
                Status = status,
                Results = new List<ErrorDetail>
                {
                    new ErrorDetail
                    {
                        Code = code,
                        Message = message,
                        Params = parameters,
                        Detail = detail
                    }
                }
            };

            return new ObjectResult(errorResponse) { StatusCode = status };
        }

        public static IActionResult InternalServerErrorResponse(string code, string message, List<string> parameters, string detail, int status = 500)
        {
            var errorResponse = new ErrorResponse
            {
                Status = status,
                Results = new List<ErrorDetail>
                {
                    new ErrorDetail
                    {
                        Code = code,
                        Message = message,
                        Params = parameters,
                        Detail = detail
                    }
                }
            };

            return new ObjectResult(errorResponse) { StatusCode = status };
        }

        public static IActionResult ConflictErrorResponse(string code, string message, List<string> parameters, string detail, int status = 409)
        {
            var errorResponse = new ErrorResponse
            {
                Status = status,
                Results = new List<ErrorDetail>
                {
                    new ErrorDetail
                    {
                        Code = code,
                        Message = message,
                        Params = parameters,
                        Detail = detail
                    }
                }
            };

            return new ObjectResult(errorResponse) { StatusCode = status };
        }

        public static IActionResult UnauthorizedErrorResponse(string code, string message, List<string> parameters, string detail, int status = 401)
        {
            var errorResponse = new ErrorResponse
            {
                Status = status,
                Results = new List<ErrorDetail>
                {
                    new ErrorDetail
                    {
                        Code = code,
                        Message = message,
                        Params = parameters,
                        Detail = detail
                    }
                }
            };

            return new ObjectResult(errorResponse) { StatusCode = status };
        }

    }
}
