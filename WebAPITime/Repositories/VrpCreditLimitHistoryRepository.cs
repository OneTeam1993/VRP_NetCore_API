using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using WebAPITime.HelperTools;
using WebAPITime.Models;

namespace WebAPITime.Repositories
{
    public class VrpCreditLimitHistoryRepository : IVrpCreditLimitHistoryRepository
    {
        string mConnStr = ConfigurationManager.AppSettings["mConnStr"];
        string mProjName = ConfigurationManager.AppSettings["mProjName"];

        public List<VrpBaseCreditLimitHistory> GetLatestBaseCreditLimit(string companyID)
        {
            List<VrpBaseCreditLimitHistory> arrCreditLimitHistory = new List<VrpBaseCreditLimitHistory>();
            VrpBaseCreditLimitHistory currCreditLimitHistory = new VrpBaseCreditLimitHistory();
            string query = string.Format("SELECT * " +
                "FROM vrp_base_credit_limit_history " +
                "WHERE company_id = @companyID " +
                "ORDER BY timestamp DESC " +
                "LIMIT 1");

            using (MySqlConnection conn = new MySqlConnection(mConnStr))
            {
                try
                {
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        conn.Open();
                        cmd.Prepare();
                        cmd.Parameters.AddWithValue("@companyID", companyID);

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if ((reader != null) && (reader.HasRows))
                            {
                                while (reader.Read())
                                {
                                    currCreditLimitHistory = DataMgrTools.BuildVrpBaseCreditLimitHistory(reader);
                                    arrCreditLimitHistory.Add(currCreditLimitHistory);
                                }
                            }
                        }

                        conn.Close();
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogEvent(mProjName, String.Format("VrpCreditLimitHistoryRepository GetLatestBaseCreditLimit() Exception: {0}", ex.Message), System.Diagnostics.EventLogEntryType.Error);
                }
            }
            return arrCreditLimitHistory;
        }

