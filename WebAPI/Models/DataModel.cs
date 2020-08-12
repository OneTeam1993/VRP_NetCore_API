using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebAPITime.Repositories;
using WebAPI;
using WebAPI.Repository;

namespace VrpModel
{
    public class DataModel
    {
        private string mConnStr = "server=149.28.195.203;uid=root;pwd=@c3c0M;database=vrp;max pool size=500;default command timeout=999999;";
        private string mProjName = "VRP";

        public long[,] DistanceMatrix;
        public static int batchSize = 4;
        public int VehicleNumber = 4; 
        public int Depot = 0;
        public long[] Demands;
        public long[] VehicleCapacities = { 10, 10, 10, 10 };
        public int[][] PickupsDeliveries = {
          new int[] {1, 6},
          //new int[] {2, 10},
          //new int[] {4, 3},
          //new int[] {5, 9},
          //new int[] {7, 8},
          //new int[] {15, 11},
          //new int[] {13, 12},
          //new int[] {16, 14},
        };

        public DataModel(long id)
        {
            InitialLocationInfo currLocation = new InitialLocationInfo();
            List<string> location = new List<string>();
            List<long> demands = new List<long>();

            //===========================================
            try
            {
                #region GET LOCATIONS, DEMANDS
                string query = string.Format("SELECT * FROM init_locations WHERE temp_loc_id = {0}", id);
                using (MySqlConnection conn = new MySqlConnection(mConnStr))
                {
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        conn.Open();
                        MySqlDataReader reader = cmd.ExecuteReader();

                        while (reader.Read())
                        {
                            currLocation = DataMgrTools.BuildInitLocations(reader);
                            location.Add(String.Format("{0},{1}", currLocation.Long, currLocation.Lat));
                            demands.Add(currLocation.Demands);
                        }
                        conn.Close();
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                Logger.LogEvent(mProjName, String.Format("Data Model Get Locations: {0}", ex.Message), System.Diagnostics.EventLogEntryType.Error);
            }

            Demands = demands.ToArray();
            DistanceMatrix = new long[location.Count, location.Count];
            Task<long[,]> task = CallService(location);
            task.Wait();
            DistanceMatrix = task.Result;
            
        }

        public static async Task<long[,]> CallService(List<string> location)
        {
            VrpInfo currVrp = new VrpInfo();
            ResponseModelLocationIQ responseModel = new ResponseModelLocationIQ();
            Service service = new Service();
            string origin = string.Empty;
            string destination = string.Empty;
            long[,] distanceArr = new long[location.Count, location.Count];

            List<string> newlist = new List<string>();

            int listCount = batchSize;
            int remainder = location.Count % batchSize;
            int numOfBatches = location.Count / batchSize;
            numOfBatches = remainder > 0 ? numOfBatches + 1 : numOfBatches;

            try
            {
                for (int i = 0; i < numOfBatches; i++)
                {
                    if (i * batchSize + listCount > location.Count)
                        listCount = remainder;

                    newlist = location.GetRange(i * batchSize, listCount);
                    origin = string.Join(";", newlist);
                    destination = string.Join(";", location);
                    var result = await service.GetAsyncDistance(origin, destination);
                    if (result != null && result.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        responseModel = JsonConvert.DeserializeObject<ResponseModelLocationIQ>(result.Content);

                        for (int j = 0; j < listCount; j++)
                        {
                            for (int k = 0; k < location.Count; k++)
                            {
                                if (origin == location[k])
                                {
                                    distanceArr[i * batchSize + j, k] = 0;
                                }
                                else
                                {
                                    distanceArr[i * batchSize + j, k] = responseModel.routes[0].legs[k].distance;
                                }
                            }
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Call Service Exception: {0}", ex.Message);
                currVrp.ErrorMessage = String.Format("Call Service Exception: {0}", ex.Message);
            }


            return distanceArr;
        }


    }
}
