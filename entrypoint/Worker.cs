using System.Net.Sockets;
using System.Runtime.InteropServices;
using MySqlConnector;

namespace PacketVisionListener
{
    public class Worker: BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var udpPort = Environment.GetEnvironmentVariable("L_PORT")!;
            var mySqlPort = Environment.GetEnvironmentVariable("DB_PORT");
            var mySqlUser = Environment.GetEnvironmentVariable("MYSQL_USER");
            var mySqlPass = Environment.GetEnvironmentVariable("MYSQL_PASS");
            
            using var client = new UdpClient(int.Parse(udpPort));
            var connectionString = $"Server=db;Port={mySqlPort};Database=PACKETVISION;User={mySqlUser};Password={mySqlPass};";

            while (!stoppingToken.IsCancellationRequested)
            {
                var result = await client.ReceiveAsync(stoppingToken);
                var packet = result.Buffer;

                using var ms = new MemoryStream(packet);
                using var br = new BinaryReader(ms);
                
                await using var conn = new MySqlConnection(connectionString);
                await conn.OpenAsync(stoppingToken);
//-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------Inserters
                switch (packet.Length)
                {
                    case 1349:
                        //-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------start of CarMotion of 1349B
                        var headerCarMotion = new Header
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
                        var carMotion = new CarMotion[22];
                        for (var i = 0; i < 22; i++) carMotion[i] = ReadCarMotion(br);
                        //-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------end
                        var dataCarMotion = carMotion[headerCarMotion.playerCarIndex];

                        await using (var cmd = new MySqlCommand(@"
                            INSERT INTO CARMOTION (
                                worldPositionX, worldPositionY, worldPositionZ,
                                worldVelocityX, worldVelocityY, worldVelocityZ,
                                worldForwardDirX, worldForwardDirY, worldForwardDirZ,
                                worldRightDirX, worldRightDirY, worldRightDirZ,
                                gForceLateral, gForceLongitudinal, gForceVertical,
                                yaw, pitch, roll
                            ) VALUES (
                                @worldPositionX, @worldPositionY, @worldPositionZ,
                                @worldVelocityX, @worldVelocityY, @worldVelocityZ,
                                @worldForwardDirX, @worldForwardDirY, @worldForwardDirZ,
                                @worldRightDirX, @worldRightDirY, @worldRightDirZ,
                                @gForceLateral, @gForceLongitudinal, @gForceVertical,
                                @yaw, @pitch, @roll
                            );", conn
                        ))
                        {
                            cmd.Parameters.AddWithValue("@worldPositionX", dataCarMotion.worldPositionX);
                            cmd.Parameters.AddWithValue("@worldPositionY", dataCarMotion.worldPositionY);
                            cmd.Parameters.AddWithValue("@worldPositionZ", dataCarMotion.worldPositionZ);
                            cmd.Parameters.AddWithValue("@worldVelocityX", dataCarMotion.worldVelocityX);
                            cmd.Parameters.AddWithValue("@worldVelocityY", dataCarMotion.worldVelocityY);
                            cmd.Parameters.AddWithValue("@worldVelocityZ", dataCarMotion.worldVelocityZ);
                            cmd.Parameters.AddWithValue("@worldForwardDirX", dataCarMotion.worldForwardDirX);
                            cmd.Parameters.AddWithValue("@worldForwardDirY", dataCarMotion.worldForwardDirY);
                            cmd.Parameters.AddWithValue("@worldForwardDirZ", dataCarMotion.worldForwardDirZ);
                            cmd.Parameters.AddWithValue("@worldRightDirX", dataCarMotion.worldRightDirX);
                            cmd.Parameters.AddWithValue("@worldRightDirY", dataCarMotion.worldRightDirY);
                            cmd.Parameters.AddWithValue("@worldRightDirZ", dataCarMotion.worldRightDirZ);
                            cmd.Parameters.AddWithValue("@gForceLateral", dataCarMotion.gForceLateral);
                            cmd.Parameters.AddWithValue("@gForceLongitudinal", dataCarMotion.gForceLongitudinal);
                            cmd.Parameters.AddWithValue("@gForceVertical", dataCarMotion.gForceVertical);
                            cmd.Parameters.AddWithValue("@yaw", dataCarMotion.yaw);
                            cmd.Parameters.AddWithValue("@pitch", dataCarMotion.pitch);
                            cmd.Parameters.AddWithValue("@roll", dataCarMotion.roll);
                            try
                            {
                                await cmd.ExecuteNonQueryAsync(stoppingToken);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"CARMOTION INSERT error : {ex.Message}");
                            }
                        }
                        break;
//--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
                    case 753:
                        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------start of Session of 753B
                        br.ReadUInt16(); //packetFormat
                        br.ReadByte(); //gameYear
                        br.ReadByte(); //gameMajorVersion
                        br.ReadByte(); //gameMinorVersion
                        br.ReadByte(); //packetVersion
                        br.ReadByte(); //packetId
                        br.ReadUInt64(); //sessionUID
                        br.ReadSingle(); //sessionTime
                        br.ReadUInt32(); //frameIdentifier
                        br.ReadUInt32(); //overallFrameIdentifier
                        br.ReadByte(); //playerCarIndex
                        br.ReadByte(); //secondaryPlayerCarIndex
                        var dataSession = ReadSession(br);
                        //-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------end
                        
                        await using (var cmd = new MySqlCommand(@"
                            INSERT INTO SESSION (
                                weather, trackTemperature, airTemperature, totalLaps, trackLength, sessionType, trackId, formula, sessionTimeLeft, sessionDuration, pitSpeedLimit, gamePaused, isSpectating, spectatorCarIndex, sliProNativeSupport, numMarshalZones,
                                marshalZones0_zoneStart, marshalZones0_zoneFlag,
                                marshalZones1_zoneStart, marshalZones1_zoneFlag,
                                marshalZones2_zoneStart, marshalZones2_zoneFlag,
                                marshalZones3_zoneStart, marshalZones3_zoneFlag,
                                marshalZones4_zoneStart, marshalZones4_zoneFlag,
                                marshalZones5_zoneStart, marshalZones5_zoneFlag,
                                marshalZones6_zoneStart, marshalZones6_zoneFlag,
                                marshalZones7_zoneStart, marshalZones7_zoneFlag,
                                marshalZones8_zoneStart, marshalZones8_zoneFlag,
                                marshalZones9_zoneStart, marshalZones9_zoneFlag,
                                marshalZones10_zoneStart, marshalZones10_zoneFlag,
                                marshalZones11_zoneStart, marshalZones11_zoneFlag,
                                marshalZones12_zoneStart, marshalZones12_zoneFlag,
                                marshalZones13_zoneStart, marshalZones13_zoneFlag,
                                marshalZones14_zoneStart, marshalZones14_zoneFlag,
                                marshalZones15_zoneStart, marshalZones15_zoneFlag,
                                marshalZones16_zoneStart, marshalZones16_zoneFlag,
                                marshalZones17_zoneStart, marshalZones17_zoneFlag,
                                marshalZones18_zoneStart, marshalZones18_zoneFlag,
                                marshalZones19_zoneStart, marshalZones19_zoneFlag,
                                marshalZones20_zoneStart, marshalZones20_zoneFlag,
                                safetyCarStatus, networkGame, numWeatherForecastSamples,
                                weatherForecastSamples0_sessionType, weatherForecastSamples0_timeOffset, weatherForecastSamples0_weather, weatherForecastSamples0_trackTemperature, weatherForecastSamples0_trackTemperatureChange, weatherForecastSamples0_airTemperature, weatherForecastSamples0_airTemperatureChange, weatherForecastSamples0_rainPercentage,
                                weatherForecastSamples1_sessionType, weatherForecastSamples1_timeOffset, weatherForecastSamples1_weather, weatherForecastSamples1_trackTemperature, weatherForecastSamples1_trackTemperatureChange, weatherForecastSamples1_airTemperature, weatherForecastSamples1_airTemperatureChange, weatherForecastSamples1_rainPercentage,
                                weatherForecastSamples2_sessionType, weatherForecastSamples2_timeOffset, weatherForecastSamples2_weather, weatherForecastSamples2_trackTemperature, weatherForecastSamples2_trackTemperatureChange, weatherForecastSamples2_airTemperature, weatherForecastSamples2_airTemperatureChange, weatherForecastSamples2_rainPercentage,
                                weatherForecastSamples3_sessionType, weatherForecastSamples3_timeOffset, weatherForecastSamples3_weather, weatherForecastSamples3_trackTemperature, weatherForecastSamples3_trackTemperatureChange, weatherForecastSamples3_airTemperature, weatherForecastSamples3_airTemperatureChange, weatherForecastSamples3_rainPercentage,
                                weatherForecastSamples4_sessionType, weatherForecastSamples4_timeOffset, weatherForecastSamples4_weather, weatherForecastSamples4_trackTemperature, weatherForecastSamples4_trackTemperatureChange, weatherForecastSamples4_airTemperature, weatherForecastSamples4_airTemperatureChange, weatherForecastSamples4_rainPercentage,
                                weatherForecastSamples5_sessionType, weatherForecastSamples5_timeOffset, weatherForecastSamples5_weather, weatherForecastSamples5_trackTemperature, weatherForecastSamples5_trackTemperatureChange, weatherForecastSamples5_airTemperature, weatherForecastSamples5_airTemperatureChange, weatherForecastSamples5_rainPercentage,
                                weatherForecastSamples6_sessionType, weatherForecastSamples6_timeOffset, weatherForecastSamples6_weather, weatherForecastSamples6_trackTemperature, weatherForecastSamples6_trackTemperatureChange, weatherForecastSamples6_airTemperature, weatherForecastSamples6_airTemperatureChange, weatherForecastSamples6_rainPercentage,
                                weatherForecastSamples7_sessionType, weatherForecastSamples7_timeOffset, weatherForecastSamples7_weather, weatherForecastSamples7_trackTemperature, weatherForecastSamples7_trackTemperatureChange, weatherForecastSamples7_airTemperature, weatherForecastSamples7_airTemperatureChange, weatherForecastSamples7_rainPercentage,
                                weatherForecastSamples8_sessionType, weatherForecastSamples8_timeOffset, weatherForecastSamples8_weather, weatherForecastSamples8_trackTemperature, weatherForecastSamples8_trackTemperatureChange, weatherForecastSamples8_airTemperature, weatherForecastSamples8_airTemperatureChange, weatherForecastSamples8_rainPercentage,
                                weatherForecastSamples9_sessionType, weatherForecastSamples9_timeOffset, weatherForecastSamples9_weather, weatherForecastSamples9_trackTemperature, weatherForecastSamples9_trackTemperatureChange, weatherForecastSamples9_airTemperature, weatherForecastSamples9_airTemperatureChange, weatherForecastSamples9_rainPercentage,
                                weatherForecastSamples10_sessionType, weatherForecastSamples10_timeOffset, weatherForecastSamples10_weather, weatherForecastSamples10_trackTemperature, weatherForecastSamples10_trackTemperatureChange, weatherForecastSamples10_airTemperature, weatherForecastSamples10_airTemperatureChange, weatherForecastSamples10_rainPercentage,
                                weatherForecastSamples11_sessionType, weatherForecastSamples11_timeOffset, weatherForecastSamples11_weather, weatherForecastSamples11_trackTemperature, weatherForecastSamples11_trackTemperatureChange, weatherForecastSamples11_airTemperature, weatherForecastSamples11_airTemperatureChange, weatherForecastSamples11_rainPercentage,
                                weatherForecastSamples12_sessionType, weatherForecastSamples12_timeOffset, weatherForecastSamples12_weather, weatherForecastSamples12_trackTemperature, weatherForecastSamples12_trackTemperatureChange, weatherForecastSamples12_airTemperature, weatherForecastSamples12_airTemperatureChange, weatherForecastSamples12_rainPercentage,
                                weatherForecastSamples13_sessionType, weatherForecastSamples13_timeOffset, weatherForecastSamples13_weather, weatherForecastSamples13_trackTemperature, weatherForecastSamples13_trackTemperatureChange, weatherForecastSamples13_airTemperature, weatherForecastSamples13_airTemperatureChange, weatherForecastSamples13_rainPercentage,
                                weatherForecastSamples14_sessionType, weatherForecastSamples14_timeOffset, weatherForecastSamples14_weather, weatherForecastSamples14_trackTemperature, weatherForecastSamples14_trackTemperatureChange, weatherForecastSamples14_airTemperature, weatherForecastSamples14_airTemperatureChange, weatherForecastSamples14_rainPercentage,
                                weatherForecastSamples15_sessionType, weatherForecastSamples15_timeOffset, weatherForecastSamples15_weather, weatherForecastSamples15_trackTemperature, weatherForecastSamples15_trackTemperatureChange, weatherForecastSamples15_airTemperature, weatherForecastSamples15_airTemperatureChange, weatherForecastSamples15_rainPercentage,
                                weatherForecastSamples16_sessionType, weatherForecastSamples16_timeOffset, weatherForecastSamples16_weather, weatherForecastSamples16_trackTemperature, weatherForecastSamples16_trackTemperatureChange, weatherForecastSamples16_airTemperature, weatherForecastSamples16_airTemperatureChange, weatherForecastSamples16_rainPercentage,
                                weatherForecastSamples17_sessionType, weatherForecastSamples17_timeOffset, weatherForecastSamples17_weather, weatherForecastSamples17_trackTemperature, weatherForecastSamples17_trackTemperatureChange, weatherForecastSamples17_airTemperature, weatherForecastSamples17_airTemperatureChange, weatherForecastSamples17_rainPercentage,
                                weatherForecastSamples18_sessionType, weatherForecastSamples18_timeOffset, weatherForecastSamples18_weather, weatherForecastSamples18_trackTemperature, weatherForecastSamples18_trackTemperatureChange, weatherForecastSamples18_airTemperature, weatherForecastSamples18_airTemperatureChange, weatherForecastSamples18_rainPercentage,
                                weatherForecastSamples19_sessionType, weatherForecastSamples19_timeOffset, weatherForecastSamples19_weather, weatherForecastSamples19_trackTemperature, weatherForecastSamples19_trackTemperatureChange, weatherForecastSamples19_airTemperature, weatherForecastSamples19_airTemperatureChange, weatherForecastSamples19_rainPercentage,
                                weatherForecastSamples20_sessionType, weatherForecastSamples20_timeOffset, weatherForecastSamples20_weather, weatherForecastSamples20_trackTemperature, weatherForecastSamples20_trackTemperatureChange, weatherForecastSamples20_airTemperature, weatherForecastSamples20_airTemperatureChange, weatherForecastSamples20_rainPercentage,
                                weatherForecastSamples21_sessionType, weatherForecastSamples21_timeOffset, weatherForecastSamples21_weather, weatherForecastSamples21_trackTemperature, weatherForecastSamples21_trackTemperatureChange, weatherForecastSamples21_airTemperature, weatherForecastSamples21_airTemperatureChange, weatherForecastSamples21_rainPercentage,
                                weatherForecastSamples22_sessionType, weatherForecastSamples22_timeOffset, weatherForecastSamples22_weather, weatherForecastSamples22_trackTemperature, weatherForecastSamples22_trackTemperatureChange, weatherForecastSamples22_airTemperature, weatherForecastSamples22_airTemperatureChange, weatherForecastSamples22_rainPercentage,
                                weatherForecastSamples23_sessionType, weatherForecastSamples23_timeOffset, weatherForecastSamples23_weather, weatherForecastSamples23_trackTemperature, weatherForecastSamples23_trackTemperatureChange, weatherForecastSamples23_airTemperature, weatherForecastSamples23_airTemperatureChange, weatherForecastSamples23_rainPercentage,
                                weatherForecastSamples24_sessionType, weatherForecastSamples24_timeOffset, weatherForecastSamples24_weather, weatherForecastSamples24_trackTemperature, weatherForecastSamples24_trackTemperatureChange, weatherForecastSamples24_airTemperature, weatherForecastSamples24_airTemperatureChange, weatherForecastSamples24_rainPercentage,
                                weatherForecastSamples25_sessionType, weatherForecastSamples25_timeOffset, weatherForecastSamples25_weather, weatherForecastSamples25_trackTemperature, weatherForecastSamples25_trackTemperatureChange, weatherForecastSamples25_airTemperature, weatherForecastSamples25_airTemperatureChange, weatherForecastSamples25_rainPercentage,
                                weatherForecastSamples26_sessionType, weatherForecastSamples26_timeOffset, weatherForecastSamples26_weather, weatherForecastSamples26_trackTemperature, weatherForecastSamples26_trackTemperatureChange, weatherForecastSamples26_airTemperature, weatherForecastSamples26_airTemperatureChange, weatherForecastSamples26_rainPercentage,
                                weatherForecastSamples27_sessionType, weatherForecastSamples27_timeOffset, weatherForecastSamples27_weather, weatherForecastSamples27_trackTemperature, weatherForecastSamples27_trackTemperatureChange, weatherForecastSamples27_airTemperature, weatherForecastSamples27_airTemperatureChange, weatherForecastSamples27_rainPercentage,
                                weatherForecastSamples28_sessionType, weatherForecastSamples28_timeOffset, weatherForecastSamples28_weather, weatherForecastSamples28_trackTemperature, weatherForecastSamples28_trackTemperatureChange, weatherForecastSamples28_airTemperature, weatherForecastSamples28_airTemperatureChange, weatherForecastSamples28_rainPercentage,
                                weatherForecastSamples29_sessionType, weatherForecastSamples29_timeOffset, weatherForecastSamples29_weather, weatherForecastSamples29_trackTemperature, weatherForecastSamples29_trackTemperatureChange, weatherForecastSamples29_airTemperature, weatherForecastSamples29_airTemperatureChange, weatherForecastSamples29_rainPercentage,
                                weatherForecastSamples30_sessionType, weatherForecastSamples30_timeOffset, weatherForecastSamples30_weather, weatherForecastSamples30_trackTemperature, weatherForecastSamples30_trackTemperatureChange, weatherForecastSamples30_airTemperature, weatherForecastSamples30_airTemperatureChange, weatherForecastSamples30_rainPercentage,
                                weatherForecastSamples31_sessionType, weatherForecastSamples31_timeOffset, weatherForecastSamples31_weather, weatherForecastSamples31_trackTemperature, weatherForecastSamples31_trackTemperatureChange, weatherForecastSamples31_airTemperature, weatherForecastSamples31_airTemperatureChange, weatherForecastSamples31_rainPercentage,
                                weatherForecastSamples32_sessionType, weatherForecastSamples32_timeOffset, weatherForecastSamples32_weather, weatherForecastSamples32_trackTemperature, weatherForecastSamples32_trackTemperatureChange, weatherForecastSamples32_airTemperature, weatherForecastSamples32_airTemperatureChange, weatherForecastSamples32_rainPercentage,
                                weatherForecastSamples33_sessionType, weatherForecastSamples33_timeOffset, weatherForecastSamples33_weather, weatherForecastSamples33_trackTemperature, weatherForecastSamples33_trackTemperatureChange, weatherForecastSamples33_airTemperature, weatherForecastSamples33_airTemperatureChange, weatherForecastSamples33_rainPercentage,
                                weatherForecastSamples34_sessionType, weatherForecastSamples34_timeOffset, weatherForecastSamples34_weather, weatherForecastSamples34_trackTemperature, weatherForecastSamples34_trackTemperatureChange, weatherForecastSamples34_airTemperature, weatherForecastSamples34_airTemperatureChange, weatherForecastSamples34_rainPercentage,
                                weatherForecastSamples35_sessionType, weatherForecastSamples35_timeOffset, weatherForecastSamples35_weather, weatherForecastSamples35_trackTemperature, weatherForecastSamples35_trackTemperatureChange, weatherForecastSamples35_airTemperature, weatherForecastSamples35_airTemperatureChange, weatherForecastSamples35_rainPercentage,
                                weatherForecastSamples36_sessionType, weatherForecastSamples36_timeOffset, weatherForecastSamples36_weather, weatherForecastSamples36_trackTemperature, weatherForecastSamples36_trackTemperatureChange, weatherForecastSamples36_airTemperature, weatherForecastSamples36_airTemperatureChange, weatherForecastSamples36_rainPercentage,
                                weatherForecastSamples37_sessionType, weatherForecastSamples37_timeOffset, weatherForecastSamples37_weather, weatherForecastSamples37_trackTemperature, weatherForecastSamples37_trackTemperatureChange, weatherForecastSamples37_airTemperature, weatherForecastSamples37_airTemperatureChange, weatherForecastSamples37_rainPercentage,
                                weatherForecastSamples38_sessionType, weatherForecastSamples38_timeOffset, weatherForecastSamples38_weather, weatherForecastSamples38_trackTemperature, weatherForecastSamples38_trackTemperatureChange, weatherForecastSamples38_airTemperature, weatherForecastSamples38_airTemperatureChange, weatherForecastSamples38_rainPercentage,
                                weatherForecastSamples39_sessionType, weatherForecastSamples39_timeOffset, weatherForecastSamples39_weather, weatherForecastSamples39_trackTemperature, weatherForecastSamples39_trackTemperatureChange, weatherForecastSamples39_airTemperature, weatherForecastSamples39_airTemperatureChange, weatherForecastSamples39_rainPercentage,
                                weatherForecastSamples40_sessionType, weatherForecastSamples40_timeOffset, weatherForecastSamples40_weather, weatherForecastSamples40_trackTemperature, weatherForecastSamples40_trackTemperatureChange, weatherForecastSamples40_airTemperature, weatherForecastSamples40_airTemperatureChange, weatherForecastSamples40_rainPercentage,
                                weatherForecastSamples41_sessionType, weatherForecastSamples41_timeOffset, weatherForecastSamples41_weather, weatherForecastSamples41_trackTemperature, weatherForecastSamples41_trackTemperatureChange, weatherForecastSamples41_airTemperature, weatherForecastSamples41_airTemperatureChange, weatherForecastSamples41_rainPercentage,
                                weatherForecastSamples42_sessionType, weatherForecastSamples42_timeOffset, weatherForecastSamples42_weather, weatherForecastSamples42_trackTemperature, weatherForecastSamples42_trackTemperatureChange, weatherForecastSamples42_airTemperature, weatherForecastSamples42_airTemperatureChange, weatherForecastSamples42_rainPercentage,
                                weatherForecastSamples43_sessionType, weatherForecastSamples43_timeOffset, weatherForecastSamples43_weather, weatherForecastSamples43_trackTemperature, weatherForecastSamples43_trackTemperatureChange, weatherForecastSamples43_airTemperature, weatherForecastSamples43_airTemperatureChange, weatherForecastSamples43_rainPercentage,
                                weatherForecastSamples44_sessionType, weatherForecastSamples44_timeOffset, weatherForecastSamples44_weather, weatherForecastSamples44_trackTemperature, weatherForecastSamples44_trackTemperatureChange, weatherForecastSamples44_airTemperature, weatherForecastSamples44_airTemperatureChange, weatherForecastSamples44_rainPercentage,
                                weatherForecastSamples45_sessionType, weatherForecastSamples45_timeOffset, weatherForecastSamples45_weather, weatherForecastSamples45_trackTemperature, weatherForecastSamples45_trackTemperatureChange, weatherForecastSamples45_airTemperature, weatherForecastSamples45_airTemperatureChange, weatherForecastSamples45_rainPercentage,
                                weatherForecastSamples46_sessionType, weatherForecastSamples46_timeOffset, weatherForecastSamples46_weather, weatherForecastSamples46_trackTemperature, weatherForecastSamples46_trackTemperatureChange, weatherForecastSamples46_airTemperature, weatherForecastSamples46_airTemperatureChange, weatherForecastSamples46_rainPercentage,
                                weatherForecastSamples47_sessionType, weatherForecastSamples47_timeOffset, weatherForecastSamples47_weather, weatherForecastSamples47_trackTemperature, weatherForecastSamples47_trackTemperatureChange, weatherForecastSamples47_airTemperature, weatherForecastSamples47_airTemperatureChange, weatherForecastSamples47_rainPercentage,
                                weatherForecastSamples48_sessionType, weatherForecastSamples48_timeOffset, weatherForecastSamples48_weather, weatherForecastSamples48_trackTemperature, weatherForecastSamples48_trackTemperatureChange, weatherForecastSamples48_airTemperature, weatherForecastSamples48_airTemperatureChange, weatherForecastSamples48_rainPercentage,
                                weatherForecastSamples49_sessionType, weatherForecastSamples49_timeOffset, weatherForecastSamples49_weather, weatherForecastSamples49_trackTemperature, weatherForecastSamples49_trackTemperatureChange, weatherForecastSamples49_airTemperature, weatherForecastSamples49_airTemperatureChange, weatherForecastSamples49_rainPercentage,
                                weatherForecastSamples50_sessionType, weatherForecastSamples50_timeOffset, weatherForecastSamples50_weather, weatherForecastSamples50_trackTemperature, weatherForecastSamples50_trackTemperatureChange, weatherForecastSamples50_airTemperature, weatherForecastSamples50_airTemperatureChange, weatherForecastSamples50_rainPercentage,
                                weatherForecastSamples51_sessionType, weatherForecastSamples51_timeOffset, weatherForecastSamples51_weather, weatherForecastSamples51_trackTemperature, weatherForecastSamples51_trackTemperatureChange, weatherForecastSamples51_airTemperature, weatherForecastSamples51_airTemperatureChange, weatherForecastSamples51_rainPercentage,
                                weatherForecastSamples52_sessionType, weatherForecastSamples52_timeOffset, weatherForecastSamples52_weather, weatherForecastSamples52_trackTemperature, weatherForecastSamples52_trackTemperatureChange, weatherForecastSamples52_airTemperature, weatherForecastSamples52_airTemperatureChange, weatherForecastSamples52_rainPercentage,
                                weatherForecastSamples53_sessionType, weatherForecastSamples53_timeOffset, weatherForecastSamples53_weather, weatherForecastSamples53_trackTemperature, weatherForecastSamples53_trackTemperatureChange, weatherForecastSamples53_airTemperature, weatherForecastSamples53_airTemperatureChange, weatherForecastSamples53_rainPercentage,
                                weatherForecastSamples54_sessionType, weatherForecastSamples54_timeOffset, weatherForecastSamples54_weather, weatherForecastSamples54_trackTemperature, weatherForecastSamples54_trackTemperatureChange, weatherForecastSamples54_airTemperature, weatherForecastSamples54_airTemperatureChange, weatherForecastSamples54_rainPercentage,
                                weatherForecastSamples55_sessionType, weatherForecastSamples55_timeOffset, weatherForecastSamples55_weather, weatherForecastSamples55_trackTemperature, weatherForecastSamples55_trackTemperatureChange, weatherForecastSamples55_airTemperature, weatherForecastSamples55_airTemperatureChange, weatherForecastSamples55_rainPercentage,
                                weatherForecastSamples56_sessionType, weatherForecastSamples56_timeOffset, weatherForecastSamples56_weather, weatherForecastSamples56_trackTemperature, weatherForecastSamples56_trackTemperatureChange, weatherForecastSamples56_airTemperature, weatherForecastSamples56_airTemperatureChange, weatherForecastSamples56_rainPercentage,
                                weatherForecastSamples57_sessionType, weatherForecastSamples57_timeOffset, weatherForecastSamples57_weather, weatherForecastSamples57_trackTemperature, weatherForecastSamples57_trackTemperatureChange, weatherForecastSamples57_airTemperature, weatherForecastSamples57_airTemperatureChange, weatherForecastSamples57_rainPercentage,
                                weatherForecastSamples58_sessionType, weatherForecastSamples58_timeOffset, weatherForecastSamples58_weather, weatherForecastSamples58_trackTemperature, weatherForecastSamples58_trackTemperatureChange, weatherForecastSamples58_airTemperature, weatherForecastSamples58_airTemperatureChange, weatherForecastSamples58_rainPercentage,
                                weatherForecastSamples59_sessionType, weatherForecastSamples59_timeOffset, weatherForecastSamples59_weather, weatherForecastSamples59_trackTemperature, weatherForecastSamples59_trackTemperatureChange, weatherForecastSamples59_airTemperature, weatherForecastSamples59_airTemperatureChange, weatherForecastSamples59_rainPercentage,
                                weatherForecastSamples60_sessionType, weatherForecastSamples60_timeOffset, weatherForecastSamples60_weather, weatherForecastSamples60_trackTemperature, weatherForecastSamples60_trackTemperatureChange, weatherForecastSamples60_airTemperature, weatherForecastSamples60_airTemperatureChange, weatherForecastSamples60_rainPercentage,
                                weatherForecastSamples61_sessionType, weatherForecastSamples61_timeOffset, weatherForecastSamples61_weather, weatherForecastSamples61_trackTemperature, weatherForecastSamples61_trackTemperatureChange, weatherForecastSamples61_airTemperature, weatherForecastSamples61_airTemperatureChange, weatherForecastSamples61_rainPercentage,
                                weatherForecastSamples62_sessionType, weatherForecastSamples62_timeOffset, weatherForecastSamples62_weather, weatherForecastSamples62_trackTemperature, weatherForecastSamples62_trackTemperatureChange, weatherForecastSamples62_airTemperature, weatherForecastSamples62_airTemperatureChange, weatherForecastSamples62_rainPercentage,
                                weatherForecastSamples63_sessionType, weatherForecastSamples63_timeOffset, weatherForecastSamples63_weather, weatherForecastSamples63_trackTemperature, weatherForecastSamples63_trackTemperatureChange, weatherForecastSamples63_airTemperature, weatherForecastSamples63_airTemperatureChange, weatherForecastSamples63_rainPercentage,
                                forecastAccuracy, aiDifficulty, seasonLinkIdentifier, weekendLinkIdentifier, sessionLinkIdentifier, pitStopWindowIdealLap, pitStopWindowLatestLap, pitStopRejoinPosition, steeringAssist, brakingAssist, gearboxAssist, pitAssist,
                                pitReleaseAssist, ersAssist, drsAssist, dynamicRacingLine, dynamicRacingLineType, gameMode, ruleSet, timeOfDay, sessionLength, speedUnitsLeadPlayer, temperatureUnitsLeadPlayer, speedUnitsSecondaryPlayer, temperatureUnitsSecondaryPlayer,
                                numSafetyCarPeriods, numVirtualSafetyCarPeriods, numRedFlagPeriods, equalCarPerformance, recoveryMode, flashbackLimit, surfaceType, lowFuelMode, raceStarts, tyreTemperature, pitLaneTyreSim, carDamage, carDamageRate, collisions,
                                collisionsOffForFirstLapOnly, mpUnsafePitRelease, mpOffForGriefing, cornerCuttingStringency, parcFermeRules, pitStopExperience, safetyCar, safetyCarExperience, formationLap, formationLapExperience, redFlags, affectsLicenceLevelSolo,
                                affectsLicenceLevelMP, numSessionsInWeekend,
                                weekendStructure0,
                                weekendStructure1,
                                weekendStructure2,
                                weekendStructure3,
                                weekendStructure4,
                                weekendStructure5,
                                weekendStructure6,
                                weekendStructure7,
                                weekendStructure8,
                                weekendStructure9,
                                weekendStructure10,
                                weekendStructure11,
                                sector2LapDistanceStart,
                                sector3LapDistanceStart
                            ) VALUES (
                                @weather, @trackTemperature, @airTemperature, @totalLaps, @trackLength, @sessionType, @trackId, @formula, @sessionTimeLeft, @sessionDuration, @pitSpeedLimit, @gamePaused, @isSpectating, @spectatorCarIndex, @sliProNativeSupport,
                                @numMarshalZones,
                                @marshalZones0_zoneStart, @marshalZones0_zoneFlag,
                                @marshalZones1_zoneStart, @marshalZones1_zoneFlag,
                                @marshalZones2_zoneStart, @marshalZones2_zoneFlag,
                                @marshalZones3_zoneStart, @marshalZones3_zoneFlag,
                                @marshalZones4_zoneStart, @marshalZones4_zoneFlag,
                                @marshalZones5_zoneStart, @marshalZones5_zoneFlag,
                                @marshalZones6_zoneStart, @marshalZones6_zoneFlag,
                                @marshalZones7_zoneStart, @marshalZones7_zoneFlag,
                                @marshalZones8_zoneStart, @marshalZones8_zoneFlag,
                                @marshalZones9_zoneStart, @marshalZones9_zoneFlag,
                                @marshalZones10_zoneStart, @marshalZones10_zoneFlag,
                                @marshalZones11_zoneStart, @marshalZones11_zoneFlag,
                                @marshalZones12_zoneStart, @marshalZones12_zoneFlag,
                                @marshalZones13_zoneStart, @marshalZones13_zoneFlag,
                                @marshalZones14_zoneStart, @marshalZones14_zoneFlag,
                                @marshalZones15_zoneStart, @marshalZones15_zoneFlag,
                                @marshalZones16_zoneStart, @marshalZones16_zoneFlag,
                                @marshalZones17_zoneStart, @marshalZones17_zoneFlag,
                                @marshalZones18_zoneStart, @marshalZones18_zoneFlag,
                                @marshalZones19_zoneStart, @marshalZones19_zoneFlag,
                                @marshalZones20_zoneStart, @marshalZones20_zoneFlag,
                                @safetyCarStatus, @networkGame, @numWeatherForecastSamples,
                                @weatherForecastSamples0_sessionType, @weatherForecastSamples0_timeOffset, @weatherForecastSamples0_weather, @weatherForecastSamples0_trackTemperature, @weatherForecastSamples0_trackTemperatureChange, @weatherForecastSamples0_airTemperature, @weatherForecastSamples0_airTemperatureChange, @weatherForecastSamples0_rainPercentage,
                                @weatherForecastSamples1_sessionType, @weatherForecastSamples1_timeOffset, @weatherForecastSamples1_weather, @weatherForecastSamples1_trackTemperature, @weatherForecastSamples1_trackTemperatureChange, @weatherForecastSamples1_airTemperature, @weatherForecastSamples1_airTemperatureChange, @weatherForecastSamples1_rainPercentage,
                                @weatherForecastSamples2_sessionType, @weatherForecastSamples2_timeOffset, @weatherForecastSamples2_weather, @weatherForecastSamples2_trackTemperature, @weatherForecastSamples2_trackTemperatureChange, @weatherForecastSamples2_airTemperature, @weatherForecastSamples2_airTemperatureChange, @weatherForecastSamples2_rainPercentage,
                                @weatherForecastSamples3_sessionType, @weatherForecastSamples3_timeOffset, @weatherForecastSamples3_weather, @weatherForecastSamples3_trackTemperature, @weatherForecastSamples3_trackTemperatureChange, @weatherForecastSamples3_airTemperature, @weatherForecastSamples3_airTemperatureChange, @weatherForecastSamples3_rainPercentage,
                                @weatherForecastSamples4_sessionType, @weatherForecastSamples4_timeOffset, @weatherForecastSamples4_weather, @weatherForecastSamples4_trackTemperature, @weatherForecastSamples4_trackTemperatureChange, @weatherForecastSamples4_airTemperature, @weatherForecastSamples4_airTemperatureChange, @weatherForecastSamples4_rainPercentage,
                                @weatherForecastSamples5_sessionType, @weatherForecastSamples5_timeOffset, @weatherForecastSamples5_weather, @weatherForecastSamples5_trackTemperature, @weatherForecastSamples5_trackTemperatureChange, @weatherForecastSamples5_airTemperature, @weatherForecastSamples5_airTemperatureChange, @weatherForecastSamples5_rainPercentage,
                                @weatherForecastSamples6_sessionType, @weatherForecastSamples6_timeOffset, @weatherForecastSamples6_weather, @weatherForecastSamples6_trackTemperature, @weatherForecastSamples6_trackTemperatureChange, @weatherForecastSamples6_airTemperature, @weatherForecastSamples6_airTemperatureChange, @weatherForecastSamples6_rainPercentage,
                                @weatherForecastSamples7_sessionType, @weatherForecastSamples7_timeOffset, @weatherForecastSamples7_weather, @weatherForecastSamples7_trackTemperature, @weatherForecastSamples7_trackTemperatureChange, @weatherForecastSamples7_airTemperature, @weatherForecastSamples7_airTemperatureChange, @weatherForecastSamples7_rainPercentage,
                                @weatherForecastSamples8_sessionType, @weatherForecastSamples8_timeOffset, @weatherForecastSamples8_weather, @weatherForecastSamples8_trackTemperature, @weatherForecastSamples8_trackTemperatureChange, @weatherForecastSamples8_airTemperature, @weatherForecastSamples8_airTemperatureChange, @weatherForecastSamples8_rainPercentage,
                                @weatherForecastSamples9_sessionType, @weatherForecastSamples9_timeOffset, @weatherForecastSamples9_weather, @weatherForecastSamples9_trackTemperature, @weatherForecastSamples9_trackTemperatureChange, @weatherForecastSamples9_airTemperature, @weatherForecastSamples9_airTemperatureChange, @weatherForecastSamples9_rainPercentage,
                                @weatherForecastSamples10_sessionType, @weatherForecastSamples10_timeOffset, @weatherForecastSamples10_weather, @weatherForecastSamples10_trackTemperature, @weatherForecastSamples10_trackTemperatureChange, @weatherForecastSamples10_airTemperature, @weatherForecastSamples10_airTemperatureChange, @weatherForecastSamples10_rainPercentage,
                                @weatherForecastSamples11_sessionType, @weatherForecastSamples11_timeOffset, @weatherForecastSamples11_weather, @weatherForecastSamples11_trackTemperature, @weatherForecastSamples11_trackTemperatureChange, @weatherForecastSamples11_airTemperature, @weatherForecastSamples11_airTemperatureChange, @weatherForecastSamples11_rainPercentage,
                                @weatherForecastSamples12_sessionType, @weatherForecastSamples12_timeOffset, @weatherForecastSamples12_weather, @weatherForecastSamples12_trackTemperature, @weatherForecastSamples12_trackTemperatureChange, @weatherForecastSamples12_airTemperature, @weatherForecastSamples12_airTemperatureChange, @weatherForecastSamples12_rainPercentage,
                                @weatherForecastSamples13_sessionType, @weatherForecastSamples13_timeOffset, @weatherForecastSamples13_weather, @weatherForecastSamples13_trackTemperature, @weatherForecastSamples13_trackTemperatureChange, @weatherForecastSamples13_airTemperature, @weatherForecastSamples13_airTemperatureChange, @weatherForecastSamples13_rainPercentage,
                                @weatherForecastSamples14_sessionType, @weatherForecastSamples14_timeOffset, @weatherForecastSamples14_weather, @weatherForecastSamples14_trackTemperature, @weatherForecastSamples14_trackTemperatureChange, @weatherForecastSamples14_airTemperature, @weatherForecastSamples14_airTemperatureChange, @weatherForecastSamples14_rainPercentage,
                                @weatherForecastSamples15_sessionType, @weatherForecastSamples15_timeOffset, @weatherForecastSamples15_weather, @weatherForecastSamples15_trackTemperature, @weatherForecastSamples15_trackTemperatureChange, @weatherForecastSamples15_airTemperature, @weatherForecastSamples15_airTemperatureChange, @weatherForecastSamples15_rainPercentage,
                                @weatherForecastSamples16_sessionType, @weatherForecastSamples16_timeOffset, @weatherForecastSamples16_weather, @weatherForecastSamples16_trackTemperature, @weatherForecastSamples16_trackTemperatureChange, @weatherForecastSamples16_airTemperature, @weatherForecastSamples16_airTemperatureChange, @weatherForecastSamples16_rainPercentage,
                                @weatherForecastSamples17_sessionType, @weatherForecastSamples17_timeOffset, @weatherForecastSamples17_weather, @weatherForecastSamples17_trackTemperature, @weatherForecastSamples17_trackTemperatureChange, @weatherForecastSamples17_airTemperature, @weatherForecastSamples17_airTemperatureChange, @weatherForecastSamples17_rainPercentage,
                                @weatherForecastSamples18_sessionType, @weatherForecastSamples18_timeOffset, @weatherForecastSamples18_weather, @weatherForecastSamples18_trackTemperature, @weatherForecastSamples18_trackTemperatureChange, @weatherForecastSamples18_airTemperature, @weatherForecastSamples18_airTemperatureChange, @weatherForecastSamples18_rainPercentage,
                                @weatherForecastSamples19_sessionType, @weatherForecastSamples19_timeOffset, @weatherForecastSamples19_weather, @weatherForecastSamples19_trackTemperature, @weatherForecastSamples19_trackTemperatureChange, @weatherForecastSamples19_airTemperature, @weatherForecastSamples19_airTemperatureChange, @weatherForecastSamples19_rainPercentage,
                                @weatherForecastSamples20_sessionType, @weatherForecastSamples20_timeOffset, @weatherForecastSamples20_weather, @weatherForecastSamples20_trackTemperature, @weatherForecastSamples20_trackTemperatureChange, @weatherForecastSamples20_airTemperature, @weatherForecastSamples20_airTemperatureChange, @weatherForecastSamples20_rainPercentage,
                                @weatherForecastSamples21_sessionType, @weatherForecastSamples21_timeOffset, @weatherForecastSamples21_weather, @weatherForecastSamples21_trackTemperature, @weatherForecastSamples21_trackTemperatureChange, @weatherForecastSamples21_airTemperature, @weatherForecastSamples21_airTemperatureChange, @weatherForecastSamples21_rainPercentage,
                                @weatherForecastSamples22_sessionType, @weatherForecastSamples22_timeOffset, @weatherForecastSamples22_weather, @weatherForecastSamples22_trackTemperature, @weatherForecastSamples22_trackTemperatureChange, @weatherForecastSamples22_airTemperature, @weatherForecastSamples22_airTemperatureChange, @weatherForecastSamples22_rainPercentage,
                                @weatherForecastSamples23_sessionType, @weatherForecastSamples23_timeOffset, @weatherForecastSamples23_weather, @weatherForecastSamples23_trackTemperature, @weatherForecastSamples23_trackTemperatureChange, @weatherForecastSamples23_airTemperature, @weatherForecastSamples23_airTemperatureChange, @weatherForecastSamples23_rainPercentage,
                                @weatherForecastSamples24_sessionType, @weatherForecastSamples24_timeOffset, @weatherForecastSamples24_weather, @weatherForecastSamples24_trackTemperature, @weatherForecastSamples24_trackTemperatureChange, @weatherForecastSamples24_airTemperature, @weatherForecastSamples24_airTemperatureChange, @weatherForecastSamples24_rainPercentage,
                                @weatherForecastSamples25_sessionType, @weatherForecastSamples25_timeOffset, @weatherForecastSamples25_weather, @weatherForecastSamples25_trackTemperature, @weatherForecastSamples25_trackTemperatureChange, @weatherForecastSamples25_airTemperature, @weatherForecastSamples25_airTemperatureChange, @weatherForecastSamples25_rainPercentage,
                                @weatherForecastSamples26_sessionType, @weatherForecastSamples26_timeOffset, @weatherForecastSamples26_weather, @weatherForecastSamples26_trackTemperature, @weatherForecastSamples26_trackTemperatureChange, @weatherForecastSamples26_airTemperature, @weatherForecastSamples26_airTemperatureChange, @weatherForecastSamples26_rainPercentage,
                                @weatherForecastSamples27_sessionType, @weatherForecastSamples27_timeOffset, @weatherForecastSamples27_weather, @weatherForecastSamples27_trackTemperature, @weatherForecastSamples27_trackTemperatureChange, @weatherForecastSamples27_airTemperature, @weatherForecastSamples27_airTemperatureChange, @weatherForecastSamples27_rainPercentage,
                                @weatherForecastSamples28_sessionType, @weatherForecastSamples28_timeOffset, @weatherForecastSamples28_weather, @weatherForecastSamples28_trackTemperature, @weatherForecastSamples28_trackTemperatureChange, @weatherForecastSamples28_airTemperature, @weatherForecastSamples28_airTemperatureChange, @weatherForecastSamples28_rainPercentage,
                                @weatherForecastSamples29_sessionType, @weatherForecastSamples29_timeOffset, @weatherForecastSamples29_weather, @weatherForecastSamples29_trackTemperature, @weatherForecastSamples29_trackTemperatureChange, @weatherForecastSamples29_airTemperature, @weatherForecastSamples29_airTemperatureChange, @weatherForecastSamples29_rainPercentage,
                                @weatherForecastSamples30_sessionType, @weatherForecastSamples30_timeOffset, @weatherForecastSamples30_weather, @weatherForecastSamples30_trackTemperature, @weatherForecastSamples30_trackTemperatureChange, @weatherForecastSamples30_airTemperature, @weatherForecastSamples30_airTemperatureChange, @weatherForecastSamples30_rainPercentage,
                                @weatherForecastSamples31_sessionType, @weatherForecastSamples31_timeOffset, @weatherForecastSamples31_weather, @weatherForecastSamples31_trackTemperature, @weatherForecastSamples31_trackTemperatureChange, @weatherForecastSamples31_airTemperature, @weatherForecastSamples31_airTemperatureChange, @weatherForecastSamples31_rainPercentage,
                                @weatherForecastSamples32_sessionType, @weatherForecastSamples32_timeOffset, @weatherForecastSamples32_weather, @weatherForecastSamples32_trackTemperature, @weatherForecastSamples32_trackTemperatureChange, @weatherForecastSamples32_airTemperature, @weatherForecastSamples32_airTemperatureChange, @weatherForecastSamples32_rainPercentage,
                                @weatherForecastSamples33_sessionType, @weatherForecastSamples33_timeOffset, @weatherForecastSamples33_weather, @weatherForecastSamples33_trackTemperature, @weatherForecastSamples33_trackTemperatureChange, @weatherForecastSamples33_airTemperature, @weatherForecastSamples33_airTemperatureChange, @weatherForecastSamples33_rainPercentage,
                                @weatherForecastSamples34_sessionType, @weatherForecastSamples34_timeOffset, @weatherForecastSamples34_weather, @weatherForecastSamples34_trackTemperature, @weatherForecastSamples34_trackTemperatureChange, @weatherForecastSamples34_airTemperature, @weatherForecastSamples34_airTemperatureChange, @weatherForecastSamples34_rainPercentage,
                                @weatherForecastSamples35_sessionType, @weatherForecastSamples35_timeOffset, @weatherForecastSamples35_weather, @weatherForecastSamples35_trackTemperature, @weatherForecastSamples35_trackTemperatureChange, @weatherForecastSamples35_airTemperature, @weatherForecastSamples35_airTemperatureChange, @weatherForecastSamples35_rainPercentage,
                                @weatherForecastSamples36_sessionType, @weatherForecastSamples36_timeOffset, @weatherForecastSamples36_weather, @weatherForecastSamples36_trackTemperature, @weatherForecastSamples36_trackTemperatureChange, @weatherForecastSamples36_airTemperature, @weatherForecastSamples36_airTemperatureChange, @weatherForecastSamples36_rainPercentage,
                                @weatherForecastSamples37_sessionType, @weatherForecastSamples37_timeOffset, @weatherForecastSamples37_weather, @weatherForecastSamples37_trackTemperature, @weatherForecastSamples37_trackTemperatureChange, @weatherForecastSamples37_airTemperature, @weatherForecastSamples37_airTemperatureChange, @weatherForecastSamples37_rainPercentage,
                                @weatherForecastSamples38_sessionType, @weatherForecastSamples38_timeOffset, @weatherForecastSamples38_weather, @weatherForecastSamples38_trackTemperature, @weatherForecastSamples38_trackTemperatureChange, @weatherForecastSamples38_airTemperature, @weatherForecastSamples38_airTemperatureChange, @weatherForecastSamples38_rainPercentage,
                                @weatherForecastSamples39_sessionType, @weatherForecastSamples39_timeOffset, @weatherForecastSamples39_weather, @weatherForecastSamples39_trackTemperature, @weatherForecastSamples39_trackTemperatureChange, @weatherForecastSamples39_airTemperature, @weatherForecastSamples39_airTemperatureChange, @weatherForecastSamples39_rainPercentage,
                                @weatherForecastSamples40_sessionType, @weatherForecastSamples40_timeOffset, @weatherForecastSamples40_weather, @weatherForecastSamples40_trackTemperature, @weatherForecastSamples40_trackTemperatureChange, @weatherForecastSamples40_airTemperature, @weatherForecastSamples40_airTemperatureChange, @weatherForecastSamples40_rainPercentage,
                                @weatherForecastSamples41_sessionType, @weatherForecastSamples41_timeOffset, @weatherForecastSamples41_weather, @weatherForecastSamples41_trackTemperature, @weatherForecastSamples41_trackTemperatureChange, @weatherForecastSamples41_airTemperature, @weatherForecastSamples41_airTemperatureChange, @weatherForecastSamples41_rainPercentage,
                                @weatherForecastSamples42_sessionType, @weatherForecastSamples42_timeOffset, @weatherForecastSamples42_weather, @weatherForecastSamples42_trackTemperature, @weatherForecastSamples42_trackTemperatureChange, @weatherForecastSamples42_airTemperature, @weatherForecastSamples42_airTemperatureChange, @weatherForecastSamples42_rainPercentage,
                                @weatherForecastSamples43_sessionType, @weatherForecastSamples43_timeOffset, @weatherForecastSamples43_weather, @weatherForecastSamples43_trackTemperature, @weatherForecastSamples43_trackTemperatureChange, @weatherForecastSamples43_airTemperature, @weatherForecastSamples43_airTemperatureChange, @weatherForecastSamples43_rainPercentage,
                                @weatherForecastSamples44_sessionType, @weatherForecastSamples44_timeOffset, @weatherForecastSamples44_weather, @weatherForecastSamples44_trackTemperature, @weatherForecastSamples44_trackTemperatureChange, @weatherForecastSamples44_airTemperature, @weatherForecastSamples44_airTemperatureChange, @weatherForecastSamples44_rainPercentage,
                                @weatherForecastSamples45_sessionType, @weatherForecastSamples45_timeOffset, @weatherForecastSamples45_weather, @weatherForecastSamples45_trackTemperature, @weatherForecastSamples45_trackTemperatureChange, @weatherForecastSamples45_airTemperature, @weatherForecastSamples45_airTemperatureChange, @weatherForecastSamples45_rainPercentage,
                                @weatherForecastSamples46_sessionType, @weatherForecastSamples46_timeOffset, @weatherForecastSamples46_weather, @weatherForecastSamples46_trackTemperature, @weatherForecastSamples46_trackTemperatureChange, @weatherForecastSamples46_airTemperature, @weatherForecastSamples46_airTemperatureChange, @weatherForecastSamples46_rainPercentage,
                                @weatherForecastSamples47_sessionType, @weatherForecastSamples47_timeOffset, @weatherForecastSamples47_weather, @weatherForecastSamples47_trackTemperature, @weatherForecastSamples47_trackTemperatureChange, @weatherForecastSamples47_airTemperature, @weatherForecastSamples47_airTemperatureChange, @weatherForecastSamples47_rainPercentage,
                                @weatherForecastSamples48_sessionType, @weatherForecastSamples48_timeOffset, @weatherForecastSamples48_weather, @weatherForecastSamples48_trackTemperature, @weatherForecastSamples48_trackTemperatureChange, @weatherForecastSamples48_airTemperature, @weatherForecastSamples48_airTemperatureChange, @weatherForecastSamples48_rainPercentage,
                                @weatherForecastSamples49_sessionType, @weatherForecastSamples49_timeOffset, @weatherForecastSamples49_weather, @weatherForecastSamples49_trackTemperature, @weatherForecastSamples49_trackTemperatureChange, @weatherForecastSamples49_airTemperature, @weatherForecastSamples49_airTemperatureChange, @weatherForecastSamples49_rainPercentage,
                                @weatherForecastSamples50_sessionType, @weatherForecastSamples50_timeOffset, @weatherForecastSamples50_weather, @weatherForecastSamples50_trackTemperature, @weatherForecastSamples50_trackTemperatureChange, @weatherForecastSamples50_airTemperature, @weatherForecastSamples50_airTemperatureChange, @weatherForecastSamples50_rainPercentage,
                                @weatherForecastSamples51_sessionType, @weatherForecastSamples51_timeOffset, @weatherForecastSamples51_weather, @weatherForecastSamples51_trackTemperature, @weatherForecastSamples51_trackTemperatureChange, @weatherForecastSamples51_airTemperature, @weatherForecastSamples51_airTemperatureChange, @weatherForecastSamples51_rainPercentage,
                                @weatherForecastSamples52_sessionType, @weatherForecastSamples52_timeOffset, @weatherForecastSamples52_weather, @weatherForecastSamples52_trackTemperature, @weatherForecastSamples52_trackTemperatureChange, @weatherForecastSamples52_airTemperature, @weatherForecastSamples52_airTemperatureChange, @weatherForecastSamples52_rainPercentage,
                                @weatherForecastSamples53_sessionType, @weatherForecastSamples53_timeOffset, @weatherForecastSamples53_weather, @weatherForecastSamples53_trackTemperature, @weatherForecastSamples53_trackTemperatureChange, @weatherForecastSamples53_airTemperature, @weatherForecastSamples53_airTemperatureChange, @weatherForecastSamples53_rainPercentage,
                                @weatherForecastSamples54_sessionType, @weatherForecastSamples54_timeOffset, @weatherForecastSamples54_weather, @weatherForecastSamples54_trackTemperature, @weatherForecastSamples54_trackTemperatureChange, @weatherForecastSamples54_airTemperature, @weatherForecastSamples54_airTemperatureChange, @weatherForecastSamples54_rainPercentage,
                                @weatherForecastSamples55_sessionType, @weatherForecastSamples55_timeOffset, @weatherForecastSamples55_weather, @weatherForecastSamples55_trackTemperature, @weatherForecastSamples55_trackTemperatureChange, @weatherForecastSamples55_airTemperature, @weatherForecastSamples55_airTemperatureChange, @weatherForecastSamples55_rainPercentage,
                                @weatherForecastSamples56_sessionType, @weatherForecastSamples56_timeOffset, @weatherForecastSamples56_weather, @weatherForecastSamples56_trackTemperature, @weatherForecastSamples56_trackTemperatureChange, @weatherForecastSamples56_airTemperature, @weatherForecastSamples56_airTemperatureChange, @weatherForecastSamples56_rainPercentage,
                                @weatherForecastSamples57_sessionType, @weatherForecastSamples57_timeOffset, @weatherForecastSamples57_weather, @weatherForecastSamples57_trackTemperature, @weatherForecastSamples57_trackTemperatureChange, @weatherForecastSamples57_airTemperature, @weatherForecastSamples57_airTemperatureChange, @weatherForecastSamples57_rainPercentage,
                                @weatherForecastSamples58_sessionType, @weatherForecastSamples58_timeOffset, @weatherForecastSamples58_weather, @weatherForecastSamples58_trackTemperature, @weatherForecastSamples58_trackTemperatureChange, @weatherForecastSamples58_airTemperature, @weatherForecastSamples58_airTemperatureChange, @weatherForecastSamples58_rainPercentage,
                                @weatherForecastSamples59_sessionType, @weatherForecastSamples59_timeOffset, @weatherForecastSamples59_weather, @weatherForecastSamples59_trackTemperature, @weatherForecastSamples59_trackTemperatureChange, @weatherForecastSamples59_airTemperature, @weatherForecastSamples59_airTemperatureChange, @weatherForecastSamples59_rainPercentage,
                                @weatherForecastSamples60_sessionType, @weatherForecastSamples60_timeOffset, @weatherForecastSamples60_weather, @weatherForecastSamples60_trackTemperature, @weatherForecastSamples60_trackTemperatureChange, @weatherForecastSamples60_airTemperature, @weatherForecastSamples60_airTemperatureChange, @weatherForecastSamples60_rainPercentage,
                                @weatherForecastSamples61_sessionType, @weatherForecastSamples61_timeOffset, @weatherForecastSamples61_weather, @weatherForecastSamples61_trackTemperature, @weatherForecastSamples61_trackTemperatureChange, @weatherForecastSamples61_airTemperature, @weatherForecastSamples61_airTemperatureChange, @weatherForecastSamples61_rainPercentage,
                                @weatherForecastSamples62_sessionType, @weatherForecastSamples62_timeOffset, @weatherForecastSamples62_weather, @weatherForecastSamples62_trackTemperature, @weatherForecastSamples62_trackTemperatureChange, @weatherForecastSamples62_airTemperature, @weatherForecastSamples62_airTemperatureChange, @weatherForecastSamples62_rainPercentage,
                                @weatherForecastSamples63_sessionType, @weatherForecastSamples63_timeOffset, @weatherForecastSamples63_weather, @weatherForecastSamples63_trackTemperature, @weatherForecastSamples63_trackTemperatureChange, @weatherForecastSamples63_airTemperature, @weatherForecastSamples63_airTemperatureChange, @weatherForecastSamples63_rainPercentage,
                                @forecastAccuracy,
                                @aiDifficulty, @seasonLinkIdentifier, @weekendLinkIdentifier, @sessionLinkIdentifier, @pitStopWindowIdealLap, @pitStopWindowLatestLap, @pitStopRejoinPosition, @steeringAssist, @brakingAssist, @gearboxAssist, @pitAssist, @pitReleaseAssist,
                                @ersAssist, @drsAssist, @dynamicRacingLine, @dynamicRacingLineType, @gameMode, @ruleSet, @timeOfDay, @sessionLength, @speedUnitsLeadPlayer, @temperatureUnitsLeadPlayer, @speedUnitsSecondaryPlayer, @temperatureUnitsSecondaryPlayer,
                                @numSafetyCarPeriods, @numVirtualSafetyCarPeriods, @numRedFlagPeriods, @equalCarPerformance, @recoveryMode, @flashbackLimit, @surfaceType, @lowFuelMode, @raceStarts, @tyreTemperature, @pitLaneTyreSim, @carDamage, @carDamageRate,
                                @collisions, @collisionsOffForFirstLapOnly, @mpUnsafePitRelease, @mpOffForGriefing, @cornerCuttingStringency, @parcFermeRules, @pitStopExperience, @safetyCar, @safetyCarExperience, @formationLap, @formationLapExperience, @redFlags,
                                @affectsLicenceLevelSolo, @affectsLicenceLevelMP, @numSessionsInWeekend,
                                @weekendStructure0,
                                @weekendStructure1,
                                @weekendStructure2,
                                @weekendStructure3,
                                @weekendStructure4,
                                @weekendStructure5,
                                @weekendStructure6,
                                @weekendStructure7,
                                @weekendStructure8,
                                @weekendStructure9,
                                @weekendStructure10,
                                @weekendStructure11,
                                @sector2LapDistanceStart,
                                @sector3LapDistanceStart
                            );", conn
                        ))
                        {
                            cmd.Parameters.AddWithValue("@weather", dataSession.weather);
                            cmd.Parameters.AddWithValue("@trackTemperature", dataSession.trackTemperature);
                            cmd.Parameters.AddWithValue("@airTemperature", dataSession.airTemperature);
                            cmd.Parameters.AddWithValue("@totalLaps", dataSession.totalLaps);
                            cmd.Parameters.AddWithValue("@trackLength", dataSession.trackLength);
                            cmd.Parameters.AddWithValue("@sessionType", dataSession.sessionType);
                            cmd.Parameters.AddWithValue("@trackId", dataSession.trackId);
                            cmd.Parameters.AddWithValue("@formula", dataSession.formula);
                            cmd.Parameters.AddWithValue("@sessionTimeLeft", dataSession.sessionTimeLeft);
                            cmd.Parameters.AddWithValue("@sessionDuration", dataSession.sessionDuration);
                            cmd.Parameters.AddWithValue("@pitSpeedLimit", dataSession.pitSpeedLimit);
                            cmd.Parameters.AddWithValue("@gamePaused", dataSession.gamePaused);
                            cmd.Parameters.AddWithValue("@isSpectating", dataSession.isSpectating);
                            cmd.Parameters.AddWithValue("@spectatorCarIndex", dataSession.spectatorCarIndex);
                            cmd.Parameters.AddWithValue("@sliProNativeSupport", dataSession.sliProNativeSupport);
                            cmd.Parameters.AddWithValue("@numMarshalZones", dataSession.numMarshalZones);
                            for (var i = 0; i < 21; i++)
                            {
                                cmd.Parameters.AddWithValue($"@marshalZones{i}_zoneStart",dataSession.marshalZones[i].zoneStart);
                                cmd.Parameters.AddWithValue($"@marshalZones{i}_zoneFlag",dataSession.marshalZones[i].zoneFlag);
                            }
                            cmd.Parameters.AddWithValue("@safetyCarStatus", dataSession.safetyCarStatus);
                            cmd.Parameters.AddWithValue("@networkGame", dataSession.networkGame);
                            cmd.Parameters.AddWithValue("@numWeatherForecastSamples",dataSession.numWeatherForecastSamples);
                            for (var i = 0; i < 64; i++)
                            {
                                cmd.Parameters.AddWithValue($"@weatherForecastSamples{i}_sessionType",dataSession.weatherForecastSamples[i].sessionType);
                                cmd.Parameters.AddWithValue($"@weatherForecastSamples{i}_timeOffset",dataSession.weatherForecastSamples[i].timeOffset);
                                cmd.Parameters.AddWithValue($"@weatherForecastSamples{i}_weather",dataSession.weatherForecastSamples[i].weather);
                                cmd.Parameters.AddWithValue($"@weatherForecastSamples{i}_trackTemperature",dataSession.weatherForecastSamples[i].trackTemperature);
                                cmd.Parameters.AddWithValue($"@weatherForecastSamples{i}_trackTemperatureChange",dataSession.weatherForecastSamples[i].trackTemperatureChange);
                                cmd.Parameters.AddWithValue($"@weatherForecastSamples{i}_airTemperature",dataSession.weatherForecastSamples[i].airTemperature);
                                cmd.Parameters.AddWithValue($"@weatherForecastSamples{i}_airTemperatureChange",dataSession.weatherForecastSamples[i].airTemperatureChange);
                                cmd.Parameters.AddWithValue($"@weatherForecastSamples{i}_rainPercentage",dataSession.weatherForecastSamples[i].rainPercentage);
                            }
                            cmd.Parameters.AddWithValue("@forecastAccuracy", dataSession.forecastAccuracy);
                            cmd.Parameters.AddWithValue("@aiDifficulty", dataSession.aiDifficulty);
                            cmd.Parameters.AddWithValue("@seasonLinkIdentifier", dataSession.seasonLinkIdentifier);
                            cmd.Parameters.AddWithValue("@weekendLinkIdentifier", dataSession.weekendLinkIdentifier);
                            cmd.Parameters.AddWithValue("@sessionLinkIdentifier", dataSession.sessionLinkIdentifier);
                            cmd.Parameters.AddWithValue("@pitStopWindowIdealLap", dataSession.pitStopWindowIdealLap);
                            cmd.Parameters.AddWithValue("@pitStopWindowLatestLap", dataSession.pitStopWindowLatestLap);
                            cmd.Parameters.AddWithValue("@pitStopRejoinPosition", dataSession.pitStopRejoinPosition);
                            cmd.Parameters.AddWithValue("@steeringAssist", dataSession.steeringAssist);
                            cmd.Parameters.AddWithValue("@brakingAssist", dataSession.brakingAssist);
                            cmd.Parameters.AddWithValue("@gearboxAssist", dataSession.gearboxAssist);
                            cmd.Parameters.AddWithValue("@pitAssist", dataSession.pitAssist);
                            cmd.Parameters.AddWithValue("@pitReleaseAssist", dataSession.pitReleaseAssist);
                            cmd.Parameters.AddWithValue("@ERSAssist", dataSession.ERSAssist);
                            cmd.Parameters.AddWithValue("@DRSAssist", dataSession.DRSAssist);
                            cmd.Parameters.AddWithValue("@dynamicRacingLine", dataSession.dynamicRacingLine);
                            cmd.Parameters.AddWithValue("@dynamicRacingLineType", dataSession.dynamicRacingLineType);
                            cmd.Parameters.AddWithValue("@gameMode", dataSession.gameMode);
                            cmd.Parameters.AddWithValue("@ruleSet", dataSession.ruleSet);
                            cmd.Parameters.AddWithValue("@timeOfDay", dataSession.timeOfDay);
                            cmd.Parameters.AddWithValue("@sessionLength", dataSession.sessionLength);
                            cmd.Parameters.AddWithValue("@speedUnitsLeadPlayer", dataSession.speedUnitsLeadPlayer);
                            cmd.Parameters.AddWithValue("@temperatureUnitsLeadPlayer",dataSession.temperatureUnitsLeadPlayer);
                            cmd.Parameters.AddWithValue("@speedUnitsSecondaryPlayer",dataSession.speedUnitsSecondaryPlayer);
                            cmd.Parameters.AddWithValue("@temperatureUnitsSecondaryPlayer",dataSession.temperatureUnitsSecondaryPlayer);
                            cmd.Parameters.AddWithValue("@numSafetyCarPeriods", dataSession.numSafetyCarPeriods);
                            cmd.Parameters.AddWithValue("@numVirtualSafetyCarPeriods",dataSession.numVirtualSafetyCarPeriods);
                            cmd.Parameters.AddWithValue("@numRedFlagPeriods", dataSession.numRedFlagPeriods);
                            cmd.Parameters.AddWithValue("@equalCarPerformance", dataSession.equalCarPerformance);
                            cmd.Parameters.AddWithValue("@recoveryMode", dataSession.recoveryMode);
                            cmd.Parameters.AddWithValue("@flashbackLimit", dataSession.flashbackLimit);
                            cmd.Parameters.AddWithValue("@surfaceType", dataSession.surfaceType);
                            cmd.Parameters.AddWithValue("@lowFuelMode", dataSession.lowFuelMode);
                            cmd.Parameters.AddWithValue("@raceStarts", dataSession.raceStarts);
                            cmd.Parameters.AddWithValue("@tyreTemperature", dataSession.tyreTemperature);
                            cmd.Parameters.AddWithValue("@pitLaneTyreSim", dataSession.pitLaneTyreSim);
                            cmd.Parameters.AddWithValue("@carDamage", dataSession.carDamage);
                            cmd.Parameters.AddWithValue("@carDamageRate", dataSession.carDamageRate);
                            cmd.Parameters.AddWithValue("@collisions", dataSession.collisions);
                            cmd.Parameters.AddWithValue("@collisionsOffForFirstLapOnly",dataSession.collisionsOffForFirstLapOnly);
                            cmd.Parameters.AddWithValue("@mpUnsafePitRelease", dataSession.mpUnsafePitRelease);
                            cmd.Parameters.AddWithValue("@mpOffForGriefing", dataSession.mpOffForGriefing);
                            cmd.Parameters.AddWithValue("@cornerCuttingStringency", dataSession.cornerCuttingStringency);
                            cmd.Parameters.AddWithValue("@parcFermeRules", dataSession.parcFermeRules);
                            cmd.Parameters.AddWithValue("@pitStopExperience", dataSession.pitStopExperience);
                            cmd.Parameters.AddWithValue("@safetyCar", dataSession.safetyCar);
                            cmd.Parameters.AddWithValue("@safetyCarExperience", dataSession.safetyCarExperience);
                            cmd.Parameters.AddWithValue("@formationLap", dataSession.formationLap);
                            cmd.Parameters.AddWithValue("@formationLapExperience", dataSession.formationLapExperience);
                            cmd.Parameters.AddWithValue("@redFlags", dataSession.redFlags);
                            cmd.Parameters.AddWithValue("@affectsLicenceLevelSolo", dataSession.affectsLicenceLevelSolo);
                            cmd.Parameters.AddWithValue("@affectsLicenceLevelMP", dataSession.affectsLicenceLevelMP);
                            cmd.Parameters.AddWithValue("@numSessionsInWeekend", dataSession.numSessionsInWeekend);
                            for (var i = 0; i < 12; i++) cmd.Parameters.AddWithValue($"@weekendStructure{i}", dataSession.weekendStructure[i]);
                            cmd.Parameters.AddWithValue("@sector2LapDistanceStart", dataSession.sector2LapDistanceStart);
                            cmd.Parameters.AddWithValue("@sector3LapDistanceStart", dataSession.sector3LapDistanceStart);
                            try
                            {
                                await cmd.ExecuteNonQueryAsync(stoppingToken);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"SESSION INSERT error : {ex.Message}");
                            }
                        }
                        break;
//--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
                    case 1285:
                        //-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------start of Lap of 1285B
                        var headerLap = new Header
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
                        var lap = new Lap[22];
                        for (var i = 0; i < 22; i++) lap[i] = ReadLap(br);
                        var timeTrialPBCarIdx = br.ReadByte();
                        var timeTrialRivalCarIdx = br.ReadByte();
                        //-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------end
                        var dataLap = lap[headerLap.playerCarIndex];

