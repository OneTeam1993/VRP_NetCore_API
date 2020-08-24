using System;

namespace WebAPITime.Models
{
    public class DriverReport
    {
        public long DriverID { get; set; }
        public DateTime ScheduledWorkTimeStart { get; set; }
        public DateTime ActualWorkTimeStart { get; set; }
        public string WorkTimeStartStatus { get; set; }
        public DateTime ScheduledWorkTimeEnd { get; set; }
        public DateTime ActualWorkTimeEnd { get; set; }
        public string WorkTimeEndStatus { get; set; }
        public int WorkDuration { get; set; }
    }

    public class DriverReportResponse
    {
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }
        public DriverReport DriverReport { get; set; }
    }

    public class DriverSchedule
    {
        public long DriverID { get; set; }
        public string TimeWindowStart { get; set; }
        public string TimeWindowEnd { get; set; }
        public int DayID { get; set; }
    }
}
