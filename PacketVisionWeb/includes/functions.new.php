<?php
require_once('db.php');
function get_latest_telemetry() {
    global $pdo;
    $stmt = $pdo->prepare("SELECT speed,
                                        throttle,
                                        steer,
                                        brake,
                                        gear,
                                        engineRPM,
                                        drs,
                                        timestamp FROM telemetry ORDER BY timestamp DESC LIMIT 1");
    $stmt->execute();
    return $stmt->fetch(PDO::FETCH_ASSOC);
}
?>