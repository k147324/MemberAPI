using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using prjMemberAPI.Models;
using prjMemberAPI.Services;
using System.Runtime.CompilerServices;
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
            TUser user = new TUser();
            user.FUsername = u.fUsername;
            user.FPassword = password;
            user.FEmail = u.fEmail;
            user.FPhone = u.fPhone;
            user.FAddress = u.fAddress;
            user.FIdNum = u.fIdNum;
            user.FCreateTime = DateTime.Now;
            user.FIsAdmin = false;
            _context.TUsers.Add(user);
            await _context.SaveChangesAsync();
            return Ok(new
            {
                message = $"Register success,username:{u.fUsername},password:{password},email:{u.fEmail}"
            });
        }
        [HttpPost("Login")]
        public async Task<IActionResult> Login(UserInfoDTO u)
        {
            
            UserServices us = new UserServices(_context);
            ArgonServices ag = new ArgonServices();
            if (u.UserName == null && u.Email == null)
            {
                return BadRequest(new
                {
                    message = "Username or email is required"
                });
            }
            TUser? user = null;
            if (u.Email == null)
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
            else if (u.UserName == null)
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
            var token = _token.GenerateToken(user);
            return Ok(new
            {
                message = "Login success",
                token = token
            });
        }
     }
}