using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;


namespace E_Commerce.Services
{
    public class JwtBearerOptionsSetup : IConfigureOptions<JwtBearerOptions>
    {
        private readonly JwtOptions _jwtOptions;
        public JwtBearerOptionsSetup(IOptions<JwtOptions> jwtOptions)
        {
            _jwtOptions = jwtOptions.Value;
        }
        public void Configure(JwtBearerOptions options)
        {

            options.SaveToken = true;
            options.RequireHttpsMetadata = false;
            Console.WriteLine("JwtBearerOptionsSetup");
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    // Extract the token from the cookie
                    var token = context.HttpContext.Request.Cookies["jwtToken"];

                    if (!string.IsNullOrEmpty(token))
                    {
                        context.Token = token;
                    }

                    return Task.CompletedTask;
                }
            };
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidIssuer = _jwtOptions.Issure,
                ValidAudience = _jwtOptions.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Key!)),
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

            };
        }
    }
}
