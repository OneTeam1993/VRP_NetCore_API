using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using VrpModel;
using WebApi.Repositories;

namespace WebAPI.Repository
{
    public class InitialLocationRepository : IInitialLocationRepository
    {
        //private string mConnStr = "server=localhost;uid=root;pwd=@c3c0M;database=vrp;max pool size=500;default command timeout=999999;";
        private string mConnStr = "server=149.28.195.203;uid=root;pwd=@c3c0M;database=vrp;max pool size=500;default command timeout=999999;";
        private string mProjName = "VRP";

        public InitialLocationInfo Get(long id)
        {
            InitialLocationInfo currLocation = new InitialLocationInfo();
            string query = string.Format("SELECT * FROM init_locations WHERE init_locations_id = {0}", id);

            using (MySqlConnection conn = new MySqlConnection(mConnStr))
            {
                try
                {
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        conn.Open();
                        MySqlDataReader reader = cmd.ExecuteReader();

                        while (reader.Read())
                        {
                            currLocation = DataMgrTools.BuildInitLocations(reader);
                        }
                        conn.Close();
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogEvent(mProjName, String.Format("Initial Location Repository Get: {0}",ex.Message), System.Diagnostics.EventLogEntryType.Error);
                }
            }
            return currLocation;
        }

        public NodeInfo GetInitLoc(long id)
        {
            NodeInfo currLocation = new NodeInfo();
            List<NodeInfo> arrNodes = new List<NodeInfo>();
            string query = string.Format("SELECT * FROM init_locations WHERE nodes = {0}", id);

            using (MySqlConnection conn = new MySqlConnection(mConnStr))
            {
                try
                {
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        conn.Open();
                        MySqlDataReader reader = cmd.ExecuteReader();

                        while (reader.Read())
                        {
                            currLocation = DataMgrTools.BuildInitNodes(reader);
                            arrNodes.Add(currLocation);
                        }
                        conn.Close();
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogEvent(mProjName, String.Format("GetInitLoc: {0}", ex.Message), System.Diagnostics.EventLogEntryType.Error);
                }
            }
            return currLocation;
        }

        public string GetInitAddress(long id)
        {
            NodeInfo currLocation = new NodeInfo();
            string query = string.Format("SELECT * FROM init_locations WHERE nodes = {0}", id);

            using (MySqlConnection conn = new MySqlConnection(mConnStr))
            {
                try
                {
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        conn.Open();
                        MySqlDataReader reader = cmd.ExecuteReader();

                        while (reader.Read())
                        {
                            currLocation = DataMgrTools.BuildInitNodes(reader);
                        }
                        conn.Close();
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogEvent(mProjName, String.Format("GetInitLoc: {0}", ex.Message), System.Diagnostics.EventLogEntryType.Error);
                }
            }
            return currLocation.Address;
        }

        public DroppedNodes GetDroppedNodes(long id)
        {
            DroppedNodes currLocation = new DroppedNodes();
            string query = string.Format("SELECT * FROM init_locations WHERE nodes = {0}", id);

            using (MySqlConnection conn = new MySqlConnection(mConnStr))
            {
                try
                {
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        conn.Open();
                        MySqlDataReader reader = cmd.ExecuteReader();

                        while (reader.Read())
                        {
                            currLocation = DataMgrTools.BuildDroppedNodes(reader);
                        }
                        conn.Close();
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogEvent(mProjName, String.Format("GetDroppedNodes: {0}", ex.Message), System.Diagnostics.EventLogEntryType.Error);
                }
            }
            return currLocation;
        }

    }
}