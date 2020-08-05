using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using WebApi.Repositories;
using WebAPITime.Models;
using WebAPITime.Repositories;

namespace WebAPITime.Controllers
{
    [EnableCors("AllowAll")]
    [Route("api/[controller]")]
    [ApiController]
    public class VrpRouteInfoController : ControllerBase
    {
        private static readonly IRouteInfoRepository repoRouteInfo = new RouteInfoRepository();

        [HttpGet]
        public IEnumerable<RouteInfo> Get(string companyID, string driverID, string flag, DateTime timeWindowStart, DateTime timeWindowEnd)
        {
            return repoRouteInfo.GetAllRouteInfoByDriver(companyID, driverID, flag, timeWindowStart, timeWindowEnd);
        }

        [HttpPost]
        public bool PostSaveRoute(string routeNo)
        {
            return repoRouteInfo.SaveRoutes(routeNo);
        }

        [HttpPut]
        public bool PutUpdateRoute(long id, [FromBody]RouteInfo currRoute)
        {
            currRoute.RouteID = id;

            return repoRouteInfo.Update(currRoute);
        }

        [HttpDelete]
        public ResponseRouteInfoDeletion DeleteRoute(long routeID, bool isRecalculation)
        {           
            return repoRouteInfo.Remove(routeID, isRecalculation);
        }
    }
}