using System;

namespace WebAPITime.Models
{
    public class VrpDelivery
    {
        public long DeliveryID { get; set; }
        public string RouteNo { get; set; }
        public int PriorityID { get; set; }
        public long DriverID { get; set; }
        public DateTime Timestamp { get; set; }
        public DateTime RxTime { get; set; }
        public long CustomerID { get; set; }
        public string OrderNo { get; set; }
        public int WaitingDuration { get; set; }
        public int ServiceDuration { get; set; }
        public int UnloadDuration { get; set; }
        public DateTime TimeWindowStart { get; set; }
        public DateTime TimeWindowEnd { get; set; }
        public string Remarks { get; set; }
        public string PickupID { get; set; }
        public string Name { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string BillingName { get; set; }
        public string BillingAddress { get; set; }
        public string BillingUnit { get; set; }
        public string BillingBuilding { get; set; }
        public string BillingPostalCode { get; set; }
        public string BillingPhone { get; set; }
        public string BillingMobile { get; set; }
        public string BillingEmail { get; set; }
        public string ShippingName { get; set; }
        public string ShippingAddress { get; set; }
        public string ShippingUnit { get; set; }
        public string ShippingBuilding { get; set; }
        public string ShippingPostalCode { get; set; }
        public string ShippingPhone { get; set; }
        public string ShippingMobile { get; set; }
        public string ShippingEmail { get; set; }
        public double Amount { get; set; }
        public string AccessoriesID { get; set; }
        public string AccessoriesName { get; set; }
        public int IsAssign { get; set; }
        public int Flag { get; set; }
    }
}
