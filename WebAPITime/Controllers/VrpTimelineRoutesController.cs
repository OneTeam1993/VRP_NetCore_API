using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using WebAPITime.Repositories;
using WebAPITime.Models;

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
