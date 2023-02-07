using Core;
using Web_API.App_Code;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Web_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DI_WarehouseV2Controller : BaseController
    {
        IDI_WarehouseV2Provider _WarehouseV2Provider;
        private readonly IDbContext _dbContext;
        public DI_WarehouseV2Controller(IDI_WarehouseV2Provider WarehouseV2Provider, IDbContext dbContext)
        {
            this._WarehouseV2Provider = WarehouseV2Provider;
            this._dbContext = dbContext;
        }



        [HttpGet("[action]")]
        [SecretKey]
        public IActionResult Dim_Date()
        {
            try
            {
                DataTable data = _WarehouseV2Provider.GetQueryString("SELECT * FROM Dim_Date AS vf");
                return Ok(data);
            }
            catch (Exception ex)
            {
                //string Data = JsonSupport.ConvertJObject(f).ToString();
                // _S.Action_Log(Dim_Date, "Dim_Date", "WebAPI", "DI_WarehouseV2Controller", "Dim_Date", Data, ex.ToString());
                return new JsonResult("Error: " + ex);
            }
        }

   

   

        [HttpGet("[action]")]
        [SecretKey]
        public IActionResult Dim_Customer()
        {
            try
            {
                DataTable data = _WarehouseV2Provider.GetQueryString("SELECT * FROM Dim_Customer AS vf");
                return Ok(data);
            }
            catch (Exception ex)
            {
                //string Data = JsonSupport.ConvertJObject(f).ToString();
                // _S.Action_Log(Dim_Customer, "Dim_Customer", "WebAPI", "DI_WarehouseV2Controller", "Dim_Customer", Data, ex.ToString());
                return new JsonResult("Error: " + ex);
            }
        }

        [HttpGet("[action]")]
        public IActionResult Dim_DataType()
        {
            try
            {
                DataTable data = _WarehouseV2Provider.GetQueryString("SELECT * FROM Dim_DataType AS vf");
                int User = UserLogin;
                return Ok(data);
            }
            catch (Exception ex)
            {
                //string Data = JsonSupport.ConvertJObject(f).ToString();
                // _S.Action_Log(Dim_DataType, "Dim_DataType", "WebAPI", "DI_WarehouseV2Controller", "Dim_DataType", Data, ex.ToString());
                return new JsonResult("Error: " + ex);
            }
        }

 


        [HttpGet("[action]")]
        public IActionResult vFact_850()
        {
            try
            {
                DataTable data = _WarehouseV2Provider.GetQueryString("SELECT * FROM vFact_850 AS vf");
                return Ok(data);
            }
            catch (Exception ex)
            {
                //string Data = JsonSupport.ConvertJObject(f).ToString();
                // _S.Action_Log(vFact_850, "vFact_850", "WebAPI", "DI_WarehouseV2Controller", "vFact_850", Data, ex.ToString());
                return new JsonResult("Error: " + ex);
            }
        }



    }
}
