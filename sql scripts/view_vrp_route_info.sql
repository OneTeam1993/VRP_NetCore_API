SELECT r.vrp_routes_id, s.company_id, r.route_no, r.asset_id, a.name, r.driver_id, d.driver_name, r.order_type, r.pickup_ids, r.delivery_ids, r.pickup_from_ids, 
r.route_distance, r.route_time, r.arrival_time, r.departure_time, s.break_time_start, s.break_time_end, r.sequence, r.status, r.feature_id, r.accessories, r.flag, r.timestamp, r.rx_time, 
SUM(v.total_weight) total_weight, SUM(v.total_volume) total_volume, SUM(v.service_duration) service_duration, 
(CASE 
	WHEN r.pickup_ids IS NULL AND r.delivery_ids IS NULL AND (r.status = 'Start location' OR r.status = 'Start location with break time') THEN s.load_duration + SUM(IFNULL(v.load_duration, 0))
    ELSE SUM(v.load_duration)
END) AS load_duration,
(CASE 
    WHEN r.pickup_ids IS NULL AND r.delivery_ids IS NULL AND (r.status = 'End location' OR r.status = 'End location with break time') THEN s.unload_duration + SUM(IFNULL(v.unload_duration, 0))
    ELSE SUM(v.unload_duration)
END) AS unload_duration,
SUM(v.waiting_duration) waiting_duration, v.priority_id,
(CASE 
	WHEN r.pickup_ids IS NULL AND r.delivery_ids IS NULL AND (r.status = 'Start location' OR r.status = 'Start location with break time') THEN s.start_latitude
    WHEN r.pickup_ids IS NULL AND r.delivery_ids IS NULL AND (r.status = 'End location' OR r.status = 'End location with break time') THEN s.end_latitude 
    ELSE v.latitude
END) AS latitude,
(CASE 
	WHEN r.pickup_ids IS NULL AND r.delivery_ids IS NULL AND (r.status = 'Start location' OR r.status = 'Start location with break time') THEN s.start_longitude
    WHEN r.pickup_ids IS NULL AND r.delivery_ids IS NULL AND (r.status = 'End location' OR r.status = 'End location with break time') THEN s.end_longitude 
    ELSE v.longitude
END) AS longitude,
(CASE 
	WHEN r.pickup_ids IS NULL AND r.delivery_ids IS NULL THEN s.time_window_start
    ELSE v.time_window_start
END) AS time_window_start,
(CASE 
    WHEN r.pickup_ids IS NULL AND r.delivery_ids IS NULL THEN s.time_window_end 
    ELSE v.time_window_end
END) AS time_window_end,
(CASE 
	WHEN r.pickup_ids IS NULL AND r.delivery_ids IS NULL AND (r.status = 'Start location' OR r.status = 'Start location with break time') THEN s.start_address 
    WHEN r.pickup_ids IS NULL AND r.delivery_ids IS NULL AND (r.status = 'End location' OR r.status = 'End location with break time') THEN s.end_address 
    ELSE v.address 
END) AS address
FROM tracksg.vrp_routes r
LEFT JOIN tracksg.assets a ON r.asset_id = a.asset_id
LEFT JOIN tracksg.drivers d ON r.driver_id = d.driver_id
LEFT JOIN tracksg.vrp_settings s ON r.asset_id = s.asset_id AND r.route_no = s.route_no
LEFT JOIN tracksg.view_vrp_pickup_delivery v ON FIND_IN_SET(v.pickup_id, r.pickup_ids) OR FIND_IN_SET(v.delivery_id, r.delivery_ids)
WHERE r.route_no = 'ws-testing4'
GROUP BY r.vrp_routes_id

