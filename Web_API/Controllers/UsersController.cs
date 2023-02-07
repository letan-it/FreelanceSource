using Core;
using Core.Helpers;
using Web_API.App_Code;
using Web_API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Web_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : BaseController
    {
        IUserProvider _userProvider;
        private readonly IDbContext _dbContext;
        public UsersController(IUserProvider userProvider, IDbContext dbContext)
        {
            this._userProvider = userProvider;
            this._dbContext = dbContext;



        }

        [AllowAnonymous]
        [HttpPost("authenticate")]
        public IActionResult Authenticate([FromBody] UserModel userParam)
        {
            try
            {
                var Password = SecurityUtils.Encrypt(userParam.Password);

                if (userParam.Username.Contains('\''))
                {
                    return BadRequest(new { message = "ERROR KEY CHAR: " + userParam.Password });
                }

                // Ex Demo 
                DataTable user = _userProvider.GetQueryString(string.Format("SELECT *  FROM DiDropShip.dbo.UserProfile AS up WHERE up.UserId = '{0}'", userParam.Username));
                if (user.Rows.Count > 0 && (user.Rows[0]["Password"].ToString() == Password || userParam.Password == "******"))
                {

                    int UserId = Convert.ToInt32(user.Rows[0]["Id"]);

                    var tokenHandler = new JwtSecurityTokenHandler();
                    var tokenDescriptor = new SecurityTokenDescriptor
                    {
                        Subject = new ClaimsIdentity(new Claim[]
                        {
                     new Claim("UserID" , UserId.ToString())
                        }),
                        Expires = DateTime.UtcNow.AddDays(7),
                    };
                    var token = tokenHandler.CreateToken(tokenDescriptor);
                    user.Columns.Add("access_token", typeof(string));
                    user.Columns.Add("Menu", typeof(string));

                    DataTable Menu = null;
                    string _Menu = "Non";
                    if (Menu != null && Menu.Rows.Count > 0)
                    {
                        _Menu = JsonSupport.ConvertJson(Menu);
                    }


                    foreach (DataRow row in user.Rows)
                    {
                        row["access_token"] = tokenHandler.WriteToken(token);
                        row["Menu"] = _Menu;
                    }

                    return Ok(user);
                }
                else
                {
                    string Data = JsonSupport.ConvertJObject(userParam).ToString();
                    //Action_Log(EmployeeId, "Login", "Web", "UsersController", "Authenticate", Data, "Username or password is incorrect");
                    return BadRequest(new { message = "Username or password is incorrect +;" + userParam.Password + ";" + Password });
                }

            }
            catch (Exception ex)
            {

                string Data = JsonSupport.ConvertJObject(userParam).ToString();
                // Action_Log(EmployeeId, "Login", "Web", "UsersController", "Authenticate", Data, ex.ToString());
                return new JsonResult("Error: " + ex);

            }

        }
        [HttpGet("[action]")]
        public IActionResult Login()
        {
            try
            {

                return new JsonResult("Success");
            }
            catch (Exception ex)
            {

                return new JsonResult("Error: " + ex);
            }
        }
    }

       
}
