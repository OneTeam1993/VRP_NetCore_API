using System;

namespace WebAPITime.Models
{
    public class VrpRouteReport
    {
        public long VrpRouteID { get; set; }
        public long FromVrpRouteID { get; set; }
        public string FromAddress { get; set; }
        public string ToAddress { get; set; }
        public long DriverID { get; set; }
        public DateTime EstDepartureTime { get; set; }
        public DateTime ActualDepartureTime { get; set; }
        public string DepartureTimeStatus { get; set; }
        public DateTime EstArrivalTime { get; set; }
        public DateTime ActualArrivalTime { get; set; }
        public string ArrivalTimeStatus { get; set; }
        public int TravelDuration { get; set; }
        public DateTime JobStartTime { get; set; }
        public DateTime JobEndTime { get; set; }
        public int JobDuration { get; set; }
        public int EstJobDuration { get; set; }
        public int CompanyID { get; set; }
    }

    public class VrpRouteReportResponse
    {
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }
        public VrpRouteReport VrpRouteReport { get; set; }
    }
}
