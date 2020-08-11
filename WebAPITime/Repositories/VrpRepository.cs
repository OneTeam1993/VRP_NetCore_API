using Google.OrTools.ConstraintSolver;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using VrpModel;
using WebAPITime.HelperTools;
using WebAPITime.Models;
using WebAPITime.Repositories;

namespace WebApi.Repositories
{
    public class VrpRepository : IVrpRepository
    {
        private static readonly IInitialLocationRepository repoInitialLocation = new InitialLocationRepository();
        private static readonly IVrpSettingsRepository repoVrpSettings = new VrpSettingsRepository();
        private static readonly IRouteInfoRepository repoRouteInfo = new RouteInfoRepository();
        private static readonly IAreaCoveredInfoRepository repoAreaCoveredInfo = new AreaCoveredInfoRepository();
        private static readonly IAssetFeatureRepository repoAssetFeature = new AssetFeatureRepository();
        private static readonly IVrpPickupRepository repoVrpPickup = new VrpPickupRepository();
        private static readonly IVrpDeliveryRepository repoVrpDelivery = new VrpDeliveryRepository();
        public long averageTime = 0;
        public long averageDistance = 0;

        public IEnumerable<VrpInfo> GetAll(string RouteNo)
        {
            List<VrpInfo> arrVrp = new List<VrpInfo>();
            VrpInfo currVrp = new VrpInfo();

            try
            {
                List<PickupDeliveryInfo> arrLocations = repoInitialLocation.GetLocationInfo(RouteNo);
                List<VrpSettingInfo> arrVrpSettings = repoVrpSettings.GetVrpSettingInfo(RouteNo);

                if (arrLocations.Count < 1)
                {
                    currVrp.ErrorMessage = String.Format("{0}", "There is no pickup/delivery order added.");
                }
                else if (arrVrpSettings.Count < 1)
                {
                    currVrp.ErrorMessage = String.Format("{0}", "There is no vehicle added.");
                }
                else
                {
                    List<AreaCoveredInfo> arrAreaCovered = repoAreaCoveredInfo.GetAllByCompanyID(arrVrpSettings.Count > 0 ? arrVrpSettings[0].CompanyID : 0);
                    DataModel data = new DataModel(arrLocations, arrVrpSettings, arrAreaCovered);
                    currVrp = VRPCalculation(RouteNo, data);
                }
            }
            catch(Exception ex)
            {
                currVrp.ErrorMessage = String.Format("Error occured when calculating route. Error message: {0}", ex.Message);
                Logger.LogEvent(ConfigurationManager.AppSettings["mProjName"], String.Format("VrpRepository GetAll(): {0}", ex.Message), System.Diagnostics.EventLogEntryType.Error);
            }

            
            arrVrp.Add(currVrp);
            return arrVrp.ToArray();

        }

