
using Web_API.Security.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Web_API.Security.Service
{
    public class UserService : IUserService
    {
        private readonly IConfiguration _config;

        public UserService(IConfiguration config)
        {
            _config = config;
        }
        public class SystemConstants
        {
            public const string MainConnectionString = "";
            public const string UserName = "admin";
            public const string NormalizedUserName = "admin";
            public const string Email = "helpdesk@tanle.com";

            public class AppSettings
            {
                public const string DefaultLanguageId = "DefaultLanguageId";
                public const string Token = "Token";
                public const string BaseAddress = "BaseAddress";
            }
        }
        public async Task<ApiResult<string>> Authencate(LoginRequest request)
        {
            try
            {
                var privateKey = "Tan";// _config[GlobalSettings.HealthMeasurementKey()];
                var Appid = "Tan"; //_config[GlobalSettings.HealthMeasurementKey()];
                var dataEncryptAES = AES.EncryptAES(Appid, privateKey);

                var dataTokenDecrypt = AES.DecryptAES(dataEncryptAES, privateKey);
                if (dataTokenDecrypt != Appid || dataTokenDecrypt == null)
                {
                    return new ApiErrorResult<string>("Incorrect login");
                }

                var claims = new[]
                {
                new Claim(ClaimTypes.Email,SystemConstants.Email),
                new Claim(ClaimTypes.GivenName,SystemConstants.UserName),
                new Claim(ClaimTypes.Name, SystemConstants.NormalizedUserName)
                };
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Tokens:Key"]));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(_config["Tokens:Issuer"],
                    _config["Tokens:Issuer"],
                    claims,
                    expires: DateTime.Now.AddHours(3),
                    signingCredentials: creds);

                return new ApiSuccessResult<string>(new JwtSecurityTokenHandler().WriteToken(token));
            }
            catch (Exception ex)
            {
                return new ApiErrorResult<string>("Incorrect login");
            }
        }
    }
}
