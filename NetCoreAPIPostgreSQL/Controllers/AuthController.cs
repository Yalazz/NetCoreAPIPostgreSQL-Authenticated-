using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using NetCoreAPIPostgreSQL.Data.Repositories;
using NetCoreAPIPostgreSQL.Model.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace NetCoreAPIPostgreSQL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IMemoryCache _cache;
        private readonly string _jwtKey;
        private readonly string _jwtIssuer;
        private readonly string _jwtAudience;

        public AuthController(IUserRepository userRepository, IConfiguration configuration, IMemoryCache cache)
        {
            _userRepository = userRepository;
            _cache = cache;
            _jwtKey = configuration["Jwt:Key"];
            _jwtIssuer = configuration["Jwt:Issuer"];
            _jwtAudience = configuration["Jwt:Audience"];
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserDto userDto)
        {
            if (userDto == null || string.IsNullOrEmpty(userDto.Username) || string.IsNullOrEmpty(userDto.Password))
                return BadRequest("Invalid user data.");

            if (await _userRepository.UserExists(userDto.Username))
                return Conflict("User already exists.");

            var user = new User { Username = userDto.Username, Password = userDto.Password };
            var createdUser = await _userRepository.Register(user);

            // Cache'yi temizle veya güncelle
            _cache.Remove($"User_{userDto.Username}");

            return Ok(createdUser);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserDto userDto)
        {
            if (userDto == null || string.IsNullOrEmpty(userDto.Username) || string.IsNullOrEmpty(userDto.Password))
                return BadRequest("Invalid login data.");

            User user;

            // Önbellekten kullanıcıyı al
            if (!_cache.TryGetValue($"User_{userDto.Username}", out user))
            {
                // Eğer önbellekte değilse, veritabanından al
                user = await _userRepository.Authenticate(userDto.Username, userDto.Password);

                if (user == null)
                    return Unauthorized("Invalid credentials.");

                // Önbelleğe ekle
                _cache.Set($"User_{userDto.Username}", user, TimeSpan.FromMinutes(30));
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.Username)
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                Issuer = _jwtIssuer,
                Audience = _jwtAudience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return Ok(new { Token = tokenString });
        }
    }
}
