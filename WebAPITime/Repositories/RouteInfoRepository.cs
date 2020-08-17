using Microsoft.AspNetCore.Routing;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using VrpModel;
using WebAPITime.Repositories;
using WebAPITime.HelperTools;
using WebAPITime.Models;
using WebAPITime.Services;
using System.Threading.Tasks;

namespace WebAPITime.Repositories
{
    public class RouteInfoRepository : IRouteInfoRepository
    {
        string mConnStr = ConfigurationManager.AppSettings["mConnStr"];
        string mProjName = ConfigurationManager.AppSettings["mProjName"];

        private static readonly IVrpPickupRepository repoVrpPickup = new VrpPickupRepository();
        private static readonly IVrpDeliveryRepository repoVrpDelivery = new VrpDeliveryRepository();
        private static readonly IInitialLocationRepository repoInitialLocation = new InitialLocationRepository();
        private static readonly IVrpSettingsRepository repoVrpSettings = new VrpSettingsRepository();
        private static readonly IRouteInfoRepository repoRouteInfo = new RouteInfoRepository();
        private static readonly IAreaCoveredInfoRepository repoAreaCoveredInfo = new AreaCoveredInfoRepository();
        private static readonly IVrpRepository repoVrpInfo = new VrpRepository();
        private static readonly IEventRepository repoEvent = new EventRepository();
        private static readonly IVrpLocationRequestsRepository repoVrpLocationRequest = new VrpLocationRequestsRepository();

        public IEnumerable<RouteInfo> GetAllRouteInfoByDriver(string companyID, string driverID, string flag, DateTime timeWindowStart, DateTime timeWindowEnd)
        {
            List<RouteInfo> arrRoutes = new List<RouteInfo>();
            RouteInfo currRoute = new RouteInfo();
            List<long> arrPickupIds = new List<long>();
            List<long> arrDeliveryIds = new List<long>();
            Dictionary<long, CustomerInfo> pickupIdToCustomerMap = new Dictionary<long, CustomerInfo>();
            Dictionary<long, CustomerInfo> deliveryIdToCustomerMap = new Dictionary<long, CustomerInfo>();
            string query = "";
            string driverFilter = "";

            if (driverID != null)
            {
                driverFilter = " AND driver_id IN (" + driverID + ")";
            }

            if (!(timeWindowStart == Convert.ToDateTime("1/1/0001 00:00:00") || timeWindowEnd == Convert.ToDateTime("1/1/0001 00:00:00")))
            {
                string dateFilter = "";
                string companyFilter = "";

                if (!(timeWindowStart == Convert.ToDateTime("1/1/0001 00:00:00") || timeWindowEnd == Convert.ToDateTime("1/1/0001 00:00:00")))
                {
                    dateFilter = " AND DATE(arrival_time) BETWEEN '" + timeWindowStart.ToString("yyyy-MM-dd") + "' AND '" + timeWindowEnd.ToString("yyyy-MM-dd") + "'";
                }

                if(companyID != null)
                {
                    companyFilter = " AND company_id = " + companyID;
                }
            
                query = string.Format("SELECT * FROM view_vrp_route_info WHERE flag IN (" + flag + ")" + dateFilter + driverFilter + companyFilter + " ORDER BY DATE(arrival_time) ASC, route_no ASC, driver_id ASC, sequence ASC");
            }
            else
            {
                query = string.Format("SELECT * FROM view_vrp_route_info WHERE DATE(arrival_time) = CURDATE()" + driverFilter + " AND flag IN (" + flag + ") ORDER BY route_no ASC, sequence ASC");
            }
            
            try
            {
                using (MySqlConnection conn = new MySqlConnection(mConnStr))
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
                                    currRoute = DataMgrTools.BuildRouteInfo(reader);
                                    arrRoutes.Add(currRoute);
                                }
                            }
                        }

