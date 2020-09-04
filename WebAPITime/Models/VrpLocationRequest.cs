using System.Collections.Generic;

namespace WebAPITime.Models
{
    public class VrpLocationRequestResponse
    {
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }
        public List<VrpLocationRequest> VrpLocationRequest { get; set; }
    }

    public class VrpLocationRequest
    {
        public int CompanyID { get; set; }
        public string Date { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }
        public int CreditLimit { get; set; }
        //public int DailyCreditLimit { get; set; }
        public int RequestCount { get; set; }
        public string Usage { get; set; }
    }
}
