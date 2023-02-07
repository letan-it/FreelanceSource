using Web_API.Security.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace Web_API
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]

    public class SecretKeyAttribute : Attribute, IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if(context.HttpContext.Request.Headers.Authorization.Count == 0)
            {
                var apiErrorResult = new ApiErrorResult<string>("Token is not Invalid!");
                context.Result = new BadRequestObjectResult(apiErrorResult);
                return;
            }
            if (context.HttpContext.Request.Headers.TryGetValue("Authorization", out var _potentialtoken))
            {
                if (_potentialtoken.ToString()=="" || _potentialtoken.ToString() ==null)
                {
                    var apiErrorResult = new ApiErrorResult<string>("Token is not Invalid!");
                    context.Result = new BadRequestObjectResult(apiErrorResult);
                    return;
                }
                var MyConfig = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
                var Appid = "MyConfig:Appid";// MyConfig.GetValue<string>("MyConfig:Appid");
                var privateKey = "MyConfig:HealthMeasurementKey";// MyConfig.GetValue<string>("MyConfig:HealthMeasurementKey");
              //  var dataEncryptAES = EncryptDecryptAES.EncryptAES(Appid, privateKey);

                var dataTokenDecrypt = AES.DecryptAES(_potentialtoken, privateKey);
                if (dataTokenDecrypt != Appid || dataTokenDecrypt == null)
                {
                    var apiErrorResult = new  ApiErrorResult<string>("Token is not Invalid!");
                    context.Result = new BadRequestObjectResult(apiErrorResult);
                    return ;
                }
            }
            await next();
        }

    }
}
