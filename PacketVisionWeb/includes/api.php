<?php
header('Content-Type: text/event-stream');
header('Cache-Control: no-cache');
header('Connection: keep-alive');

$host = 'db';
$dbname = 'PACKETVISION';
$user = 'mysqluser';
$password = 'mysqlpass';
$charset = 'utf8mb4';
$dsn = "mysql:host=$host;dbname=$dbname;charset=$charset";
$options = [
    PDO::ATTR_ERRMODE => PDO::ERRMODE_EXCEPTION,
    PDO::ATTR_DEFAULT_FETCH_MODE => PDO::FETCH_ASSOC,
];
$pdo = new PDO($dsn, $user, $password, $options);

function sendSSE(array $data) {
    echo "data: ".json_encode($data)."\n";
    @ob_flush();
    @flush();
}

while(true) {
    $SESSIONStmt = $pdo->query("SELECT * FROM SESSION ORDER BY time DESC LIMIT 1");
    $SESSIONRow = $SESSIONStmt->fetch() ?: [];

    $CARTELEMETRYStmt = $pdo->query("SELECT * FROM CARTELEMETRY ORDER BY time DESC LIMIT 1");
    $CARTELEMETRYRow = $CARTELEMETRYStmt->fetch() ?: [];

    sendSSE([
        'SESSION' => $SESSIONRow,
        'CARTELEMETRY' => $CARTELEMETRYRow,
    ]);

    usleep(17000);
}