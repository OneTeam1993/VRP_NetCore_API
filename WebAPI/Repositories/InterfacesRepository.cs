using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VrpModel;

namespace WebApi.Repositories
{
    public interface IVrpRepository
    {
        IEnumerable<VrpInfo> GetAll();
    }
    public interface IInitialLocationRepository
    {
        InitialLocationInfo Get(long id);
        NodeInfo GetInitLoc(long id);
    }
}
