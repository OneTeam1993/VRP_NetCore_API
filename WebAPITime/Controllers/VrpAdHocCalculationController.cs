using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        public async Task<IEnumerable<VrpInfo>> CheckAdhocFeasibilityAsync(string routeNo, long driverID, string pickupID, string deliveryID, string companyName, string userName, string roleID)
        {
            return await vrpRepository.InsertAdHocOrderAsync(routeNo, driverID, pickupID, deliveryID, companyName, userName, roleID);
        }
    }
}
