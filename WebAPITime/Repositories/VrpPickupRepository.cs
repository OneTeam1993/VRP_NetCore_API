using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using WebApi.Repositories;
using WebAPITime.HelperTools;
using WebAPITime.Models;

namespace WebAPITime.Repositories
{
    public class VrpPickupRepository : IVrpPickupRepository
    {
        string mConnStr = ConfigurationManager.AppSettings["mConnStr"];
        string mProjName = ConfigurationManager.AppSettings["mProjName"];

        public List<VrpPickup> GetPickupByIdsAndCustomerId(List<long> pickupIDs, int customerID)
        {
            List<VrpPickup> arrPickups = new List<VrpPickup>();
            VrpPickup currVrpPickup = new VrpPickup();
            string query = string.Format("SELECT * FROM vrp_pickup WHERE pickup_id IN (" + string.Join(",", pickupIDs) + ") AND customer_id = @CustomerID");

            using (MySqlConnection conn = new MySqlConnection(mConnStr))
            {
                try
                {
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        conn.Open();
                        cmd.Prepare();
                        cmd.Parameters.AddWithValue("@CustomerID", customerID);

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if ((reader != null) && (reader.HasRows))
                            {
                                while (reader.Read())
                                {
                                    currVrpPickup = DataMgrTools.BuildVrpPickup(reader);
                                    arrPickups.Add(currVrpPickup);
                                }
                            }
                        }

                        conn.Close();
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogEvent(mProjName, String.Format("VrpPickupRepository GetPickupByIdsAndCustomerId() Exception: {0}", ex.Message), System.Diagnostics.EventLogEntryType.Error);
                }
            }
            return arrPickups;
        }

        public List<VrpPickup> GetPickupByIds(List<long> pickupIDs)
        {
            List<VrpPickup> arrPickups = new List<VrpPickup>();
            VrpPickup currVrpPickup = new VrpPickup();
            string query = string.Format("SELECT * FROM vrp_pickup WHERE pickup_id IN (" + string.Join(",", pickupIDs) + ")");

            using (MySqlConnection conn = new MySqlConnection(mConnStr))
            {
                try
                {
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        conn.Open();
                        cmd.Prepare();

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if ((reader != null) && (reader.HasRows))
                            {
                                while (reader.Read())
                                {
                                    currVrpPickup = DataMgrTools.BuildVrpPickup(reader);
                                    arrPickups.Add(currVrpPickup);
                                }
                            }
                        }

                        conn.Close();
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogEvent(mProjName, String.Format("VrpPickupRepository GetPickupByIds() Exception: {0}", ex.Message), System.Diagnostics.EventLogEntryType.Error);
                }
            }
            return arrPickups;
        }

        public List<string> GetDistinctRouteNo(string pickupIDs)
        {
            List<string> arrRouteNo = new List<string>();
            string query = string.Format("SELECT DISTINCT route_no FROM vrp_pickup WHERE pickup_id IN (" + pickupIDs + ")");

            using (MySqlConnection conn = new MySqlConnection(mConnStr))
            {
                try
                {
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        conn.Open();
                        cmd.Prepare();

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if ((reader != null) && (reader.HasRows))
                            {
                                while (reader.Read())
                                {
                                    arrRouteNo.Add(reader.ToString("route_no"));
                                }
                            }
                        }

                        conn.Close();
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogEvent(mProjName, String.Format("VrpPickupRepository GetDistinctRouteNo() Exception: {0}", ex.Message), System.Diagnostics.EventLogEntryType.Error);
                }
            }
            return arrRouteNo;
        }

        public bool RemoveNotFeasibleAdhocPickupOrder(List<long> arrPickupIDs)
        {
            bool isDeleteSuccess = false;

            try
            {
                StringBuilder sCommand = new StringBuilder("");

                using (MySqlConnection mConnection = new MySqlConnection(mConnStr))
                {
                    List<string> RowsUpdate = new List<string>();
                    foreach (long pickupID in arrPickupIDs)
                    {
                        RowsUpdate.Add(string.Format("DELETE FROM vrp_pickup WHERE pickup_id = {0}", pickupID));
                        RowsUpdate.Add(string.Format("DELETE FROM vrp_pickup_item WHERE pickup_id = {0}", pickupID));
                    }

                    sCommand.Append(string.Join(";", RowsUpdate));
                    mConnection.Open();
                    using (MySqlCommand myCmd = new MySqlCommand(sCommand.ToString(), mConnection))
                    {
                        myCmd.CommandType = CommandType.Text;
                        if (myCmd.ExecuteNonQuery() > 0)
                        {
                            isDeleteSuccess = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogEvent(mProjName, String.Format("VrpPickupRepository RemoveNotFeasibleAdhocPickupOrder() Exception: {0}", ex.Message), System.Diagnostics.EventLogEntryType.Error);
            }

            return isDeleteSuccess;
        }
    }
}
