using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using MySqlConnector;


namespace Telemtry
{
    public class Worker : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var client = new UdpClient(20777);
            var connectionString = "Server=db;Port=3306;Database=telemetry;User=user;Password=pass;";

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
                using var conn = new MySqlConnection(connectionString);
                await conn.OpenAsync();

                using var cmd = new MySqlCommand(@"
                    INSERT INTO telemetry (
                        speed, throttle, steer, brake, clutch, gear, engineRPM, drs, revLightsPercent, revLightsBitValue,
                        brakesTemperature0, brakesTemperature1, brakesTemperature2, brakesTemperature3,
                        tyresSurfaceTemperature0, tyresSurfaceTemperature1, tyresSurfaceTemperature2, tyresSurfaceTemperature3,
                        tyresInnerTemperature0, tyresInnerTemperature1, tyresInnerTemperature2, tyresInnerTemperature3,
                        engineTemperature,
                        tyresPressure0, tyresPressure1, tyresPressure2, tyresPressure3,
                        surfaceType0, surfaceType1, surfaceType2, surfaceType3
                    ) VALUES (
                        @speed, @throttle, @steer, @brake, @clutch, @gear, @engineRPM, @drs, @revLightsPercent, @revLightsBitValue,
                        @brakesTemperature0, @brakesTemperature1, @brakesTemperature2, @brakesTemperature3,
                        @tyresSurfaceTemperature0, @tyresSurfaceTemperature1, @tyresSurfaceTemperature2, @tyresSurfaceTemperature3,
                        @tyresInnerTemperature0, @tyresInnerTemperature1, @tyresInnerTemperature2, @tyresInnerTemperature3,
                        @engineTemperature,
                        @tyresPressure0, @tyresPressure1, @tyresPressure2, @tyresPressure3,
                        @surfaceType0, @surfaceType1, @surfaceType2, @surfaceType3
                    )",
                    conn);
                cmd.Parameters.AddWithValue("@speed", data.speed);
                cmd.Parameters.AddWithValue("@throttle", data.throttle);
                cmd.Parameters.AddWithValue("@steer", data.steer);
                cmd.Parameters.AddWithValue("@brake", data.brake);
                cmd.Parameters.AddWithValue("@clutch", data.clutch);
                cmd.Parameters.AddWithValue("@gear", data.gear);
                cmd.Parameters.AddWithValue("@engineRPM", data.engineRPM);
                cmd.Parameters.AddWithValue("@drs", data.drs);
                cmd.Parameters.AddWithValue("@revLightsPercent", data.revLightsPercent);
                cmd.Parameters.AddWithValue("@revLightsBitValue", data.revLightsBitValue);

                // Arrays
                cmd.Parameters.AddWithValue("@brakesTemperature0", data.brakesTemperature?[0] ?? 0);
                cmd.Parameters.AddWithValue("@brakesTemperature1", data.brakesTemperature?[1] ?? 0);
                cmd.Parameters.AddWithValue("@brakesTemperature2", data.brakesTemperature?[2] ?? 0);
                cmd.Parameters.AddWithValue("@brakesTemperature3", data.brakesTemperature?[3] ?? 0);

                cmd.Parameters.AddWithValue("@tyresSurfaceTemperature0", data.tyresSurfaceTemperature?[0] ?? 0);
                cmd.Parameters.AddWithValue("@tyresSurfaceTemperature1", data.tyresSurfaceTemperature?[1] ?? 0);
                cmd.Parameters.AddWithValue("@tyresSurfaceTemperature2", data.tyresSurfaceTemperature?[2] ?? 0);
                cmd.Parameters.AddWithValue("@tyresSurfaceTemperature3", data.tyresSurfaceTemperature?[3] ?? 0);

                cmd.Parameters.AddWithValue("@tyresInnerTemperature0", data.tyresInnerTemperature?[0] ?? 0);
                cmd.Parameters.AddWithValue("@tyresInnerTemperature1", data.tyresInnerTemperature?[1] ?? 0);
                cmd.Parameters.AddWithValue("@tyresInnerTemperature2", data.tyresInnerTemperature?[2] ?? 0);
                cmd.Parameters.AddWithValue("@tyresInnerTemperature3", data.tyresInnerTemperature?[3] ?? 0);

                cmd.Parameters.AddWithValue("@engineTemperature", data.engineTemperature);

                cmd.Parameters.AddWithValue("@tyresPressure0", data.tyresPressure?[0] ?? 0);
                cmd.Parameters.AddWithValue("@tyresPressure1", data.tyresPressure?[1] ?? 0);
                cmd.Parameters.AddWithValue("@tyresPressure2", data.tyresPressure?[2] ?? 0);
                cmd.Parameters.AddWithValue("@tyresPressure3", data.tyresPressure?[3] ?? 0);

                cmd.Parameters.AddWithValue("@surfaceType0", data.surfaceType?[0] ?? 0);
                cmd.Parameters.AddWithValue("@surfaceType1", data.surfaceType?[1] ?? 0);
                cmd.Parameters.AddWithValue("@surfaceType2", data.surfaceType?[2] ?? 0);
                cmd.Parameters.AddWithValue("@surfaceType3", data.surfaceType?[3] ?? 0);

                try
                {
                    await cmd.ExecuteNonQueryAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"MySQL Insert Error: {ex.Message}");
                }
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
"