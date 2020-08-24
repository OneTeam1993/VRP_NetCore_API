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
    public class VrpDriverReportController : ControllerBase
    {
        private static readonly IDriverReportRepository driverReportRepo = new DriverReportRepository();

        [HttpGet]
        public DriverReportResponse GetDriverReport(long driverID, DateTime reportDate)
        {
            return driverReportRepo.GetDriverReport(driverID, reportDate);
        }

        [HttpPost]
        public DriverReportResponse UpdateDriverReport(long driverID, DateTime worktimeStart, DateTime worktimeEnd)
        {
            return driverReportRepo.UpdateDriverReport(driverID, worktimeStart, worktimeEnd);
        }
    }
}
