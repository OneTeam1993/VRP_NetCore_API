using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using WebAPITime.Models;
using WebAPITime.Repositories;

namespace WebAPITime.Controllers
{
    [EnableCors("AllowAll")]
    [Route("api/[controller]")]
    [ApiController]
    public class VrpLocationRequestsController : ControllerBase
    {
        private static readonly IVrpLocationRequestsRepository repoVrpLocationRequest = new VrpLocationRequestsRepository();

        [HttpGet]
        public VrpLocationRequestResponse Get(string companyID, string mode, string dateStart, string dateEnd)
        {
            return repoVrpLocationRequest.Get(companyID, mode, dateStart, dateEnd);
        }
    }
}
