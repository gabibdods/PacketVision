<?php
try {
    $pdo = new PDO("mysql:host=db;dbname=telemetry", "user", "pass", [
        PDO::ATTR_ERRMODE => PDO::ERRMODE_EXCEPTION,
        //PDO::ATTR_PERSISTENT => true
    ]);
} catch (PDOException $e) {
    die("Database connection failed: " . $e->getMessage());
}
?>