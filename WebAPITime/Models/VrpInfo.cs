using System;
using System.Collections.Generic;
using System.Text;

namespace VrpModel
{
    public class VrpInfo
    {
        public string StatusCode { get; set; }
        public string Status { get; set; }        
        public string ErrorMessage { get; set; }
        public bool isAdHocFeasible { get; set; }
        public double Objective { get; set; }
        public string Logs { get; set; }
        public List<DroppedNodes> DroppedNodes { get; set; }
        public double TotalDistance { get; set; }
        public double TotalWeight { get; set; }
        public double TotalVolume { get; set; }
        public Vehicle[] Vehicles { get; set; }
    }
    public class Vehicle
    {
        public int VehicleNo { get; set; }
        public string VehicleColor { get; set; }
        public Dictionary<int, string> Zones { get; set; }
        public int WeightCapacity { get; set; }
        public int VolumeCapacity { get; set; }              
        public double TotalWeightLoad { get; set; }
        public double TotalVolumeLoad { get; set; }
        public double TotalDistance { get; set; }
        public double TotalTime { get; set; }
        public string Route { get; set; }
        public Node[] Nodes { get; set; }
    }

    public class Node
    {
        public int NodeID { get; set; }
        public string Type { get; set; }
        public List<long> PickupIDs { get; set; }
        public List<long> DeliveryIDs { get; set; }
        public List<long> PickupFromIDs { get; set; }
        public List<int> FeatureIDs { get; set; }
        public List<long> Accessories { get; set; }
        public string Address { get; set; }
        public string PostalCode { get; set; }
        public string UnitNo { get; set; }
        public string Zone { get; set; }
        public double DistanceOfRoute { get; set; }
        public long TimeOfRoute { get; set; }
        public double Weight { get; set; }
        public double Volume { get; set; }       
        public int ServiceDuration { get; set; }
        public int LoadDuration { get; set; }
        public int UnloadDuration { get; set; }
        public int WaitingDuration { get; set; }
        public string ArrivalTime { get; set; }
        public string DepartureTime { get; set; }
        public string Status { get; set; }
    }
    public class DroppedNodes
    {
        public long NodeID { get; set; }
        public string Address { get; set; }
        public Dictionary<string, string> Reasons { get; set; }
    }

    public class VrpAvailableTimeInfo
    {
        public string RouteNo { get; set; }
        public long DriverID { get; set; }
        public int IsOvertime { get; set; }
        public List<AvailableTime> AvailableTime { get; set; }
    }

    public class AvailableTime
    {
        public DateTime TimeStart { get; set; }
        public DateTime TimeEnd { get; set; }
        public string Duration { get; set; }

    }
}