        public VrpInfo VRPCalculation(string routeNo, DataModel data, bool isCheckAdHocFeasibility = false, bool isInsertAdHoc = false, bool isRecalculateAfterDelete = false, List<RouteInfo> arrRouteInfo = null)
        {
            VrpInfo currVrp = new VrpInfo();
            try
            {
                if (data.isSuccess == false)
                {
                    currVrp.ErrorMessage = String.Format("{0}", data.errorMessage);
                }
                else
                {
                    RoutingIndexManager manager = new RoutingIndexManager(
                        data.TimeMatrix.GetLength(0),
                        data.VehicleCount,
                        data.Starts,
                        data.Ends);

                    RoutingModel routing = new RoutingModel(manager);

                    #region Region: Time constraint 
                    int transitCallbackIndexTime = routing.RegisterTransitCallback(
                        (long fromIndex, long toIndex) => {
                            var fromNode = manager.IndexToNode(fromIndex);
                            var toNode = manager.IndexToNode(toIndex);
                            return data.TimeMatrix[fromNode, toNode] + data.ServiceDuration[fromNode]
                            + data.LoadDuration[fromNode] + data.UnloadDuration[fromNode] + data.WaitingDuration[fromNode];
                        }
                    );

                    // Add Time constraint.
                    routing.AddDimension(
                        transitCallbackIndexTime, // transit callback
                        1440, // allow waiting time
                        1440, // vehicle maximum capacities
                        false,  // start cumul to zero
                        "Time"
                    );

                    RoutingDimension timeDimension = routing.GetMutableDimension("Time");
                    // Add time window constraints for each location except depot.
                    for (int i = data.VehicleCount + data.FixedEndLocationsCount; i < data.TimeWindows.GetLength(0); ++i)
                    {
                        long index = manager.NodeToIndex(i);
                        timeDimension.CumulVar(index).SetRange(
                            data.TimeWindows[i, 0],
                            data.TimeWindows[i, 1]);
                    }
                    // Add time window constraints for each vehicle start node.
                    for (int i = 0; i < data.VehicleCount; ++i)
                    {
                        long index = routing.Start(i);
                        timeDimension.CumulVar(index).SetRange(
                            data.TimeWindows[i, 0],
                            data.TimeWindows[i, 1]);

                        List<IntervalVar> arrIntervals = new List<IntervalVar>();
                        long breakTimeStartMinute = (data.arrVrpSettings[i].BreakTimeStart.Hour * 60) + data.arrVrpSettings[i].BreakTimeStart.Minute;
                        long breakTimeEndMinute = (data.arrVrpSettings[i].BreakTimeEnd.Hour * 60) + data.arrVrpSettings[i].BreakTimeEnd.Minute;
                        long duration = breakTimeEndMinute - breakTimeStartMinute;
                        var intervalBreak = routing.solver().MakeFixedDurationIntervalVar(breakTimeStartMinute, breakTimeStartMinute, duration, false, "break");
                        arrIntervals.Add(intervalBreak);

                        if (data.arrVrpSettings[i].IsOvertime == 0) //0: no overtime
                        {
                            var intervalEndOfDay = routing.solver().MakeFixedDurationIntervalVar(data.TimeWindows[i, 1], data.TimeWindows[i, 1], 1440 - data.TimeWindows[i, 1], false, "end of day");
                            arrIntervals.Add(intervalEndOfDay);
                        }

                        IntervalVar[] intervals = arrIntervals.ToArray();

                        List<long> nodeVisitTransitList = new List<long>();
                        for (int j = 0; j < routing.Size(); j++)
                        {
                            nodeVisitTransitList.Add(0);
                        }
                        timeDimension.SetBreakIntervalsOfVehicle(intervals, i, nodeVisitTransitList.ToArray());
                    }

                    // Instantiate route start and end times to produce feasible times.
                    for (int i = 0; i < data.VehicleCount; ++i)
                    {
                        routing.AddVariableMinimizedByFinalizer(
                            timeDimension.CumulVar(routing.Start(i)));
                        routing.AddVariableMinimizedByFinalizer(
                            timeDimension.CumulVar(routing.End(i)));
                    }
                    #endregion

                    #region Region: Distance constraint 
                    int transitCallbackIndexDistance = routing.RegisterTransitCallback(
                        (long fromIndex, long toIndex) =>
                        {
                                // Convert from routing variable Index to distance matrix NodeIndex.
                                var fromNode = manager.IndexToNode(fromIndex);
                            var toNode = manager.IndexToNode(toIndex);
                            return data.DistanceMatrix[fromNode, toNode];
                        }
                    );

                    //averageDistance = (data.MaxDistance / data.VehicleCount) + (data.VehicleCount * 11500);

                    routing.AddDimension(
                        transitCallbackIndexDistance,
                        0, // allow waiting time
                        data.MaxDistance * 5, // vehicle maximum capacities
                        true,  // start cumul to zero
                        "Distance"
                    );
                    RoutingDimension distanceDimension = routing.GetMutableDimension("Distance");

                    // Each Vehicles' distance limit/capacity
                    for (int i = 0; i < data.VehicleCount; ++i)
                    {
                        distanceDimension.SetBreakDistanceDurationOfVehicle(data.arrVrpSettings[i].DistanceCapacity * 1000, 1440, i);
                    }
                    #endregion

                    #region Region: Pickup and Deliveries

                    Solver solver = routing.solver();
                    for (int i = 0; i < data.PickupsDeliveries.GetLength(0); i++)
                    {
                        long pickupIndex = manager.NodeToIndex(data.PickupsDeliveries[i][0]);
                        long deliveryIndex = manager.NodeToIndex(data.PickupsDeliveries[i][1]);
                        routing.AddPickupAndDelivery(pickupIndex, deliveryIndex);
                        solver.Add(solver.MakeEquality(
                            routing.VehicleVar(pickupIndex),
                            routing.VehicleVar(deliveryIndex))
                        );
                        solver.Add(solver.MakeLessOrEqual(
                              distanceDimension.CumulVar(pickupIndex),
                              distanceDimension.CumulVar(deliveryIndex))
                        );
                    }
                    #endregion

                    #region Region: Weight Capacity constraint
                    if (data.VehicleCount > 0 && data.arrVrpSettings[0].WeightCapacity > 0)
                    {
                        int weightCallbackIndex = routing.RegisterUnaryTransitCallback(
                            (long fromIndex) =>
                            {
                                    // Convert from routing variable Index to demand NodeIndex.
                                    var fromNode = manager.IndexToNode(fromIndex);
                                return Convert.ToInt64(data.arrLocationWeight[fromNode]);
                            }
                        );
                        routing.AddDimensionWithVehicleCapacity(
                            weightCallbackIndex,
                            0, // null capacity slack
                            data.WeightCapacities, // vehicle maximum capacities
                            true, // start cumul to zero
                            "WeightCapacity"
                        );
                    }
                    #endregion

                    #region Region: Volume Capacity constraint 
                    if (data.VehicleCount > 0 && data.arrVrpSettings[0].VolumeCapacity > 0)
                    {
                        int volumeCallbackIndex = routing.RegisterUnaryTransitCallback(
                            (long fromIndex) =>
                            {
                                    // Convert from routing variable Index to demand NodeIndex.
                                    var fromNode = manager.IndexToNode(fromIndex);
                                return Convert.ToInt64(data.arrLocationVolume[fromNode]);
                            }
                        );
                        routing.AddDimensionWithVehicleCapacity(
                            volumeCallbackIndex,
                            0, // null capacity slack
                            data.VolumeCapacities, // vehicle maximum capacities
                            true, // start cumul to zero
                            "VolumeCapacity"
                        );
                    }
                    #endregion

                    #region Region: Priority constraint
                    //List<int> arrPriority = new List<int>();
                    //foreach (KeyValuePair<int, List<long>> priorityGroup in data.priorityMap)
                    //{
                    //    arrPriority.Add(priorityGroup.Key);
                    //}

                    //arrPriority.Sort((a, b) => b.CompareTo(a));

                    //List<long> arrRemoveValues = new List<long>();

                    //foreach (long node in data.priorityMap[arrPriority[0]])
                    //{
                    //    arrRemoveValues.Add(manager.NodeToIndex((int)node));
                    //}

                    //for (int i = 1; i < arrPriority.Count; i++)
                    //{
                    //    List<long> arrRemoveValues_temp = new List<long>();
                    //    for (int j = 0; j < data.priorityMap[arrPriority[i]].Count; j++)
                    //    {
                    //        long currNode = manager.NodeToIndex((int)data.priorityMap[arrPriority[i]][j]);
                    //        arrRemoveValues_temp.Add(currNode);
                    //        routing.NextVar(currNode).RemoveValues(arrRemoveValues.ToArray());
                    //    }

                    //    if (i + 1 < arrPriority.Count)
                    //    {
                    //        foreach (long node in arrRemoveValues_temp)
                    //        {
                    //            arrRemoveValues.Add(node);
                    //        }
                    //    }
                    //}

                    // Allow to drop nodes based on priority.
                    for (int i = data.VehicleCount + data.FixedEndLocationsCount; i < data.DistanceMatrix.GetLength(0); i++)
                    {
                        long priorityPenalty = 0;
                        int priorityVal = data.arrAllLocation[i].PriorityID;

                        if (priorityVal == 0)
                        {
                            priorityPenalty = 0;
                        }
                        else if (priorityVal == 1)
                        {
                            priorityPenalty = 500000;
                        }
                        else if (priorityVal == 2)
                        {
                            priorityPenalty = 1000000;
                        }
                        else if (priorityVal == 3)
                        {
                            priorityPenalty = 3000000;
                        }
                        else
                        {
                            priorityPenalty = 6000000;
                        }

                        routing.AddDisjunction(
                            new long[] { manager.NodeToIndex(i) }, priorityPenalty);
                    }
                    #endregion

                    #region Region: Specific Driver constraint 
                    foreach (KeyValuePair<long, long> entry in data.locationToDriverMap)
                    {
                        routing.VehicleVar(manager.NodeToIndex((int)entry.Key)).SetValues(new long[] { -1, data.driverToVehicleMap[entry.Value] });
                    }
                    #endregion

                    #region Region: Vehicles' Feature constraint 
                    foreach (KeyValuePair<int, List<long>> entry in data.nodeToRemoveVehicleMapForFeaturesContraint)
                    {
                        routing.VehicleVar(manager.NodeToIndex((int)entry.Key)).RemoveValues(entry.Value.ToArray());
                    }
                    #endregion

                    #region Region: Zone/Region constraint 
                    foreach (KeyValuePair<int, List<long>> entry in data.nodeToRemoveVehicleMapForZoneContraint)
                    {
                        routing.VehicleVar(manager.NodeToIndex((int)entry.Key)).RemoveValues(entry.Value.ToArray());
                    }
                    #endregion

                    // Define cost of each arc.
                    routing.SetArcCostEvaluatorOfAllVehicles(transitCallbackIndexDistance);

                    // Setting first solution heuristic.
                    RoutingSearchParameters searchParameters = operations_research_constraint_solver.DefaultRoutingSearchParameters();
                    searchParameters.FirstSolutionStrategy = FirstSolutionStrategy.Types.Value.PathCheapestArc;
                    searchParameters.LocalSearchMetaheuristic = LocalSearchMetaheuristic.Types.Value.GuidedLocalSearch;
                    searchParameters.TimeLimit = new Duration { Seconds = 3 };

                    try
                    {
                        Assignment solution = routing.SolveWithParameters(searchParameters);

                        int routingStatus = routing.GetStatus();

                        switch (routingStatus)
                        {

                            case 0: //status 0: Problem not solved yet (before calling RoutingModel::Solve()).
                                currVrp.Status = "ROUTING_NOT_SOLVED: Problem not solved yet.";
                                break;
                            case 1:
                                currVrp = GetVRPInfo(data, routing, manager, solution);

                                if (!isCheckAdHocFeasibility && !isInsertAdHoc && !isRecalculateAfterDelete)
                                {
                                    currVrp = repoRouteInfo.AddGeneratedRoutes(routeNo, currVrp, data.arrVrpSettings, data.arrAllLocation, false);
                                }
                                else
                                {
                                    if (currVrp.DroppedNodes.Count == 0)
                                    {
                                        currVrp.isAdHocFeasible = true;

                                        if (isInsertAdHoc || isRecalculateAfterDelete)
                                        {
                                            currVrp = repoRouteInfo.AddGeneratedRoutes(routeNo, currVrp, data.arrVrpSettings, data.arrAllLocation, true, arrRouteInfo);
                                        }
                                    }
                                }
                                
                                currVrp.Status = "ROUTING_SUCCESS: Problem solved successfully.";
                                break;
                            case 2:
                                currVrp.Status = "ROUTING_FAIL: No solution found to the problem.";
                                break;
                            case 3:
                                currVrp.Status = "ROUTING_FAIL_TIMEOUT: Time limit reached before finding a solution.";
                                break;
                            case 4:
                                currVrp.Status = "ROUTING_INVALID: Model, model parameters, or flags are not valid.";
                                break;
                            default:
                                currVrp.Status = "Unknown status code.";
                                break;
                        }

                        currVrp.StatusCode = routingStatus.ToString();
                    }
                    catch (Exception ex)
                    {
                        currVrp.ErrorMessage = String.Format("Error occured when calculating route. Error message: {0}", ex.Message);
                    }

                }
            }
            catch(Exception ex)
            {
                currVrp.ErrorMessage = String.Format("Error occured when calculating route. Error message: {0}", ex.Message);
                Logger.LogEvent(ConfigurationManager.AppSettings["mProjName"], String.Format("VrpRepository GetAll(): {0}", ex.Message), System.Diagnostics.EventLogEntryType.Error);
            }

            return currVrp;
        }

