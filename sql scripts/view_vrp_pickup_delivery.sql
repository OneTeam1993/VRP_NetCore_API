SELECT PickupDelivery.route_no, vrp_settings.company_id, pickup_delivery_id, pickup_id, delivery_id, pickup_from_id, order_name, order_type, order_no, priority_id, 
	PickupDelivery.driver_id, driver.driver_name, PickupDelivery.feature_ids, PickupDelivery.accessories, 
    PickupDelivery.latitude, PickupDelivery.longitude, PickupDelivery.address, PickupDelivery.postal_code, PickupDelivery.unit, 
    service_duration, total_weight, total_volume, 
	(CASE WHEN order_type = 'Pickup' THEN load_unload_duration ELSE 0 END) AS 'load_duration', 
    (CASE WHEN order_type = 'Delivery' THEN load_unload_duration ELSE 0 END) AS 'unload_duration', 
    waiting_duration, PickupDelivery.time_window_start, PickupDelivery.time_window_end, isAssign, flag
FROM (
		(
			SELECT p.route_no, p.pickup_id 'pickup_delivery_id', p.pickup_id, NULL delivery_id, NULL 'pickup_from_id', p.name 'order_name', 'Pickup' order_type, order_no, priority_id, driver_id, feature_ids, accessories, latitude, longitude, 
            CONCAT(
				address,
				(CASE WHEN unit IS NOT NULL THEN CONCAT(', ', unit) ELSE '' END),
				(CASE WHEN unit IS NOT NULL AND building IS NOT NULL THEN CONCAT(' ', building) WHEN unit IS NULL AND building IS NOT NULL THEN CONCAT(', ', building) ELSE '' END),
                (CASE WHEN postal_code IS NOT NULL THEN CONCAT(', ', postal_code) ELSE '' END)
            ) address, 
            postal_code, unit, total_weight, total_volume, waiting_duration, service_duration, load_duration 'load_unload_duration', time_window_start, time_window_end, IFNULL(p.isAssign, 0) 'isAssign', IFNULL(p.flag, 0) 'flag'
			FROM tracksg.vrp_pickup AS p
			LEFT JOIN (
				SELECT route_no, pickup_id, SUM(weight) AS 'total_weight', SUM(volume) AS 'total_volume', GROUP_CONCAT(NULLIF(feature_id, '')) AS 'feature_ids'
				FROM tracksg.vrp_pickup_item 
				GROUP BY pickup_id
			) AS p_item_demands ON p.pickup_id = p_item_demands.pickup_id
		)
		UNION ALL
        (
			SELECT d.route_no, d.delivery_id 'pickup_delivery_id', NULL pickup_id, d.delivery_id, pickup_id 'pickup_from_id', d.shipping_name 'order_name', 'Delivery' order_type, order_no, priority_id, driver_id, feature_ids, accessories, latitude, longitude, 
             CONCAT(
				shipping_address,
				(CASE WHEN shipping_unit IS NOT NULL THEN CONCAT(', ', shipping_unit) ELSE '' END),
				(CASE WHEN shipping_unit IS NOT NULL AND shipping_building IS NOT NULL THEN CONCAT(' ', shipping_building) WHEN shipping_unit IS NULL AND shipping_building IS NOT NULL THEN CONCAT(', ', shipping_building) ELSE '' END),
                (CASE WHEN shipping_postal_code IS NOT NULL THEN CONCAT(', ', shipping_postal_code) ELSE '' END)
            ) address, 
            shipping_postal_code, shipping_unit, total_weight, total_volume, waiting_duration, service_duration, unload_duration 'load_unload_duration', time_window_start, time_window_end, IFNULL(d.isAssign, 0) 'isAssign', IFNULL(d.flag, 0) 'flag'
			FROM tracksg.vrp_delivery AS d
            LEFT JOIN (
				SELECT route_no, delivery_id, SUM(weight) AS 'total_weight', SUM(volume) AS 'total_volume', GROUP_CONCAT(NULLIF(feature_id, '')) AS 'feature_ids'
				FROM tracksg.vrp_delivery_item 
				GROUP BY delivery_id
			) AS d_item_demands ON d.delivery_id = d_item_demands.delivery_id
		)
) AS PickupDelivery 
LEFT JOIN tracksg.drivers driver ON PickupDelivery.driver_id = driver.driver_id
CROSS JOIN  (
    SELECT vrp_settings.company_id, vrp_settings.route_no FROM tracksg.vrp_settings GROUP BY vrp_settings.route_no
) AS vrp_settings ON PickupDelivery.route_no = vrp_settings.route_no
WHERE PickupDelivery.route_no = 'ws-testing4'