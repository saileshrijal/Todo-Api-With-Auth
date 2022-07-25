using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Models.Dtos.Requests;
using Models.Dtos.Responses;

namespace Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthManagementController : ControllerBase
    {
        private UserManager<IdentityUser> _userManager;
        private JwtConfig _jwtConfig;
        public AuthManagementController(UserManager<IdentityUser> userManager,
                                        IOptionsMonitor<JwtConfig> optionsMonitor)
        {
            _userManager = userManager;
            _jwtConfig = optionsMonitor.CurrentValue;
        }

        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register([FromBody] UserRegistrationDto user)
        {
            if (ModelState.IsValid)
            {
                var existingUser = await _userManager.FindByEmailAsync(user.Email);
                if (existingUser != null)
                {
                    return BadRequest(new RegistrationResponse()
                    {
                        Errors = new List<string>(){
                            "Email already in use!",
                        },
                        Success = false
                    });
                }
                var newUser = new IdentityUser()
                {
                    Email = user.Email,
                    UserName = user.Username
                };

                var result = await _userManager.CreateAsync(newUser, user.Password);
                if (result.Succeeded)
                {
                    var jwtToken = GenerateJwtToken(newUser);
                    return Ok(new RegistrationResponse()
                    {
                        Success = true,
                        Token = jwtToken
                    });
                }
                else
                {
                    return BadRequest(new RegistrationResponse()
                    {
                        Errors = result.Errors.Select(x => x.Description).ToList(),
                        Success = false
                    });
                }
            }
            return BadRequest(new RegistrationResponse()
            {
                Errors = new List<string>(){
                    "Invalid Payload",
                },
                Success = false
            });
        }

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody] UserLoginRequest user)
        {
            if (ModelState.IsValid)
            {
                var existingUser = await _userManager.FindByEmailAsync(user.Email);
                if (existingUser == null)
                {
                    return BadRequest(new RegistrationResponse()
                    {
                        Errors = new List<string>(){
                    "Invalid Login Request",
                },
                        Success = false
                    });
                }
                var isCorrect = await _userManager.CheckPasswordAsync(existingUser, user.Password);
                if (!isCorrect)
                {
                    return BadRequest(new RegistrationResponse()
                    {
                        Errors = new List<string>(){
                    "Invalid Login Request",
                },
                        Success = false
                    });
                }
                var jwtToken = GenerateJwtToken(existingUser);
                return Ok(new RegistrationResponse()
                {
                    Success = true,
                    Token = jwtToken
                });
            }
            return BadRequest(new RegistrationResponse()
            {
                Errors = new List<string>(){
                    "Invalid Payload",
                },
                Success = false
            });
        }

        private string GenerateJwtToken(IdentityUser user)
        {
            var jwtTokeHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtConfig.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]{
                    new Claim("Id",user.Id),
                    new Claim(JwtRegisteredClaimNames.Email,user.Email),
                    new Claim(JwtRegisteredClaimNames.Sub,user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()),
                }),
                Expires = DateTime.UtcNow.AddHours(6),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
            };

            var token = jwtTokeHandler.CreateToken(tokenDescriptor);
            var jwtToken = jwtTokeHandler.WriteToken(token);
            return jwtToken;
        }
    }
}