        private VrpInfo GetVRPInfo(in DataModel data, in RoutingModel routing, in RoutingIndexManager manager, in Assignment solution)
        {
            
            VrpInfo vrpInfo = new VrpInfo();

            try
            {
                Random random = new Random();
                List<DroppedNodes> arrDroppedNodes = new List<DroppedNodes>();
                InitialLocationRepository repoInitLoc = new InitialLocationRepository();               
                List<Vehicle> lstVehicle = new List<Vehicle>();
                Node node;
                RoutingDimension timeDimension = routing.GetMutableDimension("Time");
                long totalTime = 0;
                double totalDistance = 0;
                double totalWeight = 0;
                double totalVolume = 0;

                // Display dropped nodes.
                string droppedNodes = "";
                for (int index = 0; index < routing.Size(); ++index)
                {
                    DroppedNodes currDroppedNodes = new DroppedNodes();
                    if (routing.IsStart(index) || routing.IsEnd(index))
                    {
                        continue;
                    }
                    if (solution.Value(routing.NextVar(index)) == index)
                    {
                        droppedNodes += " " + data.arrAllLocation[manager.IndexToNode(index)].Node;
                        //currDroppedNodes = repoInitLoc.GetDroppedNodes(LocationTempID, data.arrAllLocation[manager.IndexToNode(index)].Node);
                        currDroppedNodes.NodeID = data.arrAllLocation[manager.IndexToNode(index)].Node;
                        currDroppedNodes.OrderName = data.arrAllLocation[manager.IndexToNode(index)].OrderName;
                        currDroppedNodes.Address = data.arrAllLocation[manager.IndexToNode(index)].Address;

                        if (!string.IsNullOrEmpty(droppedNodes)) arrDroppedNodes.Add(currDroppedNodes);
                    }
                }

                if (!string.IsNullOrEmpty(droppedNodes))
                {
                    vrpInfo.Logs += String.Format("Dropped nodes: {0} | ", droppedNodes);
                }
                else
                {
                    vrpInfo.Logs += String.Format("Dropped nodes: {0} | ", "Nothing");
                }

                for (int i = 0; i < data.VehicleCount; ++i)
                {
                    Vehicle vehicle = new Vehicle();
                    vehicle.VehicleNo = i;
                    vehicle.VehicleColor = String.Format("#{0:X6}", random.Next(0x1000000));
                    vehicle.DriverName = data.arrVrpSettings[i].DriverName;
                    vehicle.Zones = data.arrVrpSettings[i].Zones;
                    vrpInfo.Logs += String.Format("Route for Vehicle {0}: ", i);

                    string vehicleRoute = "<table width=\"100%\">";
                    int routeNo = 1;
                    long vehicleTotalTime = 0;
                    double vehicleTotalDistance = 0;
                    double weightLoad = 0;
                    double volumeLoad = 0;
                    long routeDistance = 0;
                    long nodeTime = 0;
                    long previousNodeTime = 0;
                    List<Node> lstNodes = new List<Node>();
                    var index = routing.Start(i);
                    var currentIndex = manager.IndexToNode(index);
                    while (routing.IsEnd(index) == false)
                    {
                        var timeVar = timeDimension.CumulVar(index);
                        nodeTime += solution.Min(timeVar) - previousNodeTime;
                        previousNodeTime = solution.Min(timeVar);
                        node = new Node();
                        node.NodeID = data.arrAllLocation[manager.IndexToNode(index)].Node;
                        node.OrderName = data.arrAllLocation[manager.IndexToNode(index)].OrderName;
                        node.Type = data.arrAllLocation[manager.IndexToNode(index)].OrderType;
                        node.PickupIDs = data.arrAllLocation[manager.IndexToNode(index)].PickupIDs;
                        node.DeliveryIDs = data.arrAllLocation[manager.IndexToNode(index)].DeliveryIDs;
                        node.PickupFromIDs = data.arrAllLocation[manager.IndexToNode(index)].PickupFromIDs;
                        node.Address = data.arrAllLocation[manager.IndexToNode(index)].Address;
                        node.PostalCode = data.arrAllLocation[manager.IndexToNode(index)].PostalCode;
                        node.UnitNo = data.arrAllLocation[manager.IndexToNode(index)].UnitNo;
                        node.Zone = node.PostalCode == null ? null : data.postalSectorToAreaCovered[node.PostalCode.Substring(0,2)].RegionName;
                        node.DistanceOfRoute = Math.Round(data.DistanceMatrix[currentIndex, manager.IndexToNode(index)] / 1000.0, 2);
                        node.TimeOfRoute = data.TimeMatrix[currentIndex, manager.IndexToNode(index)];
                        if (lstNodes.Count == 0)
                        {
                            node.ArrivalTime = String.Format("{0}-{1:D2}-{2:D2} {3:D2}:{4:D2}", data.arrAllLocation[manager.IndexToNode(index)].TimeWindowStart.Year, data.arrAllLocation[manager.IndexToNode(index)].TimeWindowStart.Month, data.arrAllLocation[manager.IndexToNode(index)].TimeWindowStart.Day, data.arrAllLocation[manager.IndexToNode(index)].TimeWindowStart.Hour, data.arrAllLocation[manager.IndexToNode(index)].TimeWindowStart.Minute);
                        }
                        else
                        {
                            node.ArrivalTime = String.Format("{0}-{1:D2}-{2:D2} {3:D2}:{4:D2}", data.arrAllLocation[manager.IndexToNode(index)].TimeWindowStart.Year, data.arrAllLocation[manager.IndexToNode(index)].TimeWindowStart.Month, data.arrAllLocation[manager.IndexToNode(index)].TimeWindowStart.Day, nodeTime / 60, nodeTime % 60);
                        }

                        if (lstNodes.Count > 0)
                        {
                            long departureMinute = ((DateTime.Parse(node.ArrivalTime).Hour * 60) + DateTime.Parse(node.ArrivalTime).Minute) - node.TimeOfRoute;
                            lstNodes[lstNodes.Count - 1].DepartureTime = String.Format("{0}-{1:D2}-{2:D2} {3:D2}:{4:D2}", data.arrAllLocation[manager.IndexToNode(index)].TimeWindowStart.Year, data.arrAllLocation[manager.IndexToNode(index)].TimeWindowStart.Month, data.arrAllLocation[manager.IndexToNode(index)].TimeWindowStart.Day, departureMinute / 60, departureMinute % 60);
                        }
                        node.FeatureIDs = data.arrAllLocation[manager.IndexToNode(index)].FeatureIDs;
                        node.Accessories = data.arrAllLocation[manager.IndexToNode(index)].Accessories;
                        node.ServiceDuration = data.ServiceDuration[manager.IndexToNode(index)];
                        node.LoadDuration = data.LoadDuration[manager.IndexToNode(index)];
                        node.UnloadDuration = data.UnloadDuration[manager.IndexToNode(index)];
                        node.WaitingDuration = data.WaitingDuration[manager.IndexToNode(index)];
                        lstNodes.Add(node);
                        currentIndex = manager.IndexToNode(index);
                        node.Weight = Math.Round(data.arrLocationWeight[currentIndex], 2);
                        node.Volume = Math.Round(data.arrLocationVolume[currentIndex], 2);
                        weightLoad += node.Weight;
                        volumeLoad += node.Volume;
                        vrpInfo.Logs += String.Format("Location:{0} WeightLoad:({1}) VolumeLoad:({2})-> ", node.NodeID, weightLoad, volumeLoad);
                        var previousIndex = index;
                        index = solution.Value(routing.NextVar(index));
                        routeDistance += routing.GetArcCostForVehicle(previousIndex, index, 0);
                        vehicleTotalDistance += node.DistanceOfRoute;
                        totalDistance += node.DistanceOfRoute;
                    }


                    var endTimeVar = timeDimension.CumulVar(index);
                    nodeTime += solution.Min(endTimeVar) - previousNodeTime;
                    totalTime += solution.Min(endTimeVar);
                    vehicleTotalTime += solution.Min(endTimeVar);
                    if (data.arrVrpSettings[i].EndAddress != null && data.arrVrpSettings[i].EndAddress.Trim() != "")
                    {
                        node = new Node();
                        node.NodeID = data.arrAllLocation[manager.IndexToNode(index)].Node;
                        node.OrderName = data.arrAllLocation[manager.IndexToNode(index)].OrderName;
                        node.Type = data.arrAllLocation[manager.IndexToNode(index)].OrderType;
                        node.PickupIDs = data.arrAllLocation[manager.IndexToNode(index)].PickupIDs;
                        node.DeliveryIDs = data.arrAllLocation[manager.IndexToNode(index)].DeliveryIDs;
                        node.PickupFromIDs = data.arrAllLocation[manager.IndexToNode(index)].PickupFromIDs;
                        node.Address = data.arrAllLocation[manager.IndexToNode(index)].Address;
                        node.DistanceOfRoute = Math.Round(data.DistanceMatrix[currentIndex, manager.IndexToNode(index)] / 1000.0, 2);
                        node.TimeOfRoute = data.TimeMatrix[currentIndex, manager.IndexToNode(index)];
                        node.ArrivalTime = String.Format("{0}-{1:D2}-{2:D2} {3:D2}:{4:D2}", data.arrAllLocation[manager.IndexToNode(index)].TimeWindowStart.Year, data.arrAllLocation[manager.IndexToNode(index)].TimeWindowStart.Month, data.arrAllLocation[manager.IndexToNode(index)].TimeWindowStart.Day, nodeTime / 60, nodeTime % 60);
                        if (lstNodes.Count > 0)
                        {
                            long departureMinute = ((DateTime.Parse(node.ArrivalTime).Hour * 60) + DateTime.Parse(node.ArrivalTime).Minute) - node.TimeOfRoute;
                            lstNodes[lstNodes.Count - 1].DepartureTime = String.Format("{0}-{1:D2}-{2:D2} {3:D2}:{4:D2}", data.arrAllLocation[manager.IndexToNode(index)].TimeWindowStart.Year, data.arrAllLocation[manager.IndexToNode(index)].TimeWindowStart.Month, data.arrAllLocation[manager.IndexToNode(index)].TimeWindowStart.Day, departureMinute / 60, departureMinute % 60);
                        }
                        node.FeatureIDs = data.arrAllLocation[manager.IndexToNode(index)].FeatureIDs;
                        node.Accessories = data.arrAllLocation[manager.IndexToNode(index)].Accessories;
                        node.ServiceDuration = data.ServiceDuration[manager.IndexToNode(index)];
                        node.LoadDuration = data.LoadDuration[manager.IndexToNode(index)];
                        node.UnloadDuration = data.UnloadDuration[manager.IndexToNode(index)];
                        node.WaitingDuration = data.WaitingDuration[manager.IndexToNode(index)];
                        node.Weight = Math.Round(data.arrLocationWeight[manager.IndexToNode(index)], 2);
                        node.Volume = Math.Round(data.arrLocationVolume[manager.IndexToNode(index)], 2);
                        weightLoad += node.Weight;
                        volumeLoad += node.Volume;
                        vrpInfo.Logs += String.Format("Location:{0} WeightLoad:({1}) VolumeLoad:({2})-> ", node.NodeID, weightLoad, volumeLoad);
                        lstNodes.Add(node);
                        vehicleTotalDistance += node.DistanceOfRoute;
                        totalDistance += node.DistanceOfRoute;
                    }

                    vehicle.TotalTime = Math.Round((vehicleTotalTime - data.TimeWindows[i, 0]) / 60.0, 1);
                    vehicle.TotalDistance = Math.Round(vehicleTotalDistance, 2);
                    for (int j = 0; j < lstNodes.Count; j++)
                    {                       
                        if (j == 0)
                        {
                            lstNodes[j].Status = "Start location";
                        }
                        else if (j == lstNodes.Count - 1)
                        {
                            lstNodes[j].Status = "End location";
                        }                       
                        else
                        {
                            lstNodes[j].Status = "Waypoint";
                        }

                        if (data.arrVrpSettings[i].BreakTimeStart >= DateTime.Parse(lstNodes[j].ArrivalTime) && (lstNodes[j].DepartureTime != null && data.arrVrpSettings[i].BreakTimeStart <= DateTime.Parse(lstNodes[j].DepartureTime)))
                        {
                            lstNodes[j].Status += " with break time";
                        }

                        vehicleRoute += String.Format("<tr><td style=\"text-align: left;\">Location {0}: <span style=\"font-weight: bold;\">{1}</span></br>Customer/Order Name: {2}</br>Type: {3}</br>Status: {4}</br>{5} DateTime: {6}</br>{7}Service Duration: {8}</br>Weight: {9}</br>Volume: {10}</br>Load Duration: {11}</br>Unload Duration: {12}</br>Waiting Duration: {13}</td></tr>", 
                            routeNo++, lstNodes[j].Address, lstNodes[j].OrderName.Count > 0 ? string.Join(", ", lstNodes[j].OrderName) : "-", lstNodes[j].Type, lstNodes[j].Status, j > 0 ? "Arrival" : "Start", lstNodes[j].ArrivalTime, j + 1 != lstNodes.Count ? "Departure DateTime: " + lstNodes[j].DepartureTime + "</br>" : "", lstNodes[j].ServiceDuration, lstNodes[j].Weight, lstNodes[j].Volume, lstNodes[j].LoadDuration, lstNodes[j].UnloadDuration, lstNodes[j].WaitingDuration);
                    }
                    vehicle.Route = vehicleRoute + "</table>";
                    vehicle.WeightCapacity = data.arrVrpSettings[i].WeightCapacity;
                    vehicle.VolumeCapacity = data.arrVrpSettings[i].VolumeCapacity;
                    vehicle.TotalWeightLoad = weightLoad;
                    vehicle.TotalVolumeLoad = volumeLoad;
                    vehicle.Nodes = lstNodes.ToArray();
                    lstVehicle.Add(vehicle);

                    totalWeight += weightLoad;
                    totalVolume += volumeLoad;

                    vrpInfo.Logs += String.Format(" Distance of the route: {0} km ", vehicle.TotalDistance);
                    vrpInfo.Logs += String.Format(" Weight of the route: {0} ", weightLoad);
                    vrpInfo.Logs += String.Format(" Volume of the route: {0} | ", volumeLoad);
                }

                #region Region: Reason for dropped nodes               
                Dictionary<int, AssetFeature> assetFeatureMap = new Dictionary<int, AssetFeature>();
                Dictionary<int, bool> vehicleToFeaturesConstraintMap = new Dictionary<int, bool>();
                Dictionary<int, bool> vehicleToZonesConstraintMap = new Dictionary<int, bool>();
                bool hasRanGetAssetFeature = false;
                foreach (DroppedNodes droppedNode in arrDroppedNodes)
                {
                    PickupDeliveryInfo droppedLocation = data.arrAllLocation[(int)droppedNode.NodeID];
                    droppedNode.Reasons = new Dictionary<string, string>();
                    bool isFeaturesConstraint = false;
                    bool isZonesConstraint = false;                 
                    bool isDistanceConstraint = false;
                    bool isWeightConstraint = false;
                    bool isVolumeConstraint = false;
                    bool isTimeWindowConstraint = false;
                    bool isOvertimeConstraint = false;
                    bool isPriorityConstraint = false;
                    bool isPickupDeliveryPair = false;
                    int vehicleNotFulfillFeatureConstraintCount = 0;
                    int vehicleNotFulfillZoneConstraintCount = 0;
                    int vehicleNotFulfillDistanceConstraintCount = 0;
                    int vehicleNotFulfillWeightConstraintCount = 0;
                    int vehicleNotFulfillVolumeConstraintCount = 0;

                    for (int i=0; i<data.arrVrpSettings.Count; i++)
                    { 
                        VrpSettingInfo vrpSetting = data.arrVrpSettings[i];
                        vehicleToFeaturesConstraintMap[i] = false;
                        vehicleToZonesConstraintMap[i] = false;

                        #region Sub-Region: Check for features constraint
                        foreach (int featureID in droppedLocation.FeatureIDs)
                        {
                            if (!vrpSetting.Features.Contains(featureID))
                            {
                                vehicleNotFulfillFeatureConstraintCount++;
                                vehicleToFeaturesConstraintMap[i] = true;
                                break;
                            }
                        }
                        #endregion

                        #region Sub-Region: Check for zones constraint
                        if (!vrpSetting.Zones.ContainsKey(data.postalSectorToAreaCovered[droppedLocation.PostalCode.Substring(0, 2)].RegionID))
                        {
                            vehicleNotFulfillZoneConstraintCount++;
                            vehicleToZonesConstraintMap[i] = true;
                        }
                        #endregion

                        #region Sub-Region: Check for distance constraint
                        if ((lstVehicle[i].TotalDistance * 1000) + data.DistanceMatrix[i, droppedLocation.Node] + data.DistanceMatrix[droppedLocation.Node, lstVehicle[i].Nodes[lstVehicle[i].Nodes.Length - 1].NodeID] > (data.arrVrpSettings[0].DistanceCapacity * 1000))
                        {
                            vehicleNotFulfillDistanceConstraintCount++;
                        }
                        #endregion

                        #region Sub-Region: Check for weight constraint
                        if (data.arrVrpSettings[0].WeightCapacity > 0)
                        {
                            if(lstVehicle[i].TotalWeightLoad + droppedLocation.TotalWeight > lstVehicle[i].WeightCapacity)
                            {
                                vehicleNotFulfillWeightConstraintCount++;
                            }
                        }
                        #endregion

                        #region Sub-Region: Check for volume constraint
                        if (data.arrVrpSettings[0].VolumeCapacity > 0)
                        {
                            if (lstVehicle[i].TotalVolumeLoad + droppedLocation.TotalVolume > lstVehicle[i].VolumeCapacity)
                            {
                                vehicleNotFulfillVolumeConstraintCount++;
                            }
                        }
                        #endregion
                    }

                    #region Sub-Region: Reason for features constraint
                    if (vehicleNotFulfillFeatureConstraintCount == data.arrVrpSettings.Count)
                    {
                        if(!hasRanGetAssetFeature)
                        {
                            List<AssetFeature> arrAssetFeature = repoAssetFeature.GetAll();
                            hasRanGetAssetFeature = true;

                            foreach (AssetFeature assetFeature in arrAssetFeature)
                            {
                                if (!assetFeatureMap.ContainsKey(assetFeature.FeatureID))
                                {
                                    assetFeatureMap[assetFeature.FeatureID] = assetFeature;
                                }
                            }
                        }

                        string features = "";
                        foreach(int featureID in droppedLocation.FeatureIDs)
                        {
                            if(features.Length > 0)
                            {
                                features += ", ";
                            }
                                
                            features += assetFeatureMap[featureID].Description;
                        }
                        droppedNode.Reasons["Features constraint"] = "No available vehicles for required feature: " + features;
                        isFeaturesConstraint = true;
                    }
                    #endregion

                    #region Sub-Region: Reason for zones constraint
                    if (vehicleNotFulfillZoneConstraintCount == data.arrVrpSettings.Count)
                    {
                        droppedNode.Reasons["Zones constraint"] = "No available drivers for zone: " + data.postalSectorToAreaCovered[droppedLocation.PostalCode.Substring(0, 2)].RegionName;
                        isZonesConstraint = true;
                    }
                    #endregion

                    #region Sub-Region: Reason for distance constraint
                    if (vehicleNotFulfillDistanceConstraintCount == data.arrVrpSettings.Count)
                    {
                        string distanceDemand = "";
                        for (int i=0; i<data.arrVrpSettings.Count; i++)
                        {
                            if (distanceDemand.Length > 0)
                            {
                                distanceDemand += " |";
                            }
                            distanceDemand += "Vehicle " + i + ": ";
                            distanceDemand += Math.Round((double)((data.DistanceMatrix[i, droppedLocation.Node] + data.DistanceMatrix[droppedLocation.Node, lstVehicle[i].Nodes[lstVehicle[i].Nodes.Length - 1].NodeID]) / 1000), 1);
                        }
                        
                        droppedNode.Reasons["Distance constraint"] = "All vehicle(s) reached maximum distance limit. Estimated additional distance demand(km): " + distanceDemand;
                        isDistanceConstraint = true;
                    }
                    #endregion

                    #region Sub-Region: Reason for weight constraint
                    if (vehicleNotFulfillWeightConstraintCount == data.arrVrpSettings.Count)
                    {
                        droppedNode.Reasons["Weight constraint"] = "All vehicle(s) reached maximum weight limit. Additional weight demand(kg): " + droppedLocation.TotalWeight;
                        isWeightConstraint = true;
                    }
                    #endregion

                    #region Sub-Region: Reason for volume constraint
                    if (vehicleNotFulfillVolumeConstraintCount == data.arrVrpSettings.Count)
                    {
                        droppedNode.Reasons["Volume constraint"] = "All vehicle(s) reached maximum volume limit. Additional volume demand(m³): " + droppedLocation.TotalVolume;
                        isVolumeConstraint = true;
                    }
                    #endregion

                    #region Sub-Region: Check for time windows/overtime constraint & Reason
                    int vehicleNotFulfillTimeWindowsConstraintCount = 0;
                    int vehicleNotFulfillOvertimeConstraintCount = 0;
                    int availableVehicheCount = 0;
                    List<string> suitableDriver = new List<string>();

                    if (!isFeaturesConstraint && !isZonesConstraint && !isDistanceConstraint && !isVolumeConstraint && !isWeightConstraint)
                    {                                               
                        for (int i = 0; i < data.arrVrpSettings.Count; i++)
                        {
                            if (!vehicleToFeaturesConstraintMap[i] && !vehicleToZonesConstraintMap[i])
                            {
                                
                                if (droppedLocation.DriverID != 0)
                                {
                                    if (droppedLocation.DriverID == data.arrVrpSettings[i].DriverID)
                                    {
                                        availableVehicheCount++;
                                        suitableDriver.Add(data.arrVrpSettings[i].DriverName);

                                        if (DateTime.Parse(lstVehicle[i].Nodes[0].ArrivalTime) > droppedLocation.TimeWindowEnd)
                                        {
                                            vehicleNotFulfillTimeWindowsConstraintCount++;
                                        }
                                        else if (DateTime.Parse(lstVehicle[i].Nodes[lstVehicle[i].Nodes.Length-1].ArrivalTime) > droppedLocation.TimeWindowEnd)
                                        {
                                            vehicleNotFulfillTimeWindowsConstraintCount++;
                                        }
                                        else if (data.arrVrpSettings[i].TimeWindowEnd > DateTime.Parse(lstVehicle[i].Nodes[lstVehicle[i].Nodes.Length - 1].ArrivalTime) && data.arrVrpSettings[i].TimeWindowEnd > droppedLocation.TimeWindowEnd)
                                        {
                                            vehicleNotFulfillTimeWindowsConstraintCount++;

                                            if(data.arrVrpSettings[i].IsOvertime == 0)
                                            {
                                                vehicleNotFulfillOvertimeConstraintCount++;
                                            }
                                        }
                                        else if (data.arrVrpSettings[i].IsOvertime == 0 && droppedLocation.TimeWindowStart >= data.arrVrpSettings[i].TimeWindowEnd)
                                        {
                                            vehicleNotFulfillOvertimeConstraintCount++;
                                        }
                                        break;
                                    }                                    
                                }
                                else
                                {
                                    availableVehicheCount++;
                                    suitableDriver.Add(data.arrVrpSettings[i].DriverName);

                                    if (DateTime.Parse(lstVehicle[i].Nodes[0].ArrivalTime) > droppedLocation.TimeWindowEnd)
                                    {
                                        vehicleNotFulfillTimeWindowsConstraintCount++;
                                    }
                                    else if (DateTime.Parse(lstVehicle[i].Nodes[lstVehicle[i].Nodes.Length - 1].ArrivalTime) > droppedLocation.TimeWindowEnd)
                                    {
                                        vehicleNotFulfillTimeWindowsConstraintCount++;
                                    }
                                    else if (data.arrVrpSettings[i].TimeWindowEnd > DateTime.Parse(lstVehicle[i].Nodes[lstVehicle[i].Nodes.Length - 1].ArrivalTime) && data.arrVrpSettings[i].TimeWindowEnd > droppedLocation.TimeWindowEnd)
                                    {
                                        vehicleNotFulfillTimeWindowsConstraintCount++;

                                        if (data.arrVrpSettings[i].IsOvertime == 0)
                                        {
                                            vehicleNotFulfillOvertimeConstraintCount++;
                                        }
                                    }
                                    else if (data.arrVrpSettings[i].IsOvertime == 0 && droppedLocation.TimeWindowStart >= data.arrVrpSettings[i].TimeWindowEnd)
                                    {
                                        vehicleNotFulfillOvertimeConstraintCount++;
                                    }
                                }                             
                            }
                        }

                        if (vehicleNotFulfillTimeWindowsConstraintCount == availableVehicheCount)
                        {
                            droppedNode.Reasons["Time window constraint"] = "Time window " + droppedLocation.TimeWindowStart.ToString("yyyy-MM-dd HH:mm") + " to " + droppedLocation.TimeWindowEnd.ToString("yyyy-MM-dd HH:mm") + " does not fit into driver's schedule: " + string.Join(",", suitableDriver);
                            isTimeWindowConstraint = true;
                        }

                        if (vehicleNotFulfillOvertimeConstraintCount == availableVehicheCount)
                        {
                            droppedNode.Reasons["Overtime constraint"] = "Overtime is NOT enabled for driver: " + string.Join(",", suitableDriver);
                            isOvertimeConstraint = true;
                        }
                    }
                    #endregion

                    #region Sub-Region: Check for priority constraint & Reason
                    if (isDistanceConstraint || isWeightConstraint || isVolumeConstraint)
                    {
                        List<int> arrPriority = new List<int>();
                        foreach (KeyValuePair<int, List<long>> priorityGroup in data.priorityMap)
                        {
                            arrPriority.Add(priorityGroup.Key);
                        }

                        arrPriority.Sort((a, b) => b.CompareTo(a));

                        if(arrPriority.Count == 1)
                        {
                            droppedNode.Reasons["Priority constraint"] = "All location(s) have same priority. Location with further travel distance will be dropped off";
                            isPriorityConstraint = true;
                        }
                        else if (droppedLocation.PriorityID < arrPriority[0])
                        {
                            droppedNode.Reasons["Priority constraint"] = "This location has been set to lower priority";
                            isPriorityConstraint = true;
                        }
                    }
                    #endregion

                    #region Sub-Region: Check for pickup and delivery pair & Reason
                    string nodePairTo = "";
                    foreach(int[] pickupDelivery in data.PickupsDeliveries)
                    {
                        if(droppedLocation.Node == pickupDelivery[0] || droppedLocation.Node == pickupDelivery[1])
                        {
                            if (droppedLocation.Node == pickupDelivery[0])
                            {
                                nodePairTo += pickupDelivery[1];
                            }
                            else
                            {
                                nodePairTo += pickupDelivery[0];
                            }
                            isPickupDeliveryPair = true;
                            break;
                        }
                    }

                    if (isPickupDeliveryPair)
                    {
                        droppedNode.Reasons["Pickup and delivery pair constraint"] = "This node is pair to node " + nodePairTo + ". Both nodes of the pair will be dropped off";
                    }
                    #endregion

                    #region Sub-Region: Check for specific driver constraint & Reason
                    if (droppedLocation.DriverID != 0)
                    {
                        string constraints = "";

                        if (isFeaturesConstraint)
                        {
                            if (constraints.Length > 0)
                            {
                                constraints += ", ";
                            }
                            constraints += "features constraint";
                        }

                        if (isZonesConstraint)
                        {
                            if (constraints.Length > 0)
                            {
                                constraints += ", ";
                            }
                            constraints += "zones constraint";
                        }

                        if (isDistanceConstraint)
                        {
                            if (constraints.Length > 0)
                            {
                                constraints += ", ";
                            }
                            constraints += "distance constraint";
                        }

                        if (isWeightConstraint)
                        {
                            if (constraints.Length > 0)
                            {
                                constraints += ", ";
                            }
                            constraints += "weight constraint";
                        }

                        if (isVolumeConstraint)
                        {
                            if (constraints.Length > 0)
                            {
                                constraints += ", ";
                            }
                            constraints += "volume constraint";
                        }

                        if (isTimeWindowConstraint)
                        {
                            if (constraints.Length > 0)
                            {
                                constraints += ", ";
                            }
                            constraints += "time window constraint";
                        }

                        if (isOvertimeConstraint)
                        {
                            if (constraints.Length > 0)
                            {
                                constraints += ", ";
                            }
                            constraints += "overtime constraint";
                        }

                        if (isPriorityConstraint)
                        {
                            if (constraints.Length > 0)
                            {
                                constraints += ", ";
                            }
                            constraints += "priority constraint";
                        }

                        if (isPickupDeliveryPair)
                        {
                            if (constraints.Length > 0)
                            {
                                constraints += ", ";
                            }
                            constraints += "pickup and delivery pair constraint";
                        }

                        droppedNode.Reasons["Specific driver constraint"] = "This node is assigned to driver: " + data.arrVrpSettings[(int)data.driverToVehicleMap[droppedLocation.DriverID]].DriverName + ". It does not fulfilled " + constraints;
                    }
                    #endregion
                }
                #endregion

                vrpInfo.Objective = solution.ObjectiveValue();
                vrpInfo.DroppedNodes = arrDroppedNodes;
                vrpInfo.TotalDistance = Math.Round(totalDistance, 2);
                vrpInfo.TotalWeight = totalWeight;
                vrpInfo.TotalVolume = totalVolume;
                vrpInfo.Vehicles = lstVehicle.ToArray();
            }
            catch(Exception ex)
            {
                vrpInfo.ErrorMessage = String.Format("Error occured when calculationg route. Error message: {0}", ex.Message);
                Logger.LogEvent(ConfigurationManager.AppSettings["mProjName"], String.Format("VrpRepository GetVRPInfo(): {0}", ex.Message), System.Diagnostics.EventLogEntryType.Error);
            }
            

            return vrpInfo;
        }

