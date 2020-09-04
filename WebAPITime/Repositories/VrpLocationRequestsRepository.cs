using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using WebAPITime.HelperTools;
using WebAPITime.Models;

namespace WebAPITime.Repositories
{
    public class VrpLocationRequestsRepository : IVrpLocationRequestsRepository
    {
        string mConnStr = ConfigurationManager.AppSettings["mConnStr"];
        string mProjName = ConfigurationManager.AppSettings["mProjName"];

        public bool AddTotalRequest(string companyID, string routeNo, int totalRequest)
        {
            bool isSuccess = false;           
            string query = "INSERT INTO vrp_location_requests (company_id, date_request, route_no, request_count) VALUES " +
                "(@companyID, @date_request, @routeNo, @request_count)";

            try
            {
                using (MySqlConnection mConnection = new MySqlConnection(mConnStr))
                {
                    using (MySqlCommand cmd = new MySqlCommand(query, mConnection))
                    {
                        string currentDateTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

                        mConnection.Open();
                        cmd.Prepare();
                        cmd.Parameters.AddWithValue("@companyID", companyID);
                        cmd.Parameters.AddWithValue("@date_request", currentDateTime);
                        cmd.Parameters.AddWithValue("@routeNo", routeNo);
                        cmd.Parameters.AddWithValue("@request_count", totalRequest);

                        if (cmd.ExecuteNonQuery() > 0)
                        {
                            isSuccess = true;
                        }

                        mConnection.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogEvent(mProjName, String.Format("VrpLocationRequestsRepository AddTotalRequest() Exception: {0}", ex.Message), System.Diagnostics.EventLogEntryType.Error);
            }


            return isSuccess;
        }

        public VrpLocationRequestResponse Get(string companyID, string mode, string strDateStart, string strDateEnd)
        {
            VrpLocationRequestResponse vrpLocationRequestResponse = new VrpLocationRequestResponse();
            List<VrpLocationRequest> arrVrpLocationRequest = new List<VrpLocationRequest>();
            VrpLocationRequest currVrpLocationRequest = new VrpLocationRequest();
            bool isValidMode = false;
            DateTime datetimeStart = new DateTime();
            DateTime datetimeEnd = new DateTime();
            //string select = "";
            //string leftJoin = "";
            //string whereClause = "";
            //string groupBy = "";
            //string orderBy = "";
            string storedProcName = "";

            try
            {
                mode = mode.ToLower();

                if (mode == "year")
                {
                    storedProcName = "sp_yearly_location_request_reports";
                    datetimeStart = DateTime.Parse(String.Format("{0}-01-01 00:00:00", strDateStart));
                    datetimeEnd = DateTime.Parse(String.Format("{0}-12-31 23:59:59", strDateEnd));

                    //select = "SELECT c.company_id, YEAR(r.date_request) 'year', SUM(r.request_count) 'request_count', c.daily_credit_limit, c.credit_limit  ";

                    //groupBy = "GROUP BY c.company_id, YEAR(r.date_request) ";
                    //orderBy = "ORDER BY c.company_id ASC, YEAR(r.date_request) ASC";

                    isValidMode = true;
                }
                else if (mode == "month")
                {
                    storedProcName = "sp_monthly_location_request_reports";
                    DateTime tempDatetimeStart = DateTime.Parse(strDateStart);
                    DateTime tempDatetimeEnd = DateTime.Parse(strDateEnd);
                    datetimeStart = DateTime.Parse(String.Format("{0}-{1}-01 00:00:00", tempDatetimeStart.Year, tempDatetimeStart.Month));
                    datetimeEnd = DateTime.Parse(String.Format("{0}-{1}-{2} 23:59:59", tempDatetimeEnd.Year, tempDatetimeEnd.Month, DateTime.DaysInMonth(tempDatetimeEnd.Year, tempDatetimeEnd.Month)));

                    //select = "SELECT c.company_id, YEAR(r.date_request) 'year', MONTH(r.date_request) 'month', SUM(r.request_count) 'request_count', c.daily_credit_limit, c.credit_limit ";
                    //groupBy = "GROUP BY c.company_id, YEAR(r.date_request), MONTH(r.date_request) ";
                    //orderBy = "ORDER BY c.company_id ASC, YEAR(r.date_request) ASC, MONTH(r.date_request) ASC";

                    isValidMode = true;
                }
                else if (mode == "day")
                {
                    storedProcName = "sp_daily_location_request_reports";
                    DateTime tempDatetimeStart = DateTime.Parse(strDateStart);
                    DateTime tempDatetimeEnd = DateTime.Parse(strDateEnd);
                    datetimeStart = DateTime.Parse(String.Format("{0}-{1}-{2} 00:00:00", tempDatetimeStart.Year, tempDatetimeStart.Month, tempDatetimeStart.Day));
                    datetimeEnd = DateTime.Parse(String.Format("{0}-{1}-{2} 23:59:59", tempDatetimeEnd.Year, tempDatetimeEnd.Month, tempDatetimeEnd.Day));

                    //select = "SELECT c.company_id, YEAR(r.date_request) 'year', MONTH(r.date_request) 'month', DAY(r.date_request) 'day', SUM(r.request_count) 'request_count', c.daily_credit_limit, c.credit_limit ";
                    //groupBy = "GROUP BY c.company_id, YEAR(r.date_request), MONTH(r.date_request), DAY(r.date_request) ";
                    //orderBy = "ORDER BY c.company_id ASC, YEAR(r.date_request) ASC, MONTH(r.date_request) ASC, DAY(r.date_request) ASC";

                    isValidMode = true;
                }
            }
            catch (Exception ex)
            {
                Logger.LogEvent(mProjName, String.Format("VrpLocationRequestsRepository Get() Exception: {0}", ex.Message), System.Diagnostics.EventLogEntryType.Error);
                vrpLocationRequestResponse.IsSuccess = false;
                vrpLocationRequestResponse.ErrorMessage = "Invalid start/end date";

                return vrpLocationRequestResponse;
            }
            

            if (isValidMode)
            {
                //leftJoin = String.Format("LEFT JOIN vrp_location_requests r ON r.company_id = c.company_id AND r.date_request BETWEEN '{0}' AND '{1}' ", datetimeStart.ToString("yyyy-MM-dd HH:mm:ss"), datetimeEnd.ToString("yyyy-MM-dd HH:mm:ss"));
                //whereClause = String.Format("WHERE c.company_id = {0} ", companyID);                
                //string query = string.Format(select +
                //    "FROM companies c " + 
                //    leftJoin +
                //    whereClause + groupBy + orderBy);

                using (MySqlConnection conn = new MySqlConnection(mConnStr))
                {
                    try
                    {
                        //using (MySqlCommand cmd = new MySqlCommand(query, conn))
                        using (MySqlCommand cmd = new MySqlCommand(storedProcName, conn))
                        {
                            conn.Open();
                            //cmd.Parameters.Add(new MySqlParameter("@whereClause", whereClause));
                            //cmd.Parameters.Add(new MySqlParameter("@orderBy", orderBy));
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new MySqlParameter("@CompanyID", companyID));
                            cmd.Parameters.Add(new MySqlParameter("@RequestTimeStart", datetimeStart.ToString("yyyy-MM-dd HH:mm:ss")));
                            cmd.Parameters.Add(new MySqlParameter("@RequestTimeEnd", datetimeEnd.ToString("yyyy-MM-dd HH:mm:ss")));
                            cmd.Prepare();

                            using (MySqlDataReader reader = cmd.ExecuteReader())
                            {
                                if ((reader != null) && (reader.HasRows))
                                {
                                    while (reader.Read())
                                    {
                                        currVrpLocationRequest = DataMgrTools.BuildVrpLocationRequest(reader, mode);

                                        if (currVrpLocationRequest.Year != 0)
                                        {
                                            if (mode == "year")
                                            {
                                                currVrpLocationRequest.Date = currVrpLocationRequest.Year.ToString();

                                                //if (currVrpLocationRequest.CreditLimit != 0)
                                                //{
                                                //    currVrpLocationRequest.Usage = Math.Round((((double)currVrpLocationRequest.RequestCount / ((double)currVrpLocationRequest.CreditLimit * 12)) * 100), 2).ToString() + " %";
                                                //}
                                                //else
                                                //{
                                                //    currVrpLocationRequest.Usage = "0% ";
                                                //}

                                            }
                                            else if (mode == "month")
                                            {
                                                DateTime tempDatetime = DateTime.Parse(String.Format("{0}-{1}-01", currVrpLocationRequest.Year, currVrpLocationRequest.Month));
                                                currVrpLocationRequest.Date = tempDatetime.ToString("yyyy-MMM");

                                                //if (currVrpLocationRequest.CreditLimit != 0)
                                                //{
                                                //    currVrpLocationRequest.Usage = Math.Round((((double)currVrpLocationRequest.RequestCount / (double)currVrpLocationRequest.CreditLimit) * 100), 2).ToString() + " %";
                                                //}
                                                //else
                                                //{
                                                //    currVrpLocationRequest.Usage = "0 %";
                                                //}
                                            }
                                            else if (mode == "day")
                                            {
                                                DateTime tempDatetime = DateTime.Parse(String.Format("{0}-{1}-{2}", currVrpLocationRequest.Year, currVrpLocationRequest.Month, currVrpLocationRequest.Day));
                                                currVrpLocationRequest.Date = tempDatetime.ToString("yyyy-MMM-dd");

                                                //if (currVrpLocationRequest.CreditLimit != 0)
                                                //{
                                                //    int daysInMonth = DateTime.DaysInMonth(currVrpLocationRequest.Year, currVrpLocationRequest.Month);
                                                //    currVrpLocationRequest.Usage = Math.Round((((double)currVrpLocationRequest.RequestCount / ((double)currVrpLocationRequest.CreditLimit / daysInMonth)) * 100), 2).ToString() + " %";
                                                //}
                                                //else
                                                //{
                                                //    currVrpLocationRequest.Usage = "0 %";
                                                //}

                                                //if (currVrpLocationRequest.DailyCreditLimit != 0)
                                                //{
                                                //    currVrpLocationRequest.Usage = Math.Round((((double)currVrpLocationRequest.RequestCount / (double)currVrpLocationRequest.DailyCreditLimit) * 100), 2).ToString() + " %";
                                                //}
                                                //else
                                                //{
                                                //    currVrpLocationRequest.Usage = "0 %";
                                                //}
                                            }

                                            if (currVrpLocationRequest.CreditLimit != 0)
                                            {
                                                currVrpLocationRequest.Usage = Math.Round((((double)currVrpLocationRequest.RequestCount / (double)currVrpLocationRequest.CreditLimit) * 100), 2).ToString() + " %";
                                            }
                                            else
                                            {
                                                currVrpLocationRequest.Usage = "0 %";
                                            }
                                        }
                                        

                                        arrVrpLocationRequest.Add(currVrpLocationRequest);

                                        vrpLocationRequestResponse.IsSuccess = true;
                                        vrpLocationRequestResponse.VrpLocationRequest = arrVrpLocationRequest;
                                    }
                                }
                            }

                            conn.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.LogEvent(mProjName, String.Format("VrpLocationRequestsRepository Get() Exception: {0}", ex.Message), System.Diagnostics.EventLogEntryType.Error);
                        vrpLocationRequestResponse.IsSuccess = false;
                        vrpLocationRequestResponse.ErrorMessage = String.Format("Error occurred. Exception: {0}", ex.Message);
                    }
                }
            }
            else
            {
                vrpLocationRequestResponse.IsSuccess = false;
                vrpLocationRequestResponse.ErrorMessage = "Invalid mode";
            }

            

            
            return vrpLocationRequestResponse;
        }
    }
}