                        await using (var cmd = new MySqlCommand(@"
                            INSERT INTO LAP (
                                lastLapTimeInMS, currentLapTimeInMS,
                                sector1TimeMSPart, sector1TimeMinutesPart,
                                sector2TimeMSPart, sector2TimeMinutesPart,
                                deltaToCarInFrontMSPart, deltaToCarInFrontMinutesPart,
                                deltaToRaceLeaderMSPart, deltaToRaceLeaderMinutesPart,
                                lapDistance, totalDistance,
                                safetyCarDelta,
                                carPosition,
                                currentLapNum,
                                pitStatus, numPitStops,
                                sector, currentLapInvalid,
                                penalties, totalWarnings,
                                cornerCuttingWarnings,
                                numUnservedDriveThroughPens, numUnservedStopGoPens,
                                gridPosition,
                                driverStatus, resultStatus,
                                pitLaneTimerActive, pitLaneTimeInLaneInMS,
                                pitStopTimerInMS, pitStopShouldServePen,
                                speedTrapFastestSpeed, speedTrapFastestLap,
                                timeTrialPBCarIdx, timeTrialRivalCarIdx
                            ) VALUES (
                                @lastLapTimeInMS, @currentLapTimeInMS,
                                @sector1TimeMSPart, @sector1TimeMinutesPart,
                                @sector2TimeMSPart, @sector2TimeMinutesPart,
                                @deltaToCarInFrontMSPart, @deltaToCarInFrontMinutesPart,
                                @deltaToRaceLeaderMSPart, @deltaToRaceLeaderMinutesPart,
                                @lapDistance, @totalDistance,
                                @safetyCarDelta,
                                @carPosition,
                                @currentLapNum,
                                @pitStatus, @numPitStops,
                                @sector, @currentLapInvalid,
                                @penalties, @totalWarnings,
                                @cornerCuttingWarnings,
                                @numUnservedDriveThroughPens, @numUnservedStopGoPens,
                                @gridPosition,
                                @driverStatus, @resultStatus,
                                @pitLaneTimerActive, @pitLaneTimeInLaneInMS,
                                @pitStopTimerInMS, @pitStopShouldServePen,
                                @speedTrapFastestSpeed, @speedTrapFastestLap,
                                @timeTrialPBCarIdx, @timeTrialRivalCarIdx
                            );", conn))
                        {
                            cmd.Parameters.AddWithValue("@lastLapTimeInMS", dataLap.lastLapTimeInMS);
                            cmd.Parameters.AddWithValue("@currentLapTimeInMS", dataLap.currentLapTimeInMS);
                            cmd.Parameters.AddWithValue("@sector1TimeMSPart", dataLap.sector1TimeMSPart);
                            cmd.Parameters.AddWithValue("@sector1TimeMinutesPart", dataLap.sector1TimeMinutesPart);
                            cmd.Parameters.AddWithValue("@sector2TimeMSPart", dataLap.sector2TimeMSPart);
                            cmd.Parameters.AddWithValue("@sector2TimeMinutesPart", dataLap.sector2TimeMinutesPart);
                            cmd.Parameters.AddWithValue("@deltaToCarInFrontMSPart", dataLap.deltaToCarInFrontMSPart);
                            cmd.Parameters.AddWithValue("@deltaToCarInFrontMinutesPart", dataLap.deltaToCarInFrontMinutesPart);
                            cmd.Parameters.AddWithValue("@deltaToRaceLeaderMSPart", dataLap.deltaToRaceLeaderMSPart);
                            cmd.Parameters.AddWithValue("@deltaToRaceLeaderMinutesPart", dataLap.deltaToRaceLeaderMinutesPart);
                            cmd.Parameters.AddWithValue("@lapDistance", dataLap.lapDistance);
                            cmd.Parameters.AddWithValue("@totalDistance", dataLap.totalDistance);
                            cmd.Parameters.AddWithValue("@safetyCarDelta", dataLap.safetyCarDelta);
                            cmd.Parameters.AddWithValue("@carPosition", dataLap.carPosition);
                            cmd.Parameters.AddWithValue("@currentLapNum", dataLap.currentLapNum);
                            cmd.Parameters.AddWithValue("@pitStatus", dataLap.pitStatus);
                            cmd.Parameters.AddWithValue("@numPitStops", dataLap.numPitStops);
                            cmd.Parameters.AddWithValue("@sector", dataLap.sector);
                            cmd.Parameters.AddWithValue("@currentLapInvalid", dataLap.currentLapInvalid);
                            cmd.Parameters.AddWithValue("@penalties", dataLap.penalties);
                            cmd.Parameters.AddWithValue("@totalWarnings", dataLap.totalWarnings);
                            cmd.Parameters.AddWithValue("@cornerCuttingWarnings", dataLap.cornerCuttingWarnings);
                            cmd.Parameters.AddWithValue("@numUnservedDriveThroughPens", dataLap.numUnservedDriveThroughPens);
                            cmd.Parameters.AddWithValue("@numUnservedStopGoPens", dataLap.numUnservedStopGoPens);
                            cmd.Parameters.AddWithValue("@gridPosition", dataLap.gridPosition);
                            cmd.Parameters.AddWithValue("@driverStatus", dataLap.driverStatus);
                            cmd.Parameters.AddWithValue("@resultStatus", dataLap.resultStatus);
                            cmd.Parameters.AddWithValue("@pitLaneTimerActive", dataLap.pitLaneTimerActive);
                            cmd.Parameters.AddWithValue("@pitLaneTimeInLaneInMS", dataLap.pitLaneTimeInLaneInMS);
                            cmd.Parameters.AddWithValue("@pitStopTimerInMS", dataLap.pitStopTimerInMS);
                            cmd.Parameters.AddWithValue("@pitStopShouldServePen", dataLap.pitStopShouldServePen);
                            cmd.Parameters.AddWithValue("@speedTrapFastestSpeed", dataLap.speedTrapFastestSpeed);
                            cmd.Parameters.AddWithValue("@speedTrapFastestLap", dataLap.speedTrapFastestLap);
                            cmd.Parameters.AddWithValue("@timeTrialPBCarIdx", timeTrialPBCarIdx);
                            cmd.Parameters.AddWithValue("@timeTrialRivalCarIdx", timeTrialRivalCarIdx);
                            try
                            {
                                await cmd.ExecuteNonQueryAsync(stoppingToken);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"LAP INSERT error : {ex.Message}");
                            }
                        }
                        break;
//--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
                    case 45:
                        //-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------start of Event of 45B
                        var dataEvent = ReadEvent(br);
                        /*
                            Session Started         (SSTA)      Sent when the session starts
                            Session Ended	        (SEND)	    Sent when the session ends
                            Fastest Lap	            (FTLP)  	When a driver achieves the fastest lap
                            Retirement	            (�RTMT) 	When a driver retires
                            DRS enabled	            (DRSE)  	Race control have enabled DRS
                            DRS disabled	        (DRSD�)     Race control have disabled DRS
                            Team mate in pits	    (TMPT)  	Your team mate has entered the pits
                            Chequered flag	        (CHQF)  	The chequered flag has been waved
                            Race Winner	            (RCWN)  	The race winner is announced
                            Penalty Issued	        (PENA)  	A penalty has been issued � details in event
                            Speed Trap Triggered	(SPTP)  	Speed trap has been triggered by fastest speed
                            Start lights	        (STLG)  	Start lights � number shown
                            Lights out	            (LGOT)  	Lights out
                            Drive through served	(DTSV)  	Drive through penalty served
                            Stop go served	        (SGSV)  	Stop go penalty served
                            Flashback	            (FLBK)  	Flashback activated
                            Button status	        (BUTN)  	Button status changed
                            Red Flag	            (RDFL)  	Red flag shown
                            Overtake	            (OVTK)  	Overtake occurred
                            Safety Car	            (SCAR)  	Safety car event - details in event
                            Collision	            (COLL)  	Collision between two vehicles has occurred
                        */
                        //-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------end
                        var eventCode = System.Text.Encoding.ASCII.GetString(dataEvent.eventStringCode);
                        
                        await using (var cmd = new MySqlCommand(@"
                            INSERT INTO EVENT (
                                eventStringCode,
                                fastestLap_vehicleIdx, fastestLap_lapTime,
                                retirement_vehicleIdx,
                                teamMateInPits_vehicleIdx,
                                raceWinner_vehicleIdx,
                                penalty_penaltyType, penalty_infringementType, penalty_vehicleIdx, penalty_otherVehicleIdx, penalty_time, penalty_lapNum, penalty_placesGained,
                                speedTrap_vehicleIdx, speedTrap_speed, speedTrap_isOverallFastest, speedTrap_isDriverFastest, speedTrap_fastestVehicleIdx, speedTrap_fastestSpeed,
                                startLights_numLights,
                                driveThroughPenaltyServed_vehicleIdx,
                                stopGoPenaltyServed_vehicleIdx,
                                flashback_flashbackFrameIdentifier, flashback_flashbackSessionTime,
                                buttons_buttonStatus,
                                overtake_overtakingVehicleIdx, overtake_beingOvertakenVehicleIdx,
                                safetyCar_safetyCarType, safetyCar_EventType,
                                collision_vehicle1Idx, collision_vehicle2Idx
                            ) VALUES (
                                @eventStringCode, @fastestLap_vehicleIdx,
                                @fastestLap_lapTime,
                                @retirement_vehicleIdx,
                                @teamMateInPits_vehicleIdx,
                                @raceWinner_vehicleIdx,
                                @penalty_penaltyType, @penalty_infringementType, @penalty_vehicleIdx, @penalty_otherVehicleIdx, @penalty_time, @penalty_lapNum, @penalty_placesGained,
                                @speedTrap_vehicleIdx, @speedTrap_speed, @speedTrap_isOverallFastest, @speedTrap_isDriverFastest, @speedTrap_fastestVehicleIdx, @speedTrap_fastestSpeed,
                                @startLights_numLights,
                                @driveThroughPenaltyServed_vehicleIdx,
                                @stopGoPenaltyServed_vehicleIdx,
                                @flashback_flashbackFrameIdentifier, @flashback_flashbackSessionTime,
                                @buttons_buttonStatus,
                                @overtake_overtakingVehicleIdx, @overtake_beingOvertakenVehicleIdx,
                                @safetyCar_safetyCarType, @safetyCar_EventType,
                                @collision_vehicle1Idx, @collision_vehicle2Idx
                            );", conn
                        ))
                        {
                            cmd.Parameters.AddWithValue("@eventStringCode", dataEvent.eventStringCode);
                            switch (eventCode)
                            {
                                case "FTLP":
                                    cmd.Parameters.AddWithValue("@fastestLap_vehicleIdx",dataEvent.eventDetails.FastestLap.vehicleIdx);
                                    cmd.Parameters.AddWithValue("@fastestLap_lapTime", dataEvent.eventDetails.FastestLap.lapTime);
                                    break;
                                case "RTMT":
                                    cmd.Parameters.AddWithValue("@retirement_vehicleIdx",dataEvent.eventDetails.Retirement.vehicleIdx);
                                    break;
                                case "TMPT":
                                    cmd.Parameters.AddWithValue("@teamMateInPits_vehicleIdx",dataEvent.eventDetails.TeamMateInPits.vehicleIdx);
                                    break;
                                case "RCWN":
                                    cmd.Parameters.AddWithValue("@raceWinner_vehicleIdx",dataEvent.eventDetails.RaceWinner.vehicleIdx);
                                    break;
                                case "PENA":
                                    cmd.Parameters.AddWithValue("@penalty_penaltyType", dataEvent.eventDetails.Penalty.penaltyType);
                                    cmd.Parameters.AddWithValue("@penalty_infringementType",dataEvent.eventDetails.Penalty.infringementType);
                                    cmd.Parameters.AddWithValue("@penalty_vehicleIdx", dataEvent.eventDetails.Penalty.vehicleIdx);
                                    cmd.Parameters.AddWithValue("@penalty_otherVehicleIdx",dataEvent.eventDetails.Penalty.otherVehicleIdx);
                                    cmd.Parameters.AddWithValue("@penalty_time", dataEvent.eventDetails.Penalty.time);
                                    cmd.Parameters.AddWithValue("@penalty_lapNum", dataEvent.eventDetails.Penalty.lapNum);
                                    cmd.Parameters.AddWithValue("@penalty_placesGained",dataEvent.eventDetails.Penalty.placesGained);
                                    break;
                                case "SPTP":
                                    cmd.Parameters.AddWithValue("@speedTrap_vehicleIdx", dataEvent.eventDetails.SpeedTrap.vehicleIdx);
                                    cmd.Parameters.AddWithValue("@speedTrap_speed", dataEvent.eventDetails.SpeedTrap.speed);
                                    cmd.Parameters.AddWithValue("@speedTrap_isOverallFastest", dataEvent.eventDetails.SpeedTrap.isOverallFastestInSession);
                                    cmd.Parameters.AddWithValue("@speedTrap_isDriverFastest", dataEvent.eventDetails.SpeedTrap.isDriverFastestInSession);
                                    cmd.Parameters.AddWithValue("@speedTrap_fastestVehicleIdx", dataEvent.eventDetails.SpeedTrap.fastestVehicleIdxInSession);
                                    cmd.Parameters.AddWithValue("@speedTrap_fastestSpeed", dataEvent.eventDetails.SpeedTrap.fastestSpeedInSession);
                                    break;
                                case "STLG":
                                    cmd.Parameters.AddWithValue("@startLights_numLights", dataEvent.eventDetails.StartLights.numLights);
                                    break;
                                case "DTSV":
                                    cmd.Parameters.AddWithValue("@driveThroughPenaltyServed_vehicleIdx", dataEvent.eventDetails.DriveThroughPenaltyServed.vehicleIdx);
                                    break;
                                case "SGSV":
                                    cmd.Parameters.AddWithValue("@stopGoPenaltyServed_vehicleIdx", dataEvent.eventDetails.StopGoPenaltyServed.vehicleIdx);
                                    break;
                                case "FLBK":
                                    cmd.Parameters.AddWithValue("@flashback_flashbackFrameIdentifier", dataEvent.eventDetails.Flashback.flashbackFrameIdentifier);
                                    cmd.Parameters.AddWithValue("@flashback_flashbackSessionTime", dataEvent.eventDetails.Flashback.flashbackSessionTime);
                                    break;
                                case "BUTN":
                                    cmd.Parameters.AddWithValue("@buttons_buttonStatus", dataEvent.eventDetails.Buttons.buttonStatus);
                                    break;
                                case "OVTK":
                                    cmd.Parameters.AddWithValue("@overtake_overtakingVehicleIdx", dataEvent.eventDetails.Overtake.overtakingVehicleIdx);
                                    cmd.Parameters.AddWithValue("@overtake_beingOvertakenVehicleIdx", dataEvent.eventDetails.Overtake.beingOvertakenVehicleIdx);
                                    break;
                                case "SCAR":
                                    cmd.Parameters.AddWithValue("@safetyCar_safetyCarType", dataEvent.eventDetails.SafetyCar.safetyCarType);
                                    cmd.Parameters.AddWithValue("@safetyCar_EventType", dataEvent.eventDetails.SafetyCar.eventType);
                                    break;
                                case "COLL":
                                    cmd.Parameters.AddWithValue("@collision_vehicle1Idx", dataEvent.eventDetails.Collision.vehicle1Idx);
                                    cmd.Parameters.AddWithValue("@collision_vehicle2Idx", dataEvent.eventDetails.Collision.vehicle2Idx);
                                    break;
                            }
                            try
                            {
                                await cmd.ExecuteNonQueryAsync(stoppingToken);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"EVENT {eventCode} INSERT error : {ex.Message}");
                            }
                        }
                        break;
//--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
                    case 1350:
                        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------start of Participants of 1350B
                        var headerParticipants = new Header
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
                        var numActiveCars = br.ReadByte();
                        var participants = new Participants[22];
                        for (var i = 0; i < 22; i++) participants[i] = ReadParticipants(br);
                        //-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------end
                        var dataParticipants = participants[headerParticipants.playerCarIndex];
                        var decodedParticipantsName = System.Text.Encoding.UTF8.GetString(dataParticipants.name).TrimEnd('\0'); //trim nulls

