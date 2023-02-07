using Core.Helpers;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;

namespace Web_API.App_Code
{
    [ApiController]
    public class BaseController : ControllerBase
    {
        public int UserLogin
        {
            get
            {
                try
                {
                    string Check = Request.Headers["Authorization"];
                    if (!string.IsNullOrEmpty(Check))
                    {
                        var tokenHandler = new JwtSecurityTokenHandler();
                        dynamic C = tokenHandler.ReadToken(Check);
                        string uSer = JsonConvert.SerializeObject(C.Claims[0].Value);
                        int UserId = Convert.ToInt32(C.Claims[0].Value);// Convert.ToInt32(uSer.Replace(@"\", "").Replace("\"", ""));
                        return Convert.ToInt32(UserId);
                    }
                    else
                    {
                        try
                        {
                            string access_token = HttpContext.Request.Query["access_token"].ToString();
                            if (!string.IsNullOrEmpty(access_token))
                            {
                                var key = SecurityUtils.Decrypt(access_token, "Authorization", "Android");
                                string[] data = key.Split(new string[] { ";@;" }, StringSplitOptions.RemoveEmptyEntries);
                                return Convert.ToInt32(data[0]);

                            }
                            else
                            {
                                return 0;
                                
                            }
                        }
                        catch (Exception)
                        {
                            return 0;
                        }

                    }

                }
                catch (Exception ex)
                {

                    return 0;
                }
            }
        }




    }
}
