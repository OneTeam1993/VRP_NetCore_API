using System;
using System.Collections.Generic;

namespace WebAPITime.Models
{
    public class VrpSettingInfo
    {
        public long VrpSettingsId { get; set; }
        public string RouteNo { get; set; }
        public int AssetID { get; set; }
        public long DriverID { get; set; }
        public string DriverName { get; set; }
        public DateTime TimeStamp { get; set; }
        public DateTime RxTime { get; set; }
        public double StartLatitude { get; set; }
        public double StartLongitude { get; set; }
        public string StartAddress { get; set; }
        public double EndLatitude { get; set; }
        public double EndLongitude { get; set; }
        public string EndAddress { get; set; }
        public int WeightCapacity { get; set; }
        public int VolumeCapacity { get; set; }
        public int DistanceCapacity { get; set; }
        public DateTime TimeWindowStart { get; set; }
        public DateTime TimeWindowEnd { get; set; }
        public DateTime BreakTimeStart { get; set; }
        public DateTime BreakTimeEnd { get; set; }
        public List<int> Features { get; set; }
        public Dictionary<int, string> Zones { get; set; }
        public int IsOvertime { get; set; }
        public int CompanyID { get; set; }
    }
}