                        conn.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogEvent(mProjName, String.Format("RouteInfoRepository GetAllRouteInfoByDriver() Exception: {0}", ex.Message), System.Diagnostics.EventLogEntryType.Error);
            }

            foreach(RouteInfo routeInfo in arrRoutes)
            {
                foreach (long pickupID in routeInfo.PickupDeliveryInfo.PickupIDs)
                {
                    if (!arrPickupIds.Contains(pickupID))
                    {
                        arrPickupIds.Add(pickupID);
                    }
                }

                foreach (long deliveryID in routeInfo.PickupDeliveryInfo.DeliveryIDs)
                {
                    if (!arrDeliveryIds.Contains(deliveryID))
                    {
                        arrDeliveryIds.Add(deliveryID);
                    }
                }
            }

            if (arrPickupIds.Count > 0)
            {
                CustomerInfo currCustomerInfo = new CustomerInfo();

                query = string.Format("SELECT p.pickup_ID, p.customer_id, c.name " +
                    "FROM vrp_pickup p " +
                    "LEFT JOIN customers c ON p.customer_id = c.cus_id " +
                    "WHERE p.pickup_ID IN (" + string.Join(",", arrPickupIds) + ")");

                try
                {
                    using (MySqlConnection conn = new MySqlConnection(mConnStr))
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
                                        long pickupID = reader.ToInt64("pickup_id");
                                        currCustomerInfo = DataMgrTools.BuildCustomerInfo(reader);

                                        if (!pickupIdToCustomerMap.ContainsKey(pickupID))
                                        {
                                            pickupIdToCustomerMap[pickupID] = currCustomerInfo;
                                        }
                                    }
                                }
                            }

                            conn.Close();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogEvent(mProjName, String.Format("RouteInfoRepository GetAllRouteInfoByDriver()-PickupCustomer Exception: {0}", ex.Message), System.Diagnostics.EventLogEntryType.Error);
                }
            }

            if (arrDeliveryIds.Count > 0)
            {
                CustomerInfo currCustomerInfo = new CustomerInfo();

                query = string.Format("SELECT d.delivery_id, d.customer_id, c.name " +
                    "FROM vrp_delivery d " +
                    "LEFT JOIN customers c ON d.customer_id = c.cus_id " +
                    "WHERE d.delivery_id IN (" + string.Join(",", arrDeliveryIds) + ")");

                try
                {
                    using (MySqlConnection conn = new MySqlConnection(mConnStr))
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
                                        long deliveryID = reader.ToInt64("delivery_id");
                                        currCustomerInfo = DataMgrTools.BuildCustomerInfo(reader);

                                        if (!deliveryIdToCustomerMap.ContainsKey(deliveryID))
                                        {
                                            deliveryIdToCustomerMap[deliveryID] = currCustomerInfo;
                                        }
                                    }
                                }
                            }

                            conn.Close();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogEvent(mProjName, String.Format("RouteInfoRepository GetAllRouteInfoByDriver()-DeliveryCustomer Exception: {0}", ex.Message), System.Diagnostics.EventLogEntryType.Error);
                }
            }

            foreach (RouteInfo routeInfo in arrRoutes)
            {
                
                Dictionary<int, Customer> customerMap = new Dictionary<int, Customer>();
                foreach (long pickupID in routeInfo.PickupDeliveryInfo.PickupIDs)
                {
                    int customerID = pickupIdToCustomerMap[pickupID].CustomerID;
                    if (!customerMap.ContainsKey(customerID))
                    {
                        Customer customer = new Customer();
                        customer.CustomerID = customerID;
                        customer.CustomerName = pickupIdToCustomerMap[pickupID].Name;
                        customer.PickupIDs = new List<long>();
                        customer.DeliveryIDs = new List<long>();
                        customerMap[customerID] = customer;
                    }

                    customerMap[customerID].PickupIDs.Add(pickupID);
                }

                foreach (long deliveryID in routeInfo.PickupDeliveryInfo.DeliveryIDs)
                {
                    int customerID = deliveryIdToCustomerMap[deliveryID].CustomerID;
                    if (!customerMap.ContainsKey(customerID))
                    {
                        Customer customer = new Customer();
                        customer.CustomerID = customerID;
                        customer.CustomerName = deliveryIdToCustomerMap[deliveryID].Name;
                        customer.PickupIDs = new List<long>();
                        customer.DeliveryIDs = new List<long>();
                        customerMap[customerID] = customer;
                    }

                    customerMap[customerID].DeliveryIDs.Add(deliveryID);
                }

                routeInfo.Customers = new List<Customer>();
                foreach (KeyValuePair<int, Customer> entry in customerMap)
                {
                    routeInfo.Customers.Add(entry.Value);
                }
            }

            return arrRoutes.ToArray();
        }

        public List<RouteInfo> GetAllRouteInfoByRouteNoDriver(string routeNo, long driverID)
        {
            List<RouteInfo> arrRoutes = new List<RouteInfo>();
            RouteInfo currRoute = new RouteInfo();
            string query = string.Format("SELECT * FROM view_vrp_route_info WHERE route_no = @routeNo AND driver_id = @driverID ORDER BY sequence ASC");

            try
            {
                using (MySqlConnection conn = new MySqlConnection(mConnStr))
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
                                    currRoute = DataMgrTools.BuildRouteInfo(reader);
                                    arrRoutes.Add(currRoute);
                                }
                            }
                        }

                        conn.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogEvent(mProjName, String.Format("RouteInfoRepository GetAllRouteInfoByRouteNoDriver() Exception: {0}", ex.Message), System.Diagnostics.EventLogEntryType.Error);
            }

            return arrRoutes;
        }

        public List<RouteInfo> GetAllRouteInfoByRouteNo(string routeNo)
        {
            List<RouteInfo> arrRoutes = new List<RouteInfo>();
            RouteInfo currRoute = new RouteInfo();
            string query = string.Format("SELECT * FROM view_vrp_route_info WHERE route_no = @routeNo");
          
            try
            {
                using (MySqlConnection conn = new MySqlConnection(mConnStr))
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
                                    currRoute = DataMgrTools.BuildRouteInfo(reader);
                                    arrRoutes.Add(currRoute);
                                }
                            }
                        }

                        conn.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogEvent(mProjName, String.Format("RouteInfoRepository GetAllRouteInfoByRouteNo() Exception: {0}", ex.Message), System.Diagnostics.EventLogEntryType.Error);
            }
            
            return arrRoutes;
        }

        public List<RouteInfo> GetAllRouteInfoByRouteNoFlag(string routeNo, string flag)
        {
            List<RouteInfo> arrRoutes = new List<RouteInfo>();
            RouteInfo currRoute = new RouteInfo();

            string[] routeNos = routeNo.Split(",");
            string formattedRouteNo = "";
            foreach(string route in routeNos)
            {
                if (formattedRouteNo.Length > 0)
                {
                    formattedRouteNo += ",";
                }
                formattedRouteNo += "'" + route + "'";
            }

            if(formattedRouteNo.Length > 0)
            {
                string query = string.Format("SELECT * FROM view_vrp_route_info WHERE route_no IN (" + formattedRouteNo + ") AND flag IN (" + flag + ") ORDER BY DATE(arrival_time) ASC, route_no ASC, driver_id ASC, sequence ASC");

                try
                {
                    using (MySqlConnection conn = new MySqlConnection(mConnStr))
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
                                        currRoute = DataMgrTools.BuildRouteInfo(reader);
                                        arrRoutes.Add(currRoute);
                                    }
                                }
                            }

                            conn.Close();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogEvent(mProjName, String.Format("RouteInfoRepository GetAllRouteInfoByRouteNoFlag() Exception: {0}", ex.Message), System.Diagnostics.EventLogEntryType.Error);
                }
            }
            
            return arrRoutes;
        }

        public RouteInfo Get(long id)
        {
            RouteInfo currRoute = new RouteInfo();
            string query = string.Format("SELECT * FROM view_vrp_route_info WHERE vrp_routes_id = @RouteID");

            using (MySqlConnection conn = new MySqlConnection(mConnStr))
            {
                try
                {
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        conn.Open();
                        cmd.Prepare();
                        cmd.Parameters.AddWithValue("@RouteID", id);
                        //cmd.Parameters.AddWithValue("@flag", flag);

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if ((reader != null) && (reader.HasRows))
                            {
                                while (reader.Read())
                                {
                                    currRoute = DataMgrTools.BuildRouteInfo(reader);
                                }
                            }
                        }

                        conn.Close();
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogEvent(mProjName, String.Format("RouteInfoRepository Get() Exception: {0}", ex.Message), System.Diagnostics.EventLogEntryType.Error);
                }
            }
            return currRoute;
        }

        public VrpInfo AddGeneratedRoutes(string routeNo, VrpInfo vrpInfo, List<VrpSettingInfo> arrVrpSettings, List<PickupDeliveryInfo> arrAllLocation, bool isAdHocCalculation, List<RouteInfo> arrRouteInfo = null)
        {
            #region Region: (Delete statement for testing with same routeNo and for adhoc order)
            string query = "";
            if (isAdHocCalculation)
            {
                query = string.Format("DELETE FROM vrp_routes WHERE route_no = @routeNo AND driver_id = @driverID");
            }
            else
            {
                query = string.Format("DELETE FROM vrp_routes WHERE route_no = @routeNo; UPDATE vrp_pickup SET isAssign = 0 WHERE route_no = @routeNo; UPDATE vrp_delivery SET isAssign = 0 WHERE route_no = @routeNo");
            }
            

            using (MySqlConnection conn = new MySqlConnection(mConnStr))
            {
                try
                {
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        conn.Open();
                        cmd.Prepare();
                        cmd.Parameters.AddWithValue("@routeNo", routeNo);
                        if (isAdHocCalculation)
                        {
                            cmd.Parameters.AddWithValue("@driverID", arrVrpSettings[0].DriverID);
                        }
                        cmd.ExecuteNonQuery();

                        conn.Close();
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogEvent(mProjName, String.Format("RouteInfoRepository AddGeneratedRoutes(): {0}", ex.Message), System.Diagnostics.EventLogEntryType.Error);
                }
            }
            #endregion

            StringBuilder sCommand = new StringBuilder(
                "INSERT INTO vrp_routes (route_no, asset_id, driver_id, order_type, pickup_ids, delivery_ids, pickup_from_ids, route_distance, route_time, arrival_time, departure_time, sequence, feature_id, accessories, status, flag, timestamp, rx_time) VALUES ");
            try
            {
                using (MySqlConnection mConnection = new MySqlConnection(mConnStr))
                {                   
                    if(vrpInfo.Vehicles != null)
                    {
                        string currentDateTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
                        List<string> RowsInsert = new List<string>();
                        List<string> RowsUpdate = new List<string>();

                        for (int i = 0; i < vrpInfo.Vehicles.Length; i++)
                        {
                            Vehicle vehicle = vrpInfo.Vehicles[i];

                            for (int j = 0; j < vehicle.Nodes.Length; j++)
                            {
                                int flag = 0;
                                Node node = vehicle.Nodes[j];

                                arrAllLocation[node.NodeID].PickupIDs = arrAllLocation[node.NodeID].PickupIDs ?? new List<long>();
                                arrAllLocation[node.NodeID].DeliveryIDs = arrAllLocation[node.NodeID].DeliveryIDs ?? new List<long>();
                                arrAllLocation[node.NodeID].PickupFromIDs = arrAllLocation[node.NodeID].PickupFromIDs ?? new List<long>();
                                arrAllLocation[node.NodeID].FeatureIDs = arrAllLocation[node.NodeID].FeatureIDs ?? new List<int>();
                                arrAllLocation[node.NodeID].Accessories = arrAllLocation[node.NodeID].Accessories ?? new List<long>();

                                if (isAdHocCalculation && arrRouteInfo != null)
                                {
                                    flag = 1;
                                    foreach (RouteInfo routeInfo in arrRouteInfo)
                                    {
                                        string nodeCoordinate = string.Format("{0},{1}", arrAllLocation[node.NodeID].Lat, arrAllLocation[node.NodeID].Long);
                                        string routeCoordinate = string.Format("{0},{1}", routeInfo.PickupDeliveryInfo.Lat, routeInfo.PickupDeliveryInfo.Long);
                                        if (nodeCoordinate == routeCoordinate)
                                        {
                                            flag = routeInfo.Flag;
                                            break;
                                        }                                       
                                    }
                                }

                                RowsInsert.Add(string.Format("('{0}','{1}','{2}','{3}',{4},{5},{6},'{7}','{8}','{9}',{10},'{11}',{12},{13},'{14}','{15}','{16}','{17}')",
                                    routeNo, arrVrpSettings[i].AssetID, arrVrpSettings[i].DriverID, arrAllLocation[node.NodeID].OrderType,
                                    string.Join(",", arrAllLocation[node.NodeID].PickupIDs).Trim() == "" ? "NULL" : "'" + string.Join(",", arrAllLocation[node.NodeID].PickupIDs).Trim() + "'",
                                    string.Join(",", arrAllLocation[node.NodeID].DeliveryIDs).Trim() == "" ? "NULL" : "'" + string.Join(",", arrAllLocation[node.NodeID].DeliveryIDs).Trim() + "'",
                                    string.Join(",", arrAllLocation[node.NodeID].PickupFromIDs).Trim() == "" ? "NULL" : "'" + string.Join(",", arrAllLocation[node.NodeID].PickupFromIDs).Trim() + "'",
                                    node.DistanceOfRoute, node.TimeOfRoute,
                                    node.ArrivalTime, node.DepartureTime == null ? "NULL" : "\'" + node.DepartureTime + "\'", 
                                    j,
                                    string.Join(",", arrAllLocation[node.NodeID].FeatureIDs).Trim() == "" ? "NULL" : "'" + string.Join(",", arrAllLocation[node.NodeID].FeatureIDs).Trim() + "'",
                                    string.Join(",", arrAllLocation[node.NodeID].Accessories).Trim() == "" ? "NULL" : "'" + string.Join(",", arrAllLocation[node.NodeID].Accessories).Trim() + "'",
                                    node.Status, flag, currentDateTime, currentDateTime));

                                if (arrAllLocation[node.NodeID].OrderType != "Pickup and Delivery")
                                {
                                    string tableName = arrAllLocation[node.NodeID].OrderType == "Pickup" ? "vrp_pickup" : "vrp_delivery";
                                    string colID = arrAllLocation[node.NodeID].OrderType == "Pickup" ? "pickup_id" : "delivery_id";
                                    List<long> arrValue = arrAllLocation[node.NodeID].OrderType == "Pickup" ? arrAllLocation[node.NodeID].PickupIDs : arrAllLocation[node.NodeID].DeliveryIDs;

                                    foreach (long id in arrValue)
                                    {
                                        RowsUpdate.Add(string.Format("UPDATE {0} SET driver_id = {1}, rx_time = '{2}' WHERE {3} = {4}", tableName, arrVrpSettings[i].DriverID, currentDateTime, colID, id));
                                        RowsUpdate.Add(string.Format("UPDATE main_inventory_history SET asset_id = {1}, driver_id = {2}, rx_time = '{3}' WHERE {4} = {5}", tableName, arrVrpSettings[i].AssetID, arrVrpSettings[i].DriverID, currentDateTime, colID, id));
                                    }

                                }
                                else
                                {
                                    foreach (long pickupID in arrAllLocation[node.NodeID].PickupIDs)
                                    {
                                        RowsUpdate.Add(string.Format("UPDATE vrp_pickup SET driver_id = {0}, rx_time = '{1}' WHERE pickup_id = {2}", arrVrpSettings[i].DriverID, currentDateTime, pickupID));
                                        RowsUpdate.Add(string.Format("UPDATE main_inventory_history SET asset_id = {0}, driver_id = {1}, rx_time = '{2}' WHERE pickup_id = {3}", arrVrpSettings[i].AssetID, arrVrpSettings[i].DriverID, currentDateTime, pickupID));
                                    }

                                    foreach (long deliveryID in arrAllLocation[node.NodeID].DeliveryIDs)
                                    {
                                        RowsUpdate.Add(string.Format("UPDATE vrp_delivery SET driver_id = {0}, rx_time = '{1}' WHERE delivery_id = {2}", arrVrpSettings[i].DriverID, currentDateTime, deliveryID));
                                        RowsUpdate.Add(string.Format("UPDATE main_inventory_history SET asset_id = {0}, driver_id = {1}, rx_time = '{2}' WHERE delivery_id = {3}", arrVrpSettings[i].AssetID, arrVrpSettings[i].DriverID, currentDateTime, deliveryID));
                                    }
                                }
                            }

                        }

                        sCommand.Append(string.Join(",", RowsInsert));
                        sCommand.Append(";");
                        sCommand.Append(string.Join(";", RowsUpdate));
                        mConnection.Open();
                        using (MySqlCommand myCmd = new MySqlCommand(sCommand.ToString(), mConnection))
                        {
                            myCmd.CommandType = CommandType.Text;
                            myCmd.ExecuteNonQuery();
                        }
                        mConnection.Close();
                    }                                      
                }
            }
            catch (Exception ex)
            {
                Logger.LogEvent(mProjName, String.Format("RouteInfoRepository AddGeneratedRoutes() Exception: {0}", ex.Message), System.Diagnostics.EventLogEntryType.Error);
                vrpInfo.ErrorMessage = String.Format("RouteInfoRepository AddGeneratedRoutes() Exception: {0}", ex.Message);
            }

            return vrpInfo;
        }

        public async Task<ResponseSaveRoutes> SaveRoutesAsync(string routeNo)
        {
            ResponseSaveRoutes responseSaveRoutes = new ResponseSaveRoutes();
            responseSaveRoutes.IsSuccess = false;
            string currentDateTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            List<RouteInfo> arrRoutes = GetAllRouteInfoByRouteNo(routeNo);            

            try
            {
                using (MySqlConnection conn = new MySqlConnection(mConnStr))
                {
                    StringBuilder sCommand = new StringBuilder(
                            String.Format("UPDATE vrp_routes SET flag = '{0}', rx_time = '{1}' WHERE route_no = '{2}'", 1, currentDateTime, routeNo));

                    List<string> RowsUpdate = new List<string>();

                    for (int i = 0; i < arrRoutes.Count; i++)
                    {
                        foreach(long pickupID in arrRoutes[i].PickupDeliveryInfo.PickupIDs)
                        {
                            RowsUpdate.Add(string.Format("UPDATE vrp_pickup SET isAssign = {0}, rx_time = '{1}' WHERE pickup_id = {2}", 1, currentDateTime, pickupID));
                        }
                        foreach (long deliveryID in arrRoutes[i].PickupDeliveryInfo.DeliveryIDs)
                        {
                            RowsUpdate.Add(string.Format("UPDATE vrp_delivery SET isAssign = {0}, rx_time = '{1}' WHERE delivery_id = {2}", 1, currentDateTime, deliveryID));
                        }
                    }

                    sCommand.Append(";");
                    sCommand.Append(string.Join(";", RowsUpdate));
                    conn.Open();
                    using (MySqlCommand myCmd = new MySqlCommand(sCommand.ToString(), conn))
                    {
                        myCmd.CommandType = CommandType.Text;
                        myCmd.ExecuteNonQuery();
                        responseSaveRoutes.IsSuccess = true;
                    }
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                Logger.LogEvent(mProjName, String.Format("RouteInfoRepository SaveRoutes() Exception: {0}", ex.Message), System.Diagnostics.EventLogEntryType.Error);
                responseSaveRoutes.ErrorMessage = "Failed to save routes";
            }

            if (!responseSaveRoutes.IsSuccess)
            {
                return responseSaveRoutes;
            }

            try
            {
                List<PushNotification> tokens = GetAssetTokensByRouteNo(routeNo);

                if (tokens.Count > 0)
                {
                    PushNotificationService pushNotification = new PushNotificationService();
                    if (!(await pushNotification.NewRoutesNotification(tokens, "save_route")))
                    {
                        responseSaveRoutes.IsSuccess = false;
                        responseSaveRoutes.ErrorMessage = "Saved routes but fail to send push notification";
                    }
                }               
            }
            catch (Exception ex)
            {
                responseSaveRoutes.IsSuccess = false;
                responseSaveRoutes.ErrorMessage = "Saved routes but fail to send push notification";
                Logger.LogEvent(mProjName, String.Format("RouteInfoRepository SaveRoutes() Exception: {0}", ex.Message), System.Diagnostics.EventLogEntryType.Error);               
            }

            
            return responseSaveRoutes;
        }

        public bool Update(RouteInfo currRoute)
        {
            bool retVal = false;
            try
            {
                using (MySqlConnection conn = new MySqlConnection(mConnStr))
                {
                    using (MySqlCommand cmd = new MySqlCommand())
                    {
                        conn.Open();
                        cmd.Connection = conn;
                        cmd.CommandText = "UPDATE vrp_routes SET flag = @flag, rx_time = @rxTime WHERE vrp_routes_id = @routeID";
                        cmd.Prepare();
                        cmd.Parameters.AddWithValue("@flag", currRoute.Flag);
                        cmd.Parameters.AddWithValue("@rxTime", currRoute.RxTime.ToString("yyyy-MM-dd HH:mm:ss"));
                        cmd.Parameters.AddWithValue("@routeID", currRoute.RouteID);

                        if (cmd.ExecuteNonQuery() == 1)
                            retVal = true;
                        else
                            retVal = false;

                        conn.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogEvent(mProjName, String.Format("RouteInfoRepository Update() Exception: {0}", ex.Message), System.Diagnostics.EventLogEntryType.Error);
            }

            return retVal;
        }

        public ResponseRouteInfoDeletion Remove(long routeID, bool isRecalculation, string companyID, string companyName, string userName, string roleID)
        {
            RouteInfo currRoute = new RouteInfo();
            VrpInfo currVrp = new VrpInfo();
            bool isDeleteSuccess = false;
            StringBuilder sCommand = new StringBuilder("");

            ResponseRouteInfoDeletion responseRouteInfoDeletion = new ResponseRouteInfoDeletion();
            responseRouteInfoDeletion.RouteID = routeID;
            responseRouteInfoDeletion.RequestType = isRecalculation ? "Route deletion and recalculation" : "Route deletion";

            try
            {
                currRoute = repoRouteInfo.Get(routeID);

                if (currRoute.RouteID == 0)
                {
                    responseRouteInfoDeletion.ErrorMessage = String.Format("Failed to retrive routeID: {0}", routeID);
                    //return responseRouteInfoDeletion;
                }
            }
            catch(Exception ex)
            {
                Logger.LogEvent(mProjName, String.Format("RouteInfoRepository Remove() Failed to retrive routeID - Exception: {0}", ex.Message), System.Diagnostics.EventLogEntryType.Error);
                responseRouteInfoDeletion.ErrorMessage = String.Format("Failed to retrive routeID: {0}", routeID);
                //return responseRouteInfoDeletion;
            }

            if (responseRouteInfoDeletion.ErrorMessage == null)
            {
                try
                {
                    using (MySqlConnection mConnection = new MySqlConnection(mConnStr))
                    {
                        List<string> RowsUpdate = new List<string>();
                        RowsUpdate.Add(string.Format("DELETE FROM vrp_routes WHERE vrp_routes_id = {0}", currRoute.RouteID));
                        foreach (long pickupID in currRoute.PickupDeliveryInfo.PickupIDs)
                        {
                            RowsUpdate.Add(string.Format("DELETE FROM vrp_pickup WHERE pickup_id = {0}", pickupID));
                            RowsUpdate.Add(string.Format("DELETE FROM vrp_pickup_item WHERE pickup_id = {0}", pickupID));
                        }

                        foreach (long deliveryID in currRoute.PickupDeliveryInfo.DeliveryIDs)
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
                    Logger.LogEvent(mProjName, String.Format("RouteInfoRepository Remove() Exception: {0}", ex.Message), System.Diagnostics.EventLogEntryType.Error);
                    responseRouteInfoDeletion.ErrorMessage = String.Format("Failed to delete.");

                }

                if (isDeleteSuccess)
                {
                    if (isRecalculation)
                    {
                        List<VrpSettingInfo> arrVrpSettings = new List<VrpSettingInfo>();
                        int totalLocationRequest = 0;

                        try
                        {
                            List<PickupDeliveryInfo> arrLocations = repoInitialLocation.GetAssignedLocationInfoByRouteNoDriver(currRoute.RouteNo, currRoute.DriverID);                           
                            VrpSettingInfo vrpSettingInfo = repoVrpSettings.GetVrpSettingInfo(currRoute.RouteNo, currRoute.DriverID);
                            arrVrpSettings.Add(vrpSettingInfo);
                            List<RouteInfo> arrRouteInfo = repoRouteInfo.GetAllRouteInfoByRouteNoDriver(currRoute.RouteNo, currRoute.DriverID);
                            List<AreaCoveredInfo> arrAreaCovered = repoAreaCoveredInfo.GetAllByCompanyID(arrVrpSettings.Count > 0 ? arrVrpSettings[0].CompanyID : 0);
                            DataModel data = new DataModel(arrLocations, arrVrpSettings, arrAreaCovered);
                            totalLocationRequest = data.totalLocationRequests;

                            currVrp = repoVrpInfo.VRPCalculation(currRoute.RouteNo, data, false, false, true, arrRouteInfo);
                            responseRouteInfoDeletion.RecalculatedRouteInfo = currVrp;

                            if (responseRouteInfoDeletion.RecalculatedRouteInfo.StatusCode != null && responseRouteInfoDeletion.RecalculatedRouteInfo.StatusCode == "1" && responseRouteInfoDeletion.RecalculatedRouteInfo.DroppedNodes.Count == 0)
                            {
                                responseRouteInfoDeletion.IsSuccess = true;
                            }
                            else
                            {
                                responseRouteInfoDeletion.ErrorMessage = String.Format("Failed to recalculate after delete.");
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.LogEvent(mProjName, String.Format("RouteInfoRepository Remove() Exception: {0}", ex.Message), System.Diagnostics.EventLogEntryType.Error);
                            responseRouteInfoDeletion.ErrorMessage = String.Format("Failed to recalculate after delete.");
                        }

                        if (totalLocationRequest > 0)
                        {
                            try
                            {
                                repoVrpLocationRequest.AddTotalRequest(arrVrpSettings.Count > 0 ? arrVrpSettings[0].CompanyID.ToString() : "0", currRoute.RouteNo, totalLocationRequest);
                            }
                            catch (Exception ex)
                            {
                                responseRouteInfoDeletion.ErrorMessage = String.Format("Error occured when saving total location request. Error message: {0}", ex.Message);
                                Logger.LogEvent(ConfigurationManager.AppSettings["mProjName"], String.Format("RouteInfoRepository Remove(): {0}", ex.Message), System.Diagnostics.EventLogEntryType.Error);
                            }

                        }
                    }
                    else
                    {
                        responseRouteInfoDeletion.IsSuccess = true;
                    }
                }
            }
            

            string action = isRecalculation ? "Delete route and recalculate" : "Delete route";
            string eventLog = String.Format("RouteID: {0} Action: {1}", routeID, action);

            if (responseRouteInfoDeletion.ErrorMessage != null && responseRouteInfoDeletion.ErrorMessage.Length > 0)
            {
                eventLog += String.Format(" Error: {0}", responseRouteInfoDeletion.ErrorMessage);
            }

            repoEvent.LogVrpEvent(companyID, companyName, userName, roleID, eventLog);

            return responseRouteInfoDeletion;
        }

        public CustomerOrder GetCustomerOrder(long RouteID, int CustomerID)
        {
            RouteInfo routeInfo = Get(RouteID);
            List<VrpPickup> vrpPickups = new List<VrpPickup>();
            List<VrpDelivery> vrpDeliveries = new List<VrpDelivery>();

            if (routeInfo.PickupDeliveryInfo.PickupIDs.Count > 0)
            {
                vrpPickups = repoVrpPickup.GetPickupByIdsAndCustomerId(routeInfo.PickupDeliveryInfo.PickupIDs, CustomerID);
            }

            if (routeInfo.PickupDeliveryInfo.DeliveryIDs.Count > 0)
            {
                vrpDeliveries = repoVrpDelivery.GetDeliveryByIdsAndCustomerId(routeInfo.PickupDeliveryInfo.DeliveryIDs, CustomerID);
            }

            CustomerOrder customerOrder = new CustomerOrder();
            customerOrder.CustomerID = CustomerID;
            customerOrder.RouteID = RouteID;
            customerOrder.PickupOrders = vrpPickups;
            customerOrder.DeliveryOrders = vrpDeliveries;

            return customerOrder;
        }

        public ResponseTimelineRoutes GetTimelineRoutes(string routeNo, string flag)
        {
            ResponseTimelineRoutes responseTimelineRoutes = new ResponseTimelineRoutes();

            try
            {
                List<RouteInfo> arrRouteInfo = GetAllRouteInfoByRouteNoFlag(routeNo, flag);
                responseTimelineRoutes.IsSuccess = true;
                responseTimelineRoutes.Routes = arrRouteInfo;
            }
            catch(Exception ex)
            {
                Logger.LogEvent(mProjName, String.Format("RouteInfoRepository GetTimelineRoutes() Exception: {0}", ex.Message), System.Diagnostics.EventLogEntryType.Error);
                responseTimelineRoutes.ErrorMessage = String.Format("Failed to retrive timeline routes.");
            }

            return responseTimelineRoutes;
        }

        public List<PushNotification> GetAssetTokensByRouteNo(string routeNo)
        {
            List<PushNotification> tokens = new List<PushNotification>();

            string query = string.Format("SELECT s.time_window_start, a.token " +
                "FROM vrp_settings s " +
                "LEFT JOIN assets a ON s.asset_id = a.asset_id " +
                "WHERE s.route_no = @routeNo");

            try
            {
                using (MySqlConnection conn = new MySqlConnection(mConnStr))
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
                                    tokens.Add(DataMgrTools.BuildPushNotification(reader));
                                }
                            }
                        }

                        conn.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogEvent(mProjName, String.Format("RouteInfoRepository GetAssetTokensByRouteNo() Exception: {0}", ex.Message), System.Diagnostics.EventLogEntryType.Error);
            }

            return tokens;
        }
    }
}