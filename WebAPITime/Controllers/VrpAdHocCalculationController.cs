using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using VrpModel;
using WebAPITime.Repositories;

namespace WebAPITime.Controllers
{
    [EnableCors("AllowAll")]
    [Route("api/[controller]")]
    [ApiController]
    public class VrpAdHocCalculationController : ControllerBase
    {
        private static readonly IVrpRepository vrpRepository = new VrpRepository();

        [HttpPost]
        public IEnumerable<VrpInfo> CheckAdhocFeasibility(string routeNo, long driverID, string pickupID, string deliveryID)
        {
            return vrpRepository.InsertAdHocOrder(routeNo, driverID, pickupID, deliveryID);
        }
    }
}
