<?php
ini_set('output_buffering', 'off');
ini_set('zlib.output_compression', '0');
header('X-Accel-Buffering: no');

require __DIR__ . '/vendor/autoload.php';
$dotenv = Dotenv\Dotenv::createImmutable(__DIR__);
$dotenv->load();

header('Content-Type: text/event-stream');
header('Cache-Control: no-cache');
header('Connection: keep-alive');

while(ob_get_level() > 0) ( ob_end_flush() );
ob_implicit_flush();

ignore_user_abort(true);

$host = $_ENV['DB_HOST'];
$dbname = $_ENV['DB_NAME'];
$charset = 'utf8mb4';
$user = $_ENV['DB_USER'];
$pass = $_ENV['DB_PASS'];
$options = [
    PDO::ATTR_ERRMODE => PDO::ERRMODE_EXCEPTION,
    PDO::ATTR_DEFAULT_FETCH_MODE => PDO::FETCH_ASSOC,
];
$pdo = new PDO("mysql:host=$host;dbname=$dbname;charset=$charset", $user, $pass, $options);

while(true) {
    try {
        $SESSIONRow = $pdo->query("SELECT * FROM SESSION ORDER BY timestamp DESC LIMIT 1")
            ->fetch(PDO::FETCH_ASSOC);
        $CARTELEMETRYRow = $pdo->query("SELECT * FROM CARTELEMETRY ORDER BY timestamp DESC LIMIT 1")
            ->fetch(PDO::FETCH_ASSOC);

        echo "data: " . json_encode([
                'SESSION' => $SESSIONRow,
                'CARTELEMETRY' => $CARTELEMETRYRow,
            ]) . "\n\n";

        @ob_flush();
        @flush();

        usleep(1);
    } catch (PDOException $e) {
        die("Connection failed: " . $e->getMessage());
    }
}