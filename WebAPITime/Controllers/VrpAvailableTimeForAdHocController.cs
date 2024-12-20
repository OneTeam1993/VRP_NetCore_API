﻿using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using VrpModel;
using WebAPITime.Repositories;
using WebAPITime.Models;

namespace WebAPITime.Controllers
{
    [EnableCors("AllowAll")]
    [Route("api/[controller]")]
    [ApiController]
    public class VrpAvailableTimeForAdHocController : ControllerBase
    {
        private static readonly IVrpRepository vrpRepository = new VrpRepository();

        [HttpGet]
        public VrpAvailableTimeInfo Get(string routeNo, long driverID)
        {
            return vrpRepository.GetAvailableTimeForAdhocOrder(routeNo, driverID);
        }
    }
}
