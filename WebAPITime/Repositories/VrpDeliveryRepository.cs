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
    public class VrpDeliveryRepository : IVrpDeliveryRepository
    {
        string mConnStr = ConfigurationManager.AppSettings["mConnStr"];
        string mProjName = ConfigurationManager.AppSettings["mProjName"];

        public List<VrpDelivery> GetDeliveryByIdsAndCustomerId(List<long> deliveryIDs, int customerID)
        {
            List<VrpDelivery> arrDeliveries = new List<VrpDelivery>();
            VrpDelivery currVrpDelivery = new VrpDelivery();
            //string query = string.Format("SELECT * FROM vrp_delivery WHERE delivery_id IN (" + string.Join(",", deliveryIDs) + ") AND customer_id = @CustomerID");
            string query = string.Format("SELECT d.*, GROUP_CONCAT(NULLIF(mii.item_name, '')) AS 'accessories_name' " +
                "FROM vrp_delivery d " +
                "LEFT JOIN main_inventory_item mii ON FIND_IN_SET(mii.main_inventory_item_id, d.accessories) " +
                "WHERE delivery_id IN (" + string.Join(",", deliveryIDs) + ") AND customer_id = @CustomerID " +
                "GROUP BY d.delivery_id");

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
                                    currVrpDelivery = DataMgrTools.BuildVrpDelivery(reader);
                                    arrDeliveries.Add(currVrpDelivery);
                                }
                            }
                        }

                        conn.Close();
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogEvent(mProjName, String.Format("VrpDeliveryRepository GetDeliveryByIdsAndCustomerId() Exception: {0}", ex.Message), System.Diagnostics.EventLogEntryType.Error);
                }
            }
            return arrDeliveries;
        }

        public List<VrpDelivery> GetDeliveryByIds(List<long> deliveryIDs)
        {
            List<VrpDelivery> arrDeliveries = new List<VrpDelivery>();
            VrpDelivery currVrpDelivery = new VrpDelivery();
            string query = string.Format("SELECT * FROM vrp_delivery WHERE delivery_id IN (" + string.Join(",", deliveryIDs) + ")");

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
                                    currVrpDelivery = DataMgrTools.BuildVrpDelivery(reader);
                                    arrDeliveries.Add(currVrpDelivery);
                                }
                            }
                        }

                        conn.Close();
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogEvent(mProjName, String.Format("VrpDeliveryRepository GetDeliveryByIds() Exception: {0}", ex.Message), System.Diagnostics.EventLogEntryType.Error);
                }
            }
            return arrDeliveries;
        }

        public List<string> GetDistinctRouteNo(string deliveryIDs)
        {
            List<string> arrRouteNo = new List<string>();
            string query = string.Format("SELECT DISTINCT route_no FROM vrp_delivery WHERE delivery_id IN (" + deliveryIDs + ")");

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
                    Logger.LogEvent(mProjName, String.Format("VrpDeliveryRepository GetDistinctRouteNo() Exception: {0}", ex.Message), System.Diagnostics.EventLogEntryType.Error);
                }
            }
            return arrRouteNo;
        }

        public bool RemoveNotFeasibleAdhocDeliveryOrder(List<long> arrDeliveryIDs)
        {
            bool isDeleteSuccess = false;

            try
            {
                StringBuilder sCommand = new StringBuilder("");

                using (MySqlConnection mConnection = new MySqlConnection(mConnStr))
                {
                    List<string> RowsUpdate = new List<string>();
                    foreach (long deliveryID in arrDeliveryIDs)
                    {
                        RowsUpdate.Add(string.Format("DELETE FROM vrp_delivery WHERE delivery_id = {0}", deliveryID));
                        RowsUpdate.Add(string.Format("DELETE FROM vrp_delivery_item WHERE delivery_id = {0}", deliveryID));
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
                Logger.LogEvent(mProjName, String.Format("VrpPickupRepository RemoveNotFeasibleAdhocDeliveryOrder() Exception: {0}", ex.Message), System.Diagnostics.EventLogEntryType.Error);
            }

            return isDeleteSuccess;
        }
    }
}
