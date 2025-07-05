using E_Commerce.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
namespace E_Commerce.Services
{
    public class JwtProvider : IJwtProvider
    {
        private readonly JwtOptions _options;
        public JwtProvider(IOptions<JwtOptions> options)
        {
            _options = options.Value;
        }
        public string Generate(User user, IList<string> roles)
        {

            var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name,user.UserName!),
                    new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                     new Claim("Id", user.Id.ToString()),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email!),
                };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var authSignKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key!));

            var credentials = new SigningCredentials(authSignKey, SecurityAlgorithms.HmacSha256);

            var Sectoken = new JwtSecurityToken(
              issuer: _options.Issure,
              audience: _options.Audience,
              claims,
              expires: DateTime.Now.AddDays(7),
              signingCredentials: credentials
              );
            var token = new JwtSecurityTokenHandler().WriteToken(Sectoken);
            return token;

        }
        public string GetUserIdFromToken(string token)
        {
            if (string.IsNullOrEmpty(token))
                return null;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_options.Key!);

            try
            {
                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = _options.Issure,
                    ValidAudience = _options.Audience,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var subClaim = principal.Claims.FirstOrDefault(c => c.Type =="Id");
                if (subClaim == null)
                    throw new Exception("User ID (sub claim) not found in token.");

                return subClaim.Value;


            }
            catch(Exception ex)
            {
                return ex.Message;
            }
        }

    }
}
