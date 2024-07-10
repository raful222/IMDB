using IMDB.Services;
using IMDB.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace IMDB.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ResponseService _responseService;

        public TokenController(IConfiguration configuration,
            ResponseService responseService)
        {
            _configuration = configuration;
            _responseService = responseService;
        }

        [HttpPost]
        public IActionResult CreateToken([FromBody] LoginViewModel login)
        {
            if (!IsValidUser(login.Username, login.Password))
            {
               var resoponse = _responseService.MakeResponse(401, "username = IMDB pass=password", "");
                return Unauthorized(resoponse);
            }

            var token = GenerateToken(login.Username);
            return Ok(new { Token = token });
        }

        private string GenerateToken(string username)
        {
            var secretKey = _configuration["JwtSettings:SecretKey"];
            var issuer = _configuration["JwtSettings:Issuer"];
            var audience = _configuration["JwtSettings:Audience"];

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(20), 
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private bool IsValidUser(string username, string password)
        {
            return username == "IMDB" && password == "password";
        }

      
    }
}
