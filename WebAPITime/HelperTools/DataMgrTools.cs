using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using VrpModel;
using WebAPITime.Models;

namespace WebAPITime.HelperTools
{
    public class DataMgrTools
    {
        private static string mProjName = ConfigurationManager.AppSettings["mProjName"];

        public static PickupDeliveryInfo BuildInitialLocationInfo(MySqlDataReader dbRdr)
        {
            PickupDeliveryInfo udtLocation = new PickupDeliveryInfo();
            try
            {
                //udtLocation.PickupDeliveryID = dbRdr.ToInt64("pickup_delivery_id");
                udtLocation.OrderType = dbRdr.ToString("order_type");
                udtLocation.OrderName = new List<string>();
                udtLocation.PickupIDs = new List<long>();
                udtLocation.DeliveryIDs = new List<long>();
                udtLocation.RouteNo = dbRdr.ToString("route_no");
                udtLocation.PriorityID = dbRdr.ToInt32("priority_id");
                udtLocation.DriverID = dbRdr.ToInt64("driver_id");
                udtLocation.DriverName = dbRdr.ToString("driver_name");
                udtLocation.Lat = dbRdr.ToDouble("latitude");
                udtLocation.Long = dbRdr.ToDouble("longitude");
                udtLocation.Address = dbRdr.ToString("address");
                udtLocation.PostalCode = dbRdr.ToString("postal_code");
                udtLocation.UnitNo = dbRdr.ToString("unit");
                udtLocation.TotalWeight = dbRdr.ToDouble("total_weight");
                udtLocation.TotalVolume = dbRdr.ToDouble("total_volume");
                udtLocation.ServiceDuration = dbRdr.ToInt32("service_duration");
                udtLocation.LoadDuration = dbRdr.ToInt32("load_duration");
                udtLocation.UnloadDuration = dbRdr.ToInt32("unload_duration");
                udtLocation.WaitingDuration = dbRdr.ToInt32("waiting_duration");
                udtLocation.PickupFromIDs = new List<long>();

                string orderName = dbRdr.ToString("order_name");
                if (orderName.Trim() != "")
                {
                    udtLocation.OrderName.Add(orderName);
                }

                long pickupID = dbRdr.ToInt64("pickup_id");
                if (pickupID != 0)
                {
                    udtLocation.PickupIDs.Add(pickupID);
                }
                long deliveryID = dbRdr.ToInt64("delivery_id");
                if (deliveryID != 0)
                {
                    udtLocation.DeliveryIDs.Add(deliveryID);
                }
                string[] pickupFromIDs = dbRdr.ToString("pickup_from_id").Split(',');
                for(int i=0; i<pickupFromIDs.Length; i++)
                {
                    if(pickupFromIDs[i].Trim() != "")
                        udtLocation.PickupFromIDs.Add(Convert.ToInt64(pickupFromIDs[i]));
                }
                udtLocation.FeatureIDs = new List<int>();
                string[] featureIDs = dbRdr.ToString("feature_ids").Split(',');
                for (int i = 0; i < featureIDs.Length; i++)
                {
                    if (featureIDs[i].Trim() != "" && featureIDs[i].Trim() != ",")
                        udtLocation.FeatureIDs.Add(Convert.ToInt32(featureIDs[i]));
                }
                udtLocation.Accessories = new List<long>();
                string[] accessoriesIDs = dbRdr.ToString("accessories").Split(',');
                for (int i = 0; i < accessoriesIDs.Length; i++)
                {
                    if (accessoriesIDs[i].Trim() != "" && accessoriesIDs[i].Trim() != "0")
                        udtLocation.Accessories.Add(Convert.ToInt64(accessoriesIDs[i]));
                }
                udtLocation.FeatureIDs = udtLocation.FeatureIDs.Distinct().ToList();
                udtLocation.Accessories = udtLocation.Accessories.Distinct().ToList();
                udtLocation.TimeWindowStart = dbRdr.ToDateTime("time_window_start");
                udtLocation.TimeWindowEnd = dbRdr.ToDateTime("time_window_end");
            }
            catch (Exception ex)
            {
                Logger.LogEvent(mProjName, "DataMgrTools BuildInitialLocationInfo(): " + ex.Message, System.Diagnostics.EventLogEntryType.Error);
            }
            return udtLocation;
        }

