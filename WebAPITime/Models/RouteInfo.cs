using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using VrpModel;

namespace WebAPITime.Models
{
    public class RouteInfo
    {
        public long RouteID { get; set; }
        public string RouteNo { get; set; }
        public long AssetID { get; set; }
        public string AssetName { get; set; }
        public long DriverID { get; set; }
        public string DriverName { get; set; }
        public List<Customer> Customers { get; set; }
        public PickupDeliveryInfo PickupDeliveryInfo { get; set; }
        public double DistanceOfRoute { get; set; }
        public int TimeOfRoute { get; set; }
        public DateTime ArrivalTime { get; set; }
        public DateTime DepartureTime { get; set; }
        public DateTime BreakTimeStart { get; set; }
        public DateTime BreakTimeEnd { get; set; }
        public int Sequence { get; set; }
        public List<int> FeatureIDs { get; set; }
        public List<long> Accessories { get; set; }
        public string Status { get; set; }
        public int Flag { get; set; }
        public DateTime Timestamp { get; set; }
        public DateTime RxTime { get; set; }
    }

    public class Customer
    {
        public int CustomerID { get; set; }
        public string CustomerName { get; set; }
        public List<long> PickupIDs { get; set; }
        public List<long> DeliveryIDs { get; set; }
    }

    public class ResponseRouteInfoDeletion
    {
        public long RouteID { get; set; }
        public string RequestType { get; set; }
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }
        public VrpInfo RecalculatedRouteInfo { get; set; }
    }

    public class ResponseTimelineRoutes
    {
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }
        public List<RouteInfo> Routes { get; set; }
    }

    public class ResponseSaveRoutes
    {
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }
    }
}