                        await using (var cmd = new MySqlCommand(@"
                            INSERT INTO PARTICIPANTS (
                                numActiveCars,
                                aiControlled,
                                driverId,
                                networkId,
                                teamId,
                                myTeam,
                                raceNumber,
                                nationality,
                                name,
                                yourTelemetry,
                                showOnlineNames,
                                techLevel,
                                platform
                            ) VALUES (
                                @numActiveCars,
                                @aiControlled,
                                @driverId,
                                @networkId,
                                @teamId,
                                @myTeam,
                                @raceNumber,
                                @nationality,
                                @name,
                                @yourTelemetry,
                                @showOnlineNames,
                                @techLevel,
                                @platform
                            );", conn
                        ))
                        {
                            cmd.Parameters.AddWithValue("@numActiveCars", numActiveCars);
                            cmd.Parameters.AddWithValue("@aiControlled", dataParticipants.aiControlled);
                            cmd.Parameters.AddWithValue("@driverId", dataParticipants.driverId);
                            cmd.Parameters.AddWithValue("@networkId", dataParticipants.networkId);
                            cmd.Parameters.AddWithValue("@teamId", dataParticipants.teamId);
                            cmd.Parameters.AddWithValue("@myTeam", dataParticipants.myTeam);
                            cmd.Parameters.AddWithValue("@raceNumber", dataParticipants.raceNumber);
                            cmd.Parameters.AddWithValue("@nationality", dataParticipants.nationality);
                            cmd.Parameters.AddWithValue("@name", decodedParticipantsName);
                            cmd.Parameters.AddWithValue("@yourTelemetry", dataParticipants.yourTelemetry);
                            cmd.Parameters.AddWithValue("@showOnlineNames", dataParticipants.showOnlineNames);
                            cmd.Parameters.AddWithValue("@techLevel", dataParticipants.techLevel);
                            cmd.Parameters.AddWithValue("@platform", dataParticipants.platform);
                            try
                            {
                                await cmd.ExecuteNonQueryAsync(stoppingToken);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"PARTICIPANTS INSERT error: {ex.Message}");
                            }
                        }
                        break;
