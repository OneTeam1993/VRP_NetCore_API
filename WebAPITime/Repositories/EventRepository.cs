using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Text;
using WebAPITime.HelperTools;

namespace WebAPITime.Repositories
{
    public class EventRepository : IEventRepository
    {
        string mConnStr = ConfigurationManager.AppSettings["mConnStr"];
        string mProjName = ConfigurationManager.AppSettings["mProjName"];

        public bool LogVrpEvent(string companyID, string companyName, string userName, string roleID, string eventLog)
        {
            bool isSuccess = false;
            string currentDateTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            string remarks = string.Format("Username: {0} Company: {1} {2}", userName ?? "-", companyName ?? "-", eventLog);

            StringBuilder sCommand = new StringBuilder(string.Format("" +
                "INSERT INTO events (asset_id, tag_id, timestamp, rx_time, status_id, remarks, alert_level, alert_level_ex, flag, ack_user, ack_time, pos_id, company_id) VALUES " +
                "({0}, {1}, '{2}', '{3}', {4}, '{5}', {6}, {7}, {8}, {9}, '{10}', {11}, {12})",
                0, 0, currentDateTime, currentDateTime, 22, remarks, 0, 0, 1, roleID ?? "NULL", currentDateTime, 0, companyID ?? "NULL"));

            try
            {
                using (MySqlConnection mConnection = new MySqlConnection(mConnStr))
                {
                    mConnection.Open();
                    using (MySqlCommand myCmd = new MySqlCommand(sCommand.ToString(), mConnection))
                    {
                        myCmd.CommandType = CommandType.Text;
                       
                        if (myCmd.ExecuteNonQuery() > 0)
                        {
                            isSuccess = true;
                        }
                    }
                    mConnection.Close();
                }
            }
            catch(Exception ex)
            {
                Logger.LogEvent(mProjName, String.Format("EventRepository LogVrpEvent() Exception: {0}", ex.Message), System.Diagnostics.EventLogEntryType.Error);
            }

            return isSuccess;
        }
    }
}
