using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System;
using WebAPITime.Models;
using WebAPITime.Repositories;

namespace WebAPITime.Controllers
{
    [EnableCors("AllowAll")]
    [Route("api/[controller]")]
    [ApiController]
    public class VrpRouteReportController : ControllerBase
    {
        private static readonly IVrpRouteReportRepository vrpRouteReportRepo = new VrpRouteReportRepository();

        [HttpGet]
        public VrpRouteReportResponse GetVrpRouteReport(long routeID)
        {
            return vrpRouteReportRepo.GetRouteReport(routeID);
        }

        [HttpPost]
        public VrpRouteReportResponse UpdateVrpRouteReport(string routeNo, int driverID, long routeID, DateTime departureTime, DateTime arrivalTime, DateTime jobEndTime)
        {
            return vrpRouteReportRepo.UpdateVrpRouteReport(routeNo, driverID, routeID, departureTime, arrivalTime, jobEndTime);
        }
    }
}
