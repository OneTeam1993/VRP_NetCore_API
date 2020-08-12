using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using WebAPITime.Repositories;
using WebAPITime.Models;

namespace WebAPITime.Controllers
{
    [EnableCors("AllowAll")]
    [Route("api/[controller]")]
    [ApiController]
    public class VrpCustomerOrdersController : ControllerBase
    {
        private static readonly IRouteInfoRepository repoRouteInfo = new RouteInfoRepository();

        [HttpGet]
        public CustomerOrder Get(long RouteID, int CustomerID)
        {
            return repoRouteInfo.GetCustomerOrder(RouteID, CustomerID);
        }
    }
}
