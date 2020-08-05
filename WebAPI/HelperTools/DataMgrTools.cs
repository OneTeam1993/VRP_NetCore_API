using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VrpModel;


namespace WebAPI
{
    public class DataMgrTools
    {
        private static string mProjName = "VRP";


        public static InitialLocationInfo BuildInitLocations(MySqlDataReader dbRdr)
        {
            InitialLocationInfo udtLocation = new InitialLocationInfo();
            try
            {
                udtLocation.InitialLocationID = dbRdr.ToInt64("init_locations_id");
                udtLocation.Lat = dbRdr.ToDouble("lat");
                udtLocation.Long = dbRdr.ToDouble("long");
                udtLocation.Demands = dbRdr.ToInt32("demands");
            }
            catch (Exception ex)
            {
                // log error
                Logger.LogEvent(mProjName, "BuildInitLocations ERROR: " + ex.Message, System.Diagnostics.EventLogEntryType.Error);
            }
            return udtLocation;
        }

        public static NodeInfo BuildInitNodes(MySqlDataReader dbRdr)
        {
            NodeInfo udtLocation = new NodeInfo();
            try
            {
                udtLocation.NodeID = dbRdr.ToInt64("nodes");
                udtLocation.Address = dbRdr.ToString("address");
            }
            catch (Exception ex)
            {
                // log error
                Logger.LogEvent(mProjName, "BuildInitNodes ERROR: " + ex.Message, System.Diagnostics.EventLogEntryType.Error);
            }
            return udtLocation;
        }

        public static DroppedNodes BuildDroppedNodes(MySqlDataReader dbRdr)
        {
            DroppedNodes udtLocation = new DroppedNodes();
            try
            {
                udtLocation.NodeID = dbRdr.ToInt64("nodes");
                udtLocation.Address = dbRdr.ToString("address");
            }
            catch (Exception ex)
            {
                // log error
                Logger.LogEvent(mProjName, "BuildDroppedNodes ERROR: " + ex.Message, System.Diagnostics.EventLogEntryType.Error);
            }
            return udtLocation;
        }


    }
}