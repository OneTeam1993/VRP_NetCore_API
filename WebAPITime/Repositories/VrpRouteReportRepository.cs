using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using WebAPITime.HelperTools;
using WebAPITime.Models;

namespace WebAPITime.Repositories
{
    public class VrpRouteReportRepository : IVrpRouteReportRepository
    {
        string mConnStr = ConfigurationManager.AppSettings["mConnStr"];
        string mProjName = ConfigurationManager.AppSettings["mProjName"];

        private static readonly IRouteInfoRepository repoRouteInfo = new RouteInfoRepository();

        public VrpRouteReportResponse GetRouteReport(long routeID)
        {
            VrpRouteReportResponse vrpRouteReportResponse = new VrpRouteReportResponse();

            try
            {
                VrpRouteReport vrpRouteReport = GetRouteReportByRouteID(routeID.ToString());
                vrpRouteReportResponse.IsSuccess = true;
                vrpRouteReportResponse.VrpRouteReport = vrpRouteReport;
            }
            catch (Exception ex)
            {
                Logger.LogEvent(mProjName, String.Format("VrpRouteReportRepository GetRouteReport() Exception: {0}", ex.Message), System.Diagnostics.EventLogEntryType.Error);
                vrpRouteReportResponse.ErrorMessage = "Error occurred when getting route report";
            }

            return vrpRouteReportResponse;
        }


