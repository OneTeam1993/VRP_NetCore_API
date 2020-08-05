﻿using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using WebApi.Repositories;
using WebAPITime.Models;
using WebAPITime.Repositories;

namespace WebAPITime.Controllers
{
    [EnableCors("AllowAll")]
    [Route("api/[controller]")]
    [ApiController]
    public class VrpTimelineRoutesController : ControllerBase
    {
        private static readonly IRouteInfoRepository repoRouteInfo = new RouteInfoRepository();

        [HttpGet]
        public ResponseTimelineRoutes Get(string routeNo, string flag)
        {
            return repoRouteInfo.GetTimelineRoutes(routeNo, flag);
        }
    }
}