//--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
                    case 1133:
                        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------start of CarSetup of 1133B
                        var headerCarSetup = new Header
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
                        var carSetup = new CarSetup[22];
                        for (var i = 0; i < 22; i++) carSetup[i] = ReadCarSetup(br);
                        var nextFrontWingValue = br.ReadSingle();
                        //-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------end
                        var dataCarSetup = carSetup[headerCarSetup.playerCarIndex];
                        
                        await using (var cmd = new MySqlCommand(@"
                            INSERT INTO CARSETUP (
                                frontWing, rearWing,
                                onThrottle, offThrottle,
                                frontCamber, rearCamber,
                                frontToe, rearToe,
                                frontSuspension, rearSuspension,
                                frontAntiRollBar, rearAntiRollBar,
                                frontSuspensionHeight, rearSuspensionHeight,
                                brakePressure, brakeBias,
                                engineBraking,
                                rearLeftTyrePressure, rearRightTyrePressure,
                                frontLeftTyrePressure, frontRightTyrePressure,
                                ballast,
                                fuelLoad,
                                nextFrontWingValue
                            ) VALUES (
                                @frontWing, @rearWing,
                                @onThrottle, @offThrottle,
                                @frontCamber, @rearCamber,
                                @frontToe, @rearToe,
                                @frontSuspension, @rearSuspension,
                                @frontAntiRollBar, @rearAntiRollBar,
                                @frontSuspensionHeight, @rearSuspensionHeight,
                                @brakePressure, @brakeBias,
                                @engineBraking,
                                @rearLeftTyrePressure, @rearRightTyrePressure,
                                @frontLeftTyrePressure, @frontRightTyrePressure,
                                @ballast,
                                @fuelLoad,
                                @nextFrontWingValue
                            );", conn
                        ))
                        {
                            cmd.Parameters.AddWithValue("@frontWing", dataCarSetup.frontWing);
                            cmd.Parameters.AddWithValue("@rearWing", dataCarSetup.rearWing);
                            cmd.Parameters.AddWithValue("@onThrottle", dataCarSetup.onThrottle);
                            cmd.Parameters.AddWithValue("@offThrottle", dataCarSetup.offThrottle);
                            cmd.Parameters.AddWithValue("@frontCamber", dataCarSetup.frontCamber);
                            cmd.Parameters.AddWithValue("@rearCamber", dataCarSetup.rearCamber);
                            cmd.Parameters.AddWithValue("@frontToe", dataCarSetup.frontToe);
                            cmd.Parameters.AddWithValue("@rearToe", dataCarSetup.rearToe);
                            cmd.Parameters.AddWithValue("@frontSuspension", dataCarSetup.frontSuspension);
                            cmd.Parameters.AddWithValue("@rearSuspension", dataCarSetup.rearSuspension);
                            cmd.Parameters.AddWithValue("@frontAntiRollBar", dataCarSetup.frontAntiRollBar);
                            cmd.Parameters.AddWithValue("@rearAntiRollBar", dataCarSetup.rearAntiRollBar);
                            cmd.Parameters.AddWithValue("@frontSuspensionHeight", dataCarSetup.frontSuspensionHeight);
                            cmd.Parameters.AddWithValue("@rearSuspensionHeight", dataCarSetup.rearSuspensionHeight);
                            cmd.Parameters.AddWithValue("@brakePressure", dataCarSetup.brakePressure);
                            cmd.Parameters.AddWithValue("@brakeBias", dataCarSetup.brakeBias);
                            cmd.Parameters.AddWithValue("@engineBraking", dataCarSetup.engineBraking);
                            cmd.Parameters.AddWithValue("@rearLeftTyrePressure", dataCarSetup.rearLeftTyrePressure);
                            cmd.Parameters.AddWithValue("@rearRightTyrePressure", dataCarSetup.rearRightTyrePressure);
                            cmd.Parameters.AddWithValue("@frontLeftTyrePressure", dataCarSetup.frontLeftTyrePressure);
                            cmd.Parameters.AddWithValue("@frontRightTyrePressure", dataCarSetup.frontRightTyrePressure);
                            cmd.Parameters.AddWithValue("@ballast", dataCarSetup.ballast);
                            cmd.Parameters.AddWithValue("@fuelLoad", dataCarSetup.fuelLoad);
                            cmd.Parameters.AddWithValue("@nextFrontWingValue", nextFrontWingValue);
                            try
                            {
                                await cmd.ExecuteNonQueryAsync(stoppingToken);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"CARSETUP INSERT error: {ex.Message}");
                            }
                        }
                        break;
//--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
                    case 1352:
                        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------start of CarTelemetry of 1352B
                        var headerCarTelemetry = new Header
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
                        var carTelemetry = new CarTelemetry[22];
                        for (var i = 0; i < 22; i++) carTelemetry[i] = ReadCarTelemetry(br);
                        br.ReadByte(); //mfdPanelIndex
                        br.ReadByte(); //mfdPanelIndexSecondaryPlayer
                        br.ReadSByte(); //suggestedGear
                        //-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------end
                        var dataCarTelemetry = carTelemetry[headerCarTelemetry.playerCarIndex];

                        await using (var cmd = new MySqlCommand(@"
                            INSERT INTO CARTELEMETRY (
                                speed, throttle, steer, brake, clutch, gear, engineRPM, drs,
                                revLightsPercent, revLightsBitValue,
                                brakesTemperature0, brakesTemperature1, brakesTemperature2, brakesTemperature3,
                                tyresSurfaceTemperature0, tyresSurfaceTemperature1, tyresSurfaceTemperature2, tyresSurfaceTemperature3,
                                tyresInnerTemperature0, tyresInnerTemperature1, tyresInnerTemperature2, tyresInnerTemperature3,
                                engineTemperature,
                                tyresPressure0, tyresPressure1, tyresPressure2, tyresPressure3,
                                surfaceType0, surfaceType1, surfaceType2, surfaceType3
                            ) VALUES (
                                @speed, @throttle, @steer, @brake, @clutch, @gear, @engineRPM, @drs,
                                @revLightsPercent, @revLightsBitValue,
                                @brakesTemperature0, @brakesTemperature1, @brakesTemperature2, @brakesTemperature3,
                                @tyresSurfaceTemperature0, @tyresSurfaceTemperature1, @tyresSurfaceTemperature2, @tyresSurfaceTemperature3,
                                @tyresInnerTemperature0, @tyresInnerTemperature1, @tyresInnerTemperature2, @tyresInnerTemperature3,
                                @engineTemperature,
                                @tyresPressure0, @tyresPressure1, @tyresPressure2, @tyresPressure3,
                                @surfaceType0, @surfaceType1, @surfaceType2, @surfaceType3
                            );", conn
                        ))
                        {
                            cmd.Parameters.AddWithValue("@speed", dataCarTelemetry.speed);
                            cmd.Parameters.AddWithValue("@throttle", dataCarTelemetry.throttle);
                            cmd.Parameters.AddWithValue("@steer", dataCarTelemetry.steer);
                            cmd.Parameters.AddWithValue("@brake", dataCarTelemetry.brake);
                            cmd.Parameters.AddWithValue("@clutch", dataCarTelemetry.clutch);
                            cmd.Parameters.AddWithValue("@gear", dataCarTelemetry.gear);
                            cmd.Parameters.AddWithValue("@engineRPM", dataCarTelemetry.engineRPM);
                            cmd.Parameters.AddWithValue("@drs", dataCarTelemetry.drs);
                            cmd.Parameters.AddWithValue("@revLightsPercent", dataCarTelemetry.revLightsPercent);
                            cmd.Parameters.AddWithValue("@revLightsBitValue", dataCarTelemetry.revLightsBitValue);
                            cmd.Parameters.AddWithValue("@brakesTemperature0",dataCarTelemetry.brakesTemperature[0]);
                            cmd.Parameters.AddWithValue("@brakesTemperature1",dataCarTelemetry.brakesTemperature[1]);
                            cmd.Parameters.AddWithValue("@brakesTemperature2",dataCarTelemetry.brakesTemperature[2]);
                            cmd.Parameters.AddWithValue("@brakesTemperature3",dataCarTelemetry.brakesTemperature[3]);
                            cmd.Parameters.AddWithValue("@tyresSurfaceTemperature0",dataCarTelemetry.tyresSurfaceTemperature[0]);
                            cmd.Parameters.AddWithValue("@tyresSurfaceTemperature1",dataCarTelemetry.tyresSurfaceTemperature[1]);
                            cmd.Parameters.AddWithValue("@tyresSurfaceTemperature2",dataCarTelemetry.tyresSurfaceTemperature[2]);
                            cmd.Parameters.AddWithValue("@tyresSurfaceTemperature3",dataCarTelemetry.tyresSurfaceTemperature[3]);
                            cmd.Parameters.AddWithValue("@tyresInnerTemperature0",dataCarTelemetry.tyresInnerTemperature[0]);
                            cmd.Parameters.AddWithValue("@tyresInnerTemperature1",dataCarTelemetry.tyresInnerTemperature[1]);
                            cmd.Parameters.AddWithValue("@tyresInnerTemperature2",dataCarTelemetry.tyresInnerTemperature[2]);
                            cmd.Parameters.AddWithValue("@tyresInnerTemperature3",dataCarTelemetry.tyresInnerTemperature[3]);
                            cmd.Parameters.AddWithValue("@engineTemperature", dataCarTelemetry.engineTemperature);
                            cmd.Parameters.AddWithValue("@tyresPressure0", dataCarTelemetry.tyresPressure[0]);
                            cmd.Parameters.AddWithValue("@tyresPressure1", dataCarTelemetry.tyresPressure[1]);
                            cmd.Parameters.AddWithValue("@tyresPressure2", dataCarTelemetry.tyresPressure[2]);
                            cmd.Parameters.AddWithValue("@tyresPressure3", dataCarTelemetry.tyresPressure[3]);
                            cmd.Parameters.AddWithValue("@surfaceType0", dataCarTelemetry.surfaceType[0]);
                            cmd.Parameters.AddWithValue("@surfaceType1", dataCarTelemetry.surfaceType[1]);
                            cmd.Parameters.AddWithValue("@surfaceType2", dataCarTelemetry.surfaceType[2]);
                            cmd.Parameters.AddWithValue("@surfaceType3", dataCarTelemetry.surfaceType[3]);
                            try
                            {
                                await cmd.ExecuteNonQueryAsync(stoppingToken);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"CARTELEMETRY INSERT error : {ex.Message}");
                            }
                        }
                        break;
