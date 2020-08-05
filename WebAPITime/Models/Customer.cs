using System.Collections.Generic;

namespace WebAPITime.Models
{
    public class CustomerInfo
    {
        public int CustomerID { get; set; }
        public string Name { get; set; }
    }

    public class CustomerOrder
    {
        public int CustomerID { get; set; }
        public long RouteID { get; set; }
        public List<VrpPickup> PickupOrders { get; set; }
        public List<VrpDelivery> DeliveryOrders { get; set; }
    }
}
