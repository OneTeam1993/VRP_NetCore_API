using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebAPI;
using WebAPITime.HelperTools;
using WebAPITime.Models;

namespace VrpModel
{
    public class DataModel
    {
        private static string mProjName = ConfigurationManager.AppSettings["mProjName"];
        public bool isSuccess = false;
        public string errorMessage = "DataModel() Exception";
        public long[,] TimeMatrix;
        public long[,] DistanceMatrix;
        public int[,] TimeWindows;
        public long[] WeightCapacities;
        public long[] VolumeCapacities;
        public double[] arrLocationWeight;
        public double[] arrLocationVolume;
        public int VehicleCount;
        public int[] ServiceDuration;
        public int[] LoadDuration;
        public int[] UnloadDuration;
        public int[] WaitingDuration;
        public long MaxTime;
        public long MaxDistance;
        public int Depot = 0;
        public int[] Starts;
        public int[] Ends;
        public int[][] PickupsDeliveries;
        public List<PickupDeliveryInfo> arrAllLocation;
        public List<VrpSettingInfo> arrVrpSettings;
        public int FixedEndLocationsCount;
        public Dictionary<int, List<long>> priorityMap;
        public Dictionary<long, long> driverToVehicleMap;
        public Dictionary<long, long> locationToDriverMap;
        public Dictionary<string, int> locationMap;
        public Dictionary<long, int> pickupIdToNodeMap;
        public Dictionary<int, List<long>> nodeToRemoveVehicleMapForFeaturesContraint;
        public Dictionary<int, List<long>> nodeToRemoveVehicleMapForZoneContraint;
        public Dictionary<string, AreaCoveredInfo> postalSectorToAreaCovered;

