<?php
require_once('db.php');

function getLatestTelemetry() {
    global $pdo;
    $stmt = $pdo->query("SELECT * FROM SESSION ORDER BY timestamp DESC LIMIT 1");
    return $stmt->fetch(PDO::FETCH_ASSOC);
}