//--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
                    case 1239:
                        //-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------start of CarStatus of 1239B
                        var headerCarStatus = new Header
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
                        var carStatus = new CarStatus[22];
                        for (var i = 0; i < 22; i++) carStatus[i] = ReadCarStatus(br);
                        //-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------end
                        var dataCarStatus = carStatus[headerCarStatus.playerCarIndex];

                        await using (var cmd = new MySqlCommand(@"
                            INSERT INTO CARSTATUS (
                                tractionControl,antiLockBrakes,
                                fuelMix,
                                frontBrakeBias,
                                pitLimiterStatus,
                                fuelInTank,fuelCapacity,fuelRemainingLaps,
                                maxRPM,idleRPM,maxGears,
                                drsAllowed,drsActivationDistance,
                                actualTyreCompound,visualTyreCompound,
                                tyresAgeLaps,
                                vehicleFiaFlags,
                                enginePowerICE,enginePowerMGUK,
                                ersStoreEnergy, ersDeployMode,
                                ersHarvestedThisLapMGUK, ersHarvestedThisLapMGUH,ersDeployedThisLap,
                                networkPaused
                            ) VALUES (
                                @tractionControl,@antiLockBrakes,
                                @fuelMix,
                                @frontBrakeBias,
                                @pitLimiterStatus,
                                @fuelInTank,@fuelCapacity,@fuelRemainingLaps,
                                @maxRPM,@idleRPM,@maxGears,
                                @drsAllowed,@drsActivationDistance,
                                @actualTyreCompound,@visualTyreCompound,
                                @tyresAgeLaps,
                                @vehicleFiaFlags,
                                @enginePowerICE,@enginePowerMGUK,
                                @ersStoreEnergy, @ersDeployMode,
                                @ersHarvestedThisLapMGUK,@ersHarvestedThisLapMGUH, @ersDeployedThisLap,
                                @networkPaused
                            );", conn
                        ))
                        {
                            cmd.Parameters.AddWithValue("@tractionControl", dataCarStatus.tractionControl);
                            cmd.Parameters.AddWithValue("@antiLockBrakes", dataCarStatus.antiLockBrakes);
                            cmd.Parameters.AddWithValue("@fuelMix", dataCarStatus.fuelMix);
                            cmd.Parameters.AddWithValue("@frontBrakeBias", dataCarStatus.frontBrakeBias);
                            cmd.Parameters.AddWithValue("@pitLimiterStatus", dataCarStatus.pitLimiterStatus);
                            cmd.Parameters.AddWithValue("@fuelInTank", dataCarStatus.fuelInTank);
                            cmd.Parameters.AddWithValue("@fuelCapacity", dataCarStatus.fuelCapacity);
                            cmd.Parameters.AddWithValue("@fuelRemainingLaps", dataCarStatus.fuelRemainingLaps);
                            cmd.Parameters.AddWithValue("@maxRPM", dataCarStatus.maxRPM);
                            cmd.Parameters.AddWithValue("@idleRPM", dataCarStatus.idleRPM);
                            cmd.Parameters.AddWithValue("@maxGears", dataCarStatus.maxGears);
                            cmd.Parameters.AddWithValue("@drsAllowed", dataCarStatus.drsAllowed);
                            cmd.Parameters.AddWithValue("@drsActivationDistance", dataCarStatus.drsActivationDistance);
                            cmd.Parameters.AddWithValue("@actualTyreCompound", dataCarStatus.actualTyreCompound);
                            cmd.Parameters.AddWithValue("@visualTyreCompound", dataCarStatus.visualTyreCompound);
                            cmd.Parameters.AddWithValue("@tyresAgeLaps", dataCarStatus.tyresAgeLaps);
                            cmd.Parameters.AddWithValue("@vehicleFiaFlags", dataCarStatus.vehicleFiaFlags);
                            cmd.Parameters.AddWithValue("@enginePowerICE", dataCarStatus.enginePowerICE);
                            cmd.Parameters.AddWithValue("@enginePowerMGUK", dataCarStatus.enginePowerMGUK);
                            cmd.Parameters.AddWithValue("@ersStoreEnergy", dataCarStatus.ersStoreEnergy);
                            cmd.Parameters.AddWithValue("@ersDeployMode", dataCarStatus.ersDeployMode);
                            cmd.Parameters.AddWithValue("@ersHarvestedThisLapMGUK", dataCarStatus.ersHarvestedThisLapMGUK);
                            cmd.Parameters.AddWithValue("@ersHarvestedThisLapMGUH", dataCarStatus.ersHarvestedThisLapMGUH);
                            cmd.Parameters.AddWithValue("@ersDeployedThisLap", dataCarStatus.ersDeployedThisLap);
                            cmd.Parameters.AddWithValue("@networkPaused", dataCarStatus.networkPaused);
                            try
                            {
                                await cmd.ExecuteNonQueryAsync(stoppingToken);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"CARSTATUS INSERT error: {ex.Message}");
                            }
                        }
                        break;
//--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
                    case 1020:
                        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------start of FinalClassification of 1020B
                        var headerFinalClassification = new Header
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
                        var numCars = br.ReadByte();
                        var finalClassification = new FinalClassification[22];
                        for (var i = 0; i < 22; i++) finalClassification[i] = ReadFinalClassification(br);
                        //-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------end
                        var dataFinalClassification = finalClassification[headerFinalClassification.playerCarIndex];

                        await using (var cmd = new MySqlCommand(@"
                            INSERT INTO FINALCLASSIFICATION (
                                numCars,
                                position,
                                numLaps,
                                gridPosition,
                                points,
                                numPitStops,
                                resultStatus,
                                bestLapTimeInMS,
                                totalRaceTime,
                                penaltiesTime,
                                numPenalties,
                                numTyreStints,
                                tyreStintsActual0,
                                tyreStintsActual1,
                                tyreStintsActual2,
                                tyreStintsActual3,
                                tyreStintsActual4,
                                tyreStintsActual5,
                                tyreStintsActual6,
                                tyreStintsActual7,
                                tyreStintsVisual0,
                                tyreStintsVisual1,
                                tyreStintsVisual2,
                                tyreStintsVisual3,
                                tyreStintsVisual4,
                                tyreStintsVisual5,
                                tyreStintsVisual6,
                                tyreStintsVisual7,
                                tyreStintsEndLaps0,
                                tyreStintsEndLaps1,
                                tyreStintsEndLaps2,
                                tyreStintsEndLaps3,
                                tyreStintsEndLaps4,
                                tyreStintsEndLaps5,
                                tyreStintsEndLaps6,
                                tyreStintsEndLaps7
                            ) VALUES (
                                @numCars,
                                @position,
                                @numLaps,
                                @gridPosition,
                                @points,
                                @numPitStops,
                                @resultStatus,
                                @bestLapTimeInMS,
                                @totalRaceTime,
                                @penaltiesTime,
                                @numPenalties,
                                @numTyreStints,
                                @tyreStintsActual0,
                                @tyreStintsActual1,
                                @tyreStintsActual2,
                                @tyreStintsActual3,
                                @tyreStintsActual4,
                                @tyreStintsActual5,
                                @tyreStintsActual6,
                                @tyreStintsActual7,
                                @tyreStintsVisual0,
                                @tyreStintsVisual1,
                                @tyreStintsVisual2,
                                @tyreStintsVisual3,
                                @tyreStintsVisual4,
                                @tyreStintsVisual5,
                                @tyreStintsVisual6,
                                @tyreStintsVisual7,
                                @tyreStintsEndLaps0,
                                @tyreStintsEndLaps1,
                                @tyreStintsEndLaps2,
                                @tyreStintsEndLaps3,
                                @tyreStintsEndLaps4,
                                @tyreStintsEndLaps5,
                                @tyreStintsEndLaps6,
                                @tyreStintsEndLaps7
                            );", conn
                        ))
                        {
                            cmd.Parameters.AddWithValue("@numCars", numCars);
                            cmd.Parameters.AddWithValue("@position", dataFinalClassification.position);
                            cmd.Parameters.AddWithValue("@numLaps", dataFinalClassification.numLaps);
                            cmd.Parameters.AddWithValue("@gridPosition", dataFinalClassification.gridPosition);
                            cmd.Parameters.AddWithValue("@points", dataFinalClassification.points);
                            cmd.Parameters.AddWithValue("@numPitStops", dataFinalClassification.numPitStops);
                            cmd.Parameters.AddWithValue("@resultStatus", dataFinalClassification.resultStatus);
                            cmd.Parameters.AddWithValue("@bestLapTimeInMS", dataFinalClassification.bestLapTimeInMS);
                            cmd.Parameters.AddWithValue("@totalRaceTime", dataFinalClassification.totalRaceTime);
                            cmd.Parameters.AddWithValue("@penaltiesTime", dataFinalClassification.penaltiesTime);
                            cmd.Parameters.AddWithValue("@numPenalties", dataFinalClassification.numPenalties);
                            cmd.Parameters.AddWithValue("@numTyreStints", dataFinalClassification.numTyreStints);
                            cmd.Parameters.AddWithValue("@tyreStintsActual0", dataFinalClassification.tyreStintsActual[0]);
                            cmd.Parameters.AddWithValue("@tyreStintsActual1", dataFinalClassification.tyreStintsActual[1]);
                            cmd.Parameters.AddWithValue("@tyreStintsActual2", dataFinalClassification.tyreStintsActual[2]);
                            cmd.Parameters.AddWithValue("@tyreStintsActual3", dataFinalClassification.tyreStintsActual[3]);
                            cmd.Parameters.AddWithValue("@tyreStintsActual4", dataFinalClassification.tyreStintsActual[4]);
                            cmd.Parameters.AddWithValue("@tyreStintsActual5", dataFinalClassification.tyreStintsActual[5]);
                            cmd.Parameters.AddWithValue("@tyreStintsActual6", dataFinalClassification.tyreStintsActual[6]);
                            cmd.Parameters.AddWithValue("@tyreStintsActual7", dataFinalClassification.tyreStintsActual[7]);
                            cmd.Parameters.AddWithValue("@tyreStintsVisual0", dataFinalClassification.tyreStintsVisual[0]);
                            cmd.Parameters.AddWithValue("@tyreStintsVisual1", dataFinalClassification.tyreStintsVisual[1]);
                            cmd.Parameters.AddWithValue("@tyreStintsVisual2", dataFinalClassification.tyreStintsVisual[2]);
                            cmd.Parameters.AddWithValue("@tyreStintsVisual3", dataFinalClassification.tyreStintsVisual[3]);
                            cmd.Parameters.AddWithValue("@tyreStintsVisual4", dataFinalClassification.tyreStintsVisual[4]);
                            cmd.Parameters.AddWithValue("@tyreStintsVisual5", dataFinalClassification.tyreStintsVisual[5]);
                            cmd.Parameters.AddWithValue("@tyreStintsVisual6", dataFinalClassification.tyreStintsVisual[6]);
                            cmd.Parameters.AddWithValue("@tyreStintsVisual7", dataFinalClassification.tyreStintsVisual[7]);
                            cmd.Parameters.AddWithValue("@tyreStintsEndLaps0", dataFinalClassification.tyreStintsEndLaps[0]);
                            cmd.Parameters.AddWithValue("@tyreStintsEndLaps1", dataFinalClassification.tyreStintsEndLaps[1]);
                            cmd.Parameters.AddWithValue("@tyreStintsEndLaps2", dataFinalClassification.tyreStintsEndLaps[2]);
                            cmd.Parameters.AddWithValue("@tyreStintsEndLaps3", dataFinalClassification.tyreStintsEndLaps[3]);
                            cmd.Parameters.AddWithValue("@tyreStintsEndLaps4", dataFinalClassification.tyreStintsEndLaps[4]);
                            cmd.Parameters.AddWithValue("@tyreStintsEndLaps5", dataFinalClassification.tyreStintsEndLaps[5]);
                            cmd.Parameters.AddWithValue("@tyreStintsEndLaps6", dataFinalClassification.tyreStintsEndLaps[6]);
                            cmd.Parameters.AddWithValue("@tyreStintsEndLaps7", dataFinalClassification.tyreStintsEndLaps[7]);
                            try
                            {
                                await cmd.ExecuteNonQueryAsync(stoppingToken);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"FINALCLASSIFICATION INSERT error: {ex.Message}");
                            }
                        }
                        break;
//--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
                    case 1306:
                        //-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------start of LobbyInfo of 1306B
                        var headerLobbyInfo = new Header
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
                        var numPlayers = br.ReadByte();
                        var lobbyInfo = new LobbyInfo[22];
                        for (var i = 0; i < 22; i++) lobbyInfo[i] = ReadLobbyInfo(br);
                        //-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------end
                        var dataLobbyInfo = lobbyInfo[headerLobbyInfo.playerCarIndex];

                        await using (var cmd = new MySqlCommand(@"
                            INSERT INTO LOBBYINFO (
                                numPlayers,
                                aiControlled,
                                teamId,
                                nationality,
                                platform,
                                name,
                                carNumber,
                                yourTelemetry,
                                showOnlineNames,
                                techLevel,
                                readyStatus
                            ) VALUES (
                                @numPlayers,
                                @aiControlled,
                                @teamId,
                                @nationality,
                                @platform,
                                @name,
                                @carNumber,
                                @yourTelemetry,
                                @showOnlineNames,
                                @techLevel,
                                @readyStatus
                            );", conn
                        ))
                        {
                            cmd.Parameters.AddWithValue("@numPlayers", numPlayers);
                            cmd.Parameters.AddWithValue("@aiControlled", dataLobbyInfo.aiControlled);
                            cmd.Parameters.AddWithValue("@teamId", dataLobbyInfo.teamId);
                            cmd.Parameters.AddWithValue("@nationality", dataLobbyInfo.nationality);
                            cmd.Parameters.AddWithValue("@platform", dataLobbyInfo.platform);
                            var decodedLobbyInfoName = System.Text.Encoding.UTF8.GetString(dataLobbyInfo.name).TrimEnd('\0');
                            cmd.Parameters.AddWithValue("@name", decodedLobbyInfoName);
                            cmd.Parameters.AddWithValue("@carNumber", dataLobbyInfo.carNumber);
                            cmd.Parameters.AddWithValue("@yourTelemetry", dataLobbyInfo.yourTelemetry);
                            cmd.Parameters.AddWithValue("@showOnlineNames", dataLobbyInfo.showOnlineNames);
                            cmd.Parameters.AddWithValue("@techLevel", dataLobbyInfo.techLevel);
                            cmd.Parameters.AddWithValue("@readyStatus", dataLobbyInfo.readyStatus);
                            try
                            {
                                await cmd.ExecuteNonQueryAsync(stoppingToken);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"LOBBYINFO INSERT error: {ex.Message}");
                            }
                        }
                        break;
//--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
                    case 953:
                        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------start of CarDamage of 953B
                        var headerCarDamage = new Header
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
                        var carDamage = new CarDamage[22];
                        for (var i = 0; i < 22; i++) carDamage[i] = ReadCarDamage(br);
                        //-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------end
                        var dataCarDamage = carDamage[headerCarDamage.playerCarIndex];

                        await using (var cmd = new MySqlCommand(@"
                            INSERT INTO CARDAMAGE (
                                tyresWear0,
                                tyresWear1,
                                tyresWear2,
                                tyresWear3,
                                tyresDamage0,
                                tyresDamage1,
                                tyresDamage2,
                                tyresDamage3,
                                brakesDamage0,
                                brakesDamage1,
                                brakesDamage2,
                                brakesDamage3,
                                frontLeftWingDamage,
                                frontRightWingDamage,
                                rearWingDamage,
                                floorDamage,
                                diffuserDamage,
                                sidepodDamage,
                                drsFault,
                                ersFault,
                                gearBoxDamage,
                                engineDamage,
                                engineMGUHWear,
                                engineESWear,
                                engineCEWear,
                                engineICEWear,
                                engineMGUKWear,
                                engineTCWear,
                                engineBlown,
                                engineSeized
                            ) VALUES (
                                @tyresWear0,
                                @tyresWear1,
                                @tyresWear2,
                                @tyresWear3,
                                @tyresDamage0,
                                @tyresDamage1,
                                @tyresDamage2,
                                @tyresDamage3,
                                @brakesDamage0,
                                @brakesDamage1,
                                @brakesDamage2,
                                @brakesDamage3,
                                @frontLeftWingDamage,
                                @frontRightWingDamage,
                                @rearWingDamage,
                                @floorDamage,
                                @diffuserDamage,
                                @sidepodDamage,
                                @drsFault,
                                @ersFault,
                                @gearBoxDamage,
                                @engineDamage,
                                @engineMGUHWear,
                                @engineESWear,
                                @engineCEWear,
                                @engineICEWear,
                                @engineMGUKWear,
                                @engineTCWear,
                                @engineBlown,
                                @engineSeized
                            );", conn
                        ))
                        {
                            cmd.Parameters.AddWithValue("@tyresWear0", dataCarDamage.tyresWear[0]);
                            cmd.Parameters.AddWithValue("@tyresWear1", dataCarDamage.tyresWear[1]);
                            cmd.Parameters.AddWithValue("@tyresWear2", dataCarDamage.tyresWear[2]);
                            cmd.Parameters.AddWithValue("@tyresWear3", dataCarDamage.tyresWear[3]);
                            cmd.Parameters.AddWithValue("@tyresDamage0", dataCarDamage.tyresDamage[0]);
                            cmd.Parameters.AddWithValue("@tyresDamage1", dataCarDamage.tyresDamage[1]);
                            cmd.Parameters.AddWithValue("@tyresDamage2", dataCarDamage.tyresDamage[2]);
                            cmd.Parameters.AddWithValue("@tyresDamage3", dataCarDamage.tyresDamage[3]);
                            cmd.Parameters.AddWithValue("@brakesDamage0", dataCarDamage.brakesDamage[0]);
                            cmd.Parameters.AddWithValue("@brakesDamage1", dataCarDamage.brakesDamage[1]);
                            cmd.Parameters.AddWithValue("@brakesDamage2", dataCarDamage.brakesDamage[2]);
                            cmd.Parameters.AddWithValue("@brakesDamage3", dataCarDamage.brakesDamage[3]);
                            cmd.Parameters.AddWithValue("@frontLeftWingDamage", dataCarDamage.frontLeftWingDamage);
                            cmd.Parameters.AddWithValue("@frontRightWingDamage", dataCarDamage.frontRightWingDamage);
                            cmd.Parameters.AddWithValue("@rearWingDamage", dataCarDamage.rearWingDamage);
                            cmd.Parameters.AddWithValue("@floorDamage", dataCarDamage.floorDamage);
                            cmd.Parameters.AddWithValue("@diffuserDamage", dataCarDamage.diffuserDamage);
                            cmd.Parameters.AddWithValue("@sidepodDamage", dataCarDamage.sidepodDamage);
                            cmd.Parameters.AddWithValue("@drsFault", dataCarDamage.drsFault);
                            cmd.Parameters.AddWithValue("@ersFault", dataCarDamage.ersFault);
                            cmd.Parameters.AddWithValue("@gearBoxDamage", dataCarDamage.gearBoxDamage);
                            cmd.Parameters.AddWithValue("@engineDamage", dataCarDamage.engineDamage);
                            cmd.Parameters.AddWithValue("@engineMGUHWear", dataCarDamage.engineMGUHWear);
                            cmd.Parameters.AddWithValue("@engineESWear", dataCarDamage.engineESWear);
                            cmd.Parameters.AddWithValue("@engineCEWear", dataCarDamage.engineCEWear);
                            cmd.Parameters.AddWithValue("@engineICEWear", dataCarDamage.engineICEWear);
                            cmd.Parameters.AddWithValue("@engineMGUKWear", dataCarDamage.engineMGUKWear);
                            cmd.Parameters.AddWithValue("@engineTCWear", dataCarDamage.engineTCWear);
                            cmd.Parameters.AddWithValue("@engineBlown", dataCarDamage.engineBlown);
                            cmd.Parameters.AddWithValue("@engineSeized", dataCarDamage.engineSeized);
                            try
                            {
                                await cmd.ExecuteNonQueryAsync(stoppingToken);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"CARDAMAGE INSERT error: {ex.Message}");
                            }
                        }
                        break;
//--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
                    case 1460:
                        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------start of SessionHistory of 1460B
                        var dataSessionHistory = ReadSessionHistory(br);
                        //-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------end
                        await using (var cmd = new MySqlCommand(@"
                            INSERT INTO SESSIONHISTORY (
                                carIdx,
                                numLaps,
                                numTyreStints,
                                bestLapTimeLapNum,
                                bestSector1LapNum,
                                bestSector2LapNum,
                                bestSector3LapNum,
                                tyreStintHistory0_endLap, tyreStintHistory0_tyreActualCompound, tyreStintHistory0_tyreVisualCompound,
                                tyreStintHistory1_endLap, tyreStintHistory1_tyreActualCompound, tyreStintHistory1_tyreVisualCompound,
                                tyreStintHistory2_endLap, tyreStintHistory2_tyreActualCompound, tyreStintHistory2_tyreVisualCompound,
                                tyreStintHistory3_endLap, tyreStintHistory3_tyreActualCompound, tyreStintHistory3_tyreVisualCompound,
                                tyreStintHistory4_endLap, tyreStintHistory4_tyreActualCompound, tyreStintHistory4_tyreVisualCompound,
                                tyreStintHistory5_endLap, tyreStintHistory5_tyreActualCompound, tyreStintHistory5_tyreVisualCompound,
                                tyreStintHistory6_endLap, tyreStintHistory6_tyreActualCompound, tyreStintHistory6_tyreVisualCompound,
                                tyreStintHistory7_endLap, tyreStintHistory7_tyreActualCompound, tyreStintHistory7_tyreVisualCompound
                            ) VALUES (
                                @carIdx,
                                @numLaps,
                                @numTyreStints,
                                @bestLapTimeLapNum,
                                @bestSector1LapNum,
                                @bestSector2LapNum,
                                @bestSector3LapNum,
                                @tyreStintHistory0_endLap, @tyreStintHistory0_tyreActualCompound, @tyreStintHistory0_tyreVisualCompound,
                                @tyreStintHistory1_endLap, @tyreStintHistory1_tyreActualCompound, @tyreStintHistory1_tyreVisualCompound,
                                @tyreStintHistory2_endLap, @tyreStintHistory2_tyreActualCompound, @tyreStintHistory2_tyreVisualCompound,
                                @tyreStintHistory3_endLap, @tyreStintHistory3_tyreActualCompound, @tyreStintHistory3_tyreVisualCompound,
                                @tyreStintHistory4_endLap, @tyreStintHistory4_tyreActualCompound, @tyreStintHistory4_tyreVisualCompound,
                                @tyreStintHistory5_endLap, @tyreStintHistory5_tyreActualCompound, @tyreStintHistory5_tyreVisualCompound,
                                @tyreStintHistory6_endLap, @tyreStintHistory6_tyreActualCompound, @tyreStintHistory6_tyreVisualCompound,
                                @tyreStintHistory7_endLap, @tyreStintHistory7_tyreActualCompound, @tyreStintHistory7_tyreVisualCompound
                            );", conn
                        ))
                        {
                            cmd.Parameters.AddWithValue("@carIdx", dataSessionHistory.carIdx);
                            cmd.Parameters.AddWithValue("@numLaps", dataSessionHistory.numLaps);
                            cmd.Parameters.AddWithValue("@numTyreStints", dataSessionHistory.numTyreStints);
                            cmd.Parameters.AddWithValue("@bestLapTimeLapNum", dataSessionHistory.bestLapTimeLapNum);
                            cmd.Parameters.AddWithValue("@bestSector1LapNum", dataSessionHistory.bestSector1LapNum);
                            cmd.Parameters.AddWithValue("@bestSector2LapNum", dataSessionHistory.bestSector2LapNum);
                            cmd.Parameters.AddWithValue("@bestSector3LapNum", dataSessionHistory.bestSector3LapNum);
                            cmd.Parameters.AddWithValue("@tyreStintHistory0_endLap", dataSessionHistory.tyreStintHistory[0].endLap);
                            cmd.Parameters.AddWithValue("@tyreStintHistory0_tyreActualCompound", dataSessionHistory.tyreStintHistory[0].tyreActualCompound);
                            cmd.Parameters.AddWithValue("@tyreStintHistory0_tyreVisualCompound", dataSessionHistory.tyreStintHistory[0].tyreVisualCompound);
                            cmd.Parameters.AddWithValue("@tyreStintHistory1_endLap", dataSessionHistory.tyreStintHistory[1].endLap);
                            cmd.Parameters.AddWithValue("@tyreStintHistory1_tyreActualCompound", dataSessionHistory.tyreStintHistory[1].tyreActualCompound);
                            cmd.Parameters.AddWithValue("@tyreStintHistory1_tyreVisualCompound", dataSessionHistory.tyreStintHistory[1].tyreVisualCompound);
                            cmd.Parameters.AddWithValue("@tyreStintHistory2_endLap", dataSessionHistory.tyreStintHistory[2].endLap);
                            cmd.Parameters.AddWithValue("@tyreStintHistory2_tyreActualCompound", dataSessionHistory.tyreStintHistory[2].tyreActualCompound);
                            cmd.Parameters.AddWithValue("@tyreStintHistory2_tyreVisualCompound", dataSessionHistory.tyreStintHistory[2].tyreVisualCompound);
                            cmd.Parameters.AddWithValue("@tyreStintHistory3_endLap", dataSessionHistory.tyreStintHistory[3].endLap);
                            cmd.Parameters.AddWithValue("@tyreStintHistory3_tyreActualCompound", dataSessionHistory.tyreStintHistory[3].tyreActualCompound);
                            cmd.Parameters.AddWithValue("@tyreStintHistory3_tyreVisualCompound", dataSessionHistory.tyreStintHistory[3].tyreVisualCompound);
                            cmd.Parameters.AddWithValue("@tyreStintHistory4_endLap", dataSessionHistory.tyreStintHistory[4].endLap);
                            cmd.Parameters.AddWithValue("@tyreStintHistory4_tyreActualCompound", dataSessionHistory.tyreStintHistory[4].tyreActualCompound);
                            cmd.Parameters.AddWithValue("@tyreStintHistory4_tyreVisualCompound", dataSessionHistory.tyreStintHistory[4].tyreVisualCompound);
                            cmd.Parameters.AddWithValue("@tyreStintHistory5_endLap",dataSessionHistory.tyreStintHistory[5].endLap);
                            cmd.Parameters.AddWithValue("@tyreStintHistory5_tyreActualCompound", dataSessionHistory.tyreStintHistory[5].tyreActualCompound);
                            cmd.Parameters.AddWithValue("@tyreStintHistory5_tyreVisualCompound", dataSessionHistory.tyreStintHistory[5].tyreVisualCompound);
                            cmd.Parameters.AddWithValue("@tyreStintHistory6_endLap", dataSessionHistory.tyreStintHistory[6].endLap);
                            cmd.Parameters.AddWithValue("@tyreStintHistory6_tyreActualCompound", dataSessionHistory.tyreStintHistory[6].tyreActualCompound);
                            cmd.Parameters.AddWithValue("@tyreStintHistory6_tyreVisualCompound", dataSessionHistory.tyreStintHistory[6].tyreVisualCompound);
                            cmd.Parameters.AddWithValue("@tyreStintHistory7_endLap", dataSessionHistory.tyreStintHistory[7].endLap);
                            cmd.Parameters.AddWithValue("@tyreStintHistory7_tyreActualCompound", dataSessionHistory.tyreStintHistory[7].tyreActualCompound);
                            cmd.Parameters.AddWithValue("@tyreStintHistory7_tyreVisualCompound", dataSessionHistory.tyreStintHistory[7].tyreVisualCompound);
                            try
                            {
                                await cmd.ExecuteNonQueryAsync(stoppingToken);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"TYRESET INSERT error: {ex.Message}");
                            }
                        }
                        for (var i = 0; i < 8; i++)
                        {
                            await using (var cmd = new MySqlCommand(@"
                                INSERT INTO SESSIONHISTORYLAPHISTORY (
                                    lapTimeInMS,
                                    sector1TimeInMS,
                                    sector1TimeMinutes,
                                    sector2TimeInMS,
                                    sector2TimeMinutes,
                                    sector3TimeInMS,
                                    sector3TimeMinutes,
                                    lapValidBitFlags
                                ) VALUES (
                                    @lapTimeInMS,
                                    @sector1TimeInMS,
                                    @sector1TimeMinutes,
                                    @sector2TimeInMS,
                                    @sector2TimeMinutes,
                                    @sector3TimeInMS,
                                    @sector3TimeMinutes,
                                    @lapValidBitFlags
                                );", conn
                            ))
                            {
                                cmd.Parameters.AddWithValue("@lapTimeInMS", dataSessionHistory.lapHistory[i].lapTimeInMS);
                                cmd.Parameters.AddWithValue("@sector1TimeInMS", dataSessionHistory.lapHistory[i].sector1TimeInMS);
                                cmd.Parameters.AddWithValue("@sector1TimeMinutes", dataSessionHistory.lapHistory[i].sector1TimeMinutes);
                                cmd.Parameters.AddWithValue("@sector2TimeInMS", dataSessionHistory.lapHistory[i].sector2TimeInMS);
                                cmd.Parameters.AddWithValue("@sector2TimeMinutes", dataSessionHistory.lapHistory[i].sector2TimeMinutes);
                                cmd.Parameters.AddWithValue("@sector3TimeInMS", dataSessionHistory.lapHistory[i].sector3TimeInMS);
                                cmd.Parameters.AddWithValue("@sector3TimeMinutes", dataSessionHistory.lapHistory[i].sector3TimeMinutes);
                                cmd.Parameters.AddWithValue("@lapValidBitFlags", dataSessionHistory.lapHistory[i].lapValidBitFlags);
                                try
                                {
                                    await cmd.ExecuteNonQueryAsync(stoppingToken);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"TYRESET INSERT error: {ex.Message}");
                                }
                            }
                        }
                        break;