        public DataModel(List<PickupDeliveryInfo> arrLocations, List<VrpSettingInfo> arrVrpSettings, List<AreaCoveredInfo> arrAreaCovered, bool isAdHocSolution = false, List<RouteInfo> arrRouteInfo = null, List<long> arrAdHocPickupID = null, List<long> arrAdHocDeliveryID = null)
        {
            VehicleCount = arrVrpSettings.Count;
            arrAllLocation = new List<PickupDeliveryInfo>();
            List<int> startLocations = new List<int>();
            List<int> endLocations = new List<int>();
            List<int> randomEndVehicles = new List<int>();
            List<int[]> pickupsDeliveries = new List<int[]>();
            locationMap = new Dictionary<string, int>();
            pickupIdToNodeMap = new Dictionary<long, int>();
            postalSectorToAreaCovered = new Dictionary<string, AreaCoveredInfo>();

            driverToVehicleMap = new Dictionary<long, long>();
            this.arrVrpSettings = arrVrpSettings;

            for (int i = 0; i < arrAreaCovered.Count; i++)
            {
                if (!postalSectorToAreaCovered.ContainsKey(string.Format("{0:D2}", arrAreaCovered[i].PostalSector)))
                {
                    postalSectorToAreaCovered[string.Format("{0:D2}", arrAreaCovered[i].PostalSector)] = arrAreaCovered[i];
                }
            }

            for (int i = 0; i < arrVrpSettings.Count; i++)
            {
                PickupDeliveryInfo vehicleStartLocation = new PickupDeliveryInfo();
                vehicleStartLocation.OrderType = "Pickup";
                vehicleStartLocation.PickupIDs = new List<long>();
                vehicleStartLocation.DeliveryIDs = new List<long>();
                vehicleStartLocation.RouteNo = arrVrpSettings[i].RouteNo;
                vehicleStartLocation.DriverID = arrVrpSettings[i].DriverID;
                vehicleStartLocation.DriverName = arrVrpSettings[i].DriverName;
                vehicleStartLocation.Lat = arrVrpSettings[i].StartLatitude;
                vehicleStartLocation.Long = arrVrpSettings[i].StartLongitude;
                vehicleStartLocation.Address = arrVrpSettings[i].StartAddress;
                vehicleStartLocation.Node = i;
                vehicleStartLocation.TotalWeight = 0;
                vehicleStartLocation.TotalVolume = 0;
                vehicleStartLocation.ServiceDuration = 0;
                vehicleStartLocation.LoadDuration = 0;
                vehicleStartLocation.UnloadDuration = 0;
                vehicleStartLocation.WaitingDuration = 0;
                vehicleStartLocation.TimeWindowStart = arrVrpSettings[i].TimeWindowStart;
                vehicleStartLocation.TimeWindowEnd = arrVrpSettings[i].TimeWindowEnd;
                vehicleStartLocation.PickupIDs = new List<long>();
                vehicleStartLocation.DeliveryIDs = new List<long>();
                vehicleStartLocation.PickupFromIDs = new List<long>();
                vehicleStartLocation.FeatureIDs = new List<int>();
                vehicleStartLocation.Accessories = new List<long>();
                arrAllLocation.Add(vehicleStartLocation);
                startLocations.Add(i);

                if (!driverToVehicleMap.ContainsKey(arrVrpSettings[i].DriverID))
                {
                    driverToVehicleMap[arrVrpSettings[i].DriverID] = i;
                }

                if (!locationMap.ContainsKey(arrVrpSettings[i].StartLongitude + "," + arrVrpSettings[i].StartLatitude))
                {
                    locationMap[arrVrpSettings[i].StartLongitude + "," + arrVrpSettings[i].StartLatitude] = i;
                }
            }

            FixedEndLocationsCount = 0;
            for (int i = 0; i < arrVrpSettings.Count; i++)
            {
                if (arrVrpSettings[i].EndAddress == null || arrVrpSettings[i].EndAddress.Trim() == "")
                {
                    randomEndVehicles.Add(i);
                    endLocations.Add(i);
                }
                else
                {
                    PickupDeliveryInfo vehicleEndLocation = new PickupDeliveryInfo();
                    vehicleEndLocation.OrderType = "Delivery";
                    vehicleEndLocation.PickupIDs = new List<long>();
                    vehicleEndLocation.DeliveryIDs = new List<long>();
                    vehicleEndLocation.RouteNo = arrVrpSettings[i].RouteNo;
                    vehicleEndLocation.DriverID = arrVrpSettings[i].DriverID;
                    vehicleEndLocation.DriverName = arrVrpSettings[i].DriverName;
                    vehicleEndLocation.Lat = arrVrpSettings[i].EndLatitude;
                    vehicleEndLocation.Long = arrVrpSettings[i].EndLongitude;
                    vehicleEndLocation.Address = arrVrpSettings[i].EndAddress;
                    vehicleEndLocation.Node = arrAllLocation.Count;
                    vehicleEndLocation.TotalWeight = 0;
                    vehicleEndLocation.TotalVolume = 0;
                    vehicleEndLocation.ServiceDuration = 0;
                    vehicleEndLocation.LoadDuration = 0;
                    vehicleEndLocation.UnloadDuration = 0;
                    vehicleEndLocation.WaitingDuration = 0;
                    vehicleEndLocation.TimeWindowStart = arrVrpSettings[i].TimeWindowStart;
                    vehicleEndLocation.TimeWindowEnd = arrVrpSettings[i].TimeWindowEnd;
                    vehicleEndLocation.PickupIDs = new List<long>();
                    vehicleEndLocation.DeliveryIDs = new List<long>();
                    vehicleEndLocation.PickupFromIDs = new List<long>();
                    vehicleEndLocation.FeatureIDs = new List<int>();
                    vehicleEndLocation.Accessories = new List<long>();

                    arrAllLocation.Add(vehicleEndLocation);
                    int locationIndex = arrAllLocation.Count - 1;
                    endLocations.Add(locationIndex);
                    FixedEndLocationsCount++;

                    if (!locationMap.ContainsKey(arrVrpSettings[i].EndLongitude + "," + arrVrpSettings[i].EndLatitude))
                    {
                        locationMap[arrVrpSettings[i].EndLongitude + "," + arrVrpSettings[i].EndLatitude] = vehicleEndLocation.Node;
                    }
                }
            }

            PickupDeliveryInfo combineLocation(PickupDeliveryInfo from, PickupDeliveryInfo combinedLocation)
            {
                combinedLocation.TotalWeight += from.TotalWeight;
                combinedLocation.TotalVolume += from.TotalVolume;
                combinedLocation.ServiceDuration += from.ServiceDuration;
                combinedLocation.LoadDuration += from.LoadDuration;
                combinedLocation.UnloadDuration += from.UnloadDuration;
                combinedLocation.WaitingDuration += from.WaitingDuration;

                foreach (int featureID in from.FeatureIDs)
                {
                    if (!combinedLocation.FeatureIDs.Contains(featureID))
                    {
                        combinedLocation.FeatureIDs.Add(featureID);
                    }
                }

                foreach (long accessoriesID in from.Accessories)
                {
                    if (!combinedLocation.Accessories.Contains(accessoriesID))
                    {
                        combinedLocation.Accessories.Add(accessoriesID);
                    }
                }

                if(combinedLocation.OrderType != "Pickup and Delivery")
                {
                    switch (from.OrderType)
                    {
                        case "Pickup":
                            if (combinedLocation.OrderType == "Delivery")
                            {
                                combinedLocation.OrderType = "Pickup and Delivery";
                            }                           
                            break;

                        case "Delivery":
                            if (combinedLocation.OrderType == "Pickup")
                            {
                                combinedLocation.OrderType = "Pickup and Delivery";
                            }
                            break;
                    }
                }

                switch (from.OrderType)
                {
                    case "Pickup":
                        combinedLocation.PickupIDs.Add(from.PickupIDs[0]);

                        if (!pickupIdToNodeMap.ContainsKey(from.PickupIDs[0]))
                            pickupIdToNodeMap[from.PickupIDs[0]] = combinedLocation.Node;
                        break;

                    case "Delivery":
                        combinedLocation.DeliveryIDs.Add(from.DeliveryIDs[0]);

                        if (from.PickupFromIDs.Count > 0)
                        {
                            foreach (long pickupID in from.PickupFromIDs)
                            {
                                if(pickupID != 0)
                                {
                                    if (!combinedLocation.PickupFromIDs.Contains(pickupID))
                                    {
                                        combinedLocation.PickupFromIDs.Add(pickupID);
                                    }

                                    if (pickupIdToNodeMap[pickupID] != combinedLocation.Node)
                                    {
                                        pickupsDeliveries.Add(new int[] { pickupIdToNodeMap[pickupID], combinedLocation.Node });
                                    }
                                }                                                     
                            }
                        }
                        break;
                }

                for (int k = 0; k < arrVrpSettings.Count; k++)
                {
                    bool isValidPostalCode = false;

                    if (from.PostalCode.Trim() != "")
                    {
                        if (postalSectorToAreaCovered.ContainsKey(from.PostalCode.Substring(0, 2)))
                        {
                            if (!arrVrpSettings[k].Zones.ContainsKey(postalSectorToAreaCovered[from.PostalCode.Substring(0, 2)].RegionID))
                            {
                                if (!nodeToRemoveVehicleMapForZoneContraint.ContainsKey(combinedLocation.Node))
                                {
                                    nodeToRemoveVehicleMapForZoneContraint[combinedLocation.Node] = new List<long>();

                                }

                                if (!nodeToRemoveVehicleMapForZoneContraint[combinedLocation.Node].Contains(k))
                                {
                                    nodeToRemoveVehicleMapForZoneContraint[combinedLocation.Node].Add(k);
                                }
                            }
                            isValidPostalCode = true;
                        }
                    }

                    if (!isValidPostalCode)
                    {
                        errorMessage = String.Format("Node {0} Invalid postal code", combinedLocation.Node);
                        return null;
                    }

                    if (from.FeatureIDs.Count > 0)
                    {
                        for (int j = 0; j < from.FeatureIDs.Count; j++)
                        {

                            if (!arrVrpSettings[k].Features.Contains(from.FeatureIDs[j]))
                            {
                                if (!nodeToRemoveVehicleMapForFeaturesContraint.ContainsKey(combinedLocation.Node))
                                {
                                    nodeToRemoveVehicleMapForFeaturesContraint[combinedLocation.Node] = new List<long>();

                                }

                                if (!nodeToRemoveVehicleMapForFeaturesContraint[combinedLocation.Node].Contains(k))
                                {
                                    nodeToRemoveVehicleMapForFeaturesContraint[combinedLocation.Node].Add(k);
                                }
                            }
                        }
                    }
                }               

                return combinedLocation;
            }

            priorityMap = new Dictionary<int, List<long>>();
            locationToDriverMap = new Dictionary<long, long>();
            nodeToRemoveVehicleMapForFeaturesContraint = new Dictionary<int, List<long>>();
            nodeToRemoveVehicleMapForZoneContraint = new Dictionary<int, List<long>>();

            for (int i = 0; i < arrLocations.Count; i++)
            {
                if (arrAdHocPickupID != null || arrAdHocDeliveryID != null)
                {
                    if (arrLocations[i].OrderType == "Pickup")
                    {
                        if(arrAdHocPickupID != null && !arrAdHocPickupID.Contains(arrLocations[i].PickupIDs[0]))
                        {
                            foreach (RouteInfo routeInfo in arrRouteInfo)
                            {
                                if (routeInfo.PickupDeliveryInfo.PickupIDs.Contains(arrLocations[i].PickupIDs[0]))
                                {
                                    arrLocations[i].TimeWindowStart = routeInfo.ArrivalTime;
                                    arrLocations[i].TimeWindowEnd = routeInfo.DepartureTime;
                                    break;
                                }
                            }
                        }                       
                    }
                    else if (arrLocations[i].OrderType == "Delivery")
                    {
                        if (arrAdHocDeliveryID != null && !arrAdHocDeliveryID.Contains(arrLocations[i].DeliveryIDs[0]))
                        {
                            foreach (RouteInfo routeInfo in arrRouteInfo)
                            {
                                if (routeInfo.PickupDeliveryInfo.DeliveryIDs.Contains(arrLocations[i].DeliveryIDs[0]))
                                {
                                    arrLocations[i].TimeWindowStart = routeInfo.ArrivalTime;
                                    arrLocations[i].TimeWindowEnd = routeInfo.DepartureTime;
                                    break;
                                }
                            }
                        }
                    }
                }
                else if (isAdHocSolution && i != arrLocations.Count - 1)
                {
                    arrLocations[i].PriorityID += 3;

                    if (arrLocations[i].OrderType == "Pickup")
                    {
                        foreach(RouteInfo routeInfo in arrRouteInfo)
                        {
                            if (routeInfo.PickupDeliveryInfo.PickupIDs.Contains(arrLocations[i].PickupIDs[0]))
                            {
                                arrLocations[i].TimeWindowStart = routeInfo.ArrivalTime;
                                arrLocations[i].TimeWindowEnd = routeInfo.DepartureTime;
                                break;
                            }
                        }
                    }
                    else if (arrLocations[i].OrderType == "Delivery")
                    {
                        foreach (RouteInfo routeInfo in arrRouteInfo)
                        {
                            if (routeInfo.PickupDeliveryInfo.DeliveryIDs.Contains(arrLocations[i].DeliveryIDs[0]))
                            {
                                arrLocations[i].TimeWindowStart = routeInfo.ArrivalTime;
                                arrLocations[i].TimeWindowEnd = routeInfo.DepartureTime;
                                break;
                            }
                        }
                    }
                }
                
                arrLocations[i].Node = arrAllLocation.Count;
                                                               
                if (locationMap.ContainsKey(arrLocations[i].Long + "," + arrLocations[i].Lat))
                {
                    bool isStartOrEndLocation = false;
                    
                    for (int k = 0; k < arrVrpSettings.Count; k++)
                    {
                        bool isEndLocation = arrLocations[i].Long + "," + arrLocations[i].Lat == arrVrpSettings[k].EndLongitude + "," + arrVrpSettings[k].EndLatitude;
                        if (arrLocations[i].Long + "," + arrLocations[i].Lat == arrVrpSettings[k].StartLongitude + "," + arrVrpSettings[k].StartLatitude
                            || isEndLocation)
                        {
                            isStartOrEndLocation = true;
                            if (arrLocations[i].DriverID != 0)
                            {
                                if(arrVrpSettings[k].DriverID == arrLocations[i].DriverID)
                                {
                                    if (isEndLocation)
                                    {
                                        arrAllLocation[k + VehicleCount] = combineLocation(arrLocations[i], arrAllLocation[k + VehicleCount]);
                                        if (arrAllLocation[k + VehicleCount] == null)
                                        {
                                            return;
                                        }
                                            
                                    }
                                    else
                                    {
                                        arrAllLocation[k] = combineLocation(arrLocations[i], arrAllLocation[k]);
                                        if (arrAllLocation[k] == null)
                                        {
                                            return;
                                        }                                           
                                    }                                                                       
                                    break;
                                }
                            }
                            else
                            {
                                int index = locationMap[arrLocations[i].Long + "," + arrLocations[i].Lat];
                                arrAllLocation[index] = combineLocation(arrLocations[i], arrAllLocation[index]);
                                if(arrAllLocation[index] == null)
                                {
                                    return;
                                }
                                break;
                            }
                        }
                    }

                    if (!isStartOrEndLocation)
                    {
                        int index = locationMap[arrLocations[i].Long + "," + arrLocations[i].Lat];
                        arrAllLocation[index] = combineLocation(arrLocations[i], arrAllLocation[index]);
                        if (arrAllLocation[index] == null)
                        {
                            return;
                        }
                    }
                }
                else
                {
                    locationMap[arrLocations[i].Long + "," + arrLocations[i].Lat] = arrLocations[i].Node;

                    if (arrLocations[i].OrderType == "Pickup")
                    {
                        //arrLocations[i].PickupIDs.Add(arrLocations[i].PickupDeliveryID);

                        if (!pickupIdToNodeMap.ContainsKey(arrLocations[i].PickupIDs[0]))
                        {
                            pickupIdToNodeMap[arrLocations[i].PickupIDs[0]] = arrLocations[i].Node;
                        }
                    }
                    else if (arrLocations[i].OrderType == "Delivery")
                    {
                        //arrLocations[i].DeliveryIDs.Add(arrLocations[i].DeliveryIDs[0]);

                        if (arrLocations[i].PickupFromIDs.Count > 0)
                        {
                            foreach (long pickupID in arrLocations[i].PickupFromIDs)
                            {
                                if(pickupID != 0 && pickupIdToNodeMap[pickupID] != arrLocations[i].Node)
                                {
                                    pickupsDeliveries.Add(new int[] { pickupIdToNodeMap[pickupID], arrLocations[i].Node });
                                }                              
                            }
                        }
                    }
                    arrAllLocation.Add(arrLocations[i]);

                    if (priorityMap.ContainsKey(arrLocations[i].PriorityID))
                    {
                        priorityMap[arrLocations[i].PriorityID].Add(arrLocations[i].Node);
                    }
                    else
                    {
                        priorityMap[arrLocations[i].PriorityID] = new List<long>();
                        priorityMap[arrLocations[i].PriorityID].Add(arrLocations[i].Node);
                    }

                    if (arrLocations[i].DriverID != 0)
                    {
                        if (!locationToDriverMap.ContainsKey(arrLocations[i].Node))
                        {
                            locationToDriverMap[arrLocations[i].Node] = arrLocations[i].DriverID;
                        }
                    }

                    bool isValidPostalCode = false;
                    for (int k = 0; k < arrVrpSettings.Count; k++)
                    {
                        if(arrLocations[i].PostalCode.Trim() != "")
                        {
                            if (postalSectorToAreaCovered.ContainsKey(arrLocations[i].PostalCode.Substring(0, 2)))
                            {
                                if(!arrVrpSettings[k].Zones.ContainsKey(postalSectorToAreaCovered[arrLocations[i].PostalCode.Substring(0, 2)].RegionID))
                                {
                                    if (!nodeToRemoveVehicleMapForZoneContraint.ContainsKey(arrLocations[i].Node))
                                    {
                                        nodeToRemoveVehicleMapForZoneContraint[arrLocations[i].Node] = new List<long>();

                                    }

                                    if (!nodeToRemoveVehicleMapForZoneContraint[arrLocations[i].Node].Contains(k))
                                    {
                                        nodeToRemoveVehicleMapForZoneContraint[arrLocations[i].Node].Add(k);
                                    }
                                }
                                isValidPostalCode = true;
                            }
                        }

                        if (!isValidPostalCode)
                        {
                            errorMessage = String.Format("Node {0} Invalid postal code", arrLocations[i].Node);
                            return;
                        }

                        if (arrLocations[i].FeatureIDs.Count > 0)
                        {
                            for (int j = 0; j < arrLocations[i].FeatureIDs.Count; j++)
                            {
                                if (!arrVrpSettings[k].Features.Contains(arrLocations[i].FeatureIDs[j]))
                                {
                                    if (!nodeToRemoveVehicleMapForFeaturesContraint.ContainsKey(arrLocations[i].Node))
                                    {
                                        nodeToRemoveVehicleMapForFeaturesContraint[arrLocations[i].Node] = new List<long>();

                                    }

                                    if (!nodeToRemoveVehicleMapForFeaturesContraint[arrLocations[i].Node].Contains(k))
                                    {
                                        nodeToRemoveVehicleMapForFeaturesContraint[arrLocations[i].Node].Add(k);
                                    }
                                }
                            }
                        }
                    }
                    
                }               
            }           
            //for (int i = 0; i < arrLocations.Count; i++)
            //{
            //    if (arrLocations[i].DeliverToNode != -1)
            //    {
            //        //NodeID minus(-) FixedEndLocationsCount to set pick/deliver index correspond to arrAllLocation
            //        pickupsDeliveries.Add(new int[] { arrLocations[i].Node - FixedEndLocationsCount, arrLocations[i].DeliverToNode - FixedEndLocationsCount });
            //    }
            //}

            List<double> arrWeight = new List<double>();
            List<double> arrVolume = new List<double>();
            List<int> serviceDurations = new List<int>();
            List<int> loadDurations = new List<int>();
            List<int> unloadDurations = new List<int>();
            List<int> waitingDurations = new List<int>();
            List<long> weightCapacities = new List<long>();
            List<long> volumeCapacities = new List<long>();
            List<string> locationCoordinates = new List<string>();           
            TimeMatrix = new long[arrAllLocation.Count, arrAllLocation.Count];
            DistanceMatrix = new long[arrAllLocation.Count, arrAllLocation.Count];
            TimeWindows = new int[arrAllLocation.Count, 2];

            for (int i=0; i < arrAllLocation.Count; i++)
            {
                locationCoordinates.Add(arrAllLocation[i].Long + "," + arrAllLocation[i].Lat);
                arrWeight.Add(arrAllLocation[i].TotalWeight);
                arrVolume.Add(arrAllLocation[i].TotalVolume);
                serviceDurations.Add(Convert.ToInt32(arrAllLocation[i].ServiceDuration));
                loadDurations.Add(Convert.ToInt32(arrAllLocation[i].LoadDuration));
                unloadDurations.Add(Convert.ToInt32(arrAllLocation[i].UnloadDuration));
                waitingDurations.Add(Convert.ToInt32(arrAllLocation[i].WaitingDuration));

                //Set time windows
                for (int j=0; j < 2; j++)
                {
                    if (j == 0) //start time
                    {
                        //TimeWindows[i, j] = (startTimeHour * 60) + startTimeMinute;
                        TimeWindows[i, j] = (arrAllLocation[i].TimeWindowStart.Hour * 60) + arrAllLocation[i].TimeWindowStart.Minute;
                    }
                    else //end time
                    {
                        //TimeWindows[i, j] = (endTimeHour * 60) + endTimeMinute;
                        TimeWindows[i, j] = ((arrAllLocation[i].TimeWindowEnd.Hour * 60) + arrAllLocation[i].TimeWindowEnd.Minute) - arrAllLocation[i].ServiceDuration - arrAllLocation[i].WaitingDuration 
                            - arrAllLocation[i].LoadDuration - arrAllLocation[i].UnloadDuration;

                        if(TimeWindows[i, j] < TimeWindows[i, 0])
                        {
                            isSuccess = false;
                            errorMessage = String.Format("Node {0} Invalid time window. Total service, waiting and load/unload duration: {1} minutes is more than the available time between {2} to {3}", 
                                arrAllLocation[i].Node,
                                (arrAllLocation[i].ServiceDuration + arrAllLocation[i].WaitingDuration + arrAllLocation[i].LoadDuration + arrAllLocation[i].UnloadDuration), 
                                arrAllLocation[i].TimeWindowStart.ToString("yyyy-MM-dd HH:mm"), arrAllLocation[i].TimeWindowEnd.ToString("yyyy-MM-dd HH:mm"));
                            return;
                        }
                    }
                }
            }

            //Set vehicle capacity 
            for(int i = 0; i < arrVrpSettings.Count; i++)
            {
                weightCapacities.Add(arrVrpSettings[i].WeightCapacity);
                volumeCapacities.Add(arrVrpSettings[i].VolumeCapacity);
            }
           
            Starts = startLocations.ToArray();
            Ends = endLocations.ToArray();
            arrLocationWeight = arrWeight.ToArray();
            arrLocationVolume = arrVolume.ToArray();
            ServiceDuration = serviceDurations.ToArray();
            LoadDuration = loadDurations.ToArray();
            UnloadDuration = unloadDurations.ToArray();
            WaitingDuration = waitingDurations.ToArray();
            WeightCapacities = weightCapacities.ToArray();
            VolumeCapacities = volumeCapacities.ToArray();

            Task<List<long[,]>> task = CallService(locationCoordinates);
            task.Wait();
            if (task.Result != null)
            {
                TimeMatrix = task.Result[0];
                DistanceMatrix = task.Result[1];

                MaxTime = 0;
                MaxDistance = 0;
                for (int i = 0; i < arrAllLocation.Count; i++)
                {
                    //Set time and distance to 0 if no end location
                    for(int j = 0; j < arrAllLocation.Count; j++)
                    {
                        //if (j < arrVrpSettings.Count)
                        //{
                        //    TimeMatrix[i, j] = 0;
                        //    DistanceMatrix[i, j] = 0;
                        //}

                        if (randomEndVehicles.Contains(j))
                        {
                            TimeMatrix[i, j] = 0;
                            DistanceMatrix[i, j] = 0;
                        }
                    }
                    
                    if(i != (arrAllLocation.Count - 1))
                    {
                        MaxTime += TimeMatrix[i, i + 1];
                        MaxDistance += DistanceMatrix[i, i + 1];
                    }                   
                }

                isSuccess = true;
            }
            else
            {
                isSuccess = false;
                return;
            }

            //Break time validation
            for (int i = 0; i < arrVrpSettings.Count; i++)
            {
                int timeWindowStart = (arrVrpSettings[i].TimeWindowStart.Hour * 60) + arrVrpSettings[i].TimeWindowStart.Minute;
                int timeWindowEnd = (arrVrpSettings[i].TimeWindowEnd.Hour * 60) + arrVrpSettings[i].TimeWindowEnd.Minute;
                int breakTimeStart = (arrVrpSettings[i].BreakTimeStart.Hour * 60) + arrVrpSettings[i].BreakTimeStart.Minute;
                int breakTimeEnd = (arrVrpSettings[i].BreakTimeEnd.Hour * 60) + arrVrpSettings[i].BreakTimeEnd.Minute;

                if(breakTimeEnd < breakTimeStart)
                {
                    isSuccess = false;
                }
                else if(breakTimeStart < timeWindowStart || breakTimeEnd > timeWindowEnd)
                {
                    isSuccess = false;
                }

                if (!isSuccess)
                {
                    errorMessage = String.Format("VehicleNo {0} Invalid break time", i);
                    return;
                }
            }

            //Add to pickups deliveries array
            PickupsDeliveries = new int[pickupsDeliveries.Count][];
            for (int i = 0; i < pickupsDeliveries.Count; i++)
            {
                PickupsDeliveries[i] = new int[2];
                PickupsDeliveries[i][0] = pickupsDeliveries[i][0];
                PickupsDeliveries[i][1] = pickupsDeliveries[i][1];
            }
        }

