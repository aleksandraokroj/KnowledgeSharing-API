using KnowledgeSharing.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;


namespace KnowledgeSharing.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        public IConfiguration _configuration;
        private readonly KnowledgeSharingContext _context;
        public UserInfo user;
        

        public TokenController(IConfiguration config, KnowledgeSharingContext context)
        {
            _configuration = config;
            _context = context;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Post(UserInfo _userData)
        {

            if (_userData != null && _userData.Email != null && _userData.Password != null)
            {
                this.user = await GetUser(_userData.Email, _userData.Password);

                if (this.user != null)
                {
                    //create claims details based on the user information
                    var claims = new[] {
                    new Claim(JwtRegisteredClaimNames.Sub, _configuration["Jwt:Subject"]),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()),
                    new Claim("Id", this.user.UserId.ToString()),
                    new Claim("FirstName", this.user.FirstName),
                    new Claim("LastName", this.user.LastName),
                    new Claim("UserName", this.user.UserName),
                    new Claim("Email", this.user.Email)
                   };

                    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));

                    var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                    var token = new JwtSecurityToken(_configuration["Jwt:Issuer"], _configuration["Jwt:Audience"], claims, expires: DateTime.UtcNow.AddDays(1), signingCredentials: signIn);

                    return Ok(new JwtSecurityTokenHandler().WriteToken(token));
                }
                else
                {
                    throw new ArgumentException("Invalid credentials! Status: " + HttpStatusCode.NotFound);
                }
            }
            else
            {
                throw new ArgumentException("Please fill in all the fields to log in. Status: " + HttpStatusCode.BadRequest);
            }
        }

        public async Task<UserInfo> GetUser(string email, string password)
        {
            return await _context.UserInfo.FirstOrDefaultAsync(u => u.Email == email && u.Password == password);
        }
        [HttpPost("registration")]
        public async Task<ActionResult<UserInfo>> PostUser(UserInfo userInfo)
        {
            if (userInfo != null)
            {
                _context.UserInfo.Add(userInfo);
                await _context.SaveChangesAsync();

                return CreatedAtAction("GetUser", new { email = userInfo.Email, password = userInfo.Password }, userInfo);
            }

            else
            {
                throw new ArgumentException("Please fill in all the fields to log in. Status: " + HttpStatusCode.BadRequest);
            }
        }
    }

}

