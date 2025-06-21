<?php
require_once('db.php');

function get_latest_telemetry() {
    global $pdo;
    $stmt = $pdo->query("SELECT * FROM telemetry ORDER BY timestamp DESC LIMIT 1");
    return $stmt->fetch(PDO::FETCH_ASSOC);
}
?>