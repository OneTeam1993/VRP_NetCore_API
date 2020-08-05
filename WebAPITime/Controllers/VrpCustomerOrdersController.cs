using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using WebApi.Repositories;
using WebAPITime.Models;
using WebAPITime.Repositories;

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
