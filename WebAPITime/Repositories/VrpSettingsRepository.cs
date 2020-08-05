using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using WebApi.Repositories;
using WebAPITime.HelperTools;
using WebAPITime.Models;

namespace WebAPITime.Repositories
{
    public class VrpSettingsRepository : IVrpSettingsRepository
    {
        string mConnStr = ConfigurationManager.AppSettings["mConnStr"];
        string mProjName = ConfigurationManager.AppSettings["mProjName"];

        public List<VrpSettingInfo> GetVrpSettingInfo(string routeNo)
        {
            List<VrpSettingInfo> arrVrpSettings = new List<VrpSettingInfo>();
            VrpSettingInfo currVrpSetting = new VrpSettingInfo();
            string query = string.Format("SELECT vs.*, d.driver_name, GROUP_CONCAT(vsa.area_covered_region_id SEPARATOR ',') AS 'area_covered_region_id', GROUP_CONCAT(acr.region_name SEPARATOR ',') AS 'region_name' " +
                "FROM vrp_settings vs " +
                "LEFT JOIN vrp_settings_area vsa ON vs.vrp_settings_id = vsa.vrp_settings_id " +
                "LEFT JOIN area_covered_region acr ON vsa.area_covered_region_id = acr.acr_id " +
                "LEFT JOIN drivers d ON vs.driver_id = d.driver_id " +
                "WHERE vs.route_no = @routeNo " +
                "GROUP BY vs.vrp_settings_id");

            using (MySqlConnection conn = new MySqlConnection(mConnStr))
            {
                try
                {
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        conn.Open();
                        cmd.Prepare();
                        cmd.Parameters.AddWithValue("@routeNo", routeNo);

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if ((reader != null) && (reader.HasRows))
                            {
                                while (reader.Read())
                                {
                                    currVrpSetting = DataMgrTools.BuildVrpSettingInfo(reader);
                                    arrVrpSettings.Add(currVrpSetting);
                                }
                            }
                        }

                        conn.Close();
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogEvent(mProjName, String.Format("VrpSettingsRepository GetVrpSettingInfo(routeNo) Exception: {0}", ex.Message), System.Diagnostics.EventLogEntryType.Error);
                }
            }
            return arrVrpSettings;
        }

        public VrpSettingInfo GetVrpSettingInfo(string routeNo, long driverID)
        {
            VrpSettingInfo currVrpSetting = new VrpSettingInfo();
            //string query = string.Format("SELECT * FROM vrp_settings WHERE route_no = @routeNo AND driver_id = @driverID");
            string query = string.Format("SELECT vs.*, d.driver_name, GROUP_CONCAT(vsa.area_covered_region_id SEPARATOR ',') AS 'area_covered_region_id', GROUP_CONCAT(acr.region_name SEPARATOR ',') AS 'region_name' " +
                "FROM vrp_settings vs " +
                "LEFT JOIN vrp_settings_area vsa ON vs.vrp_settings_id = vsa.vrp_settings_id " +
                "LEFT JOIN area_covered_region acr ON vsa.area_covered_region_id = acr.acr_id " +
                "LEFT JOIN drivers d ON vs.driver_id = d.driver_id " +
                "WHERE vs.route_no = @routeNo AND vs.driver_id = @driverID " +
                "GROUP BY vs.vrp_settings_id");

            using (MySqlConnection conn = new MySqlConnection(mConnStr))
            {
                try
                {
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        conn.Open();
                        cmd.Prepare();
                        cmd.Parameters.AddWithValue("@routeNo", routeNo);
                        cmd.Parameters.AddWithValue("@driverID", driverID);

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if ((reader != null) && (reader.HasRows))
                            {
                                while (reader.Read())
                                {
                                    currVrpSetting = DataMgrTools.BuildVrpSettingInfo(reader);
                                }
                            }
                        }

                        conn.Close();
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogEvent(mProjName, String.Format("VrpSettingsRepository GetVrpSettingInfo(routeNo, driverID) Exception: {0}", ex.Message), System.Diagnostics.EventLogEntryType.Error);
                }
            }
            return currVrpSetting;
        }
    }
}
