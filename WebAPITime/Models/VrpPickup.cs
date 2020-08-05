using System;

namespace WebAPITime.Models
{
    public class VrpPickup
    {
        public long PickupID { get; set; }
        public string RouteNo { get; set; }
        public int PriorityID { get; set; }
        public long DriverID { get; set; }
        public DateTime Timestamp { get; set; }
        public DateTime RxTime { get; set; }
        public long CustomerID { get; set; }
        public string OrderNo { get; set; }
        public int WaitingDuration { get; set; }
        public int ServiceDuration { get; set; }
        public int LoadDuration { get; set; }
        public DateTime TimeWindowStart { get; set; }
        public DateTime TimeWindowEnd { get; set; }
        public string Remarks { get; set; }
        public string Name { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Address { get; set; }
        public string Unit { get; set; }
        public string Building { get; set; }
        public string PostalCode { get; set; }
        public string Phone { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
        public double Amount { get; set; }
        public string Accessories { get; set; }
        public int IsAssign { get; set; }
        public int Flag { get; set; }
    }
}
