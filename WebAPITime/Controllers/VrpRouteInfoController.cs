using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using WebAPITime.Repositories;
using WebAPITime.Models;

namespace WebAPITime.Controllers
{
    [EnableCors("AllowAll")]
    [Route("api/[controller]")]
    [ApiController]
    public class VrpRouteInfoController : ControllerBase
    {
        private static readonly IRouteInfoRepository repoRouteInfo = new RouteInfoRepository();
        private static readonly IEventRepository repoEvent = new EventRepository();

        [HttpGet]
        public IEnumerable<RouteInfo> Get(string companyID, string driverID, string flag, DateTime timeWindowStart, DateTime timeWindowEnd)
        {
            return repoRouteInfo.GetAllRouteInfoByDriver(companyID, driverID, flag, timeWindowStart, timeWindowEnd);
        }

        [HttpPost]
        public bool PostSaveRoute(string routeNo, string companyID, string companyName, string userName, string roleID)
        {
            string eventLog = String.Format("RouteNo: {0} Action: Save Route", routeNo);
            bool isSuccess = repoRouteInfo.SaveRoutes(routeNo);
            
            if (!isSuccess)
            {
                eventLog += " Error Message: Failed to save route";
            }

            repoEvent.LogVrpEvent(companyID, companyName, userName, roleID, eventLog);

            return isSuccess;
        }

        [HttpPut]
        public bool PutUpdateRoute(long id, [FromBody]RouteInfo currRoute)
        {
            currRoute.RouteID = id;

            return repoRouteInfo.Update(currRoute);
        }

        [HttpDelete]
        public ResponseRouteInfoDeletion DeleteRoute(long routeID, bool isRecalculation, string companyID, string companyName, string userName, string roleID)
        {           
            return repoRouteInfo.Remove(routeID, isRecalculation, companyID, companyName, userName, roleID);
        }
    }
}