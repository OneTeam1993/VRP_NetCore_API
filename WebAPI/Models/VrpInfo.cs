using System;
using System.Collections.Generic;
using System.Text;

namespace VrpModel
{
    public class VrpInfo
    {
        public double Objective { get; set; }
        public string Logs { get; set; }
        public List<DroppedNodes> DroppedNodes { get; set; }
        public List<VehicleInfo> Vehicle { get; set; }
        public double TotalDistance { get; set; }
        public double TotalLoad { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class VehicleInfo
    {       
        public int VehicleNo { get; set; }
        public string Routes { get; set; }
        public List<NodeInfo> Nodes { get; set; }
        public long DistanceofRoute { get; set; }
        public long LoadofRoute { get; set; }
    }
    public class NodeInfo
    {
        public long NodeID { get; set; }
        public string Address { get; set; }
    }

    public class DroppedNodes
    {
        public long NodeID { get; set; }
        public string Address { get; set; }
    }
}
