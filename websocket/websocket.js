const cluster = require('cluster');
const os = require('os');
const websocket = require('ws');
const mysql = require('mysql2/promise');

const numCPUs = os.cpus().length;
const tables = [
    'CARTELEMETRY',
    'SESSION'
];
const pollInterval = 17;
if (cluster.isMaster) {
    for (let i = 0; i < numCPUs; i++) {
        cluster.fork();
    }
} else {
    const wss = new websocket.Server({ port: 3000 });

    wss.on('connection', async (ws) => {
        const conn = await mysql.createConnection({
            host: 'pvdb',
            user: '',
            password: '',
            database: '',
        });

        let polling = true;

        ws.on('message', (msg) => {
            if (msg.toString() === '__stop__') polling = false;
            if (msg.toString() === '__start__') polling = true;
        });
        const poll = async () => {
            while (polling) {
                const result = {};

                await Promise.all(tables.map(async (table) => {
                    try {
                        const [rows] = await conn.query(`SELECT * FROM ${table} ORDER BY timestamp DESC LIMIT 1`);
                        result[table] = rows[0] || {};
                    } catch (err) {
                        result[table] = null;
                    }
                }));
                ws.send(JSON.stringify(result));
                await new Promise(resolve => setTimeout(resolve, pollInterval));
            }
        };
        poll().catch(console.error);
    });
    /*
    (async () => {
        const pool = mysql.createPool(DB_CONFIG);
        const wss = new websocket.Server({ port: 3000 });

        let lastId = 0;
        let lastHeartbeat = Date.now();
        let pollingActive = true;

        wss.on('connection', (ws) => {
            ws.on('message', (message) => {
                if (message.toString() === '__ping__') {
                    lastHeartbeat = Date.now();
                    if (!pollingActive) pollingActive = true;
                }
            });
        });
        function pausePollingIfStale() {
            const now = Date.now();
            if (now - lastHeartbeat > 1000) pollingActive = false;
        }
        setInterval(pausePollingIfStale, pollInterval);

        setInterval(async () => {
            if (!pollingActive) return;

            try {
                const [rowsCarTelemetryData] = await pool.query(
                    "SELECT * FROM CARTELEMETRY ORDER BY timestamp DESC LIMIT 1"
                );
                const latestCarTelemetryData = rowsCarTelemetryData[0];
                if (latestCarTelemetryData && latestCarTelemetryData.id > lastId) {
                    lastId = latestCarTelemetryData.id;
                    const message = JSON.stringify(latestCarTelemetryData);

                    wss.clients.forEach(client => {
                        if (client.readyState === websocket.OPEN) {
                            client.send(message);
                        }
                    });
                }

                const [rowsSessionData] = await pool.query(
                    "SELECT * FROM telemetryCarTelemetryData ORDER BY timestamp DESC LIMIT 1"
                );
                const latestSessionData = rowsSessionData[0];
                if (latestSessionData && latestSessionData.id > lastId) {
                    lastId = latestSessionData.id;
                    const message = JSON.stringify(latestSessionData);

                    wss.clients.forEach(client => {
                        if (client.readyState === websocket.OPEN) {
                            client.send(message);
                        }
                    });
                }
            } catch (err) {
                console.error(`Worker ${process.pid} polling error:`, err);
            }
        }, 1);
    })();
    */
}