        public VrpAvailableTimeInfo GetAvailableTimeForAdhocOrder(string routeNo, long driverID)
        {
            VrpSettingInfo vrpSettingInfo = repoVrpSettings.GetVrpSettingInfo(routeNo, driverID);
            List<RouteInfo> arrRouteInfo = repoRouteInfo.GetAllRouteInfoByRouteNoDriver(routeNo, driverID);

            VrpAvailableTimeInfo vrpAvailableTimeInfo = new VrpAvailableTimeInfo();
            vrpAvailableTimeInfo.RouteNo = routeNo;
            vrpAvailableTimeInfo.DriverID = driverID;
            vrpAvailableTimeInfo.IsOvertime = vrpSettingInfo.IsOvertime;
            vrpAvailableTimeInfo.AvailableTime = new List<AvailableTime>();

            int breakTimeStartMinute = (vrpSettingInfo.BreakTimeStart.Hour * 60) + vrpSettingInfo.BreakTimeStart.Minute;
            int breakTimeEndMinute = (vrpSettingInfo.BreakTimeEnd.Hour * 60) + vrpSettingInfo.BreakTimeEnd.Minute;
            int breakDuration = breakTimeEndMinute - breakTimeStartMinute;

            for (int i = 0; i < arrRouteInfo.Count; i++)
            {

                RouteInfo currRoute = arrRouteInfo[i];
                PickupDeliveryInfo pickupDeliveryInfo = currRoute.PickupDeliveryInfo;
                int totalDuration = pickupDeliveryInfo.ServiceDuration + pickupDeliveryInfo.LoadDuration + pickupDeliveryInfo.UnloadDuration + pickupDeliveryInfo.WaitingDuration;

                //if (currRoute.Status.Contains("with break time"))
                //{
                //    totalDuration += breakDuration;
                //}

                if (currRoute.DepartureTime == Convert.ToDateTime("1/1/2000 00:00:00"))
                {
                    AvailableTime availableTime = new AvailableTime();
                    availableTime.TimeStart = currRoute.ArrivalTime.AddMinutes(totalDuration);

                    if (vrpSettingInfo.IsOvertime == 0)
                    {
                        availableTime.TimeEnd = vrpSettingInfo.TimeWindowEnd;
                        availableTime.Duration = Convert.ToInt32(availableTime.TimeEnd.Subtract(availableTime.TimeStart).TotalMinutes).ToString();
                    }
                    else
                    {
                        availableTime.Duration = "infinite";
                    }

                    vrpAvailableTimeInfo.AvailableTime.Add(availableTime);

                    //To exclude break time
                    if (vrpSettingInfo.BreakTimeStart > availableTime.TimeStart && vrpSettingInfo.BreakTimeStart < availableTime.TimeEnd)
                    {
                        AvailableTime availableTimeSplit = new AvailableTime();
                        availableTimeSplit.TimeStart = vrpSettingInfo.BreakTimeEnd;
                        availableTimeSplit.TimeEnd = availableTime.TimeEnd;
                        availableTimeSplit.Duration = Convert.ToInt32(availableTimeSplit.TimeEnd.Subtract(availableTimeSplit.TimeStart).TotalMinutes).ToString();
                        vrpAvailableTimeInfo.AvailableTime.Add(availableTimeSplit);

                        availableTime.TimeEnd = vrpSettingInfo.BreakTimeStart;
                        availableTime.Duration = Convert.ToInt32(availableTime.TimeEnd.Subtract(availableTime.TimeStart).TotalMinutes).ToString();
                    }                  
                }
                else if (currRoute.ArrivalTime.AddMinutes(totalDuration) != currRoute.DepartureTime)
                {
                    AvailableTime availableTime = new AvailableTime();
                    availableTime.TimeStart = currRoute.ArrivalTime.AddMinutes(totalDuration);
                    availableTime.TimeEnd = currRoute.DepartureTime;
                    availableTime.Duration = Convert.ToInt32(availableTime.TimeEnd.Subtract(availableTime.TimeStart).TotalMinutes).ToString();

                    if (!(availableTime.TimeStart == vrpSettingInfo.BreakTimeStart && availableTime.TimeEnd == vrpSettingInfo.BreakTimeEnd))
                    {
                        vrpAvailableTimeInfo.AvailableTime.Add(availableTime);

                        //To exclude break time
                        if (vrpSettingInfo.BreakTimeStart > availableTime.TimeStart && vrpSettingInfo.BreakTimeStart < availableTime.TimeEnd)
                        {
                            AvailableTime availableTimeSplit = new AvailableTime();
                            availableTimeSplit.TimeStart = vrpSettingInfo.BreakTimeEnd;
                            availableTimeSplit.TimeEnd = availableTime.TimeEnd;
                            availableTimeSplit.Duration = Convert.ToInt32(availableTimeSplit.TimeEnd.Subtract(availableTimeSplit.TimeStart).TotalMinutes).ToString();
                            vrpAvailableTimeInfo.AvailableTime.Add(availableTimeSplit);

                            availableTime.TimeEnd = vrpSettingInfo.BreakTimeStart;
                            availableTime.Duration = Convert.ToInt32(availableTime.TimeEnd.Subtract(availableTime.TimeStart).TotalMinutes).ToString();
                        }

                        
                    }

                }
            }

            return vrpAvailableTimeInfo;
        }

