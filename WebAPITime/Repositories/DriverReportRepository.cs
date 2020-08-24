using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using WebAPITime.HelperTools;
using WebAPITime.Models;

namespace WebAPITime.Repositories
{
    public class DriverReportRepository : IDriverReportRepository
    {
        string mConnStr = ConfigurationManager.AppSettings["mConnStr"];
        string mProjName = ConfigurationManager.AppSettings["mProjName"];

        public DriverReportResponse GetDriverReport(long driverID, DateTime reportDate)
        {
            DriverReportResponse driverReportResponse = new DriverReportResponse();

            if(reportDate == Convert.ToDateTime("1/1/0001 00:00:00"))
            {
                driverReportResponse.ErrorMessage = "Invalid parameter: ReportDate";
                return driverReportResponse;
            }

            try
            {
                DriverReport driverReport = GetDriverReportByDate(driverID, reportDate);
                driverReportResponse.IsSuccess = true;
                driverReportResponse.DriverReport = driverReport;
            }
            catch (Exception ex)
            {
                Logger.LogEvent(mProjName, String.Format("DriverReportRepository GetDriverReport() Exception: {0}", ex.Message), System.Diagnostics.EventLogEntryType.Error);
                driverReportResponse.ErrorMessage = "Error occurred when getting driver report record";
            }

            return driverReportResponse;
        }
        public DriverReportResponse UpdateDriverReport(long driverID, DateTime worktimeStart, DateTime worktimeEnd)
        {
            DriverReportResponse driverReportResponse = new DriverReportResponse();

            try
            {
                bool isUpdateRecord = false;

                if (worktimeStart == Convert.ToDateTime("1/1/0001 00:00:00") && worktimeEnd == Convert.ToDateTime("1/1/0001 00:00:00"))
                {
                    driverReportResponse.ErrorMessage = "Invalid start/end worktime";
                    return driverReportResponse;
                }

                DateTime recordDatetime = worktimeStart != Convert.ToDateTime("1/1/0001 00:00:00") ? worktimeStart : worktimeEnd;

                DriverReport driverReport = GetDriverReportByDate(driverID, recordDatetime);

                if (driverReport == null)
                {
                    driverReport = new DriverReport();
                    driverReport.DriverID = driverID;

                    DriverSchedule driverSchedule = GetDriverSchedule(driverID, ((int)recordDatetime.DayOfWeek == 0) ? 7 : (int)DateTime.Now.DayOfWeek);

                    if (driverSchedule != null)
                    {
                        driverReport.ScheduledWorkTimeStart = DateTime.Parse(String.Format("{0} {1}", recordDatetime.ToString("yyyy-MM-dd"), driverSchedule.TimeWindowStart));
                        driverReport.ScheduledWorkTimeEnd = DateTime.Parse(String.Format("{0} {1}", recordDatetime.ToString("yyyy-MM-dd"), driverSchedule.TimeWindowEnd));
                    }
                }
                else
                {
                    isUpdateRecord = true;
                }

                if (worktimeStart != Convert.ToDateTime("1/1/0001 00:00:00"))
                {
                    driverReport.ActualWorkTimeStart = worktimeStart;

                    if (driverReport.ScheduledWorkTimeStart != Convert.ToDateTime("1/1/0001 00:00:00") && driverReport.ScheduledWorkTimeStart != Convert.ToDateTime("1/1/2000 00:00:00"))
                    {
                        if (driverReport.ActualWorkTimeStart > driverReport.ScheduledWorkTimeStart)
                        {
                            driverReport.WorkTimeStartStatus = "Late";
                        }
                        else
                        {
                            driverReport.WorkTimeStartStatus = "On-Time";
                        }
                        
                    }
                }

                if (worktimeEnd != Convert.ToDateTime("1/1/0001 00:00:00"))
                {
                    driverReport.ActualWorkTimeEnd = worktimeEnd;

                    if (driverReport.ScheduledWorkTimeEnd != Convert.ToDateTime("1/1/0001 00:00:00") && driverReport.ScheduledWorkTimeEnd != Convert.ToDateTime("1/1/2000 00:00:00"))
                    {
                        if (driverReport.ActualWorkTimeEnd < driverReport.ScheduledWorkTimeEnd)
                        {
                            driverReport.WorkTimeEndStatus = "Early";
                        }
                        else
                        {
                            driverReport.WorkTimeEndStatus = "On-Time";
                        }

                    }

                    if (driverReport.ActualWorkTimeStart != Convert.ToDateTime("1/1/0001 00:00:00") && driverReport.ActualWorkTimeStart != Convert.ToDateTime("1/1/2000 00:00:00"))
                    {
                        driverReport.WorkDuration = (int)driverReport.ActualWorkTimeEnd.Subtract(driverReport.ActualWorkTimeStart).TotalMinutes;
                    }
                }

                if (!isUpdateRecord)
                {
                    if (InsertDriverReport(driverReport))
                    {
                        driverReportResponse.IsSuccess = true;
                        driverReportResponse.DriverReport = driverReport;
                    }
                }
                else
                {
                    if (UpdateDriverReportRecord(driverReport, recordDatetime))
                    {
                        driverReportResponse.IsSuccess = true;
                        driverReportResponse.DriverReport = driverReport;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogEvent(mProjName, String.Format("DriverReportRepository UpdateDriverReport() Exception: {0}", ex.Message), System.Diagnostics.EventLogEntryType.Error);
            }

            if (!driverReportResponse.IsSuccess)
            {
                driverReportResponse.ErrorMessage = "Error occured when updating driver report";
            }

            return driverReportResponse;
        }
        public DriverReport GetDriverReportByDate(long driverID, DateTime recordDatetime)
        {
            DriverReport driverReport = null;
            string query = "SELECT * FROM driver_reports WHERE driver_id = @driverID AND " +
                "(DATE(scheduled_work_time_start) = DATE(@recordDatetime) OR DATE(actual_work_time_start) = DATE(@recordDatetime) OR DATE(scheduled_work_time_end) = DATE(@recordDatetime) OR DATE(actual_work_time_end) = DATE(@recordDatetime))";

            try
            {
                using (MySqlConnection mConnection = new MySqlConnection(mConnStr))
                {
                    using (MySqlCommand cmd = new MySqlCommand(query, mConnection))
                    {
                        mConnection.Open();
                        cmd.Prepare();
                        cmd.Parameters.AddWithValue("@driverID", driverID);
                        cmd.Parameters.AddWithValue("@recordDatetime", recordDatetime.ToString("yyyy-MM-dd HH:mm:ss"));

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if ((reader != null) && (reader.HasRows))
                            {
                                while (reader.Read())
                                {
                                    driverReport = DataMgrTools.BuildDriverReport(reader);
                                }
                            }
                        }

                        mConnection.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogEvent(mProjName, String.Format("DriverReportRepository GetDriverReportByDate() Exception: {0}", ex.Message), System.Diagnostics.EventLogEntryType.Error);
            }

            return driverReport;
        }

        public DriverSchedule GetDriverSchedule(long driverID, int day)
        {
            DriverSchedule driverSchedule = null;
            string query = "SELECT * FROM driver_schedule WHERE driver_id = @driverID AND day_id = @dayID";

            try
            {
                using (MySqlConnection mConnection = new MySqlConnection(mConnStr))
                {
                    using (MySqlCommand cmd = new MySqlCommand(query, mConnection))
                    {
                        mConnection.Open();
                        cmd.Prepare();
                        cmd.Parameters.AddWithValue("@driverID", driverID);
                        cmd.Parameters.AddWithValue("@dayID", day);

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if ((reader != null) && (reader.HasRows))
                            {
                                while (reader.Read())
                                {
                                    driverSchedule = DataMgrTools.BuildDriverSchedule(reader);
                                }
                            }
                        }

                        mConnection.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogEvent(mProjName, String.Format("DriverReportRepository GetDriverScheduleByDriverID() Exception: {0}", ex.Message), System.Diagnostics.EventLogEntryType.Error);
            }

            return driverSchedule;
        }

        public bool InsertDriverReport(DriverReport driverReport)
        {
            bool isSuccess = false;

            driverReport.WorkTimeStartStatus = driverReport.WorkTimeStartStatus ?? "";
            driverReport.WorkTimeEndStatus = driverReport.WorkTimeEndStatus ?? "";

            string query = String.Format("INSERT INTO driver_reports (driver_id, scheduled_work_time_start, actual_work_time_start, status_work_time_start, scheduled_work_time_end, actual_work_time_end, status_work_time_end, work_duration) " +
                "VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7})",
                driverReport.DriverID,
                driverReport.ScheduledWorkTimeStart == Convert.ToDateTime("1/1/0001 00:00:00") ? "NULL" : String.Format("'{0}'", driverReport.ScheduledWorkTimeStart.ToString("yyyy-MM-dd HH:mm:ss")),
                driverReport.ActualWorkTimeStart == Convert.ToDateTime("1/1/0001 00:00:00") ? "NULL" : String.Format("'{0}'", driverReport.ActualWorkTimeStart.ToString("yyyy-MM-dd HH:mm:ss")),
                driverReport.WorkTimeStartStatus == "" ? "NULL" : String.Format("'{0}'", driverReport.WorkTimeStartStatus),
                driverReport.ScheduledWorkTimeEnd == Convert.ToDateTime("1/1/0001 00:00:00") ? "NULL" : String.Format("'{0}'", driverReport.ScheduledWorkTimeEnd.ToString("yyyy-MM-dd HH:mm:ss")),
                driverReport.ActualWorkTimeEnd == Convert.ToDateTime("1/1/0001 00:00:00") ? "NULL" : String.Format("'{0}'", driverReport.ActualWorkTimeEnd.ToString("yyyy-MM-dd HH:mm:ss")),
                driverReport.WorkTimeEndStatus == "" ? "NULL" : String.Format("'{0}'", driverReport.WorkTimeEndStatus),
                driverReport.WorkDuration == 0 ? "NULL" : driverReport.WorkDuration.ToString()
                );

            try
            {
                using (MySqlConnection mConnection = new MySqlConnection(mConnStr))
                {
                    using (MySqlCommand cmd = new MySqlCommand(query, mConnection))
                    {
                        mConnection.Open();
                        cmd.Prepare();

                        using (MySqlCommand myCmd = new MySqlCommand(query, mConnection))
                        {
                            if (myCmd.ExecuteNonQuery() > 0)
                            {
                                isSuccess = true;
                            }
                        }

                        mConnection.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogEvent(mProjName, String.Format("DriverReportRepository InsertDriverReport() Exception: {0}", ex.Message), System.Diagnostics.EventLogEntryType.Error);
            }

            return isSuccess;
        }

        public bool UpdateDriverReportRecord(DriverReport vrpDriverReport, DateTime recordDatetime)
        {
            bool isSuccess = false;

            vrpDriverReport.WorkTimeStartStatus = vrpDriverReport.WorkTimeStartStatus ?? "";
            vrpDriverReport.WorkTimeEndStatus = vrpDriverReport.WorkTimeEndStatus ?? "";

            string query = String.Format("UPDATE driver_reports " +
                "SET scheduled_work_time_start = {0}, actual_work_time_start = {1}, status_work_time_start = {2}, " +
                "scheduled_work_time_end = {3},  actual_work_time_end = {4}, status_work_time_end = {5}, work_duration = {6} " +
                "WHERE driver_id = {7} AND (DATE(scheduled_work_time_start) = DATE('{8}') OR DATE(actual_work_time_start) = DATE('{8}') OR DATE(scheduled_work_time_end) = DATE('{8}') OR DATE(actual_work_time_end) = DATE('{8}'))",                
                vrpDriverReport.ScheduledWorkTimeStart == Convert.ToDateTime("1/1/2000 00:00:00") ? "NULL" : String.Format("'{0}'", vrpDriverReport.ScheduledWorkTimeStart.ToString("yyyy-MM-dd HH:mm:ss")),
                vrpDriverReport.ActualWorkTimeStart == Convert.ToDateTime("1/1/200 00:00:00") ? "NULL" : String.Format("'{0}'", vrpDriverReport.ActualWorkTimeStart.ToString("yyyy-MM-dd HH:mm:ss")),
                vrpDriverReport.WorkTimeStartStatus == "" ? "NULL" : String.Format("'{0}'", vrpDriverReport.WorkTimeStartStatus),
                vrpDriverReport.ScheduledWorkTimeEnd == Convert.ToDateTime("1/1/2000 00:00:00") ? "NULL" : String.Format("'{0}'", vrpDriverReport.ScheduledWorkTimeEnd.ToString("yyyy-MM-dd HH:mm:ss")),
                vrpDriverReport.ActualWorkTimeEnd == Convert.ToDateTime("1/1/2000 00:00:00") ? "NULL" : String.Format("'{0}'", vrpDriverReport.ActualWorkTimeEnd.ToString("yyyy-MM-dd HH:mm:ss")),
                vrpDriverReport.WorkTimeEndStatus == "" ? "NULL" : String.Format("'{0}'", vrpDriverReport.WorkTimeEndStatus),
                vrpDriverReport.WorkDuration == 0 ? "NULL" : vrpDriverReport.WorkDuration.ToString(),
                vrpDriverReport.DriverID,
                recordDatetime.ToString("yyyy-MM-dd HH:mm:ss")
                );

            try
            {
                using (MySqlConnection mConnection = new MySqlConnection(mConnStr))
                {
                    using (MySqlCommand cmd = new MySqlCommand(query, mConnection))
                    {
                        mConnection.Open();
                        cmd.Prepare();

                        using (MySqlCommand myCmd = new MySqlCommand(query, mConnection))
                        {
                            if (myCmd.ExecuteNonQuery() > 0)
                            {
                                isSuccess = true;
                            }
                        }

                        mConnection.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogEvent(mProjName, String.Format("DriverReportRepository UpdateDriverReport() Exception: {0}", ex.Message), System.Diagnostics.EventLogEntryType.Error);
            }

            return isSuccess;
        }
    }
}
