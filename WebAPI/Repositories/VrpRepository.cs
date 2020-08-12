using Google.OrTools.ConstraintSolver;
using Google.Protobuf.WellKnownTypes; // Duration
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VrpModel;
using WebAPI.Repository;

namespace WebAPITime.Repositories
{
    public class VrpRepository : IVrpRepository
    {

        public long penalty = 1000000;
        public long distance_constraint = 1000000;

        public IEnumerable<VrpInfo> GetAll()
        {

            List<VrpInfo> arrVrp = new List<VrpInfo>();
            VrpInfo currVrp = new VrpInfo();
            int temp_id = 1;
            // Instantiate the data problem.
            // [START data]
            DataModel data = new DataModel(temp_id);
            // [END data]
            if (data.DistanceMatrix == null)
            {
                currVrp.ErrorMessage = String.Format("Call Service Exception: {0}", "Invalid Options");
            }
            else
            {         
                // Create Routing Index Manager
                // [START index_manager]
                RoutingIndexManager manager = new RoutingIndexManager(
                data.DistanceMatrix.GetLength(0),
                data.VehicleNumber,
                data.Depot);
                // [END index_manager]

                // Create Routing Model.
                // [START routing_model]
                RoutingModel routing = new RoutingModel(manager);
                // [END routing_model]


                // Create and register a transit callback.
                // [START transit_callback]
                int transitCallbackIndex = routing.RegisterTransitCallback(
                  (long fromIndex, long toIndex) =>
                  {
                  // Convert from routing variable Index to distance matrix NodeIndex.
                  var fromNode = manager.IndexToNode(fromIndex);
                      var toNode = manager.IndexToNode(toIndex);
                      return data.DistanceMatrix[fromNode, toNode];
                  }
                );
                // [END transit_callback]

                try
                {
                    // Define cost of each arc.
                    // [START arc_cost]
                    routing.SetArcCostEvaluatorOfAllVehicles(transitCallbackIndex);
                    // [END arc_cost]
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Arc Cost Exception: {0}", ex.Message);
                    currVrp.ErrorMessage = String.Format("Arc Cost Exception: {0}", ex.Message);
                }

                try
                {
                    // Add Distance constraint.
                    routing.AddDimension(transitCallbackIndex, 0, distance_constraint,
                        true,  // start cumul to zero
                        "Distance");
                    RoutingDimension distanceDimension = routing.GetMutableDimension("Distance");
                    distanceDimension.SetGlobalSpanCostCoefficient(100);

                    //Pickup and Deliveries
                    // Define Transportation Requests.
                    Solver solver = routing.solver();
                    for (int i = 0; i < data.PickupsDeliveries.GetLength(0); i++)
                    {
                        long pickupIndex = manager.NodeToIndex(data.PickupsDeliveries[i][0]);
                        long deliveryIndex = manager.NodeToIndex(data.PickupsDeliveries[i][1]);
                        routing.AddPickupAndDelivery(pickupIndex, deliveryIndex);
                        solver.Add(solver.MakeEquality(
                              routing.VehicleVar(pickupIndex),
                              routing.VehicleVar(deliveryIndex)));
                        solver.Add(solver.MakeLessOrEqual(
                              distanceDimension.CumulVar(pickupIndex),
                              distanceDimension.CumulVar(deliveryIndex)));
                    }
                    //routing.SetPickupAndDeliveryPolicyOfAllVehicles(RoutingModel.PICKUP_AND_DELIVERY_FIFO); //First In, First Out
                    //routing.SetPickupAndDeliveryPolicyOfAllVehicles(RoutingModel.PICKUP_AND_DELIVERY_LIFO); //Last In, First Out
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Distance Exception: {0}", ex.Message);
                    currVrp.ErrorMessage = String.Format("Distance Exception: {0}", ex.Message);
                }

                try
                {
                    //Add Capacity constraint.
                    int demandCallbackIndex = routing.RegisterUnaryTransitCallback(
                      (long fromIndex) =>
                      {
                          // Convert from routing variable Index to demand NodeIndex.
                          var fromNode = manager.IndexToNode(fromIndex);
                          return data.Demands[fromNode];
                      }
                    );
                    routing.AddDimensionWithVehicleCapacity(
                      demandCallbackIndex, 0,  // null capacity slack
                      data.VehicleCapacities,   // vehicle maximum capacities
                      true,                      // start cumul to zero
                      "Capacity");

                    //Penalties and Droppping Visits
                    // Allow to drop nodes.                   
                    for (int i = 1; i < data.DistanceMatrix.GetLength(0); ++i)
                    {
                        routing.AddDisjunction(
                            new long[] { manager.NodeToIndex(i) }, penalty);
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine("Capacity Constraint Exception: {0}", ex.Message);
                    currVrp.ErrorMessage = String.Format("Capacity Constraint Exception: {0}", ex.Message);
                }


                // Setting first solution heuristic.
                // [START parameters]
                RoutingSearchParameters searchParameters = operations_research_constraint_solver.DefaultRoutingSearchParameters();
                searchParameters.FirstSolutionStrategy = FirstSolutionStrategy.Types.Value.PathCheapestArc;
                // [END parameters]
                //searchParameters.TimeLimit = new Duration { Seconds = 10 }; //Set Time Limit in Search

                try
                {
                    // Solve the problem.
                    // [START solve]
                    Assignment solution = routing.SolveWithParameters(searchParameters);
                    // [END solve]
                    if (solution == null)
                    {
                        currVrp.ErrorMessage = "ROUTING_FAIL";
                    }
                    else
                    {
                        // Print solution on console.
                        // [START print_solution]
                        currVrp = PrintSolution(data, routing, manager, solution);
                        // [END print_solution]
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine("Assignment Solution Exception: {0}", ex.Message);
                    currVrp.ErrorMessage = String.Format("Assignment Solution Exception: {0}", ex.Message);
                }
            }

            arrVrp.Add(currVrp);
            return arrVrp.ToArray();
        }

        // [START solution_printer]
        /// <summary>
        ///   Print the solution.
        /// </summary>
        public VrpInfo PrintSolution(in DataModel data,in RoutingModel routing,in RoutingIndexManager manager,in Assignment solution)
        {
            VrpInfo currVrp = new VrpInfo();
    
            List<DroppedNodes> arrDroppedNodes = new List<DroppedNodes>();
            List<NodeInfo> arrNodes = new List<NodeInfo>();
            List<VehicleInfo> arrVehicles = new List<VehicleInfo>();
            InitialLocationRepository repoInitLoc = new InitialLocationRepository();

            try
            {
                currVrp.Objective = solution.ObjectiveValue();

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
                        droppedNodes += " " + manager.IndexToNode(index);
                        //currDroppedNodes.NodeID = manager.IndexToNode(index);
                        currDroppedNodes = repoInitLoc.GetDroppedNodes(manager.IndexToNode(index));
                        if (!string.IsNullOrEmpty(droppedNodes)) arrDroppedNodes.Add(currDroppedNodes);
                    }
                }

                if (!string.IsNullOrEmpty(droppedNodes))
                {
                    currVrp.Logs += String.Format("Dropped nodes: {0} | ", droppedNodes);
                }
                else
                {
                    currVrp.Logs += String.Format("Dropped nodes: {0} | ", "Nothing");
                }

                //Console.WriteLine("Objective: {0}", solution.ObjectiveValue());
                // Inspect solution.
                long totalDistance = 0;
                long totalLoad = 0;
                var loopCounter = 0;

                for (int i = 0; i < data.VehicleNumber; ++i)
                {
                    //Console.WriteLine("Route for Vehicle {0}:", i);
                    VehicleInfo currVehicle = new VehicleInfo();

                    currVehicle.VehicleNo = i;
                    currVrp.Logs += String.Format("Route for Vehicle {0}: ", i);

                    long routeDistance = 0;
                    long routeLoad = 0;
                    var index = routing.Start(i);
                    long nodeIndex = 0;

                    int count = 1;
                    while (routing.IsEnd(index) == false)
                    {  
                        nodeIndex = manager.IndexToNode(index);
                        routeLoad += data.Demands[nodeIndex];
                        currVrp.Logs += String.Format("Location:{0} Load:({1})-> ", nodeIndex, routeLoad);
                        currVehicle.Routes += String.Format("{0} ", nodeIndex);
                        arrNodes.Add(repoInitLoc.GetInitLoc(nodeIndex));
                        var previousIndex = index;
                        index = solution.Value(routing.NextVar(index));
                        routeDistance += routing.GetArcCostForVehicle(previousIndex, index, 0);
                        count++;
                    }

         
                    //Console.WriteLine("{0}", manager.IndexToNode((int)index));
                    //Console.WriteLine("Distance of the route: {0}m", routeDistance);
                    currVrp.Logs += String.Format(" Distance of the route: {0} km ", routeDistance / 1000);
                    currVrp.Logs += String.Format(" Load of the route: {0} | ", routeLoad);
                    currVehicle.DistanceofRoute = routeDistance / 1000;
                    currVehicle.LoadofRoute = routeLoad;

                    currVehicle.Nodes = arrNodes;
               
                    arrVehicles.Add(currVehicle);
     
                    totalDistance += routeDistance;
                    totalLoad += routeLoad;
                    loopCounter++;
                }

                currVrp.DroppedNodes = arrDroppedNodes;
                currVrp.Vehicle = arrVehicles;     
                currVrp.TotalDistance = totalDistance / 1000;
                currVrp.TotalLoad = totalLoad;
                currVrp.ErrorMessage = "Success";
           
            }
            catch (Exception ex)
            {
                Console.WriteLine("Print Solution Exception: {0}", ex.Message);
                currVrp.ErrorMessage = String.Format("Print Solution Exception: {0}", ex.Message);
            }


           return currVrp;
        }



    }
}
