using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UserService.BO.DTO;

namespace UserService.API.Extensions
{
    public static class ApiResponseExtensions
    {
        public static IActionResult ToActionResult<T>(this ControllerBase controller, ApiResponse<T>? response)
        {
            if (response == null)
            {
                var err = ApiResponse<object>.ServerError("Null response from service");
                return controller.StatusCode((int)HttpStatusCode.InternalServerError, err);
            }

            if (response.StatusCode == (int)HttpStatusCode.NoContent)
                return controller.NoContent();

            return controller.StatusCode((int)response.StatusCode, response);
        }
    }
}