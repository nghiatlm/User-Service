
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.BO.DTO;
using UserService.BO.Enums;
using UserService.Service;

namespace UserService.API.Controllers
{
    [ApiController]
    [Route("api/v1/users")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [Authorize(Roles = "ROLE_ADMIN, ROLE_USER")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _userService.GetById(id);
            return Ok(ApiResponse<UserResponse>.SuccessResponse(result, "Get user information successfully"));
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
                                                [FromQuery] string? query,
                                                [FromQuery] RoleName? roleName,
                                                [FromQuery] Status? status,
                                                [FromQuery] int pageNumber = 1,
                                                [FromQuery] int pageSize = 10
                                                )
        {
            var result = await _userService.GetAll(query, roleName, status, pageNumber, pageSize);
            return Ok(ApiResponse<PagedResult<UserResponse>>.SuccessResponse(result, "Get user list successfully"));
        }

        [Authorize(Roles = "ROLE_ADMIN")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] UserRequest request)
        {
            var result = await _userService.Create(request);
            return Ok(ApiResponse<bool>.SuccessResponse(result, "User created successfully"));
        }

        [Authorize(Roles = "ROLE_ADMIN")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _userService.Delete(id);
            return Ok(ApiResponse<bool>.SuccessResponse(result, "Delete user successfully"));
        }


        [Authorize(Roles = "ROLE_ADMIN, ROLE_USER")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UserUpdateRequest request)
        {
            var result = await _userService.Update(id, request);
            return Ok(ApiResponse<bool>.SuccessResponse(result, "User update successful"));
        }
    }
}