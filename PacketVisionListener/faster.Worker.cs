using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using MySqlConnector;

namespace Telemtry
{
    public class Worker : BackgroundService
    {
        private ClientWebSocket _webSocket;
        private readonly Uri _webSocketUri = new("ws://telemtry-websocket:3000");
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var client = new UdpClient(20777);
            var connectionString = "Server=db;Port=3306;Database=telemetry;User=user;Password=pass;";

            _webSocket = new ClientWebSocket();
            try
            {
                await _webSocket.ConnectAsync(_webSocketUri, stoppingToken);
                Console.WriteLine("Connected to WebSocket server.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"WebSocket connection failed: {ex.Message}");
            }
            List<CarTelemetryData> carTelemetryDataBatch = new();
            int batchSize = 20;
            while (!stoppingToken.IsCancellationRequested)
            {
                UdpReceiveResult result = await client.ReceiveAsync();
                var packet = result.Buffer;

                using var ms = new MemoryStream(packet);
                using var br = new BinaryReader(ms);

                int expectedSize = 1352; // Header (24) + 22 cars * 60 bytes + 3 final bytes + 5 padding bytes

                if (packet.Length != expectedSize)
                {
                    Console.WriteLine("Warning: Packet too small! Skipping...");
                    continue;
                }

                // First, read the header
                PacketHeader header = new PacketHeader
                {
                    packetFormat = br.ReadUInt16(),
                    gameYear = br.ReadByte(),
                    gameMajorVersion = br.ReadByte(),
                    gameMinorVersion = br.ReadByte(),
                    packetVersion = br.ReadByte(),
                    packetId = br.ReadByte(),
                    sessionUID = br.ReadUInt64(),
                    sessionTime = br.ReadSingle(),
                    frameIdentifier = br.ReadUInt32(),
                    overallFrameIdentifier = br.ReadUInt32(),
                    playerCarIndex = br.ReadByte(),
                    secondaryPlayerCarIndex = br.ReadByte()
                };

                // Then read all cars’ telemetry data (22 total)
                CarTelemetryData[] carsData = new CarTelemetryData[22];
                for (int i = 0; i < 22; i++)
                {
                    carsData[i] = ReadCarTelemetryData(br);
                }

                // Finally, read remaining fields of PacketCarTelemetryData
                byte mfdPanelIndex = br.ReadByte();
                byte mfdPanelIndexSecondaryPlayer = br.ReadByte();
                sbyte suggestedGear = br.ReadSByte();

                // Now you can use the telemetry data for the **player’s car**:
                var playerIndex = header.playerCarIndex;
                var data = carsData[playerIndex];

                if (_webSocket.State == WebSocketState.Open)
                {
                    var json = JsonSerializer.Serialize(data);
                    var bytes = Encoding.UTF8.GetBytes(json);
                    var buffer = new ArraySegment<byte>(bytes);
                    await _webSocket.SendAsync(buffer, WebSocketMessageType.Text, true, stoppingToken);
                    // Send heartbeat
                    var pingBuffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes("__ping__"));
                    await _webSocket.SendAsync(pingBuffer, WebSocketMessageType.Text, true, stoppingToken);
                } else
                {
                    Console.WriteLine($"WebSocket not open: {_webSocket.State}");
                }
                /*
                Console.WriteLine(
                    $"Spd:{data.speed} | " +
                    $"Thr:{data.throttle:0.00} | " +
                    $"Str:{data.steer:0.00} | " +
                    $"Brk:{data.brake:0.00} | " +
                    $"Cl:{data.clutch} | " +
                    $"Gr:{data.gear} | " +
                    $"RPM:{data.engineRPM} | " +
                    $"DRS:{data.drs} | " +
                    $"RL%:{data.revLightsPercent} | " +
                    $"RLB:{data.revLightsBitValue}\n" +  // Wrap to new line here
                    $"BT:{string.Join(",", data.brakesTemperature)} | " +
                    $"TS:{string.Join(",", data.tyresSurfaceTemperature)} | " +
                    $"TI:{string.Join(",", data.tyresInnerTemperature)} | " +
                    $"ET:{data.engineTemperature}\n" +    // Wrap again
                    $"TP:{string.Join(",", data.tyresPressure.Select(p => p.ToString("0.00")))} | " +
                    $"ST:{string.Join(",", data.surfaceType)}"
                );
                */
                using var conn = new MySqlConnection(connectionString);
                await conn.OpenAsync();

                carTelemetryDataBatch.Add(data);

                if (carTelemetryDataBatch.Count >= batchSize)
                {
                    await InsertTelemetryBatchAsync(carTelemetryDataBatch, connectionString, stoppingToken);
                    carTelemetryDataBatch.Clear();
                }
            }
            if (carTelemetryDataBatch.Count > 0)
            {
                await InsertTelemetryBatchAsync(carTelemetryDataBatch, connectionString, stoppingToken);
                carTelemetryDataBatch.Clear();
            }
        }
        private async Task InsertTelemetryBatchAsync(List<CarTelemetryData> batch, string connectionString, CancellationToken token)
        {
            if (batch.Count == 0) return;

            try
            {
                using var conn = new MySqlConnection(connectionString);
                await conn.OpenAsync(token);

                var sb = new StringBuilder();
                sb.Append(@"
                    INSERT INTO telemetry (
                        speed, throttle, steer, brake, clutch, gear, engineRPM, drs, revLightsPercent, revLightsBitValue,
                        brakesTemperature0, brakesTemperature1, brakesTemperature2, brakesTemperature3,
                        tyresSurfaceTemperature0, tyresSurfaceTemperature1, tyresSurfaceTemperature2, tyresSurfaceTemperature3,
                        tyresInnerTemperature0, tyresInnerTemperature1, tyresInnerTemperature2, tyresInnerTemperature3,
                        engineTemperature,
                        tyresPressure0, tyresPressure1, tyresPressure2, tyresPressure3,
                        surfaceType0, surfaceType1, surfaceType2, surfaceType3
                    ) VALUES ");

                var parameters = new List<MySqlParameter>();
                for (int i = 0; i < batch.Count; i++)
                {
                    var d = batch[i];
                    var row = new StringBuilder("(");
                    row.Append($"@speed{i}, @throttle{i}, @steer{i}, @brake{i}, @clutch{i}, @gear{i}, @engineRPM{i}, @drs{i}, @revLightsPercent{i}, @revLightsBitValue{i},");
                    for (int j = 0; j < 4; j++) row.Append($"@brakesTemperature{i}_{j},");
                    for (int j = 0; j < 4; j++) row.Append($"@tyresSurfaceTemperature{i}_{j},");
                    for (int j = 0; j < 4; j++) row.Append($"@tyresInnerTemperature{i}_{j},");
                    row.Append($"@engineTemperature{i},");
                    for (int j = 0; j < 4; j++) row.Append($"@tyresPressure{i}_{j},");
                    for (int j = 0; j < 4; j++) row.Append($"@surfaceType{i}_{j},");
                    row.Length--; // remove last comma
                    row.Append("),");
                    sb.Append(row);

                    parameters.AddRange(new[] {
                new MySqlParameter($"@speed{i}", d.speed),
                new MySqlParameter($"@throttle{i}", d.throttle),
                new MySqlParameter($"@steer{i}", d.steer),
                new MySqlParameter($"@brake{i}", d.brake),
                new MySqlParameter($"@clutch{i}", d.clutch),
                new MySqlParameter($"@gear{i}", d.gear),
                new MySqlParameter($"@engineRPM{i}", d.engineRPM),
                new MySqlParameter($"@drs{i}", d.drs),
                new MySqlParameter($"@revLightsPercent{i}", d.revLightsPercent),
                new MySqlParameter($"@revLightsBitValue{i}", d.revLightsBitValue),
                new MySqlParameter($"@engineTemperature{i}", d.engineTemperature)
            });

                    for (int j = 0; j < 4; j++)
                    {
                        parameters.Add(new MySqlParameter($"@brakesTemperature{i}_{j}", d.brakesTemperature?[j] ?? 0));
                        parameters.Add(new MySqlParameter($"@tyresSurfaceTemperature{i}_{j}", d.tyresSurfaceTemperature?[j] ?? 0));
                        parameters.Add(new MySqlParameter($"@tyresInnerTemperature{i}_{j}", d.tyresInnerTemperature?[j] ?? 0));
                        parameters.Add(new MySqlParameter($"@tyresPressure{i}_{j}", d.tyresPressure?[j] ?? 0));
                        parameters.Add(new MySqlParameter($"@surfaceType{i}_{j}", d.surfaceType?[j] ?? 0));
                    }
                }

                sb.Length--; // Remove trailing comma
                using var cmd = new MySqlCommand(sb.ToString(), conn);
                cmd.Parameters.AddRange(parameters.ToArray());

                await cmd.ExecuteNonQueryAsync(token);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Batch insert error: {ex.Message}");
            }
        }
        private CarTelemetryData ReadCarTelemetryData(BinaryReader br)
        {
            CarTelemetryData data = new CarTelemetryData();

            try
            {
                data.speed = br.ReadUInt16();
                data.throttle = br.ReadSingle();
                data.steer = br.ReadSingle();
                data.brake = br.ReadSingle();
                data.clutch = br.ReadByte();
                data.gear = br.ReadSByte();
                data.engineRPM = br.ReadUInt16();
                data.drs = br.ReadByte();
                data.revLightsPercent = br.ReadByte();
                data.revLightsBitValue = br.ReadUInt16();
                data.brakesTemperature = new ushort[4];
                data.tyresSurfaceTemperature = new byte[4];
                data.tyresInnerTemperature = new byte[4];
                data.tyresPressure = new float[4];
                data.surfaceType = new byte[4];

                for (int i = 0; i < 4; i++) data.brakesTemperature[i] = br.ReadUInt16();
                for (int i = 0; i < 4; i++) data.tyresSurfaceTemperature[i] = br.ReadByte();
                for (int i = 0; i < 4; i++) data.tyresInnerTemperature[i] = br.ReadByte();
                data.engineTemperature = br.ReadUInt16();
                for (int i = 0; i < 4; i++) data.tyresPressure[i] = br.ReadSingle();
                for (int i = 0; i < 4; i++) data.surfaceType[i] = br.ReadByte();
            }
            catch (EndOfStreamException)
            {
                Console.WriteLine("Warning: Incomplete CarTelemetryData packet. Skipping...");          // Return a default / zeroed struct
            }

            return data;
        }

    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PacketHeader
    {
        public ushort packetFormat;
        public byte gameYear;
        public byte gameMajorVersion;
        public byte gameMinorVersion;
        public byte packetVersion;
        public byte packetId;
        public ulong sessionUID;
        public float sessionTime;
        public uint frameIdentifier;
        public uint overallFrameIdentifier;
        public byte playerCarIndex;
        public byte secondaryPlayerCarIndex;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct CarTelemetryData
    {
        public ushort speed;                    // Speed of car in kilometres per hour
        public float throttle;                  // Amount of throttle applied (0.0 to 1.0)
        public float steer;                     // Steering (-1.0 (full lock left) to 1.0 (full lock right))
        public float brake;                     // Amount of brake applied (0.0 to 1.0)
        public byte clutch;                     // Amount of clutch applied (0 to 100)
        public sbyte gear;                      // Gear selected (1-8, N=0, R=-1)
        public ushort engineRPM;                // Engine RPM
        public byte drs;                        // 0 = off, 1 = on
        public byte revLightsPercent;           // Rev lights indicator (percentage)
        public ushort revLightsBitValue;        // Rev lights (bit 0 = leftmost LED, bit 14 = rightmost LED)
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public ushort[] brakesTemperature;      // Brakes temperature (celsius)
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] tyresSurfaceTemperature;  // Tyres surface temperature (celsius)
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] tyresInnerTemperature;    // Tyres inner temperature (celsius)
        public ushort engineTemperature;        // Engine temperature (celsius)
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public float[] tyresPressure;           // Tyres pressure (PSI)
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] surfaceType;              // Driving surface, see appendices
    }
}