        public static VrpSettingInfo BuildVrpSettingInfo(MySqlDataReader dbRdr)
        {
            VrpSettingInfo udtVrpSetting = new VrpSettingInfo();
            try
            {
                udtVrpSetting.VrpSettingsId = dbRdr.ToInt64("vrp_settings_id");
                udtVrpSetting.RouteNo = dbRdr.ToString("route_no");
                udtVrpSetting.AssetID = dbRdr.ToInt32("asset_id");
                udtVrpSetting.DriverID = dbRdr.ToInt64("driver_id");
                udtVrpSetting.DriverName = dbRdr.ToString("driver_name");
                udtVrpSetting.TimeStamp = dbRdr.ToDateTime("timestamp");
                udtVrpSetting.RxTime = dbRdr.ToDateTime("rx_time");
                udtVrpSetting.DriverID = dbRdr.ToInt64("driver_id");
                udtVrpSetting.StartLatitude = dbRdr.ToDouble("start_latitude");
                udtVrpSetting.StartLongitude = dbRdr.ToDouble("start_longitude");
                udtVrpSetting.StartAddress = dbRdr.ToString("start_address");
                udtVrpSetting.EndLatitude = dbRdr.ToDouble("end_latitude");
                udtVrpSetting.EndLongitude = dbRdr.ToDouble("end_longitude");
                udtVrpSetting.EndAddress = dbRdr.ToString("end_address");              
                udtVrpSetting.WeightCapacity = dbRdr.ToInt32("weight_capacity");
                udtVrpSetting.VolumeCapacity = dbRdr.ToInt32("volume_capacity");
                udtVrpSetting.DistanceCapacity = dbRdr.ToInt32("distance_capacity");
                udtVrpSetting.TimeWindowStart = dbRdr.ToDateTime("time_window_start");
                udtVrpSetting.TimeWindowEnd = dbRdr.ToDateTime("time_window_end");
                udtVrpSetting.BreakTimeStart = dbRdr.ToDateTime("break_time_start");
                udtVrpSetting.BreakTimeEnd = dbRdr.ToDateTime("break_time_end");
                udtVrpSetting.IsOvertime = dbRdr.ToInt32("isOvertime");
                udtVrpSetting.CompanyID = dbRdr.ToInt32("company_id");
                udtVrpSetting.Features = new List<int>();
                string[] featureIDs = dbRdr.ToString("features").Split(',');
                for (int i = 0; i < featureIDs.Length; i++)
                {
                    if (featureIDs[i].Trim() != "")
                        udtVrpSetting.Features.Add(Convert.ToInt32(featureIDs[i]));
                }

                string[] acr_ids = dbRdr.ToString("area_covered_region_id").Split(',');
                string[] region_names = dbRdr.ToString("region_name").Split(',');
                udtVrpSetting.Zones = new Dictionary<int, string>();
                for(int i=0; i<acr_ids.Length; i++)
                {
                    if (acr_ids[i].Trim() != "" && !udtVrpSetting.Zones.ContainsKey(Convert.ToInt32(acr_ids[i])))
                    {
                        udtVrpSetting.Zones[Convert.ToInt32(acr_ids[i])] = region_names[i];
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogEvent(mProjName, "DataMgrTools BuildVrpSettingInfo(): " + ex.Message, System.Diagnostics.EventLogEntryType.Error);
            }
            return udtVrpSetting;
        }

        public static DroppedNodes BuildDroppedNodes(MySqlDataReader dbRdr)
        {
            DroppedNodes udtLocation = new DroppedNodes();
            try
            {
                udtLocation.NodeID = dbRdr.ToInt64("node");
                udtLocation.Address = dbRdr.ToString("address");
            }
            catch (Exception ex)
            {
                Logger.LogEvent(mProjName, "DataMgrTools BuildDroppedNodes(): " + ex.Message, System.Diagnostics.EventLogEntryType.Error);
            }
            return udtLocation;
        }

        public static RouteInfo BuildRouteInfo(MySqlDataReader dbRdr)
        {
            RouteInfo udtRoute = new RouteInfo();
            PickupDeliveryInfo udtLocation = new PickupDeliveryInfo();
            try
            {
                //udtLocation.PickupDeliveryID = dbRdr.ToInt64("pickup_delivery_id");
                udtLocation.OrderType = dbRdr.ToString("order_type");
                udtLocation.RouteNo = dbRdr.ToString("route_no");
                udtLocation.PriorityID = dbRdr.ToInt32("priority_id");
                udtLocation.DriverID = dbRdr.ToInt64("driver_id");
                udtLocation.DriverName = dbRdr.ToString("driver_name");
                udtLocation.Lat = dbRdr.ToDouble("latitude");
                udtLocation.Long = dbRdr.ToDouble("longitude");
                udtLocation.Address = dbRdr.ToString("address");
                udtLocation.TotalWeight = dbRdr.ToDouble("total_weight");
                udtLocation.TotalVolume = dbRdr.ToDouble("total_volume");
                udtLocation.ServiceDuration = dbRdr.ToInt32("service_duration");
                udtLocation.LoadDuration = dbRdr.ToInt32("load_duration");
                udtLocation.UnloadDuration = dbRdr.ToInt32("unload_duration");
                udtLocation.WaitingDuration = dbRdr.ToInt32("waiting_duration");
                udtLocation.TimeWindowStart = dbRdr.ToDateTime("time_window_start");
                udtLocation.TimeWindowEnd = dbRdr.ToDateTime("time_window_end");

                udtLocation.PickupIDs = new List<long>();
                udtLocation.DeliveryIDs = new List<long>();
                udtLocation.PickupFromIDs = new List<long>();
                udtLocation.FeatureIDs = new List<int>();
                udtLocation.Accessories = new List<long>();
                string[] pickupIDs = dbRdr.ToString("pickup_ids").Split(',');
                string[] deliveryIDs = dbRdr.ToString("delivery_ids").Split(',');
                string[] pickupFromIDs = dbRdr.ToString("pickup_from_ids").Split(',');
                string[] featureIDs = dbRdr.ToString("feature_id").Split(',');
                string[] accessories = dbRdr.ToString("accessories").Split(',');


                for (int i = 0; i < pickupIDs.Length; i++)
                {
                    if (pickupIDs[i].Trim() != "")
                        udtLocation.PickupIDs.Add(Convert.ToInt64(pickupIDs[i]));
                }

                for (int i = 0; i < deliveryIDs.Length; i++)
                {
                    if (deliveryIDs[i].Trim() != "")
                        udtLocation.DeliveryIDs.Add(Convert.ToInt64(deliveryIDs[i]));
                }

                for (int i = 0; i < pickupFromIDs.Length; i++)
                {
                    if (pickupFromIDs[i].Trim() != "")
                        udtLocation.PickupFromIDs.Add(Convert.ToInt64(pickupFromIDs[i]));
                }

                for (int i = 0; i < featureIDs.Length; i++)
                {
                    if (featureIDs[i].Trim() != "" && featureIDs[i].Trim() != ",")
                        udtLocation.FeatureIDs.Add(Convert.ToInt32(featureIDs[i]));
                }

                for (int i = 0; i < accessories.Length; i++)
                {
                    if (accessories[i].Trim() != "" && accessories[i].Trim() != "0")
                        udtLocation.Accessories.Add(Convert.ToInt64(accessories[i]));
                }

                udtLocation.FeatureIDs = udtLocation.FeatureIDs.Distinct().ToList();
                udtLocation.Accessories = udtLocation.Accessories.Distinct().ToList();

                udtRoute.RouteID = dbRdr.ToInt64("vrp_routes_id");
                udtRoute.RouteNo = dbRdr.ToString("route_no");
                udtRoute.AssetID = dbRdr.ToInt64("asset_id");
                udtRoute.AssetName = dbRdr.ToString("name");
                udtRoute.DriverID = dbRdr.ToInt64("driver_id");
                udtRoute.DriverName = dbRdr.ToString("driver_name");
                udtRoute.PickupDeliveryInfo = udtLocation;
                udtRoute.DistanceOfRoute = dbRdr.ToDouble("route_distance");
                udtRoute.TimeOfRoute = dbRdr.ToInt32("route_time");
                udtRoute.ArrivalTime = dbRdr.ToDateTime("arrival_time");
                udtRoute.DepartureTime = dbRdr.ToDateTime("departure_time");
                udtRoute.BreakTimeStart = dbRdr.ToDateTime("break_time_start");
                udtRoute.BreakTimeEnd = dbRdr.ToDateTime("break_time_end");
                udtRoute.Sequence = dbRdr.ToInt32("sequence");
                udtRoute.Status = dbRdr.ToString("status");
                udtRoute.Flag = dbRdr.ToInt32("flag");
                udtRoute.Timestamp = dbRdr.ToDateTime("timestamp");
                udtRoute.RxTime = dbRdr.ToDateTime("rx_time");
            }
            catch (Exception ex)
            {
                Logger.LogEvent(mProjName, "DataMgrTools BuildRouteInfo(): " + ex.Message, System.Diagnostics.EventLogEntryType.Error);
            }
            return udtRoute;
        }

        public static AreaCoveredInfo BuildAreaCoveredInfo(MySqlDataReader dbRdr)
        {
            AreaCoveredInfo udtAreaCovered = new AreaCoveredInfo();
            try
            {
                udtAreaCovered.DistrictID = dbRdr.ToInt32("district_id");
                udtAreaCovered.CompanyID = dbRdr.ToInt32("company_id");
                udtAreaCovered.PostalDistrict = dbRdr.ToInt32("postal_district");
                udtAreaCovered.PostalSector = dbRdr.ToInt32("postal_sector");
                udtAreaCovered.GeneralLocation = dbRdr.ToString("general_location");
                udtAreaCovered.RegionID = dbRdr.ToInt32("area_covered_region_id");
                udtAreaCovered.RegionName = dbRdr.ToString("region_name");

            }
            catch (Exception ex)
            {
                Logger.LogEvent(mProjName, "DataMgrTools BuildAreaCoveredInfo(): " + ex.Message, System.Diagnostics.EventLogEntryType.Error);
            }
            return udtAreaCovered;
        }

        public static AssetFeature BuildAssetFeature(MySqlDataReader dbRdr)
        {
            AssetFeature udtAssetFeature = new AssetFeature();
            try
            {
                udtAssetFeature.FeatureID = dbRdr.ToInt32("feature_id");
                udtAssetFeature.Description = dbRdr.ToString("description");

            }
            catch (Exception ex)
            {
                Logger.LogEvent(mProjName, "DataMgrTools BuildAssetFeature(): " + ex.Message, System.Diagnostics.EventLogEntryType.Error);
            }
            return udtAssetFeature;
        }

        public static CustomerInfo BuildCustomerInfo(MySqlDataReader dbRdr)
        {
            CustomerInfo udtCustomerInfo = new CustomerInfo();
            try
            {
                udtCustomerInfo.CustomerID = dbRdr.ToInt32("customer_id");
                udtCustomerInfo.Name = dbRdr.ToString("name");

            }
            catch (Exception ex)
            {
                Logger.LogEvent(mProjName, "DataMgrTools BuildCustomerInfo(): " + ex.Message, System.Diagnostics.EventLogEntryType.Error);
            }
            return udtCustomerInfo;
        }

        public static VrpPickup BuildVrpPickup(MySqlDataReader dbRdr)
        {
            VrpPickup udtVrpPickup = new VrpPickup();
            try
            {
                udtVrpPickup.PickupID = dbRdr.ToInt64("pickup_id");
                udtVrpPickup.RouteNo = dbRdr.ToString("route_no");
                udtVrpPickup.PriorityID = dbRdr.ToInt32("priority_id");
                udtVrpPickup.DriverID = dbRdr.ToInt64("driver_id");
                udtVrpPickup.Timestamp = dbRdr.ToDateTime("timestamp");
                udtVrpPickup.RxTime = dbRdr.ToDateTime("rx_time");
                udtVrpPickup.CustomerID = dbRdr.ToInt64("customer_id");
                udtVrpPickup.OrderNo = dbRdr.ToString("order_no");
                udtVrpPickup.WaitingDuration = dbRdr.ToInt32("waiting_duration");
                udtVrpPickup.ServiceDuration = dbRdr.ToInt32("service_duration");
                udtVrpPickup.LoadDuration = dbRdr.ToInt32("load_duration");
                udtVrpPickup.TimeWindowStart = dbRdr.ToDateTime("time_window_start");
                udtVrpPickup.TimeWindowEnd = dbRdr.ToDateTime("time_window_end");
                udtVrpPickup.Remarks = dbRdr.ToString("remarks");
                udtVrpPickup.Name = dbRdr.ToString("name");
                udtVrpPickup.Latitude = dbRdr.ToDouble("latitude");
                udtVrpPickup.Longitude = dbRdr.ToDouble("longitude");
                udtVrpPickup.Address = dbRdr.ToString("address");
                udtVrpPickup.Unit = dbRdr.ToString("unit");
                udtVrpPickup.Building = dbRdr.ToString("building");
                udtVrpPickup.PostalCode = dbRdr.ToString("postal_code");
                udtVrpPickup.Phone = dbRdr.ToString("phone");
                udtVrpPickup.Mobile = dbRdr.ToString("mobile");
                udtVrpPickup.Email = dbRdr.ToString("email");
                udtVrpPickup.Amount = dbRdr.ToDouble("amount");
                udtVrpPickup.AccessoriesID = dbRdr.ToString("accessories");
                udtVrpPickup.AccessoriesName = dbRdr.ToString("accessories_name");
                udtVrpPickup.IsAssign = dbRdr.ToInt32("isAssign");
                udtVrpPickup.Flag = dbRdr.ToInt32("flag");

            }
            catch (Exception ex)
            {
                Logger.LogEvent(mProjName, "DataMgrTools BuildVrpPickup(): " + ex.Message, System.Diagnostics.EventLogEntryType.Error);
            }
            return udtVrpPickup;
        }

        public static VrpDelivery BuildVrpDelivery(MySqlDataReader dbRdr)
        {
            VrpDelivery udtVrpDelivery = new VrpDelivery();
            try
            {
                udtVrpDelivery.DeliveryID = dbRdr.ToInt64("delivery_id");
                udtVrpDelivery.RouteNo = dbRdr.ToString("route_no");
                udtVrpDelivery.PriorityID = dbRdr.ToInt32("priority_id");
                udtVrpDelivery.DriverID = dbRdr.ToInt64("driver_id");
                udtVrpDelivery.Timestamp = dbRdr.ToDateTime("timestamp");
                udtVrpDelivery.RxTime = dbRdr.ToDateTime("rx_time");
                udtVrpDelivery.CustomerID = dbRdr.ToInt64("customer_id");
                udtVrpDelivery.OrderNo = dbRdr.ToString("order_no");
                udtVrpDelivery.WaitingDuration = dbRdr.ToInt32("waiting_duration");
                udtVrpDelivery.ServiceDuration = dbRdr.ToInt32("service_duration");
                udtVrpDelivery.UnloadDuration = dbRdr.ToInt32("unload_duration");
                udtVrpDelivery.TimeWindowStart = dbRdr.ToDateTime("time_window_start");
                udtVrpDelivery.TimeWindowEnd = dbRdr.ToDateTime("time_window_end");
                udtVrpDelivery.Remarks = dbRdr.ToString("remarks");
                udtVrpDelivery.PickupID = dbRdr.ToString("pickup_id");
                udtVrpDelivery.Name = dbRdr.ToString("name");
                udtVrpDelivery.Latitude = dbRdr.ToDouble("latitude");
                udtVrpDelivery.Longitude = dbRdr.ToDouble("longitude");
                udtVrpDelivery.BillingName = dbRdr.ToString("billing_name");
                udtVrpDelivery.BillingAddress = dbRdr.ToString("billing_address");
                udtVrpDelivery.BillingUnit = dbRdr.ToString("billing_unit");
                udtVrpDelivery.BillingBuilding = dbRdr.ToString("billing_building");
                udtVrpDelivery.BillingPostalCode = dbRdr.ToString("billing_postal_code");
                udtVrpDelivery.BillingPhone = dbRdr.ToString("billing_phone");
                udtVrpDelivery.BillingMobile = dbRdr.ToString("billing_mobile");
                udtVrpDelivery.BillingEmail = dbRdr.ToString("billing_email");
                udtVrpDelivery.ShippingName = dbRdr.ToString("shipping_name");
                udtVrpDelivery.ShippingAddress = dbRdr.ToString("shipping_address");
                udtVrpDelivery.ShippingUnit = dbRdr.ToString("shipping_unit");
                udtVrpDelivery.ShippingBuilding = dbRdr.ToString("shipping_building");
                udtVrpDelivery.ShippingPostalCode = dbRdr.ToString("shipping_postal_code");
                udtVrpDelivery.ShippingPhone = dbRdr.ToString("shipping_phone");
                udtVrpDelivery.ShippingMobile = dbRdr.ToString("shipping_mobile");
                udtVrpDelivery.ShippingEmail = dbRdr.ToString("shipping_email");
                udtVrpDelivery.Amount = dbRdr.ToDouble("amount");
                udtVrpDelivery.AccessoriesID = dbRdr.ToString("accessories");
                udtVrpDelivery.AccessoriesName = dbRdr.ToString("accessories_name");
                udtVrpDelivery.IsAssign = dbRdr.ToInt32("isAssign");
                udtVrpDelivery.Flag = dbRdr.ToInt32("flag");


            }
            catch (Exception ex)
            {
                Logger.LogEvent(mProjName, "DataMgrTools BuildVrpDelivery(): " + ex.Message, System.Diagnostics.EventLogEntryType.Error);
            }
            return udtVrpDelivery;
        }

        public static VrpLocationRequest BuildVrpLocationRequest(MySqlDataReader dbRdr, string groupMode)
        {           
            VrpLocationRequest udtVrpLocationRequest = new VrpLocationRequest();
            
            try
            {               
                udtVrpLocationRequest.CompanyID = dbRdr.ToInt32("company_id");
                udtVrpLocationRequest.RequestCount = dbRdr.ToInt32("request_count");
                udtVrpLocationRequest.CreditLimit = dbRdr.ToInt32("credit_limit");
                udtVrpLocationRequest.Year = dbRdr.ToInt32("year");

                groupMode = groupMode.ToLower();

                if (groupMode == "day" || groupMode == "month")
                {
                    udtVrpLocationRequest.Month = dbRdr.ToInt32("month");

                    if (groupMode == "day")
                    {
                        udtVrpLocationRequest.Day = dbRdr.ToInt32("day");
                    }
                }
                

            }
            catch (Exception ex)
            {
                Logger.LogEvent(mProjName, "DataMgrTools BuildVrpLocationRequest(): " + ex.Message, System.Diagnostics.EventLogEntryType.Error);
            }
            return udtVrpLocationRequest;
        }

        public static PushNotification BuildPushNotification(MySqlDataReader dbRdr)
        {
            PushNotification udtPushNotification = new PushNotification();

            try
            {
                udtPushNotification.TimeWindowStart = dbRdr.ToDateTime("time_window_start");
                udtPushNotification.Token = dbRdr.ToString("token");
            }
            catch (Exception ex)
            {
                Logger.LogEvent(mProjName, "DataMgrTools BuildPushNotification(): " + ex.Message, System.Diagnostics.EventLogEntryType.Error);
            }
            return udtPushNotification;
        }

        public static VrpRouteReport BuildVrpRouteReport(MySqlDataReader dbRdr)
        {
            VrpRouteReport udtVrpRouteReport = new VrpRouteReport();

            try
            {
                udtVrpRouteReport.VrpRouteID = dbRdr.ToInt64("vrp_routes_id");
                udtVrpRouteReport.FromVrpRouteID = dbRdr.ToInt64("from_vrp_routes_id");
                udtVrpRouteReport.EstDepartureTime = dbRdr.ToDateTime("est_departure_time");
                udtVrpRouteReport.ActualDepartureTime = dbRdr.ToDateTime("actual_departure_time");
                udtVrpRouteReport.DepartureTimeStatus = dbRdr.ToString("status_departure_time");
                udtVrpRouteReport.EstArrivalTime = dbRdr.ToDateTime("est_arrival_time");
                udtVrpRouteReport.ActualArrivalTime = dbRdr.ToDateTime("actual_arrival_time");
                udtVrpRouteReport.ArrivalTimeStatus = dbRdr.ToString("status_arrival_time");
                udtVrpRouteReport.TravelDuration = dbRdr.ToInt32("travel_duration");
                udtVrpRouteReport.JobStartTime = dbRdr.ToDateTime("job_start_time");
                udtVrpRouteReport.JobEndTime = dbRdr.ToDateTime("job_end_time");
                udtVrpRouteReport.JobDuration = dbRdr.ToInt32("job_duration");
                udtVrpRouteReport.EstJobDuration = dbRdr.ToInt32("est_job_duration");

            }
            catch (Exception ex)
            {
                Logger.LogEvent(mProjName, "DataMgrTools BuildVrpRouteReport(): " + ex.Message, System.Diagnostics.EventLogEntryType.Error);
            }
            return udtVrpRouteReport;
        }

        public static DriverReport BuildDriverReport(MySqlDataReader dbRdr)
        {
            DriverReport udtDriverReport = new DriverReport();

            try
            {
                udtDriverReport.DriverID = dbRdr.ToInt64("driver_id");
                udtDriverReport.ScheduledWorkTimeStart = dbRdr.ToDateTime("scheduled_work_time_start");
                udtDriverReport.ActualWorkTimeStart = dbRdr.ToDateTime("actual_work_time_start");
                udtDriverReport.WorkTimeStartStatus = dbRdr.ToString("status_work_time_start");
                udtDriverReport.ScheduledWorkTimeEnd = dbRdr.ToDateTime("scheduled_work_time_end");
                udtDriverReport.ActualWorkTimeEnd = dbRdr.ToDateTime("actual_work_time_end");
                udtDriverReport.WorkTimeEndStatus = dbRdr.ToString("status_work_time_end");
                udtDriverReport.WorkDuration = dbRdr.ToInt32("work_duration");

            }
            catch (Exception ex)
            {
                Logger.LogEvent(mProjName, "DataMgrTools BuildDriverReport(): " + ex.Message, System.Diagnostics.EventLogEntryType.Error);
            }
            return udtDriverReport;
        }

        public static DriverSchedule BuildDriverSchedule(MySqlDataReader dbRdr)
        {
            DriverSchedule udtDriverSchedule = new DriverSchedule();

            try
            {
                udtDriverSchedule.DriverID = dbRdr.ToInt64("driver_id");
                udtDriverSchedule.TimeWindowStart = dbRdr.ToString("time_window_start");
                udtDriverSchedule.TimeWindowEnd = dbRdr.ToString("time_window_end");
                udtDriverSchedule.DayID = dbRdr.ToInt32("day_id");

            }
            catch (Exception ex)
            {
                Logger.LogEvent(mProjName, "DataMgrTools BuildDriverSchedule(): " + ex.Message, System.Diagnostics.EventLogEntryType.Error);
            }
            return udtDriverSchedule;
        }
    }
}
