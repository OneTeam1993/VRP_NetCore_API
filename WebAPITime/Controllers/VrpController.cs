﻿using System.Collections.Generic;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VrpModel;
using WebAPITime.Repositories;

namespace WebAPI.Controllers
{
    [EnableCors("AllowAll")]
    [ApiController]
    [Route("[controller]")]
    public class VrpController : ControllerBase
    {
        private static readonly IVrpRepository vrpRepository = new VrpRepository();
        private static readonly IInitialLocationRepository initLocationRepository = new InitialLocationRepository();
        private readonly ILogger<VrpController> _logger;

        public VrpController(ILogger<VrpController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<VrpInfo> Get(string routeNo, string companyName, string userName, string roleID)
        {
            return vrpRepository.GetAll(routeNo, companyName, userName, roleID);
        }
    }
}
