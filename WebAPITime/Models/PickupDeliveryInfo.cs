using System;
using System.Collections.Generic;

namespace WebAPITime.Models
{
    public class PickupDeliveryInfo
    {
        //public long PickupDeliveryID { get; set; }
        public string OrderType { get; set; }
        public List<long> PickupIDs { get; set; }
        public List<long> DeliveryIDs { get; set; }
        public string RouteNo { get; set; }
        public int PriorityID { get; set; }
        public long DriverID { get; set; }
        public string DriverName{ get; set; }
        public double Lat { get; set; }
        public double Long { get; set; }
        public string Address { get; set; }
        public string PostalCode { get; set; }
        public string UnitNo { get; set; }
        public double TotalWeight { get; set; }
        public double TotalVolume { get; set; }
        public int ServiceDuration { get; set; }
        public int LoadDuration { get; set; }
        public int UnloadDuration { get; set; }
        public int WaitingDuration { get; set; }
        public int Node { get; set; }
        public List<long> PickupFromIDs { get; set; }
        public List<int> FeatureIDs { get; set; }
        public List<long> Accessories { get; set; }
        public DateTime TimeWindowStart { get; set; }
        public DateTime TimeWindowEnd { get; set; }
    }

    public class TempAdHocLocation
    {
        public string Address { get; set; }
        public string PostalCode { get; set; }
        public double Lat { get; set; }
        public double Long { get; set; }
        public DateTime TimeWindowStart { get; set; }
        public DateTime TimeWindowEnd { get; set; }
    }
}
