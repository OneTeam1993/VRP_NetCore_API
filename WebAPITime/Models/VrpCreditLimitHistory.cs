using System;

namespace WebAPITime.Models
{
    public class VrpCreditLimitHistory
    {
        public int CompanyID { get; set; }
        public DateTime Timestamp { get; set; }
        public int CreditLimit { get; set; }
    }

    public class VrpBaseCreditLimitHistory
    {
        public int CompanyID { get; set; }
        public DateTime Timestamp { get; set; }
        public int DailyCreditLimit { get; set; }
        public int MonthlyCreditLimit { get; set; }
        public int YearlyCreditLimit { get; set; }
    }
}
