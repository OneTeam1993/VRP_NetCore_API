using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using VrpModel;
using WebAPITime.Repositories;

namespace WebAPITime.Controllers
{
    [EnableCors("AllowAll")]
    [Route("api/[controller]")]
    [ApiController]
    public class VrpUpdateBreaktimeController : ControllerBase
    {
        private static readonly IVrpRepository vrpRepository = new VrpRepository();

        //[HttpPost]
        //public IEnumerable<VrpInfo> UpdateBreaktime(string routeNo, long driverID, DateTime breaktimeStart, DateTime breaktimeEnd, string companyName, string userName, string roleID)
        //{
        //    return ;
        //}
    }
}