        public IEnumerable<VrpInfo> CheckAdHocOrderFeasibility(string routeNo, long driverID, TempAdHocLocation tempAdHocLocation)
        {
            List<VrpInfo> arrVrp = new List<VrpInfo>();
            VrpInfo currVrp = new VrpInfo();
            
            try
            {
                List<PickupDeliveryInfo> arrLocations = repoInitialLocation.GetAssignedLocationInfoByRouteNoDriver(routeNo, driverID);
                List<VrpSettingInfo> arrVrpSettings = new List<VrpSettingInfo>();
                VrpSettingInfo vrpSettingInfo = repoVrpSettings.GetVrpSettingInfo(routeNo, driverID);
                arrVrpSettings.Add(vrpSettingInfo);
                List<RouteInfo> arrRouteInfo = repoRouteInfo.GetAllRouteInfoByRouteNoDriver(routeNo, driverID);
                List<AreaCoveredInfo> arrAreaCovered = repoAreaCoveredInfo.GetAllByCompanyID(arrVrpSettings.Count > 0 ? arrVrpSettings[0].CompanyID : 0);

                PickupDeliveryInfo adHocLocation = new PickupDeliveryInfo();
                //adHocLocation.OrderType = "Pickup";
                adHocLocation.PickupIDs = new List<long>();
                adHocLocation.DeliveryIDs = new List<long>();
                adHocLocation.RouteNo = routeNo;
                adHocLocation.PriorityID = 2;
                adHocLocation.DriverID = driverID;
                adHocLocation.DriverName = arrVrpSettings[0].DriverName;
                adHocLocation.Lat = tempAdHocLocation.Lat;
                adHocLocation.Long = tempAdHocLocation.Long;
                adHocLocation.Address = tempAdHocLocation.Address;
                adHocLocation.PostalCode = tempAdHocLocation.PostalCode;
                adHocLocation.TotalWeight = 0;
                adHocLocation.TotalVolume = 0;
                adHocLocation.ServiceDuration = 0;
                adHocLocation.LoadDuration = 0;
                adHocLocation.UnloadDuration = 0;
                adHocLocation.WaitingDuration = 0;
                adHocLocation.TimeWindowStart = tempAdHocLocation.TimeWindowStart;
                adHocLocation.TimeWindowEnd = tempAdHocLocation.TimeWindowEnd;
                adHocLocation.PickupFromIDs = new List<long>();
                adHocLocation.FeatureIDs = new List<int>();
                adHocLocation.Accessories = new List<long>();

                arrLocations.Add(adHocLocation);

                DataModel data = new DataModel(arrLocations, arrVrpSettings, arrAreaCovered, true, arrRouteInfo);

                currVrp = VRPCalculation(routeNo, data, true);
            }
            catch(Exception ex)
            {
                currVrp.ErrorMessage = String.Format("Error occured when calculating route. Error message: {0}", ex.Message);
            }

            arrVrp.Add(currVrp);
            return arrVrp.ToArray();
        }

