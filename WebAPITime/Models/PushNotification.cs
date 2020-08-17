using System;

namespace WebAPITime.Models
{
    public class PushNotification
    {
        public DateTime TimeWindowStart { get; set; }
        public string Token { get; set; }
    }
}
