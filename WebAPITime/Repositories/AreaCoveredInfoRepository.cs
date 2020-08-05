using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using WebApi.Repositories;
using WebAPITime.HelperTools;
using WebAPITime.Models;

namespace WebAPITime.Repositories
{
    public class AreaCoveredInfoRepository : IAreaCoveredInfoRepository
    {
        string mConnStr = ConfigurationManager.AppSettings["mConnStr"];
        string mProjName = ConfigurationManager.AppSettings["mProjName"];

        public List<AreaCoveredInfo> GetAllByCompanyID(int companyID)
        {
            List<AreaCoveredInfo> arrAreaCovered = new List<AreaCoveredInfo>();
            AreaCoveredInfo currAreaCovered = new AreaCoveredInfo();
            string query = string.Format("SELECT acd.*, acr.region_name " +
                "FROM area_covered_district acd " +
                "LEFT JOIN area_covered_region acr ON acd.area_covered_region_id = acr.acr_id " +
                "WHERE acd.company_id = @companyID");

            try
            {
                using (MySqlConnection conn = new MySqlConnection(mConnStr))
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
                                    currAreaCovered = DataMgrTools.BuildAreaCoveredInfo(reader);
                                    arrAreaCovered.Add(currAreaCovered);
                                }
                            }
                        }

                        conn.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogEvent(mProjName, String.Format("AreaCoveredInfoRepository GetAllByCompanyID() Exception: {0}", ex.Message), System.Diagnostics.EventLogEntryType.Error);
            }

            return arrAreaCovered;
        }
    }
}