        public VrpRouteReportResponse UpdateVrpRouteReport(string routeNo, long driverID, long routeID, DateTime departureTime, DateTime arrivalTime, DateTime jobEndTime)
        {
            VrpRouteReportResponse vrpRouteReportResponse = new VrpRouteReportResponse();

            try
            {
                bool isUpdateRecord = false;
                VrpRouteReport vrpRouteReport = GetRouteReportByRouteID(routeID.ToString());

                if (vrpRouteReport == null)
                {
                    List<RouteInfo> arrRouteInfo = repoRouteInfo.GetAllRouteInfoByRouteNoDriver(routeNo, driverID);

                    RouteInfo previousRouteInfo = null;
                    RouteInfo routeInfo = null;

                    for (int i=0; i<arrRouteInfo.Count; i++)
                    {
                        if(arrRouteInfo[i].RouteID == routeID)
                        {
                            routeInfo = arrRouteInfo[i];
                            break;
                        }

                        previousRouteInfo = arrRouteInfo[i];
                    }

                    vrpRouteReport = new VrpRouteReport();
                    vrpRouteReport.VrpRouteID = routeID;

                    if (previousRouteInfo != null)
                    {
                        vrpRouteReport.FromVrpRouteID = previousRouteInfo.RouteID;
                        vrpRouteReport.EstDepartureTime = previousRouteInfo.DepartureTime;
                    }
                    else
                    {
                        vrpRouteReport.EstDepartureTime = routeInfo.DepartureTime;
                    }

                    vrpRouteReport.EstArrivalTime = routeInfo.ArrivalTime;
                    vrpRouteReport.EstJobDuration = routeInfo.PickupDeliveryInfo.ServiceDuration + routeInfo.PickupDeliveryInfo.WaitingDuration + routeInfo.PickupDeliveryInfo.UnloadDuration + routeInfo.PickupDeliveryInfo.LoadDuration;
                }
                else
                {
                    isUpdateRecord = true;
                }
              
                if (departureTime != Convert.ToDateTime("1/1/0001 00:00:00"))
                {
                    vrpRouteReport.ActualDepartureTime = departureTime;

                    if (vrpRouteReport.ActualDepartureTime > vrpRouteReport.EstDepartureTime)
                    {
                        vrpRouteReport.DepartureTimeStatus = "Late";
                    }
                    else
                    {
                        vrpRouteReport.DepartureTimeStatus = "On-Time";
                    }
                }

                if (arrivalTime != Convert.ToDateTime("1/1/0001 00:00:00"))
                {
                    vrpRouteReport.ActualArrivalTime = arrivalTime;
                    vrpRouteReport.JobStartTime = arrivalTime;

                    if (vrpRouteReport.ActualArrivalTime > vrpRouteReport.EstArrivalTime)
                    {
                        vrpRouteReport.ArrivalTimeStatus = "Late";
                    }
                    else
                    {
                        vrpRouteReport.ArrivalTimeStatus = "On-Time";
                    }

                    vrpRouteReport.TravelDuration = (int)vrpRouteReport.ActualArrivalTime.Subtract(vrpRouteReport.ActualDepartureTime).TotalMinutes;
                }

                if (jobEndTime != Convert.ToDateTime("1/1/0001 00:00:00"))
                {
                    vrpRouteReport.JobEndTime = jobEndTime;
                    vrpRouteReport.JobDuration = (int)vrpRouteReport.JobEndTime.Subtract(vrpRouteReport.JobStartTime).TotalMinutes;
                }

                if (!isUpdateRecord)
                {
                    if (InsertRouteReport(vrpRouteReport))
                    {
                        vrpRouteReportResponse.IsSuccess = true;
                        vrpRouteReportResponse.VrpRouteReport = vrpRouteReport;
                    }
                }
                else
                {
                    if (UpdateRouteReport(vrpRouteReport))
                    {
                        vrpRouteReportResponse.IsSuccess = true;
                        vrpRouteReportResponse.VrpRouteReport = vrpRouteReport;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogEvent(mProjName, String.Format("VrpRouteReportRepository UpdateVrpRouteReport() Exception: {0}", ex.Message), System.Diagnostics.EventLogEntryType.Error);
            }

            if (!vrpRouteReportResponse.IsSuccess)
            {
                vrpRouteReportResponse.ErrorMessage = "Error occured when updating route report";
            }

            return vrpRouteReportResponse;
        }

        public VrpRouteReport GetRouteReportByRouteID(string routeID)
        {
            VrpRouteReport vrpRouteReport = null;
            string query = "SELECT * FROM vrp_route_reports WHERE vrp_routes_id = @routeID";

            try
            {
                using (MySqlConnection mConnection = new MySqlConnection(mConnStr))
                {
                    using (MySqlCommand cmd = new MySqlCommand(query, mConnection))
                    {
                        mConnection.Open();
                        cmd.Prepare();
                        cmd.Parameters.AddWithValue("@routeID", routeID);

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if ((reader != null) && (reader.HasRows))
                            {
                                while (reader.Read())
                                {
                                    vrpRouteReport = DataMgrTools.BuildVrpRouteReport(reader);
                                }
                            }
                        }

                        mConnection.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogEvent(mProjName, String.Format("VrpRouteReportRepository GetRouteReportByRouteID() Exception: {0}", ex.Message), System.Diagnostics.EventLogEntryType.Error);
            }

            return vrpRouteReport;
        }

        public bool InsertRouteReport(VrpRouteReport vrpRouteReport)
        {
            bool isSuccess = false;

            vrpRouteReport.DepartureTimeStatus = vrpRouteReport.DepartureTimeStatus ?? "";
            vrpRouteReport.ArrivalTimeStatus = vrpRouteReport.ArrivalTimeStatus ?? "";

            string query = String.Format("INSERT INTO vrp_route_reports (vrp_routes_id, from_vrp_routes_id, est_departure_time, actual_departure_time, status_departure_time, est_arrival_time, actual_arrival_time, status_arrival_time, travel_duration, job_start_time, job_end_time, job_duration, est_job_duration) " +
                "VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12})",
                vrpRouteReport.VrpRouteID == 0 ? "NULL" : vrpRouteReport.VrpRouteID.ToString(),
                vrpRouteReport.FromVrpRouteID == 0 ? "NULL" : vrpRouteReport.FromVrpRouteID.ToString(),
                vrpRouteReport.EstDepartureTime == Convert.ToDateTime("1/1/0001 00:00:00") ? "NULL" : String.Format("'{0}'", vrpRouteReport.EstDepartureTime.ToString("yyyy-MM-dd HH:mm:ss")),
                vrpRouteReport.ActualDepartureTime == Convert.ToDateTime("1/1/0001 00:00:00") ? "NULL" : String.Format("'{0}'", vrpRouteReport.ActualDepartureTime.ToString("yyyy-MM-dd HH:mm:ss")),
                vrpRouteReport.DepartureTimeStatus == "" ? "NULL" : String.Format("'{0}'", vrpRouteReport.DepartureTimeStatus),
                vrpRouteReport.EstArrivalTime == Convert.ToDateTime("1/1/0001 00:00:00") ? "NULL" : String.Format("'{0}'", vrpRouteReport.EstArrivalTime.ToString("yyyy-MM-dd HH:mm:ss")),
                vrpRouteReport.ActualArrivalTime == Convert.ToDateTime("1/1/0001 00:00:00") ? "NULL" : String.Format("'{0}'", vrpRouteReport.ActualArrivalTime.ToString("yyyy-MM-dd HH:mm:ss")),
                vrpRouteReport.ArrivalTimeStatus == "" ? "NULL" : String.Format("'{0}'", vrpRouteReport.ArrivalTimeStatus),
                vrpRouteReport.TravelDuration == 0 ? "NULL" : vrpRouteReport.TravelDuration.ToString(),
                vrpRouteReport.JobStartTime == Convert.ToDateTime("1/1/0001 00:00:00") ? "NULL" : String.Format("'{0}'", vrpRouteReport.JobStartTime.ToString("yyyy-MM-dd HH:mm:ss")),
                vrpRouteReport.JobEndTime == Convert.ToDateTime("1/1/0001 00:00:00") ? "NULL" : String.Format("'{0}'", vrpRouteReport.JobEndTime.ToString("yyyy-MM-dd HH:mm:ss")),
                vrpRouteReport.JobDuration == 0 ? "NULL" : vrpRouteReport.JobDuration.ToString(),
                vrpRouteReport.EstJobDuration
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
                Logger.LogEvent(mProjName, String.Format("VrpRouteReportRepository InsertRouteReport() Exception: {0}", ex.Message), System.Diagnostics.EventLogEntryType.Error);
            }

            return isSuccess;
        }

        public bool UpdateRouteReport(VrpRouteReport vrpRouteReport)
        {
            bool isSuccess = false;

            vrpRouteReport.DepartureTimeStatus = vrpRouteReport.DepartureTimeStatus ?? "";
            vrpRouteReport.ArrivalTimeStatus = vrpRouteReport.ArrivalTimeStatus ?? "";

            string query = String.Format("UPDATE vrp_route_reports " +
                "SET from_vrp_routes_id = {0}, est_departure_time = {1}, actual_departure_time = {2}, status_departure_time = {3}, " +
                "est_arrival_time = {4}, actual_arrival_time = {5}, status_arrival_time = {6}, travel_duration = {7}, job_start_time = {8}, " +
                "job_end_time = {9}, job_duration = {10}, est_job_duration = {11} " +
                "WHERE vrp_routes_id = {12}",                       
                vrpRouteReport.FromVrpRouteID == 0 ? "NULL" : vrpRouteReport.FromVrpRouteID.ToString(),
                vrpRouteReport.EstDepartureTime == Convert.ToDateTime("1/1/2000 00:00:00") ? "NULL" : String.Format("'{0}'", vrpRouteReport.EstDepartureTime.ToString("yyyy-MM-dd HH:mm:ss")),
                vrpRouteReport.ActualDepartureTime == Convert.ToDateTime("1/1/2000 00:00:00") ? "NULL" : String.Format("'{0}'", vrpRouteReport.ActualDepartureTime.ToString("yyyy-MM-dd HH:mm:ss")),
                vrpRouteReport.DepartureTimeStatus == "" ? "NULL" : String.Format("'{0}'", vrpRouteReport.DepartureTimeStatus),
                vrpRouteReport.EstArrivalTime == Convert.ToDateTime("1/1/2000 00:00:00") ? "NULL" : String.Format("'{0}'", vrpRouteReport.EstArrivalTime.ToString("yyyy-MM-dd HH:mm:ss")),
                vrpRouteReport.ActualArrivalTime == Convert.ToDateTime("1/1/2000 00:00:00") ? "NULL" : String.Format("'{0}'", vrpRouteReport.ActualArrivalTime.ToString("yyyy-MM-dd HH:mm:ss")),
                vrpRouteReport.ArrivalTimeStatus == "" ? "NULL" : String.Format("'{0}'", vrpRouteReport.ArrivalTimeStatus),
                vrpRouteReport.TravelDuration == 0 ? "NULL" : vrpRouteReport.TravelDuration.ToString(),
                vrpRouteReport.JobStartTime == Convert.ToDateTime("1/1/2000 00:00:00") ? "NULL" : String.Format("'{0}'", vrpRouteReport.JobStartTime.ToString("yyyy-MM-dd HH:mm:ss")),
                vrpRouteReport.JobEndTime == Convert.ToDateTime("1/1/2000 00:00:00") ? "NULL" : String.Format("'{0}'", vrpRouteReport.JobEndTime.ToString("yyyy-MM-dd HH:mm:ss")),
                vrpRouteReport.JobDuration == 0 ? "NULL" : vrpRouteReport.JobDuration.ToString(),
                vrpRouteReport.EstJobDuration,
                vrpRouteReport.VrpRouteID == 0 ? "NULL" : vrpRouteReport.VrpRouteID.ToString()
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
                Logger.LogEvent(mProjName, String.Format("VrpRouteReportRepository UpdateRouteReport() Exception: {0}", ex.Message), System.Diagnostics.EventLogEntryType.Error);
            }

            return isSuccess;
        }
    }
}