//--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
                    case 231:
                        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------start of TyreSet of 231B
                        var headerTyreSet = new Header
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
                        var carIdx = br.ReadByte();
                        var tyreSet = new TyreSet[20];
                        for (var i = 0; i < 20; i++) tyreSet[i] = ReadTyreSet(br);
                        var fittedIdx = br.ReadByte();
                        //-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------end
                        if (carIdx == headerTyreSet.playerCarIndex)
                        {
                            for (var i = 0; i < 20; i++)
                            {
                                await using var cmd = new MySqlCommand(@"
                                    INSERT INTO TYRESET (
                                        carIdx,
                                        actualTyreCompound,
                                        visualTyreCompound,
                                        wear,
                                        available,
                                        recommendedSession,
                                        lifeSpan,
                                        usableLife,
                                        lapDeltaTime,
                                        fitted,
                                        fittedIdx
                                    ) VALUES (
                                        @carIdx,
                                        @actualTyreCompound,
                                        @visualTyreCompound,
                                        @wear,
                                        @available,
                                        @recommendedSession,
                                        @lifeSpan,
                                        @usableLife,
                                        @lapDeltaTime,
                                        @fitted,
                                        @fittedIdx
                                    );", conn
                                );
                                cmd.Parameters.AddWithValue("@carIdx", carIdx);
                                cmd.Parameters.AddWithValue("@actualTyreCompound", tyreSet[i].actualTyreCompound);
                                cmd.Parameters.AddWithValue("@visualTyreCompound", tyreSet[i].visualTyreCompound);
                                cmd.Parameters.AddWithValue("@wear", tyreSet[i].wear);
                                cmd.Parameters.AddWithValue("@available", tyreSet[i].available);
                                cmd.Parameters.AddWithValue("@recommendedSession", tyreSet[i].recommendedSession);
                                cmd.Parameters.AddWithValue("@lifeSpan", tyreSet[i].lifeSpan);
                                cmd.Parameters.AddWithValue("@usableLife", tyreSet[i].usableLife);
                                cmd.Parameters.AddWithValue("@lapDeltaTime", tyreSet[i].lapDeltaTime);
                                cmd.Parameters.AddWithValue("@fitted", tyreSet[i].fitted);
                                cmd.Parameters.AddWithValue("@fittedIdx", fittedIdx);
                                try
                                {
                                    await cmd.ExecuteNonQueryAsync(stoppingToken);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"TYRESET {i} INSERT error: {ex.Message}");
                                }
                            }
                        }
                        break;
//--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
                    case 237:
                        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------start of MotionEx of 237B
                        var dataMotionEx = ReadMotionEx(br);
                        //-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------end
                        await using (var cmd = new MySqlCommand(@"
                            INSERT INTO MOTIONEX (
                                suspensionPosition0, suspensionPosition1, suspensionPosition2, suspensionPosition3,
                                suspensionVelocity0, suspensionVelocity1, suspensionVelocity2, suspensionVelocity3,
                                suspensionAcceleration0, suspensionAcceleration1, suspensionAcceleration2, suspensionAcceleration3,
                                wheelSpeed0, wheelSpeed1, wheelSpeed2, wheelSpeed3,
                                wheelSlipRatio0, wheelSlipRatio1, wheelSlipRatio2, wheelSlipRatio3,
                                wheelSlipAngle0, wheelSlipAngle1, wheelSlipAngle2, wheelSlipAngle3,
                                wheelLatForce0, wheelLatForce1, wheelLatForce2, wheelLatForce3,
                                wheelLongForce0, wheelLongForce1, wheelLongForce2, wheelLongForce3,
                                heightOfCOGAboveGround,
                                localVelocityX, localVelocityY, localVelocityZ,
                                angularVelocityX, angularVelocityY, angularVelocityZ,
                                angularAccelerationX, angularAccelerationY, angularAccelerationZ,
                                frontWheelsAngle,
                                wheelVertForce0, wheelVertForce1, wheelVertForce2, wheelVertForce3,
                                frontAeroHeight, rearAeroHeight,
                                frontRollAngle, rearRollAngle,
                                chassisYaw
                            ) VALUES (
                                @suspensionPosition0, @suspensionPosition1, @suspensionPosition2, @suspensionPosition3,
                                @suspensionVelocity0, @suspensionVelocity1, @suspensionVelocity2, @suspensionVelocity3,
                                @suspensionAcceleration0, @suspensionAcceleration1, @suspensionAcceleration2, @suspensionAcceleration3,
                                @wheelSpeed0, @wheelSpeed1, @wheelSpeed2, @wheelSpeed3,
                                @wheelSlipRatio0, @wheelSlipRatio1, @wheelSlipRatio2, @wheelSlipRatio3,
                                @wheelSlipAngle0, @wheelSlipAngle1, @wheelSlipAngle2, @wheelSlipAngle3,
                                @wheelLatForce0, @wheelLatForce1, @wheelLatForce2, @wheelLatForce3,
                                @wheelLongForce0, @wheelLongForce1, @wheelLongForce2, @wheelLongForce3,
                                @heightOfCOGAboveGround,
                                @localVelocityX, @localVelocityY, @localVelocityZ,
                                @angularVelocityX, @angularVelocityY, @angularVelocityZ,
                                @angularAccelerationX, @angularAccelerationY, @angularAccelerationZ,
                                @frontWheelsAngle,
                                @wheelVertForce0, @wheelVertForce1, @wheelVertForce2, @wheelVertForce3,
                                @frontAeroHeight, @rearAeroHeight,
                                @frontRollAngle, @rearRollAngle,
                                @chassisYaw
                            );", conn
                        ))
                        {
                            cmd.Parameters.AddWithValue("@suspensionPosition0", dataMotionEx.suspensionPosition[0]);
                            cmd.Parameters.AddWithValue("@suspensionPosition1", dataMotionEx.suspensionPosition[1]);
                            cmd.Parameters.AddWithValue("@suspensionPosition2", dataMotionEx.suspensionPosition[2]);
                            cmd.Parameters.AddWithValue("@suspensionPosition3", dataMotionEx.suspensionPosition[3]);
                            cmd.Parameters.AddWithValue("@suspensionVelocity0", dataMotionEx.suspensionVelocity[0]);
                            cmd.Parameters.AddWithValue("@suspensionVelocity1", dataMotionEx.suspensionVelocity[1]);
                            cmd.Parameters.AddWithValue("@suspensionVelocity2", dataMotionEx.suspensionVelocity[2]);
                            cmd.Parameters.AddWithValue("@suspensionVelocity3", dataMotionEx.suspensionVelocity[3]);
                            cmd.Parameters.AddWithValue("@suspensionAcceleration0", dataMotionEx.suspensionAcceleration[0]);
                            cmd.Parameters.AddWithValue("@suspensionAcceleration1", dataMotionEx.suspensionAcceleration[1]);
                            cmd.Parameters.AddWithValue("@suspensionAcceleration2", dataMotionEx.suspensionAcceleration[2]);
                            cmd.Parameters.AddWithValue("@suspensionAcceleration3", dataMotionEx.suspensionAcceleration[3]);
                            cmd.Parameters.AddWithValue("@wheelSpeed0", dataMotionEx.wheelSpeed[0]);
                            cmd.Parameters.AddWithValue("@wheelSpeed1", dataMotionEx.wheelSpeed[1]);
                            cmd.Parameters.AddWithValue("@wheelSpeed2", dataMotionEx.wheelSpeed[2]);
                            cmd.Parameters.AddWithValue("@wheelSpeed3", dataMotionEx.wheelSpeed[3]);
                            cmd.Parameters.AddWithValue("@wheelSlipRatio0", dataMotionEx.wheelSlipRatio[0]);
                            cmd.Parameters.AddWithValue("@wheelSlipRatio1", dataMotionEx.wheelSlipRatio[1]);
                            cmd.Parameters.AddWithValue("@wheelSlipRatio2", dataMotionEx.wheelSlipRatio[2]);
                            cmd.Parameters.AddWithValue("@wheelSlipRatio3", dataMotionEx.wheelSlipRatio[3]);
                            cmd.Parameters.AddWithValue("@wheelSlipAngle0", dataMotionEx.wheelSlipAngle[0]);
                            cmd.Parameters.AddWithValue("@wheelSlipAngle1", dataMotionEx.wheelSlipAngle[1]);
                            cmd.Parameters.AddWithValue("@wheelSlipAngle2", dataMotionEx.wheelSlipAngle[2]);
                            cmd.Parameters.AddWithValue("@wheelSlipAngle3", dataMotionEx.wheelSlipAngle[3]);
                            cmd.Parameters.AddWithValue("@wheelLatForce0", dataMotionEx.wheelLatForce[0]);
                            cmd.Parameters.AddWithValue("@wheelLatForce1", dataMotionEx.wheelLatForce[1]);
                            cmd.Parameters.AddWithValue("@wheelLatForce2", dataMotionEx.wheelLatForce[2]);
                            cmd.Parameters.AddWithValue("@wheelLatForce3", dataMotionEx.wheelLatForce[3]);
                            cmd.Parameters.AddWithValue("@wheelLongForce0", dataMotionEx.wheelLongForce[0]);
                            cmd.Parameters.AddWithValue("@wheelLongForce1", dataMotionEx.wheelLongForce[1]);
                            cmd.Parameters.AddWithValue("@wheelLongForce2", dataMotionEx.wheelLongForce[2]);
                            cmd.Parameters.AddWithValue("@wheelLongForce3", dataMotionEx.wheelLongForce[3]);
                            cmd.Parameters.AddWithValue("@heightOfCOGAboveGround", dataMotionEx.heightOfCOGAboveGround);
                            cmd.Parameters.AddWithValue("@localVelocityX", dataMotionEx.localVelocityX);
                            cmd.Parameters.AddWithValue("@localVelocityY", dataMotionEx.localVelocityY);
                            cmd.Parameters.AddWithValue("@localVelocityZ", dataMotionEx.localVelocityZ);
                            cmd.Parameters.AddWithValue("@angularVelocityX", dataMotionEx.angularVelocityX);
                            cmd.Parameters.AddWithValue("@angularVelocityY", dataMotionEx.angularVelocityY);
                            cmd.Parameters.AddWithValue("@angularVelocityZ", dataMotionEx.angularVelocityZ);
                            cmd.Parameters.AddWithValue("@angularAccelerationX", dataMotionEx.angularAccelerationX);
                            cmd.Parameters.AddWithValue("@angularAccelerationY", dataMotionEx.angularAccelerationY);
                            cmd.Parameters.AddWithValue("@angularAccelerationZ", dataMotionEx.angularAccelerationZ);
                            cmd.Parameters.AddWithValue("@frontWheelsAngle", dataMotionEx.frontWheelsAngle);
                            cmd.Parameters.AddWithValue("@wheelVertForce0", dataMotionEx.wheelVertForce[0]);
                            cmd.Parameters.AddWithValue("@wheelVertForce1", dataMotionEx.wheelVertForce[1]);
                            cmd.Parameters.AddWithValue("@wheelVertForce2", dataMotionEx.wheelVertForce[2]);
                            cmd.Parameters.AddWithValue("@wheelVertForce3", dataMotionEx.wheelVertForce[3]);
                            cmd.Parameters.AddWithValue("@frontAeroHeight", dataMotionEx.frontAeroHeight);
                            cmd.Parameters.AddWithValue("@rearAeroHeight", dataMotionEx.rearAeroHeight);
                            cmd.Parameters.AddWithValue("@frontRollAngle", dataMotionEx.frontRollAngle);
                            cmd.Parameters.AddWithValue("@rearRollAngle", dataMotionEx.rearRollAngle);
                            cmd.Parameters.AddWithValue("@chassisYaw", dataMotionEx.chassisYaw);
                            try
                            {
                                await cmd.ExecuteNonQueryAsync(stoppingToken);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"MOTIONEX INSERT error: {ex.Message}");
                            }
                        }
                        break;
//--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
                    case 101:
                        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------start of TimeTrial of 101B
                        br.ReadUInt16(); //packetFormat
                        br.ReadByte(); //gameYear
                        br.ReadByte(); //gameMajorVersion
                        br.ReadByte(); //gameMinorVersion
                        br.ReadByte(); //packetVersion
                        br.ReadByte(); //packetId
                        br.ReadUInt64(); //sessionUID
                        br.ReadSingle(); //sessionTime
                        br.ReadUInt32(); //frameIdentifier
                        br.ReadUInt32(); //overallFrameIdentifier
                        br.ReadByte(); //playerCarIndex
                        br.ReadByte(); //secondaryPlayerCarIndex
                        var dataPlayerSessionBestDataSet = ReadTimeTrial(br);
                        var dataPersonalBestDataSet = ReadTimeTrial(br);
                        ReadTimeTrial(br); //dataRivalDataSet
                        //-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------end
                        await using (var cmd = new MySqlCommand(@"
                            INSERT INTO TIMETRIAL (
                                playerSessionBestDataSet_carIdx,
                                playerSessionBestDataSet_teamId,
                                playerSessionBestDataSet_lapTimeInMS,
                                playerSessionBestDataSet_sector1TimeInMS,
                                playerSessionBestDataSet_sector2TimeInMS,
                                playerSessionBestDataSet_sector3TimeInMS,
                                playerSessionBestDataSet_tractionControl,
                                playerSessionBestDataSet_gearboxAssist,
                                playerSessionBestDataSet_antiLockBrakes,
                                playerSessionBestDataSet_equalCarPerformance,
                                playerSessionBestDataSet_customSetup,
                                playerSessionBestDataSet_valid,
                                personalBestDataSet_carIdx,
                                personalBestDataSet_teamId,
                                personalBestDataSet_lapTimeInMS,
                                personalBestDataSet_sector1TimeInMS,
                                personalBestDataSet_sector2TimeInMS,
                                personalBestDataSet_sector3TimeInMS,
                                personalBestDataSet_tractionControl,
                                personalBestDataSet_gearboxAssist,
                                personalBestDataSet_antiLockBrakes,
                                personalBestDataSet_equalCarPerformance,
                                personalBestDataSet_customSetup,
                                personalBestDataSet_valid
                            ) VALUES (
                                @playerSessionBestDataSet_carIdx,
                                @playerSessionBestDataSet_teamId,
                                @playerSessionBestDataSet_lapTimeInMS,
                                @playerSessionBestDataSet_sector1TimeInMS,
                                @playerSessionBestDataSet_sector2TimeInMS,
                                @playerSessionBestDataSet_sector3TimeInMS,
                                @playerSessionBestDataSet_tractionControl,
                                @playerSessionBestDataSet_gearboxAssist,
                                @playerSessionBestDataSet_antiLockBrakes,
                                @playerSessionBestDataSet_equalCarPerformance,
                                @playerSessionBestDataSet_customSetup,
                                @playerSessionBestDataSet_valid,
                                @personalBestDataSet_carIdx,
                                @personalBestDataSet_teamId,
                                @personalBestDataSet_lapTimeInMS,
                                @personalBestDataSet_sector1TimeInMS,
                                @personalBestDataSet_sector2TimeInMS,
                                @personalBestDataSet_sector3TimeInMS,
                                @personalBestDataSet_tractionControl,
                                @personalBestDataSet_gearboxAssist,
                                @personalBestDataSet_antiLockBrakes,
                                @personalBestDataSet_equalCarPerformance,
                                @personalBestDataSet_customSetup,
                                @personalBestDataSet_valid
                            );", conn
                        ))
                        {
                            cmd.Parameters.AddWithValue("@playerSessionBestDataSet_carIdx", dataPlayerSessionBestDataSet.carIdx);
                            cmd.Parameters.AddWithValue("@playerSessionBestDataSet_teamId", dataPlayerSessionBestDataSet.teamId);
                            cmd.Parameters.AddWithValue("@playerSessionBestDataSet_lapTimeInMS", dataPlayerSessionBestDataSet.lapTimeInMS);
                            cmd.Parameters.AddWithValue("@playerSessionBestDataSet_sector1TimeInMS", dataPlayerSessionBestDataSet.sector1TimeInMS);
                            cmd.Parameters.AddWithValue("@playerSessionBestDataSet_sector2TimeInMS", dataPlayerSessionBestDataSet.sector2TimeInMS);
                            cmd.Parameters.AddWithValue("@playerSessionBestDataSet_sector3TimeInMS", dataPlayerSessionBestDataSet.sector3TimeInMS);
                            cmd.Parameters.AddWithValue("@playerSessionBestDataSet_tractionControl", dataPlayerSessionBestDataSet.tractionControl);
                            cmd.Parameters.AddWithValue("@playerSessionBestDataSet_gearboxAssist", dataPlayerSessionBestDataSet.gearboxAssist);
                            cmd.Parameters.AddWithValue("@playerSessionBestDataSet_antiLockBrakes", dataPlayerSessionBestDataSet.antiLockBrakes);
                            cmd.Parameters.AddWithValue("@playerSessionBestDataSet_equalCarPerformance", dataPlayerSessionBestDataSet.equalCarPerformance);
                            cmd.Parameters.AddWithValue("@playerSessionBestDataSet_customSetup", dataPlayerSessionBestDataSet.customSetup);
                            cmd.Parameters.AddWithValue("@playerSessionBestDataSet_valid", dataPlayerSessionBestDataSet.valid);
                            cmd.Parameters.AddWithValue("@personalBestDataSet_carIdx", dataPersonalBestDataSet.carIdx);
                            cmd.Parameters.AddWithValue("@personalBestDataSet_teamId", dataPersonalBestDataSet.teamId);
                            cmd.Parameters.AddWithValue("@personalBestDataSet_lapTimeInMS", dataPersonalBestDataSet.lapTimeInMS);
                            cmd.Parameters.AddWithValue("@personalBestDataSet_sector1TimeInMS", dataPersonalBestDataSet.sector1TimeInMS);
                            cmd.Parameters.AddWithValue("@personalBestDataSet_sector2TimeInMS", dataPersonalBestDataSet.sector2TimeInMS);
                            cmd.Parameters.AddWithValue("@personalBestDataSet_sector3TimeInMS", dataPersonalBestDataSet.sector3TimeInMS);
                            cmd.Parameters.AddWithValue("@personalBestDataSet_tractionControl", dataPersonalBestDataSet.tractionControl);
                            cmd.Parameters.AddWithValue("@personalBestDataSet_gearboxAssist", dataPersonalBestDataSet.gearboxAssist);
                            cmd.Parameters.AddWithValue("@personalBestDataSet_antiLockBrakes", dataPersonalBestDataSet.antiLockBrakes);
                            cmd.Parameters.AddWithValue("@personalBestDataSet_equalCarPerformance", dataPersonalBestDataSet.equalCarPerformance);
                            cmd.Parameters.AddWithValue("@personalBestDataSet_customSetup", dataPersonalBestDataSet.customSetup);
                            cmd.Parameters.AddWithValue("@personalBestDataSet_valid", dataPersonalBestDataSet.valid);
                            try
                            {
                                await cmd.ExecuteNonQueryAsync(stoppingToken);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"TIMETRIAL INSERT error: {ex.Message}");
                            }
                        }
                        break;
                }
            }
        }
//-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------Readers
        private static CarMotion ReadCarMotion(BinaryReader br)
        {
            CarMotion carMotion = new();
            try
            {
                carMotion.worldPositionX = br.ReadSingle();
                carMotion.worldPositionY = br.ReadSingle();
                carMotion.worldPositionZ = br.ReadSingle();
                carMotion.worldVelocityX = br.ReadSingle();
                carMotion.worldVelocityY = br.ReadSingle();
                carMotion.worldVelocityZ = br.ReadSingle();
                carMotion.worldForwardDirX = br.ReadInt16();
                carMotion.worldForwardDirY = br.ReadInt16();
                carMotion.worldForwardDirZ = br.ReadInt16();
                carMotion.worldRightDirX = br.ReadInt16();
                carMotion.worldRightDirY = br.ReadInt16();
                carMotion.worldRightDirZ = br.ReadInt16();
                carMotion.gForceLateral = br.ReadSingle();
                carMotion.gForceLongitudinal = br.ReadSingle();
                carMotion.gForceVertical = br.ReadSingle();
                carMotion.yaw = br.ReadSingle();
                carMotion.pitch = br.ReadSingle();
                carMotion.roll = br.ReadSingle();
            }
            catch (EndOfStreamException)
            {
                Console.WriteLine("carMotion Packet unreadable");
            }
            return carMotion;
        }