        public async Task<List<long[,]>> CallService(List<string> locationCoordinates)
        {
            ResponseModelLocationIQ responseModel = new ResponseModelLocationIQ();
            Service service = new Service();
            string origin = string.Empty;
            string destination = string.Empty;
            List<long[,]> matrixList = new List<long[,]>();
            long[,] distanceArr = new long[locationCoordinates.Count, locationCoordinates.Count];
            long[,] timeArr = new long[locationCoordinates.Count, locationCoordinates.Count];

            List<string> newlist = new List<string>();

            int batchSize = 1;
            int listCount = batchSize;
            int remainder = locationCoordinates.Count % batchSize;
            int numOfBatches = locationCoordinates.Count / batchSize;
            numOfBatches = remainder > 0 ? numOfBatches + 1 : numOfBatches;

            try
            {
                List<Task<IRestResponse>> TaskList = new List<Task<IRestResponse>>();
                for (int i = 0; i < numOfBatches; i++)
                {
                    if (i * batchSize + listCount > locationCoordinates.Count)
                        listCount = remainder;

                    newlist = locationCoordinates.GetRange(i * batchSize, listCount);
                    origin = string.Join(";", newlist);
                    for (int j = 0; j < numOfBatches; j++)
                    {
                        if (i != j)
                        {
                            if (destination.Length > 0)
                            {
                                destination += ";";
                            }

                            destination += locationCoordinates[j];
                        }
                    }
                    TaskList.Add(service.GetAsyncDistance(origin, destination));
                    destination = string.Empty;
                }

                await Task.WhenAll(TaskList.ToArray());

                for (int i = 0; i < numOfBatches; i++)
                {
                    if (i * batchSize + listCount > locationCoordinates.Count)
                        listCount = remainder;

                    newlist = locationCoordinates.GetRange(i * batchSize, listCount);
                    origin = string.Join(";", newlist);

                    var result = TaskList[i].Result;
                    if (result != null && result.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        responseModel = JsonConvert.DeserializeObject<ResponseModelLocationIQ>(result.Content);
                        int k = 1;
                        for (int j = 0; j < locationCoordinates.Count; j++, k++)
                        {
                            if (origin == locationCoordinates[j])
                            {
                                timeArr[i * batchSize, j] = 0;
                                distanceArr[i * batchSize, j] = 0;
                                k = j;
                            }
                            else
                            {

                                timeArr[i * batchSize, j] = responseModel.Durations[0][k] / 60;
                                distanceArr[i * batchSize, j] = responseModel.Distances[0][k];
                            }
                        }
                    }
                    else
                    {
                        errorMessage = String.Format("Cannot find address");
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage = String.Format("Cannot find address");
                Logger.LogEvent(mProjName, "DataModel CallService() Exception: " + ex.Message, System.Diagnostics.EventLogEntryType.Error);
                return null;
            }

            matrixList.Add(timeArr);
            matrixList.Add(distanceArr);
            return matrixList;
        }


    }
}
