using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VrpModel;
using WebAPITime.Repositories;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class VrpController : ControllerBase
    {
        private static readonly IVrpRepository repository = new VrpRepository();
        private readonly ILogger<VrpController> _logger;

        public VrpController(ILogger<VrpController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<VrpInfo> Get()
        {
            return repository.GetAll();
        }
    }
}