        public IEnumerable<VrpInfo> InsertAdHocOrder(string routeNo, long driverID, string pickupID, string deliveryID)
        {
            List<VrpInfo> arrVrp = new List<VrpInfo>();
            VrpInfo currVrp = new VrpInfo();

            string[] pickupIDs;
            string[] deliveryIDs;
            List<long> arrAdHocPickup = null;
            List<long> arrAdHocDelivery = null;

            try
            {
                List<PickupDeliveryInfo> arrLocations = repoInitialLocation.GetAssignedLocationInfoByRouteNoDriver(routeNo, driverID);
                List<VrpSettingInfo> arrVrpSettings = new List<VrpSettingInfo>();
                VrpSettingInfo vrpSettingInfo = repoVrpSettings.GetVrpSettingInfo(routeNo, driverID);
                arrVrpSettings.Add(vrpSettingInfo);
                List<RouteInfo> arrRouteInfo = repoRouteInfo.GetAllRouteInfoByRouteNoDriver(routeNo, driverID);
                List<AreaCoveredInfo> arrAreaCovered = repoAreaCoveredInfo.GetAllByCompanyID(arrVrpSettings.Count > 0 ? arrVrpSettings[0].CompanyID : 0);
              
                if (pickupID != null && pickupID.Trim() != "")
                {
                    pickupIDs = pickupID.Split(',');

                    for (int i = 0; i < pickupIDs.Length; i++)
                    {
                        if (pickupIDs[i].Trim() != "" && pickupIDs[i].Trim() != ",")
                        {
                            arrAdHocPickup = arrAdHocPickup ?? new List<long>();
                            arrAdHocPickup.Add(Convert.ToInt64(pickupIDs[i]));
                        }
                            
                    }
                }
              
                if (deliveryID != null && deliveryID.Trim() != "")
                {
                    deliveryIDs = deliveryID.Split(',');

                    for (int i = 0; i < deliveryIDs.Length; i++)
                    {
                        if (deliveryIDs[i].Trim() != "" && deliveryIDs[i].Trim() != ",")
                        {
                            arrAdHocDelivery = arrAdHocDelivery ?? new List<long>();
                            arrAdHocDelivery.Add(Convert.ToInt64(deliveryIDs[i]));
                        }

                    }
                }

                DataModel data = new DataModel(arrLocations, arrVrpSettings, arrAreaCovered, true, arrRouteInfo, arrAdHocPickup, arrAdHocDelivery);

                currVrp = VRPCalculation(routeNo, data, false, true, false, arrRouteInfo);
            }
            catch (Exception ex)
            {
                Logger.LogEvent(ConfigurationManager.AppSettings["mProjName"], String.Format("VrpRepository InsertAdHocOrder(): {0}", ex.Message), System.Diagnostics.EventLogEntryType.Error);
                currVrp.ErrorMessage = String.Format("Error occured when calculating route. Error message: {0}", ex.Message);
            }

            if (!currVrp.isAdHocFeasible && (arrAdHocPickup != null || arrAdHocDelivery != null))
            {
                try
                {
                    bool isDeleteSuccess = true;

                    if (arrAdHocPickup != null && arrAdHocPickup.Count > 0)
                    {
                        if (!repoVrpPickup.RemoveNotFeasibleAdhocPickupOrder(arrAdHocPickup))
                        {
                            isDeleteSuccess = false;
                        }
                    }

                    if (arrAdHocDelivery != null && arrAdHocDelivery.Count > 0)
                    {
                        if (!repoVrpPickup.RemoveNotFeasibleAdhocPickupOrder(arrAdHocDelivery))
                        {
                            isDeleteSuccess = false;
                        }
                    }

                    if (!isDeleteSuccess)
                    {
                        currVrp.ErrorMessage = String.Format("Error occured when deleting ad hoc order that is not feasible.");
                    }
                }
                catch(Exception ex)
                {
                    Logger.LogEvent(ConfigurationManager.AppSettings["mProjName"], String.Format("VrpRepository InsertAdHocOrder(): Error occured when deleting ad hoc order that is not feasible. Exception: {0}", ex.Message), System.Diagnostics.EventLogEntryType.Error);
                    currVrp.ErrorMessage = String.Format("Error occured when deleting ad hoc order that is not feasible.");
                }
            }

            arrVrp.Add(currVrp);
            return arrVrp.ToArray();
        }
        
    }
}
