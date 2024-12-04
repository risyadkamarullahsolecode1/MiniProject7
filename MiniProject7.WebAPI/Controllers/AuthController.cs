using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniProject7.Application.Dtos.Account;
using MiniProject7.Application.Interfaces;

namespace MiniProject7.WebAPI.Controllers
{
    [Route("api/[Controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterEmployee model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = await _authService.SignUpAsync(model);

            if (result.Status == "Error")
            {
                return BadRequest(result.Message);
            }

            return Ok(result);
        }
        // login
        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginModel model)
        {
            if (!ModelState.IsValid)

                return BadRequest(ModelState);

            var result = await _authService.LoginAsync(model);
            if (result.Status == "Error")

                return Unauthorized();

            SetRefreshTokenCookie("AuthToken", result.Token, result.ExpiredOn);
            SetRefreshTokenCookie("RefreshToken", result.RefreshToken, result.RefreshTokenExpiryTime);

            return Ok(result);
        }

        private void SetRefreshTokenCookie(string tokenType, string? token, DateTime? expires)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,  // Hanya dapat diakses oleh server
                Secure = false,    // Hanya dikirim melalui HTTPS
                SameSite = SameSiteMode.Strict, // Cegah serangan CSRF
                Expires = expires // Waktu kadaluarsa token
            };

            Response.Cookies.Append(tokenType, token, cookieOptions);
        }

        [HttpPut("update-user")]
        public async Task<IActionResult> UpdateUserAsync(string userName, [FromBody] RegisterModel model)
        {
            var result = await _authService.UpdateUserAsync(userName, model);
            return Ok(result);
        }

        [HttpDelete("Delete-User")]
        public async Task<IActionResult> DeleteAsync(string userName)
        {
            var result = await _authService.DeleteAsync(userName);
            return Ok(result);
        }

        [HttpPost("set-role")]
        public async Task<IActionResult> CreateRoleAsync(string rolename)
        {
            var result = await _authService.CreateRoleAsync(rolename);
            return Ok(result);
        }

        [HttpPost("assign-role")]
        public async Task<IActionResult> AssignToRoleAsync(string userName, [FromBody] string rolename)
        {
            var result = await _authService.AssignToRoleAsync(userName, rolename);
            return Ok(result);
        }

        // modify role name
        [HttpPut("modify-role")]
        public async Task<IActionResult> UpdateRoleAsync(string rolename)
        {
            var result = await _authService.UpdateRoleAsync(rolename);
            return Ok(result);
        }

        // delete the role
        [HttpDelete("delete-role")]
        public async Task<IActionResult> DeleteRole(string rolename)
        {
            var result = await _authService.DeleteRoleAsync(rolename);
            return Ok(result);
        }

        // update role from user 
        [HttpPut("update-user-role")]
        public async Task<IActionResult> UpdateRoleAsync(string userName, [FromBody] string rolename)
        {
            var result = await _authService.UpdateToRoleAsync(userName, rolename);
            return Ok(result);
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            try
            {
                // Hapus cookie
                Response.Cookies.Delete("AuthToken", new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict
                });

                // Hapus cookie
                /**Response.Cookies.Delete("RefreshToken", new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict
                });**/

                return Ok("Logout successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred during logout");
            }
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken(HttpContext httpContext)
        {
            var result = await _authService.RefreshAccessTokenAsync(httpContext);

            // Set new cookies
            Response.Cookies.Append("AuthToken", result.AccessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // Set to `false` in development
                SameSite = SameSiteMode.Lax,
                Expires = result.AccessTokenExpiryTime
            });

            Response.Cookies.Append("RefreshToken", result.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // Set to `false` in development
                SameSite = SameSiteMode.Lax,
                Expires = result.RefreshTokenExpiryTime
            });

            return Ok(result);
        }
    }
}
