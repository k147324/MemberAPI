using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using prjMemberAPI.Models;
using prjMemberAPI.Services;
using System.Runtime.CompilerServices;
using System.Security.Claims;
namespace prjMemberAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserAPIController : ControllerBase
    {
        private readonly tempdbContext _context;
        private readonly TokenServices _token;
        public UserAPIController(tempdbContext c,TokenServices t)
        {
            _context = c;
            _token= t;
        }
        [HttpPost("Register")]
        public async Task<IActionResult> Register(UserRegisterDTO u)
        {
            UserServices us = new UserServices(_context);
            ArgonServices ag = new ArgonServices();
            if (await us.IsUsernameExists(u.fUsername))
            {
                return BadRequest(new
                {
                    message = "Username already exists"
                });
            }
            if (await us.IsEmailExists(u.fEmail))
            {
                return BadRequest(new
                {
                    message = "Email already exists"
                });
            }
            string password = await ag.HashPassword(u.fPassword);
            TUser user=await us.AddUser(u, password);
            var token = await _token.CreateTokenAsync(
            user.FId, "EmailVerify", TimeSpan.FromHours(24));
            var verifyUrl = Url.Action("VerifyEmail", "Account", new { token }, Request.Scheme);
            return Ok(new
            {
                message = $"            {u.fEmail},{verifyUrl}"
            });
        }
        [HttpPost("Login")]
        public async Task<IActionResult> Login(UserInfoDTO u)
        {
            
            UserServices us = new UserServices(_context);
            ArgonServices ag = new ArgonServices();
            if (string.IsNullOrEmpty(u.UserName) && string.IsNullOrEmpty(u.Email))
            {
                return BadRequest(new
                {
                    message = "Username or email is required"
                });
            }
            TUser? user = null;
            if (string.IsNullOrEmpty(u.Email))
            {
                if (!await us.IsUsernameExists(u.UserName))
                {
                    return BadRequest(new { message = "Username or email is not exists" });
                }

                string pass = await us.GetPasswordByUsername(u.UserName);
                if (!await ag.VerifyPassword(u.Password, pass))
                {
                    return BadRequest(new { message = "Something went wrong,please try again" });
                }

                user = await us.GetUserByUsername(u.UserName);
            }
            else if (string.IsNullOrEmpty(u.UserName))
            {
                if (!await us.IsEmailExists(u.Email))
                {
                    return BadRequest(new { message = "Username or email is not exists" });
                }

                string pass = await us.GetPasswordByEmail(u.Email);
                if (!await ag.VerifyPassword(u.Password, pass))
                {
                    return BadRequest(new { message = "Something went wrong,please try again" });
                }

                user = await us.GetUserByEmail(u.Email);
            }
            if (user == null)
            {
                return BadRequest(new { message = "Username or email is not exists" });
            }
            if(user.FIsActive == false)
            {
                return BadRequest(new { message = "Account is not active" });
            }
            var token = _token.GenerateToken(user);
            return Ok(new
            {
                message = "Login success",
                token = token
            });
        }
        [Authorize]
        [HttpGet("Test")]
        public IActionResult Test()
        {

            return Ok("驗證通過才看得到這個訊息");
        }
     }
}