using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VrpModel;
using WebAPITime.Models;

namespace WebAPITime.Repositories
{
    public interface IVrpRepository
    {
        IEnumerable<VrpInfo> GetAll(string routeNo, string companyName, string userName, string roleID);
        VrpAvailableTimeInfo GetAvailableTimeForAdhocOrder(string routeNo, long driverID);
        Task<IEnumerable<VrpInfo>> InsertAdHocOrderAsync(string routeNo, long driverID, string pickupID, string deliveryID, string companyName, string userName, string roleID);
        VrpInfo VRPCalculation(string routeNo, DataModel data, bool isCheckAdHocFeasibility = false, bool isInsertAdHoc = false, bool isRecalculateAfterDelete = false, List<RouteInfo> arrRouteInfo = null);
    }

    public interface IInitialLocationRepository
    {
        List<PickupDeliveryInfo> GetLocationInfo(string routeNo);
        List<PickupDeliveryInfo> GetAssignedLocationInfoByRouteNoDriver(string routeNo, long driverID);
        DroppedNodes GetDroppedNodes(string routeNo, int nodeID);
    }

    public interface IVrpSettingsRepository
    {
        List<VrpSettingInfo> GetVrpSettingInfo(string routeNo);
        VrpSettingInfo GetVrpSettingInfo(string routeNo, long driverID);
    }

    public interface IRouteInfoRepository
    {
        IEnumerable<RouteInfo> GetAllRouteInfoByDriver(string companyID, string driverId, string flag, DateTime timeWindowStart, DateTime timeWindowEnd);
        List<RouteInfo> GetAllRouteInfoByRouteNoDriver(string routeNo, long driverID);
        List<RouteInfo> GetAllRouteInfoByRouteNoFlag(string routeNo, string flag);
        RouteInfo Get(long id);
        VrpInfo AddGeneratedRoutes(string routeNo, VrpInfo vrpInfo, List<VrpSettingInfo> arrVrpSettings, List<PickupDeliveryInfo> arrAllLocation, bool isAdHocCalculation, List<RouteInfo> arrRouteInfo = null);
        Task<ResponseSaveRoutes> SaveRoutesAsync(string routeNo);
        bool Update(RouteInfo routeInfo);
        ResponseRouteInfoDeletion Remove(long routeID, bool isRecalculation, string companyID, string companyName, string userName, string roleID);
        CustomerOrder GetCustomerOrder(long RouteID, int CustomerID);
        ResponseTimelineRoutes GetTimelineRoutes(string routeNo, string flag);
        List<PushNotification> GetAssetTokensByRouteNo(string routeNo);
    }

    public interface IAreaCoveredInfoRepository
    {
        List<AreaCoveredInfo> GetAllByCompanyID(int companyID);
    }

    public interface IAssetFeatureRepository
    {
        List<AssetFeature> GetAll();
    }

    public interface IVrpPickupRepository
    {
        List<VrpPickup> GetPickupByIdsAndCustomerId(List<long> pickupIDs, int customerID);
        List<VrpPickup> GetPickupByIds(List<long> pickupIDs);
        List<string> GetDistinctRouteNo(string pickupIDs);
        bool RemoveNotFeasibleAdhocPickupOrder(List<long> arrPickupIDs);
    }

    public interface IVrpDeliveryRepository
    {
        List<VrpDelivery> GetDeliveryByIdsAndCustomerId(List<long> deliveryIDs, int customerID);
        List<VrpDelivery> GetDeliveryByIds(List<long> deliveryIDs);
        List<string> GetDistinctRouteNo(string deliveryIDs);
        bool RemoveNotFeasibleAdhocDeliveryOrder(List<long> arrDeliveryIDs);
    }

    public interface IEventRepository
    {
        bool LogVrpEvent(string companyID, string companyName, string userName, string roleID, string eventLog);
    }

    public interface IVrpLocationRequestsRepository
    {
        bool AddTotalRequest(string companyID, string routeNo, int totalRequest);
        VrpLocationRequestResponse Get(string companyID, string mode, string dateStart, string dateEnd);
    }

    public interface IVrpRouteReportRepository
    {
        VrpRouteReportResponse GetRouteReport(long routeID);
        VrpRouteReportResponse UpdateVrpRouteReport(string routeNo, long driverID, long routeID, DateTime departureTime, DateTime arrivalTime, DateTime jobEndTime);
        VrpRouteReport GetRouteReportByRouteID(string routeID);
    }
}