        public List<VrpCreditLimitHistory> GetDailyHistory(string companyID, DateTime historyDateTime)
        {
            List<VrpCreditLimitHistory> arrCreditLimitHistory = new List<VrpCreditLimitHistory>();
            VrpCreditLimitHistory currCreditLimitHistory = new VrpCreditLimitHistory();
            string query = string.Format("SELECT * " +
                "FROM vrp_daily_credit_limit_history " +
                "WHERE company_id = @companyID AND timestamp BETWEEN @dateStart AND @dateEnd " +
                "ORDER BY timestamp DESC " +
                "LIMIT 1");

            using (MySqlConnection conn = new MySqlConnection(mConnStr))
            {
                try
                {
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        conn.Open();
                        cmd.Prepare();
                        cmd.Parameters.AddWithValue("@companyID", companyID);
                        cmd.Parameters.AddWithValue("@dateStart", string.Format("{0}-{1}-{2} 00:00:00", historyDateTime.Year, historyDateTime.Month, historyDateTime.Day));
                        cmd.Parameters.AddWithValue("@dateEnd", string.Format("{0}-{1}-{2} 23:59:59", historyDateTime.Year, historyDateTime.Month, historyDateTime.Day));

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if ((reader != null) && (reader.HasRows))
                            {
                                while (reader.Read())
                                {
                                    currCreditLimitHistory = DataMgrTools.BuildVrpCreditLimitHistory(reader);
                                    arrCreditLimitHistory.Add(currCreditLimitHistory);
                                }
                            }
                        }

                        conn.Close();
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogEvent(mProjName, String.Format("VrpCreditLimitHistoryRepository GetDailyHistory() Exception: {0}", ex.Message), System.Diagnostics.EventLogEntryType.Error);
                }
            }
            return arrCreditLimitHistory;
        }

        public List<VrpCreditLimitHistory> GetMonthlyHistory(string companyID, DateTime historyDateTime)
        {
            List<VrpCreditLimitHistory> arrCreditLimitHistory = new List<VrpCreditLimitHistory>();
            VrpCreditLimitHistory currCreditLimitHistory = new VrpCreditLimitHistory();
            string query = string.Format("SELECT * " +
                "FROM vrp_monthly_credit_limit_history " +
                "WHERE company_id = @companyID AND timestamp BETWEEN @dateStart AND @dateEnd " +
                "ORDER BY timestamp DESC " +
                "LIMIT 1");

            using (MySqlConnection conn = new MySqlConnection(mConnStr))
            {
                try
                {
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        conn.Open();
                        cmd.Prepare();
                        cmd.Parameters.AddWithValue("@companyID", companyID);
                        cmd.Parameters.AddWithValue("@dateStart", string.Format("{0}-{1}-01 00:00:00", historyDateTime.Year, historyDateTime.Month));
                        cmd.Parameters.AddWithValue("@dateEnd", string.Format("{0}-{1}-{2} 23:59:59", historyDateTime.Year, historyDateTime.Month, DateTime.DaysInMonth(historyDateTime.Year, historyDateTime.Month)));

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if ((reader != null) && (reader.HasRows))
                            {
                                while (reader.Read())
                                {
                                    currCreditLimitHistory = DataMgrTools.BuildVrpCreditLimitHistory(reader);
                                    arrCreditLimitHistory.Add(currCreditLimitHistory);
                                }
                            }
                        }

                        conn.Close();
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogEvent(mProjName, String.Format("VrpCreditLimitHistoryRepository GetMonthlyHistory() Exception: {0}", ex.Message), System.Diagnostics.EventLogEntryType.Error);
                }
            }
            return arrCreditLimitHistory;
        }

        public List<VrpCreditLimitHistory> GetYearlyHistory(string companyID, DateTime historyDateTime)
        {
            List<VrpCreditLimitHistory> arrCreditLimitHistory = new List<VrpCreditLimitHistory>();
            VrpCreditLimitHistory currCreditLimitHistory = new VrpCreditLimitHistory();
            string query = string.Format("SELECT * " +
                "FROM vrp_yearly_credit_limit_history " +
                "WHERE company_id = @companyID AND timestamp BETWEEN @dateStart AND @dateEnd " +
                "ORDER BY timestamp DESC " +
                "LIMIT 1");

            using (MySqlConnection conn = new MySqlConnection(mConnStr))
            {
                try
                {
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        conn.Open();
                        cmd.Prepare();
                        cmd.Parameters.AddWithValue("@companyID", companyID);
                        cmd.Parameters.AddWithValue("@dateStart", string.Format("{0}-01-01 00:00:00", historyDateTime.Year));
                        cmd.Parameters.AddWithValue("@dateEnd", string.Format("{0}-12-31 23:59:59", historyDateTime.Year));

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if ((reader != null) && (reader.HasRows))
                            {
                                while (reader.Read())
                                {
                                    currCreditLimitHistory = DataMgrTools.BuildVrpCreditLimitHistory(reader);
                                    arrCreditLimitHistory.Add(currCreditLimitHistory);
                                }
                            }
                        }

                        conn.Close();
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogEvent(mProjName, String.Format("VrpCreditLimitHistoryRepository GetYearlyHistory() Exception: {0}", ex.Message), System.Diagnostics.EventLogEntryType.Error);
                }
            }
            return arrCreditLimitHistory;
        }

        public bool InsertCreditLimitHistory(string companyID, DateTime historyDateTime, int creditLimit, string type)
        {
            bool isSuccess = false;

            string tableName = "";

            if (type == "daily")
            {
                tableName = "vrp_daily_credit_limit_history";
            }
            else if (type == "monthly")
            {
                tableName = "vrp_monthly_credit_limit_history";
            }
            else if (type == "yearly")
            {
                tableName = "vrp_yearly_credit_limit_history";
            }
            else
            {
                return false;
            }

            string query = "INSERT INTO " + tableName + " (company_id, timestamp, credit_limit) VALUES " +
                "(@companyID, @timestamp, @creditLimit)";
           
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
                        cmd.Parameters.AddWithValue("@timestamp", historyDateTime.ToString("yyyy-MM-dd HH:mm:ss"));
                        cmd.Parameters.AddWithValue("@creditLimit", creditLimit);

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
                Logger.LogEvent(mProjName, String.Format("VrpCreditLimitHistoryRepository InsertCreditLimitHistory() Exception: {0}", ex.Message), System.Diagnostics.EventLogEntryType.Error);
            }


            return isSuccess;
        }

    }
}
