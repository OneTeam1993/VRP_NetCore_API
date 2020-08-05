using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using VrpModel;
using WebApi.Repositories;
using WebAPITime.HelperTools;
using WebAPITime.Models;

namespace WebAPITime.Repositories
{
    public class InitialLocationRepository : IInitialLocationRepository
    {
        string mConnStr = ConfigurationManager.AppSettings["mConnStr"];
        string mProjName = ConfigurationManager.AppSettings["mProjName"];

        public List<PickupDeliveryInfo> GetLocationInfo(string routeNo)
        {
            List<PickupDeliveryInfo> arrLocations = new List<PickupDeliveryInfo>();
            PickupDeliveryInfo currLocation = new PickupDeliveryInfo();
            string query = string.Format("SELECT * FROM view_vrp_pickup_delivery WHERE route_no = @routeNo");

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
                                    currLocation = DataMgrTools.BuildInitialLocationInfo(reader);
                                    arrLocations.Add(currLocation);
                                }
                            }
                        }

                        conn.Close();
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogEvent(mProjName, String.Format("InitialLocationRepository GetLocationInfo() Exception: {0}", ex.Message), System.Diagnostics.EventLogEntryType.Error);
                }
            }
            return arrLocations;
        }

        public List<PickupDeliveryInfo> GetAssignedLocationInfoByRouteNoDriver(string routeNo, long driverID)
        {
            List<PickupDeliveryInfo> arrLocations = new List<PickupDeliveryInfo>();
            PickupDeliveryInfo currLocation = new PickupDeliveryInfo();
            string query = string.Format("SELECT * FROM view_vrp_pickup_delivery WHERE route_no = @routeNo AND driver_id = @driverID AND isAssign = 1");

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
                                    currLocation = DataMgrTools.BuildInitialLocationInfo(reader);
                                    arrLocations.Add(currLocation);
                                }
                            }
                        }

                        conn.Close();
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogEvent(mProjName, String.Format("InitialLocationRepository GetAssignedLocationInfoByRouteNoDriver() Exception: {0}", ex.Message), System.Diagnostics.EventLogEntryType.Error);
                }
            }
            return arrLocations;
        }

        public DroppedNodes GetDroppedNodes(string routeNo, int nodeID)
        {
            DroppedNodes currLocation = new DroppedNodes();
            string query = string.Format("SELECT * FROM vrp_init_locations WHERE route_no = @routeNo AND node = @node");

            using (MySqlConnection conn = new MySqlConnection(mConnStr))
            {
                try
                {
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        conn.Open();
                        cmd.Prepare();
                        cmd.Parameters.AddWithValue("@routeNo", routeNo);
                        cmd.Parameters.AddWithValue("@node", nodeID);

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if ((reader != null) && (reader.HasRows))
                            {
                                while (reader.Read())
                                {
                                    currLocation = DataMgrTools.BuildDroppedNodes(reader);
                                }
                            }                              
                        }
                        conn.Close();
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogEvent(mProjName, String.Format("InitialLocationRepository GetDroppedNodes() Exception: {0}", ex.Message), System.Diagnostics.EventLogEntryType.Error);
                }
            }
            return currLocation;
        }
    }
}
