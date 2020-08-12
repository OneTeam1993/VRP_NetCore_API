using MySql.Data.MySqlClient;
using System;
using WebAPITime.HelperTools;

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
    }
}