//--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private static Session ReadSession(BinaryReader br)
        {
            Session session = new();
            try
            {
                session.weather = br.ReadByte();
                session.trackTemperature = br.ReadSByte();
                session.airTemperature = br.ReadSByte();
                session.totalLaps = br.ReadByte();
                session.trackLength = br.ReadUInt16();
                session.sessionType = br.ReadByte();
                session.trackId = br.ReadSByte();
                session.formula = br.ReadByte();
                session.sessionTimeLeft = br.ReadUInt16();
                session.sessionDuration = br.ReadUInt16();
                session.pitSpeedLimit = br.ReadByte();
                session.gamePaused = br.ReadByte();
                session.isSpectating = br.ReadByte();
                session.spectatorCarIndex = br.ReadByte();
                session.sliProNativeSupport = br.ReadByte();
                session.numMarshalZones = br.ReadByte();
                session.marshalZones = new MarshalZone[21];
                for (var i = 0; i < 21; i++)
                {
                    session.marshalZones[i].zoneStart = br.ReadSingle();
                    session.marshalZones[i].zoneFlag = br.ReadSByte();
                }
                session.safetyCarStatus = br.ReadByte();
                session.networkGame = br.ReadByte();
                session.numWeatherForecastSamples = br.ReadByte();
                session.weatherForecastSamples = new WeatherForecastSample[64];
                for (var i = 0; i < 64; i++)
                {
                    session.weatherForecastSamples[i].sessionType = br.ReadByte();
                    session.weatherForecastSamples[i].timeOffset = br.ReadByte();
                    session.weatherForecastSamples[i].weather = br.ReadByte();
                    session.weatherForecastSamples[i].trackTemperature = br.ReadSByte();
                    session.weatherForecastSamples[i].trackTemperatureChange = br.ReadSByte();
                    session.weatherForecastSamples[i].airTemperature = br.ReadSByte();
                    session.weatherForecastSamples[i].airTemperatureChange = br.ReadSByte();
                    session.weatherForecastSamples[i].rainPercentage = br.ReadByte();
                }
                session.forecastAccuracy = br.ReadByte();
                session.aiDifficulty = br.ReadByte();
                session.seasonLinkIdentifier = br.ReadUInt32();
                session.weekendLinkIdentifier = br.ReadUInt32();
                session.sessionLinkIdentifier = br.ReadUInt32();
                session.pitStopWindowIdealLap = br.ReadByte();
                session.pitStopWindowLatestLap = br.ReadByte();
                session.pitStopRejoinPosition = br.ReadByte();
                session.steeringAssist = br.ReadByte();
                session.brakingAssist = br.ReadByte();
                session.gearboxAssist = br.ReadByte();
                session.pitAssist = br.ReadByte();
                session.pitReleaseAssist = br.ReadByte();
                session.ERSAssist = br.ReadByte();
                session.DRSAssist = br.ReadByte();
                session.dynamicRacingLine = br.ReadByte();
                session.dynamicRacingLineType = br.ReadByte();
                session.gameMode = br.ReadByte();
                session.ruleSet = br.ReadByte();
                session.timeOfDay = br.ReadUInt32();
                session.sessionLength = br.ReadByte();
                session.speedUnitsLeadPlayer = br.ReadByte();
                session.temperatureUnitsLeadPlayer = br.ReadByte();
                session.speedUnitsSecondaryPlayer = br.ReadByte();
                session.temperatureUnitsSecondaryPlayer = br.ReadByte();
                session.numSafetyCarPeriods = br.ReadByte();
                session.numVirtualSafetyCarPeriods = br.ReadByte();
                session.numRedFlagPeriods = br.ReadByte();
                session.equalCarPerformance = br.ReadByte();
                session.recoveryMode = br.ReadByte();
                session.flashbackLimit = br.ReadByte();
                session.surfaceType = br.ReadByte();
                session.lowFuelMode = br.ReadByte();
                session.raceStarts = br.ReadByte();
                session.tyreTemperature = br.ReadByte();
                session.pitLaneTyreSim = br.ReadByte();
                session.carDamage = br.ReadByte();
                session.carDamageRate = br.ReadByte();
                session.collisions = br.ReadByte();
                session.collisionsOffForFirstLapOnly = br.ReadByte();
                session.mpUnsafePitRelease = br.ReadByte();
                session.mpOffForGriefing = br.ReadByte();
                session.cornerCuttingStringency = br.ReadByte();
                session.parcFermeRules = br.ReadByte();
                session.pitStopExperience = br.ReadByte();
                session.safetyCar = br.ReadByte();
                session.safetyCarExperience = br.ReadByte();
                session.formationLap = br.ReadByte();
                session.formationLapExperience = br.ReadByte();
                session.redFlags = br.ReadByte();
                session.affectsLicenceLevelSolo = br.ReadByte();
                session.affectsLicenceLevelMP = br.ReadByte();
                session.numSessionsInWeekend = br.ReadByte();
                session.weekendStructure = br.ReadBytes(12);
                session.sector2LapDistanceStart = br.ReadSingle();
                session.sector3LapDistanceStart = br.ReadSingle();
            }
            catch (EndOfStreamException)
            {
                Console.WriteLine("session Packet unreadable");
            }
            return session;
        }
//--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private static Lap ReadLap(BinaryReader br)
        {
            Lap lap = new();
            try
            {
                lap.lastLapTimeInMS = br.ReadUInt32();
                lap.currentLapTimeInMS = br.ReadUInt32();
                lap.sector1TimeMSPart = br.ReadUInt16();
                lap.sector1TimeMinutesPart = br.ReadByte();
                lap.sector2TimeMSPart = br.ReadUInt16();
                lap.sector2TimeMinutesPart = br.ReadByte();
                lap.deltaToCarInFrontMSPart = br.ReadUInt16();
                lap.deltaToCarInFrontMinutesPart = br.ReadByte();
                lap.deltaToRaceLeaderMSPart = br.ReadUInt16();
                lap.deltaToRaceLeaderMinutesPart = br.ReadByte();
                lap.lapDistance = br.ReadSingle();
                lap.totalDistance = br.ReadSingle();
                lap.safetyCarDelta = br.ReadSingle();
                lap.carPosition = br.ReadByte();
                lap.currentLapNum = br.ReadByte();
                lap.pitStatus = br.ReadByte();
                lap.numPitStops = br.ReadByte();
                lap.sector = br.ReadByte();
                lap.currentLapInvalid = br.ReadByte();
                lap.penalties = br.ReadByte();
                lap.totalWarnings = br.ReadByte();
                lap.cornerCuttingWarnings = br.ReadByte();
                lap.numUnservedDriveThroughPens = br.ReadByte();
                lap.numUnservedStopGoPens = br.ReadByte();
                lap.gridPosition = br.ReadByte();
                lap.driverStatus = br.ReadByte();
                lap.resultStatus = br.ReadByte();
                lap.pitLaneTimerActive = br.ReadByte();
                lap.pitLaneTimeInLaneInMS = br.ReadUInt16();
                lap.pitStopTimerInMS = br.ReadUInt16();
                lap.pitStopShouldServePen = br.ReadByte();
                lap.speedTrapFastestSpeed = br.ReadSingle();
                lap.speedTrapFastestLap = br.ReadByte();
            }
            catch (EndOfStreamException)
            {
                Console.WriteLine("lap Packet unreadable");
            }
            return lap;
        }
//--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private static Event ReadEvent(BinaryReader br)
        {
            Event evenT = new();
            try
            {
                evenT.eventStringCode = br.ReadBytes(4);
                var eventCode = System.Text.Encoding.ASCII.GetString(evenT.eventStringCode);
                switch (eventCode)
                {
                    case "FTLP":
                        evenT.eventDetails.FastestLap.vehicleIdx = br.ReadByte();
                        evenT.eventDetails.FastestLap.lapTime = br.ReadSingle();
                        break;
                    case "RTMT":
                        evenT.eventDetails.Retirement.vehicleIdx = br.ReadByte();
                        break;
                    case "TMPT":
                        evenT.eventDetails.TeamMateInPits.vehicleIdx = br.ReadByte();
                        break;
                    case "RCWN":
                        evenT.eventDetails.RaceWinner.vehicleIdx = br.ReadByte();
                        break;
                    case "PENA":
                        evenT.eventDetails.Penalty.penaltyType = br.ReadByte();
                        evenT.eventDetails.Penalty.infringementType = br.ReadByte();
                        evenT.eventDetails.Penalty.vehicleIdx = br.ReadByte();
                        evenT.eventDetails.Penalty.otherVehicleIdx = br.ReadByte();
                        evenT.eventDetails.Penalty.time = br.ReadByte();
                        evenT.eventDetails.Penalty.lapNum = br.ReadByte();
                        evenT.eventDetails.Penalty.placesGained = br.ReadByte();
                        break;
                    case "SPTP":
                        evenT.eventDetails.SpeedTrap.vehicleIdx = br.ReadByte();
                        evenT.eventDetails.SpeedTrap.speed = br.ReadSingle();
                        evenT.eventDetails.SpeedTrap.isOverallFastestInSession = br.ReadByte();
                        evenT.eventDetails.SpeedTrap.isDriverFastestInSession = br.ReadByte();
                        evenT.eventDetails.SpeedTrap.fastestVehicleIdxInSession = br.ReadByte();
                        evenT.eventDetails.SpeedTrap.fastestSpeedInSession = br.ReadSingle();
                        break;
                    case "STLG":
                        evenT.eventDetails.StartLights.numLights = br.ReadByte();
                        break;
                    case "DTSV":
                        evenT.eventDetails.DriveThroughPenaltyServed.vehicleIdx = br.ReadByte();
                        break;
                    case "SGSV":
                        evenT.eventDetails.StopGoPenaltyServed.vehicleIdx = br.ReadByte();
                        break;
                    case "FLBK":
                        evenT.eventDetails.Flashback.flashbackFrameIdentifier = br.ReadUInt32();
                        evenT.eventDetails.Flashback.flashbackSessionTime = br.ReadSingle();
                        break;
                    case "BUTN":
                        evenT.eventDetails.Buttons.buttonStatus = br.ReadUInt32();
                        break;
                    case "OVTK":
                        evenT.eventDetails.Overtake.overtakingVehicleIdx = br.ReadByte();
                        evenT.eventDetails.Overtake.beingOvertakenVehicleIdx = br.ReadByte();
                        break;
                    case "SCAR":
                        evenT.eventDetails.SafetyCar.safetyCarType = br.ReadByte();
                        evenT.eventDetails.SafetyCar.eventType = br.ReadByte();
                        break;
                    case "COLL":
                        evenT.eventDetails.Collision.vehicle1Idx = br.ReadByte();
                        evenT.eventDetails.Collision.vehicle2Idx = br.ReadByte();
                        break;
                    // Other events like SSTA, SEND, DRSE, DRSD, CHQF, LGOT, RDFL don't carry extra data
                }
            }
            catch (EndOfStreamException)
            {
                Console.WriteLine("event Packet unreadable");
            }
            return evenT;
        }
//--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private static Participants ReadParticipants(BinaryReader br)
        {
            Participants participants = new();
            try
            {
                participants.aiControlled = br.ReadByte();
                participants.driverId = br.ReadByte();
                participants.networkId = br.ReadByte();
                participants.teamId = br.ReadByte();
                participants.myTeam = br.ReadByte();
                participants.raceNumber = br.ReadByte();
                participants.nationality = br.ReadByte();
                participants.name = br.ReadBytes(48);
                participants.yourTelemetry = br.ReadByte();
                participants.showOnlineNames = br.ReadByte();
                participants.techLevel = br.ReadUInt16();
                participants.platform = br.ReadByte();
            }
            catch (EndOfStreamException)
            {
                Console.WriteLine("participants Packet unreadable");
            }
            return participants;
        }
//--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private static CarSetup ReadCarSetup(BinaryReader br)
        {
            CarSetup carSetup = new();
            try
            {
                carSetup.frontWing = br.ReadByte();
                carSetup.rearWing = br.ReadByte();
                carSetup.onThrottle = br.ReadByte();
                carSetup.offThrottle = br.ReadByte();
                carSetup.frontCamber = br.ReadSingle();
                carSetup.rearCamber = br.ReadSingle();
                carSetup.frontToe = br.ReadSingle();
                carSetup.rearToe = br.ReadSingle();
                carSetup.frontSuspension = br.ReadByte();
                carSetup.rearSuspension = br.ReadByte();
                carSetup.frontAntiRollBar = br.ReadByte();
                carSetup.rearAntiRollBar = br.ReadByte();
                carSetup.frontSuspensionHeight = br.ReadByte();
                carSetup.rearSuspensionHeight = br.ReadByte();
                carSetup.brakePressure = br.ReadByte();
                carSetup.brakeBias = br.ReadByte();
                carSetup.engineBraking = br.ReadByte();
                carSetup.rearLeftTyrePressure = br.ReadSingle();
                carSetup.rearRightTyrePressure = br.ReadSingle();
                carSetup.frontLeftTyrePressure = br.ReadSingle();
                carSetup.frontRightTyrePressure = br.ReadSingle();
                carSetup.ballast = br.ReadByte();
                carSetup.fuelLoad = br.ReadSingle();
            }
            catch (EndOfStreamException)
            {
                Console.WriteLine("carSetup Packet unreadable");
            }
            return carSetup;
        }
//--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private static CarTelemetry ReadCarTelemetry(BinaryReader br)
        {
            CarTelemetry carTelemetry = new();
            try
            {
                carTelemetry.speed = br.ReadUInt16();
                carTelemetry.throttle = br.ReadSingle();
                carTelemetry.steer = br.ReadSingle();
                carTelemetry.brake = br.ReadSingle();
                carTelemetry.clutch = br.ReadByte();
                carTelemetry.gear = br.ReadSByte();
                carTelemetry.engineRPM = br.ReadUInt16();
                carTelemetry.drs = br.ReadByte();
                carTelemetry.revLightsPercent = br.ReadByte();
                carTelemetry.revLightsBitValue = br.ReadUInt16();
                carTelemetry.brakesTemperature =
                [
                    br.ReadUInt16(),
                    br.ReadUInt16(),
                    br.ReadUInt16(),
                    br.ReadUInt16()
                ];
                carTelemetry.tyresSurfaceTemperature = br.ReadBytes(4);
                carTelemetry.tyresInnerTemperature = br.ReadBytes(4);
                carTelemetry.engineTemperature = br.ReadUInt16();
                carTelemetry.tyresPressure =
                [
                    br.ReadSingle(),
                    br.ReadSingle(),
                    br.ReadSingle(),
                    br.ReadSingle()
                ];
                carTelemetry.surfaceType = br.ReadBytes(4);
            }
            catch (EndOfStreamException)
            {
                Console.WriteLine("carTelemetry Packet unreadable");
            }
            return carTelemetry;
        }
//--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private static CarStatus ReadCarStatus(BinaryReader br)
        {
            CarStatus carStatus = new();
            try
            {
                carStatus.tractionControl = br.ReadByte();
                carStatus.antiLockBrakes = br.ReadByte();
                carStatus.fuelMix = br.ReadByte();
                carStatus.frontBrakeBias = br.ReadByte();
                carStatus.pitLimiterStatus = br.ReadByte();
                carStatus.fuelInTank = br.ReadSingle();
                carStatus.fuelCapacity = br.ReadSingle();
                carStatus.fuelRemainingLaps = br.ReadSingle();
                carStatus.maxRPM = br.ReadUInt16();
                carStatus.idleRPM = br.ReadUInt16();
                carStatus.maxGears = br.ReadByte();
                carStatus.drsAllowed = br.ReadByte();
                carStatus.drsActivationDistance = br.ReadUInt16();
                carStatus.actualTyreCompound = br.ReadByte();
                carStatus.visualTyreCompound = br.ReadByte();
                carStatus.tyresAgeLaps = br.ReadByte();
                carStatus.vehicleFiaFlags = br.ReadSByte();
                carStatus.enginePowerICE = br.ReadSingle();
                carStatus.enginePowerMGUK = br.ReadSingle();
                carStatus.ersStoreEnergy = br.ReadSingle();
                carStatus.ersDeployMode = br.ReadByte();
                carStatus.ersHarvestedThisLapMGUK = br.ReadSingle();
                carStatus.ersHarvestedThisLapMGUH = br.ReadSingle();
                carStatus.ersDeployedThisLap = br.ReadSingle();
                carStatus.networkPaused = br.ReadByte();
            }
            catch (EndOfStreamException)
            {
                Console.WriteLine("carStatus Packet unreadable");
            }
            return carStatus;
        }
//--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private static FinalClassification ReadFinalClassification(BinaryReader br)
        {
            FinalClassification finalClassification = new();
            try
            {
                finalClassification.position = br.ReadByte();
                finalClassification.numLaps = br.ReadByte();
                finalClassification.gridPosition = br.ReadByte();
                finalClassification.points = br.ReadByte();
                finalClassification.numPitStops = br.ReadByte();
                finalClassification.resultStatus = br.ReadByte();
                finalClassification.bestLapTimeInMS = br.ReadUInt32();
                finalClassification.totalRaceTime = br.ReadDouble();
                finalClassification.penaltiesTime = br.ReadByte();
                finalClassification.numPenalties = br.ReadByte();
                finalClassification.numTyreStints = br.ReadByte();
                finalClassification.tyreStintsActual = br.ReadBytes(8);
                finalClassification.tyreStintsVisual = br.ReadBytes(8);
                finalClassification.tyreStintsEndLaps = br.ReadBytes(8);
            }
            catch (EndOfStreamException)
            {
                Console.WriteLine("finalClassification Packet unreadable");
            }
            return finalClassification;
        }
//--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private static LobbyInfo ReadLobbyInfo(BinaryReader br)
        {
            LobbyInfo lobbyInfo = new();
            try
            {
                lobbyInfo.aiControlled = br.ReadByte();
                lobbyInfo.teamId = br.ReadByte();
                lobbyInfo.nationality = br.ReadByte();
                lobbyInfo.platform = br.ReadByte();
                lobbyInfo.name = br.ReadBytes(48);
                lobbyInfo.carNumber = br.ReadByte();
                lobbyInfo.yourTelemetry = br.ReadByte();
                lobbyInfo.showOnlineNames = br.ReadByte();
                lobbyInfo.techLevel = br.ReadUInt16();
                lobbyInfo.readyStatus = br.ReadByte();
            }
            catch (EndOfStreamException)
            {
                Console.WriteLine("lobbyInfo Packet unreadable");
            }
            return lobbyInfo;
        }
//--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private static CarDamage ReadCarDamage(BinaryReader br)
        {
            CarDamage carDamage = new();
            try
            {
                carDamage.tyresWear = new float[4];
                for (var i = 0; i < 4; i++) carDamage.tyresWear[i] = br.ReadSingle();
                carDamage.tyresDamage = br.ReadBytes(4);
                carDamage.brakesDamage = br.ReadBytes(4);
                carDamage.frontLeftWingDamage = br.ReadByte();
                carDamage.frontRightWingDamage = br.ReadByte();
                carDamage.rearWingDamage = br.ReadByte();
                carDamage.floorDamage = br.ReadByte();
                carDamage.diffuserDamage = br.ReadByte();
                carDamage.sidepodDamage = br.ReadByte();
                carDamage.drsFault = br.ReadByte();
                carDamage.ersFault = br.ReadByte();
                carDamage.gearBoxDamage = br.ReadByte();
                carDamage.engineDamage = br.ReadByte();
                carDamage.engineMGUHWear = br.ReadByte();
                carDamage.engineESWear = br.ReadByte();
                carDamage.engineCEWear = br.ReadByte();
                carDamage.engineICEWear = br.ReadByte();
                carDamage.engineMGUKWear = br.ReadByte();
                carDamage.engineTCWear = br.ReadByte();
                carDamage.engineBlown = br.ReadByte();
                carDamage.engineSeized = br.ReadByte();
            }
            catch (EndOfStreamException)
            {
                Console.WriteLine("carDamage Packet unreadable");
            }
            return carDamage;
        }
//--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private static SessionHistory ReadSessionHistory(BinaryReader br)
        {
            SessionHistory sessionHistory = new();
            try
            {
                sessionHistory.carIdx = br.ReadByte();
                sessionHistory.numLaps = br.ReadByte();
                sessionHistory.numTyreStints = br.ReadByte();
                sessionHistory.bestLapTimeLapNum = br.ReadByte();
                sessionHistory.bestSector1LapNum = br.ReadByte();
                sessionHistory.bestSector2LapNum = br.ReadByte();
                sessionHistory.bestSector3LapNum = br.ReadByte();
                sessionHistory.lapHistory = new LapHistory[100];
                for (var i = 0; i < 100; i++)
                {
                    sessionHistory.lapHistory[i].lapTimeInMS = br.ReadUInt32();
                    sessionHistory.lapHistory[i].sector1TimeInMS = br.ReadUInt16();
                    sessionHistory.lapHistory[i].sector1TimeMinutes = br.ReadByte();
                    sessionHistory.lapHistory[i].sector2TimeInMS = br.ReadUInt16();
                    sessionHistory.lapHistory[i].sector2TimeMinutes = br.ReadByte();
                    sessionHistory.lapHistory[i].sector3TimeInMS = br.ReadUInt16();
                    sessionHistory.lapHistory[i].sector3TimeMinutes = br.ReadByte();
                    sessionHistory.lapHistory[i].lapValidBitFlags = br.ReadByte();
                }
                sessionHistory.tyreStintHistory = new TyreStintHistory[8];
                for (var i = 0; i < 8; i++)
                {
                    sessionHistory.tyreStintHistory[i].endLap = br.ReadByte();
                    sessionHistory.tyreStintHistory[i].tyreActualCompound = br.ReadByte();
                    sessionHistory.tyreStintHistory[i].tyreVisualCompound = br.ReadByte();
                }
            }
            catch (EndOfStreamException)
            {
                Console.WriteLine("sessionHistory Packet unreadable");
            }
            return sessionHistory;
        }
//--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private static TyreSet ReadTyreSet(BinaryReader br)
        {
            TyreSet tyreSet = new();
            try
            {
                tyreSet.actualTyreCompound = br.ReadByte();
                tyreSet.visualTyreCompound = br.ReadByte();
                tyreSet.wear = br.ReadByte();
                tyreSet.available = br.ReadByte();
                tyreSet.recommendedSession = br.ReadByte();
                tyreSet.lifeSpan = br.ReadByte();
                tyreSet.usableLife = br.ReadByte();
                tyreSet.lapDeltaTime = br.ReadInt16();
                tyreSet.fitted = br.ReadByte();
            }
            catch (EndOfStreamException)
            {
                Console.WriteLine("tyreSet Packet unreadable");
            }
            return tyreSet;
        }
//--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private static MotionEx ReadMotionEx(BinaryReader br)
        {
            MotionEx motionEx = new();
            try
            {
                motionEx.suspensionPosition = new float[4];
                for (var i = 0; i < 4; i++) motionEx.suspensionPosition[i] = br.ReadSingle();
                motionEx.suspensionVelocity = new float[4];
                for (var i = 0; i < 4; i++) motionEx.suspensionVelocity[i] = br.ReadSingle();
                motionEx.suspensionAcceleration = new float[4];
                for (var i = 0; i < 4; i++) motionEx.suspensionAcceleration[i] = br.ReadSingle();
                motionEx.wheelSpeed = new float[4];
                for (var i = 0; i < 4; i++) motionEx.wheelSpeed[i] = br.ReadSingle();
                motionEx.wheelSlipRatio = new float[4];
                for (var i = 0; i < 4; i++) motionEx.wheelSlipRatio[i] = br.ReadSingle();
                motionEx.wheelSlipAngle = new float[4];
                for (var i = 0; i < 4; i++) motionEx.wheelSlipAngle[i] = br.ReadSingle();
                motionEx.wheelLatForce = new float[4];
                for (var i = 0; i < 4; i++) motionEx.wheelLatForce[i] = br.ReadSingle();
                motionEx.wheelLongForce = new float[4];
                for (var i = 0; i < 4; i++) motionEx.wheelLongForce[i] = br.ReadSingle();
                motionEx.heightOfCOGAboveGround = br.ReadSingle();
                motionEx.localVelocityX = br.ReadSingle();
                motionEx.localVelocityY = br.ReadSingle();
                motionEx.localVelocityZ = br.ReadSingle();
                motionEx.angularVelocityX = br.ReadSingle();
                motionEx.angularVelocityY = br.ReadSingle();
                motionEx.angularVelocityZ = br.ReadSingle();
                motionEx.angularAccelerationX = br.ReadSingle();
                motionEx.angularAccelerationY = br.ReadSingle();
                motionEx.angularAccelerationZ = br.ReadSingle();
                motionEx.frontWheelsAngle = br.ReadSingle();
                motionEx.wheelVertForce = new float[4];
                for (var i = 0; i < 4; i++) motionEx.wheelVertForce[i] = br.ReadSingle();
                motionEx.frontAeroHeight = br.ReadSingle();
                motionEx.rearAeroHeight = br.ReadSingle();
                motionEx.frontRollAngle = br.ReadSingle();
                motionEx.rearRollAngle = br.ReadSingle();
                motionEx.chassisYaw = br.ReadSingle();
            }
            catch (EndOfStreamException)
            {
                Console.WriteLine("motionEx Packet unreadable");
            }
            return motionEx;
        }
//--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private static TimeTrial ReadTimeTrial(BinaryReader br)
        {
            TimeTrial timeTrial = new();
            try
            {
                timeTrial.carIdx = br.ReadByte();
                timeTrial.teamId = br.ReadByte();
                timeTrial.lapTimeInMS = br.ReadUInt32();
                timeTrial.sector1TimeInMS = br.ReadUInt32();
                timeTrial.sector2TimeInMS = br.ReadUInt32();
                timeTrial.sector3TimeInMS = br.ReadUInt32();
                timeTrial.tractionControl = br.ReadByte();
                timeTrial.gearboxAssist = br.ReadByte();
                timeTrial.antiLockBrakes = br.ReadByte();
                timeTrial.equalCarPerformance = br.ReadByte();
                timeTrial.customSetup = br.ReadByte();
                timeTrial.valid = br.ReadByte();
            }
            catch (EndOfStreamException)
            {
                Console.WriteLine("timeTrial Packet unreadable");
            }
            return timeTrial;
        }
    }
//-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------Structs
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Header
    {//29B
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
//--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct CarMotion
    {//1349B
        public float worldPositionX; // World space X position - metres
        public float worldPositionY; // World space Y position
        public float worldPositionZ; // World space Z position
        public float worldVelocityX; // Velocity in world space X � metres/s
        public float worldVelocityY; // Velocity in world space Y
        public float worldVelocityZ; // Velocity in world space Z
        public short worldForwardDirX; // World space forward X direction (normalised)
        public short worldForwardDirY; // World space forward Y direction (normalised)
        public short worldForwardDirZ; // World space forward Z direction (normalised)
        public short worldRightDirX; // World space right X direction (normalised)
        public short worldRightDirY; // World space right Y direction (normalised)
        public short worldRightDirZ; // World space right Z direction (normalised)
        public float gForceLateral; // Lateral G-Force component
        public float gForceLongitudinal; // Longitudinal G-Force component
        public float gForceVertical; // Vertical G-Force component
        public float yaw; // Yaw angle in radians
        public float pitch; // Pitch angle in radians
        public float roll; // Roll angle in radians
    }
//--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct MarshalZone
    {//part of Session
        public float zoneStart; // Fraction (0..1) of way through the lap the marshal zone starts
        public sbyte zoneFlag; // -1 = invalid/unknown, 0 = none, 1 = green, 2 = blue, 3 = yellow
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct WeatherForecastSample
    {// part of Session
        public byte sessionType; // 0 = unknown
        public byte timeOffset; // Time in minutes the forecast is for
        public byte weather; // Weather - 0 = clear, 1 = light cloud, 2 = overcast, 3 = light rain, 4 = heavy rain, 5 = storm
        public sbyte trackTemperature; // Track temp. in degrees Celsius
        public sbyte trackTemperatureChange; // Track temp. change � 0 = up, 1 = down, 2 = no change
        public sbyte airTemperature; // Air temp. in degrees celsius
        public sbyte airTemperatureChange; // Air temp. change � 0 = up, 1 = down, 2 = no change
        public byte rainPercentage; // Rain percentage (0-100)
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Session
    {//753B
        public byte weather; // Weather - 0 = clear, 1 = light cloud, 2 = overcast, 3 = light rain, 4 = heavy rain, 5 = storm
        public sbyte trackTemperature; // Track temp. in degrees celsius
        public sbyte airTemperature; // Air temp. in degrees celsius
        public byte totalLaps; // Total number of laps in this race
        public ushort trackLength; // Track length in metres
        public byte sessionType; // 0 = unknown
        public sbyte trackId; // -1 for unknown
        public byte formula; // Formula, 0 = F1 Modern, 1 = F1 Classic, 2 = F2, 3 = F1 Generic, 4 = Beta, 6 = Esports, 8 = F1 World, 9 = F1 Elimination
        public ushort sessionTimeLeft; // Time left in session in seconds
        public ushort sessionDuration; // Session duration in seconds
        public byte pitSpeedLimit; // Pit speed limit in kilometres per hour
        public byte gamePaused; // Whether the game is paused � network game only
        public byte isSpectating; // Whether the player is spectating
        public byte spectatorCarIndex; // Index of the car being spectated
        public byte sliProNativeSupport; // SLI Pro support, 0 = inactive, 1 = active
        public byte numMarshalZones; // Number of marshal zones to follow
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 21)]
        public MarshalZone[] marshalZones; // List of marshal zones � max 21
        public byte safetyCarStatus; // 0 = no safety car, 1 = full, 2 = virtual, 3 = formation lap
        public byte networkGame; // 0 = offline, 1 = online
        public byte numWeatherForecastSamples; // Number of weather samples to follow
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public WeatherForecastSample[] weatherForecastSamples; // Array of weather forecast samples
        public byte forecastAccuracy; // 0 = Perfect, 1 = Approximate
        public byte aiDifficulty; // AI Difficulty rating � 0-110
        public uint seasonLinkIdentifier; // Identifier for season - persists across saves
        public uint weekendLinkIdentifier; // Identifier for weekend - persists across saves
        public uint sessionLinkIdentifier; // Identifier for session - persists across saves
        public byte pitStopWindowIdealLap; // Ideal lap to pit on for current strategy (player)
        public byte pitStopWindowLatestLap; // Latest lap to pit on for current strategy (player)
        public byte pitStopRejoinPosition; // Predicted position to rejoin at (player)
        public byte steeringAssist; // 0 = off, 1 = on
        public byte brakingAssist; // 0 = off, 1 = low, 2 = medium, 3 = high
        public byte gearboxAssist; // 1 = manual, 2 = manual & suggested gear, 3 = auto
        public byte pitAssist; // 0 = off, 1 = on
        public byte pitReleaseAssist; // 0 = off, 1 = on
        public byte ERSAssist; // 0 = off, 1 = on
        public byte DRSAssist; // 0 = off, 1 = on
        public byte dynamicRacingLine; // 0 = off, 1 = corners only, 2 = full
        public byte dynamicRacingLineType; // 0 = 2D, 1 = 3D
        public byte gameMode; // Game mode id
        public byte ruleSet; // Ruleset
        public uint timeOfDay; // Local time of day - minutes since midnight
        public byte sessionLength; // 0 = None, 2 = Very Short, 3 = Short, 4 = Medium, 5 = Medium Long, 6 = Long, 7 = Full
        public byte speedUnitsLeadPlayer; // 0 = MPH, 1 = KPH
        public byte temperatureUnitsLeadPlayer; // 0 = Celsius, 1 = Fahrenheit
        public byte speedUnitsSecondaryPlayer; // 0 = MPH, 1 = KPH
        public byte temperatureUnitsSecondaryPlayer; // 0 = Celsius, 1 = Fahrenheit
        public byte numSafetyCarPeriods; // Number of safety cars called during session
        public byte numVirtualSafetyCarPeriods; // Number of virtual safety cars called
        public byte numRedFlagPeriods; // Number of red flags called during session
        public byte equalCarPerformance; // 0 = Off, 1 = On
        public byte recoveryMode; // 0 = None, 1 = Flashbacks, 2 = Auto-recovery
        public byte flashbackLimit; // 0 = Low, 1 = Medium, 2 = High, 3 = Unlimited
        public byte surfaceType; // 0 = Simplified, 1 = Realistic
        public byte lowFuelMode; // 0 = Easy, 1 = Hard
        public byte raceStarts; // 0 = Manual, 1 = Assisted
        public byte tyreTemperature; // 0 = Surface only, 1 = Surface & Carcass
        public byte pitLaneTyreSim; // 0 = On, 1 = Off
        public byte carDamage; // 0 = Off, 1 = Reduced, 2 = Standard, 3 = Simulation
        public byte carDamageRate; // 0 = Reduced, 1 = Standard, 2 = Simulation
        public byte collisions; // 0 = Off, 1 = Player-to-Player Off, 2 = On
        public byte collisionsOffForFirstLapOnly; // 0 = Disabled, 1 = Enabled
        public byte mpUnsafePitRelease; // 0 = On, 1 = Off (Multiplayer)
        public byte mpOffForGriefing; // 0 = Disabled, 1 = Enabled (Multiplayer)
        public byte cornerCuttingStringency; // 0 = Regular, 1 = Strict
        public byte parcFermeRules; // 0 = Off, 1 = On
        public byte pitStopExperience; // 0 = Automatic, 1 = Broadcast, 2 = Immersive
        public byte safetyCar; // 0 = Off, 1 = Reduced, 2 = Standard, 3 = Increased
        public byte safetyCarExperience; // 0 = Broadcast, 1 = Immersive
        public byte formationLap; // 0 = Off, 1 = On
        public byte formationLapExperience; // 0 = Broadcast, 1 = Immersive
        public byte redFlags; // 0 = Off, 1 = Reduced, 2 = Standard, 3 = Increased
        public byte affectsLicenceLevelSolo; // 0 = Off, 1 = On
        public byte affectsLicenceLevelMP; // 0 = Off, 1 = On
        public byte numSessionsInWeekend; // Number of session in following array
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        public byte[] weekendStructure; // List of session types to show weekend; structure
        public float sector2LapDistanceStart; // Distance in m around track where sector 2 starts
        public float sector3LapDistanceStart; // Distance in m around track where sector 3 starts
    }
//--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Lap
    {//1285B
        public uint lastLapTimeInMS; // Last lap time in milliseconds
        public uint currentLapTimeInMS; // Current time around the lap in milliseconds
        public ushort sector1TimeMSPart; // Sector 1 time milliseconds part
        public byte sector1TimeMinutesPart; // Sector 1 whole minute part
        public ushort sector2TimeMSPart; // Sector 2 time milliseconds part
        public byte sector2TimeMinutesPart; // Sector 2 whole minute part
        public ushort deltaToCarInFrontMSPart; // Time delta to car in front milliseconds part
        public byte deltaToCarInFrontMinutesPart; // Time delta to car in front whole minute part
        public ushort deltaToRaceLeaderMSPart; // Time delta to race leader milliseconds part
        public byte deltaToRaceLeaderMinutesPart; // Time delta to race leader whole minute part
        public float lapDistance; // Distance vehicle is around current lap in metres � could be negative if line hasn�t been crossed yet
        public float totalDistance; // Total distance travelled in session in metres � could be negative if line hasn�t been crossed yet
        public float safetyCarDelta; // Delta in seconds for safety car
        public byte carPosition; // Car race position
        public byte currentLapNum; // Current lap number
        public byte pitStatus; // 0 = none, 1 = pitting, 2 = in pit area
        public byte numPitStops; // Number of pit stops taken in this race
        public byte sector; // 0 = sector1, 1 = sector2, 2 = sector3
        public byte currentLapInvalid; // Current lap invalid - 0 = valid, 1 = invalid
        public byte penalties; // Accumulated time penalties in seconds to be added
        public byte totalWarnings; // Accumulated number of warnings issued
        public byte cornerCuttingWarnings; // Accumulated number of corner cutting warnings issued
        public byte numUnservedDriveThroughPens; // Num drive through pens left to serve
        public byte numUnservedStopGoPens; // Num stop-go pens left to serve
        public byte gridPosition; // Grid position the vehicle started the race in
        public byte driverStatus; // Status of driver - 0 = in garage, 1 = flying lap, 2 = in lap, 3 = out lap, 4 = on track
        public byte resultStatus; // Result status - 0 = invalid, 1 = inactive, 2 = active, 3 = finished, 4 = didnotfinish, 5 = disqualified, 6 = not classified, 7 = retired
        public byte pitLaneTimerActive; // Pit lane timing, 0 = inactive, 1 = active
        public ushort pitLaneTimeInLaneInMS; // If active, the current time spent in the pit lane in ms
        public ushort pitStopTimerInMS; // Time of the actual pit stop in ms
        public byte pitStopShouldServePen; // Whether the car should serve a penalty at this stop
        public float speedTrapFastestSpeed; // Fastest speed through speed trap for this car in kmph
        public byte speedTrapFastestLap; // Lap no the fastest speed was achieved, 255 = not set
    }
//--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Event
    {//45B
        //public Header header;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] eventStringCode;
        public EventDetails eventDetails;
    }
    [StructLayout(LayoutKind.Explicit)]
    public struct EventDetails
    {// part of Event
        [FieldOffset(0)] public FastestLap FastestLap;
        [FieldOffset(0)] public Retirement Retirement;
        [FieldOffset(0)] public TeamMateInPits TeamMateInPits;
        [FieldOffset(0)] public RaceWinner RaceWinner;
        [FieldOffset(0)] public Penalty Penalty;
        [FieldOffset(0)] public SpeedTrap SpeedTrap;
        [FieldOffset(0)] public StartLights StartLights;
        [FieldOffset(0)] public DriveThroughPenaltyServed DriveThroughPenaltyServed;
        [FieldOffset(0)] public StopGoPenaltyServed StopGoPenaltyServed;
        [FieldOffset(0)] public Flashback Flashback;
        [FieldOffset(0)] public Buttons Buttons;
        [FieldOffset(0)] public Overtake Overtake;
        [FieldOffset(0)] public SafetyCar SafetyCar;
        [FieldOffset(0)] public Collision Collision;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct FastestLap
    {// part of EventDetails
        public byte vehicleIdx;
        public float lapTime;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Retirement
    {// part of EventDetails
        public byte vehicleIdx;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TeamMateInPits
    {// part of EventDetails
        public byte vehicleIdx;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct RaceWinner
    {// part of EventDetails
        public byte vehicleIdx;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Penalty
    {// part of EventDetails
        public byte penaltyType;
        public byte infringementType;
        public byte vehicleIdx;
        public byte otherVehicleIdx;
        public byte time;
        public byte lapNum;
        public byte placesGained;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SpeedTrap
    {// part of EventDetails
        public byte vehicleIdx;
        public float speed;
        public byte isOverallFastestInSession;
        public byte isDriverFastestInSession;
        public byte fastestVehicleIdxInSession;
        public float fastestSpeedInSession;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct StartLights
    {// part of EventDetails
        public byte numLights;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DriveThroughPenaltyServed
    {// part of EventDetails
        public byte vehicleIdx;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct StopGoPenaltyServed
    {// part of EventDetails
        public byte vehicleIdx;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Flashback
    {// part of EventDetails
        public uint flashbackFrameIdentifier;
        public float flashbackSessionTime;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Buttons
    {// part of EventDetails
        public uint buttonStatus;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Overtake
    {// part of EventDetails
        public byte overtakingVehicleIdx;
        public byte beingOvertakenVehicleIdx;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SafetyCar
    {// part of EventDetails
        public byte safetyCarType;
        public byte eventType;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Collision
    {// part of EventDetails
        public byte vehicle1Idx;
        public byte vehicle2Idx;
    }
//--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Participants
    {//1350B
        public byte aiControlled; // 1 = AI, 0 = Human
        public byte driverId; // Driver ID
        public byte networkId; // Unique network ID
        public byte teamId; // Team ID
        public byte myTeam; // 1 = My Team
        public byte raceNumber; // Car number
        public byte nationality; // Nationality
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 48)]
        public byte[] name; // UTF-8 null-terminated name
        public byte yourTelemetry; // 0 = restricted, 1 = public
        public byte showOnlineNames; // 0 = off, 1 = on
        public ushort techLevel; // F1 World tech level
        public byte platform; // Platform ID
    }
//--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct CarSetup
    {//1133B
        public byte frontWing; // Front wing aero
        public byte rearWing; // Rear wing aero
        public byte onThrottle; // Diff on throttle
        public byte offThrottle; // Diff off throttle
        public float frontCamber; // Front camber angle
        public float rearCamber; // Rear camber angle
        public float frontToe; // Front toe angle
        public float rearToe; // Rear toe angle
        public byte frontSuspension; // Front suspension
        public byte rearSuspension; // Rear suspension
        public byte frontAntiRollBar; // Front anti-roll bar
        public byte rearAntiRollBar; // Rear anti-roll bar
        public byte frontSuspensionHeight; // Front ride height
        public byte rearSuspensionHeight; // Rear ride height
        public byte brakePressure; // Brake pressure %
        public byte brakeBias; // Brake bias %
        public byte engineBraking; // Engine braking %
        public float rearLeftTyrePressure; // Rear left tyre pressure
        public float rearRightTyrePressure; // Rear right tyre pressure
        public float frontLeftTyrePressure; // Front left tyre pressure
        public float frontRightTyrePressure; // Front right tyre pressure
        public byte ballast; // Ballast
        public float fuelLoad; // Fuel load
    }
//--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct CarTelemetry
    {//1352B
        public ushort speed; // Speed of car in kilometres per hour
        public float throttle; // Amount of throttle applied (0.0 to 1.0)
        public float steer; // Steering (-1.0 (full lock left) to 1.0 (full lock right))
        public float brake; // Amount of brake applied (0.0 to 1.0)
        public byte clutch; // Amount of clutch applied (0 to 100)
        public sbyte gear; // Gear selected (1-8, N=0, R=-1)
        public ushort engineRPM; // Engine RPM
        public byte drs; // 0 = off, 1 = on
        public byte revLightsPercent; // Rev lights indicator (percentage)
        public ushort revLightsBitValue; // Rev lights (bit 0 = leftmost LED, bit 14 = rightmost LED)
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public ushort[] brakesTemperature;        // Brakes temperature (celsius)
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] tyresSurfaceTemperature; // Tyres surface temperature (celsius)
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] tyresInnerTemperature; // Tyres inner temperature (celsius)
        public ushort engineTemperature; // Engine temperature (celsius)
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public float[] tyresPressure; // Tyres pressure (PSI)
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] surfaceType; // Driving surface
    }
//--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct CarStatus
    {//1239B
        public byte tractionControl; // 0 = off, 1 = medium, 2 = full
        public byte antiLockBrakes; // 0 = off, 1 = on
        public byte fuelMix; // 0 = lean, 1 = standard, 2 = rich, 3 = max
        public byte frontBrakeBias; // percentage
        public byte pitLimiterStatus; // 0 = off, 1 = on
        public float fuelInTank; // fuel mass
        public float fuelCapacity; // fuel capacity
        public float fuelRemainingLaps; // laps of fuel remaining
        public ushort maxRPM; // max engine RPM
        public ushort idleRPM; // idle RPM
        public byte maxGears; // max gear count
        public byte drsAllowed; // 0 = not allowed, 1 = allowed
        public ushort drsActivationDistance; // 0 = not available, else in meters
        public byte actualTyreCompound; // compound code
        public byte visualTyreCompound; // visual compound
        public byte tyresAgeLaps; // age of tyres in laps
        public sbyte vehicleFiaFlags; // -1 = invalid, 0 = none, 1 = green, 2 = blue, 3 = yellow
        public float enginePowerICE; // W
        public float enginePowerMGUK; // W
        public float ersStoreEnergy; // ERS in Joules
        public byte ersDeployMode; // 0 = none, 1 = medium, 2 = hotlap, 3 = overtake
        public float ersHarvestedThisLapMGUK; // harvested this lap by MGU-K
        public float ersHarvestedThisLapMGUH; // harvested this lap by MGU-H
        public float ersDeployedThisLap; // deployed this lap
        public byte networkPaused; // 1 = paused
    }
//--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct FinalClassification
    {//1020B
        public byte position; // Finishing position
        public byte numLaps; // Laps completed
        public byte gridPosition; // Starting grid position
        public byte points; // Points scored
        public byte numPitStops; // Number of pit stops
        public byte resultStatus; // Status of result (0-7)
        public uint bestLapTimeInMS; // Best lap in ms
        public double totalRaceTime; // Total race time (s)
        public byte penaltiesTime; // Total penalties (s)
        public byte numPenalties; // Count of penalties
        public byte numTyreStints; // Total tyre stints
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public byte[] tyreStintsActual; // Actual tyre types used
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public byte[] tyreStintsVisual; // Visual tyre types
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public byte[] tyreStintsEndLaps; // Lap each stint ended on
    }
//--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct LobbyInfo
    {//1306B
        public byte aiControlled; // 1 = AI, 0 = Human
        public byte teamId; // 255 = no team
        public byte nationality; // Nationality ID
        public byte platform; // Platform ID
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 48)]
        public byte[] name; // UTF-8 name, null-terminated
        public byte carNumber; // Car number
        public byte yourTelemetry; // 0 = restricted, 1 = public
        public byte showOnlineNames; // 0 = off, 1 = on
        public ushort techLevel; // F1 World tech level
        public byte readyStatus; // 0 = not ready, 1 = ready, 2 = spectating
    }
//--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct CarDamage
    {//953B
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public float[] tyresWear; // Percentage (0.0 to 1.0)
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] tyresDamage; // Percentage (0 to 100)
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] brakesDamage; // Percentage (0 to 100)
        public byte frontLeftWingDamage; // Percentage
        public byte frontRightWingDamage; // Percentage
        public byte rearWingDamage; // Percentage
        public byte floorDamage; // Percentage
        public byte diffuserDamage; // Percentage
        public byte sidepodDamage; // Percentage
        public byte drsFault; // 0 = OK, 1 = fault
        public byte ersFault; // 0 = OK, 1 = fault
        public byte gearBoxDamage; // Percentage
        public byte engineDamage; // Percentage
        public byte engineMGUHWear; // Percentage
        public byte engineESWear; // Percentage
        public byte engineCEWear; // Percentage
        public byte engineICEWear; // Percentage
        public byte engineMGUKWear; // Percentage
        public byte engineTCWear; // Percentage
        public byte engineBlown; // 0 = OK, 1 = fault
        public byte engineSeized; // 0 = OK, 1 = fault
    }
//--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct LapHistory
    {//part of SessionHistory
        public uint lapTimeInMS; // Lap time in milliseconds
        public ushort sector1TimeInMS; // Sector 1 time in ms
        public byte sector1TimeMinutes; // Sector 1 whole minutes
        public ushort sector2TimeInMS; // Sector 2 time in ms
        public byte sector2TimeMinutes; // Sector 2 whole minutes
        public ushort sector3TimeInMS; // Sector 3 time in ms
        public byte sector3TimeMinutes; // Sector 3 whole minutes
        public byte lapValidBitFlags; // Bit flags for valid lap/sector
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TyreStintHistory
    {//part of SessionHistory
        public byte endLap; // 255 = current tyre
        public byte tyreActualCompound; // Actual compound
        public byte tyreVisualCompound; // Visual compound
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SessionHistory
    {//1460B
        //public Header header;
        public byte carIdx; // Index of car
        public byte numLaps; // Total laps (including current)
        public byte numTyreStints; // Total stints
        public byte bestLapTimeLapNum; // Lap of the best lap
        public byte bestSector1LapNum; // Lap of the best sector 1
        public byte bestSector2LapNum; // Lap of the best sector 2
        public byte bestSector3LapNum; // Lap of the best sector 3
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 100)]
        public LapHistory[] lapHistory;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public TyreStintHistory[] tyreStintHistory;
    }
//--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TyreSet
    {//231B
        public byte actualTyreCompound; // Actual compound
        public byte visualTyreCompound; // Visual compound
        public byte wear; // Wear percentage
        public byte available; // 1 = available
        public byte recommendedSession; // Recommended session
        public byte lifeSpan; // Laps remaining
        public byte usableLife; // Max laps suggested
        public short lapDeltaTime; // Delta time (ms)
        public byte fitted; // 1 = fitted
    }
//--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct MotionEx
    {//237B
        //public Header header;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public float[] suspensionPosition; // RL, RR, FL, FR
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public float[] suspensionVelocity; // RL, RR, FL, FR
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public float[] suspensionAcceleration; // RL, RR, FL, FR
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public float[] wheelSpeed; // Speed of each wheel
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public float[] wheelSlipRatio; // Slip ratio of each wheel
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public float[] wheelSlipAngle; // Slip angle of each wheel
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public float[] wheelLatForce; // Lateral force of each wheel
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public float[] wheelLongForce; // Longitudinal force of each wheel
        public float heightOfCOGAboveGround;
        public float localVelocityX;
        public float localVelocityY;
        public float localVelocityZ;
        public float angularVelocityX;
        public float angularVelocityY;
        public float angularVelocityZ;
        public float angularAccelerationX;
        public float angularAccelerationY;
        public float angularAccelerationZ;
        public float frontWheelsAngle;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public float[] wheelVertForce; // Vertical force per wheel
        public float frontAeroHeight;
        public float rearAeroHeight;
        public float frontRollAngle;
        public float rearRollAngle;
        public float chassisYaw;
    }
//--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TimeTrial
    {//101B
        public byte carIdx; // Car index
        public byte teamId; // Team ID
        public uint lapTimeInMS; // Lap time in milliseconds
        public uint sector1TimeInMS; // Sector 1 time
        public uint sector2TimeInMS; // Sector 2 time
        public uint sector3TimeInMS; // Sector 3 time
        public byte tractionControl; // 0 = off, 1 = medium, 2 = full
        public byte gearboxAssist; // 1 = manual, 2 = manual+suggested, 3 = auto
        public byte antiLockBrakes; // 0 = off, 1 = on
        public byte equalCarPerformance; // 0 = Realistic, 1 = Equal
        public byte customSetup; // 0 = No, 1 = Yes
        public byte valid; // 0 = invalid, 1 = valid
    }
}