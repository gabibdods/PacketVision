using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using MySqlConnector;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
namespace Telemtry
{
    public class Worker : BackgroundService
    {
        private readonly ConcurrentQueue<PacketMotionData> _packetMotionDataQueue = new();
        private readonly ConcurrentQueue<PacketSessionData> _packetSessionDataQueue = new();
        private readonly ConcurrentQueue<PacketLapData> _packetLapDataQueue = new();
        private readonly ConcurrentQueue<PacketEventData> _packetEventDataQueue = new();
        private readonly ConcurrentQueue<PacketParticipantsData> _packetParticipantsDataQueue = new();
        private readonly ConcurrentQueue<PacketCarSetupData> _packetCarSetupDataQueue = new();
        private readonly ConcurrentQueue<PacketCarTelemetryData> _packetCarTelemetryDataQueue = new();
        private readonly ConcurrentQueue<PacketCarStatusData> _packetCarStatusDataQueue = new();
        private readonly ConcurrentQueue<PacketFinalClassificationData> _packetFinalClassificationDataQueue = new();
        private readonly ConcurrentQueue<PacketLobbyInfoData> _packetLobbyInfoDataQueue = new();
        private readonly ConcurrentQueue<PacketCarDamageData> _packetCarDamageDataQueue = new();
        private readonly ConcurrentQueue<PacketSessionHistoryData> _packetSessionHistoryDataQueue = new();
        private readonly ConcurrentQueue<PacketTyreSetsData> _packetTyreSetsDataQueue = new();
        private readonly ConcurrentQueue<PacketMotionExData> _packetMotionExDataQueue = new();
        private readonly ConcurrentQueue<PacketTimeTrialData> _packetTimeTrialDataQueue = new();
        private readonly ConcurrentQueue<CarTelemetryData> _packetPlayerCarTelemetryDataQueue = new();                      //turn this off
        private readonly string _connectionString = "Server=db;Port=3306;Database=telemetry;User=user;Password=pass;";
        private readonly Dictionary<byte, int> _packetSizes = new()
        {
            { 0, 1349 },                //PacketMotionData
            { 1, 753 },                 //PacketSessionData
            { 2, 1285 },                //PacketLapData
            { 3, 45 },                  //PacketEventData
            { 4, 1350 },                //PacketParticipantsData
            { 5, 1133 },                //PacketCarSetupData
            { 6, 1352 },                //PacketCarTelemetryData
            { 7, 1239 },                //PacketCarStatusData
            { 8, 1020 },                //PacketFinalClassificationData
            { 9, 1306 },                //PacketLobbyInfoData
            { 10, 953 },                //PacketCarDamageData
            { 11, 1460 },               //PacketSessionHistoryData
            { 12, 231 },                //PacketTyreSetsData
            { 13, 237 },                //PacketMotionExData
            { 14, 101 },                //PacketTimeTrialData
        };
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var client = new UdpClient(20777);
            _ = Task.Run(() => ProcessQueueAsync(stoppingToken), CancellationToken.None);

            while (!stoppingToken.IsCancellationRequested)
            {
                UdpReceiveResult result = await client.ReceiveAsync(CancellationToken.None);
                using var ms = new MemoryStream(result.Buffer);
                using var br = new BinaryReader(ms);
                    
                PacketHeader ph = ReadPacketHeader(br);

                if (_packetSizes.TryGetValue(ph.packetId, out int expectedSize) && result.Buffer.Length == expectedSize)
                {
                    switch (ph.packetId)
                    {
                        case 0: //PacketMotionData
                            PacketMotionData packetMotionData = new();
                            packetMotionData = ReadPacketMotionData(br);
                            _packetMotionDataQueue.Enqueue(packetMotionData);
                            break;
                        case 1: //PacketSessionData
                            PacketSessionData packetSessionData = new();
                            packetSessionData = ReadPacketSessionData(br);
                            _packetSessionDataQueue.Enqueue(packetSessionData);
                            break;
                        case 2: //PacketLapData
                            PacketLapData packetLapData = new();
                            packetLapData = ReadPacketLapData(br);
                            _packetLapDataQueue.Enqueue(packetLapData);
                            break;
                        case 3: //PacketEventData
                            PacketEventData packetEventData = new();
                            packetEventData = ReadPacketEventData(br);
                            _packetEventDataQueue.Enqueue(packetEventData);
                            break;
                        case 4: //PacketParticipantsData
                            PacketParticipantsData packetParticipantsData = new();
                            packetParticipantsData = ReadPacketParticipantsData(br);
                            _packetParticipantsDataQueue.Enqueue(packetParticipantsData);
                            break;
                        case 5: //PacketCarSetupData
                            PacketCarSetupData packetCarSetupData = new();
                            packetCarSetupData = ReadPacketCarSetupData(br);
                            _packetCarSetupDataQueue.Enqueue(packetCarSetupData);
                            break;
                        case 6: //PacketCarTelemetryData
                            PacketCarTelemetryData packetCarTelemetryData = new();
                            packetCarTelemetryData = ReadPacketCarTelemetryData(br);
                            _packetCarTelemetryDataQueue.Enqueue(packetCarTelemetryData);
                            _packetPlayerCarTelemetryDataQueue.Enqueue(packetCarTelemetryData.carTelemetryData[ph.playerCarIndex]);
                            break;
                        case 7: //PacketCarStatusData
                            PacketCarStatusData packetCarStatusData = new();
                            packetCarStatusData = ReadPacketCarStatusData(br);
                            _packetCarStatusDataQueue.Enqueue(packetCarStatusData);
                            break;
                        case 8: //PacketFinalClassificationData
                            PacketFinalClassificationData packetFinalClassificationData = new();
                            packetFinalClassificationData = ReadPacketFinalClassificationData(br);
                            _packetFinalClassificationDataQueue.Enqueue(packetFinalClassificationData);
                            break;
                        case 9: //PacketLobbyInfoData
                            PacketLobbyInfoData packetLobbyInfoData = new();
                            packetLobbyInfoData = ReadPacketLobbyInfoData(br);
                            _packetLobbyInfoDataQueue.Enqueue(packetLobbyInfoData);
                            break;
                        case 10: //PacketCarDamageData
                            PacketCarDamageData packetCarDamageData = new();
                            packetCarDamageData = ReadPacketCarDamageData(br);
                            _packetCarDamageDataQueue.Enqueue(packetCarDamageData);
                            break;
                        case 11: //PacketSessionHistoryData
                            PacketSessionHistoryData packetSessionHistoryData = new();
                            packetSessionHistoryData = ReadPacketSessionHistoryData(br);
                            _packetSessionHistoryDataQueue.Enqueue(packetSessionHistoryData);
                            break;
                        case 12: //PacketTyreSetsData
                            PacketTyreSetsData packetTyreSetsData = new();
                            packetTyreSetsData = ReadPacketTyreSetsData(br);
                            _packetTyreSetsDataQueue.Enqueue(packetTyreSetsData);
                            break;
                        case 13: //PacketMotionExData
                            PacketMotionExData packetMotionExData = new();
                            packetMotionExData = ReadPacketMotionExData(br);
                            _packetMotionExDataQueue.Enqueue(packetMotionExData);
                            break;
                        case 14: //PacketTimeTrialData
                            PacketTimeTrialData packetTimeTrialData = new();
                            packetTimeTrialData = ReadPacketTimeTrialData(br);
                            _packetTimeTrialDataQueue.Enqueue(packetTimeTrialData);
                            break;
                        default:
                            break; //Deformed Packets
                    }
                }
            }
        }
//Queuings-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private async Task ProcessQueueAsync(CancellationToken stoppingToken)
        {
            List<CarMotionData> carMotionDataBatch = [];
            List<PacketSessionData> _packetSessionDataBatch = [];
            List<PacketLapData> _packetLapDataBatch = [];
            List<PacketEventData> _packetEventDataBatch = [];
            List<PacketParticipantsData> _packetParticipantsDataBatch = [];             //find limit
            List<CarSetupData> _carSetupDataBatch = [];
            List<CarTelemetryData> carTelemetryDataBatch = [];
            List<CarStatusData> _carStatusDataBatch = [];
            List<FinalClassificationData> _finalClassificationDataBatch = [];           //find limit
            List<LobbyInfoData> _lobbyInfoDataBatch = [];                               //find limit
            List<CarDamageData> _carDamageDataBatch = [];
            List<PacketSessionHistoryData> _packetSessionHistoryDataBatch = [];
            List<PacketTyreSetsData> _packetTyreSetsDataBatch = [];
            List<PacketMotionExData> _packetMotionExDataBatch = [];
            List<PacketTimeTrialData> _packetTimeTrialDataBatch = [];
            while (!stoppingToken.IsCancellationRequested)
            {
                while (_packetPlayerCarTelemetryDataQueue.TryDequeue(out var carTelemetryDataQueueItem))
                {
                    carTelemetryDataBatch.Add(carTelemetryDataQueueItem);
                    if (carTelemetryDataBatch.Count >= 10)
                        break;
                }
                if (carTelemetryDataBatch.Count > 0)
                {
                    try
                    {
                        using var conn = new MySqlConnection(_connectionString);
                        await conn.OpenAsync(stoppingToken);
                        foreach (var carTelemetryDataBatchItem in carTelemetryDataBatch)
                        {
                            using var cmd = CreateInsertCommandCarTelemetryData(conn, carTelemetryDataBatchItem);
                            await cmd.ExecuteNonQueryAsync(stoppingToken);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"MySQL Batch Insert Error: {ex.Message}");
                    }
                    carTelemetryDataBatch.Clear();
                }
                await Task.Delay(10, stoppingToken);
            }
        }
//Readers------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private static PacketHeader ReadPacketHeader(BinaryReader br)
        {
            PacketHeader packetHeader = new();
            try
            {
                packetHeader.packetFormat = br.ReadUInt16();
                packetHeader.gameYear = br.ReadByte();
                packetHeader.gameMajorVersion = br.ReadByte();
                packetHeader.gameMinorVersion = br.ReadByte();
                packetHeader.packetVersion = br.ReadByte();
                packetHeader.packetId = br.ReadByte();
                packetHeader.sessionUID = br.ReadUInt64();
                packetHeader.sessionTime = br.ReadSingle();
                packetHeader.frameIdentifier = br.ReadUInt32();
                packetHeader.overallFrameIdentifier = br.ReadUInt32();
                packetHeader.playerCarIndex = br.ReadByte();
                packetHeader.secondaryPlayerCarIndex = br.ReadByte();
            }
            catch (EndOfStreamException)
            {
                Console.WriteLine("Warning: Incomplete PacketHeader packet. Skipping...");
            }
            return packetHeader;
        }
//-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private static CarMotionData ReadCarMotionData(BinaryReader br)
        {
            CarMotionData carMotionData = new();
            try
            {
                carMotionData.worldPositionX = br.ReadSingle();
                carMotionData.worldPositionY = br.ReadSingle();
                carMotionData.worldPositionZ = br.ReadSingle();
                carMotionData.worldVelocityX = br.ReadSingle();
                carMotionData.worldVelocityY = br.ReadSingle();
                carMotionData.worldVelocityZ = br.ReadSingle();
                carMotionData.worldForwardDirX = br.ReadInt16();
                carMotionData.worldForwardDirY = br.ReadInt16();
                carMotionData.worldForwardDirZ = br.ReadInt16();
                carMotionData.worldRightDirX = br.ReadInt16();
                carMotionData.worldRightDirY = br.ReadInt16();
                carMotionData.worldRightDirZ = br.ReadInt16();
                carMotionData.gForceLateral = br.ReadSingle();
                carMotionData.gForceLongitudinal = br.ReadSingle();
                carMotionData.gForceVertical = br.ReadSingle();
                carMotionData.yaw = br.ReadSingle();
                carMotionData.pitch = br.ReadSingle();
                carMotionData.roll = br.ReadSingle();
            }
            catch (EndOfStreamException)
            {
                Console.WriteLine("Warning: Incomplete CarMotionData packet. Skipping...");
            }
            return carMotionData;
        }
        private static PacketMotionData ReadPacketMotionData(BinaryReader br)
        {//id=0, Packets sent on Frame 1, Packets sent on Frame 2, Packets sent on Frame 31
            PacketMotionData packetMotionData = new();
            try
            {
                packetMotionData.header = ReadPacketHeader(br);
                packetMotionData.carMotionData = new CarMotionData[22];
                for (int i = 0; i < 22; i++) packetMotionData.carMotionData[i] = ReadCarMotionData(br);
            }
            catch (EndOfStreamException)
            {
                Console.WriteLine("Warning: Incomplete PacketMotionData packet. Skipping...");
            }
            return packetMotionData;
        }
//-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private static MarshalZone ReadMarshalZone(BinaryReader br)
        {
            MarshalZone marshalZone = new();
            try
            {
                marshalZone.zoneStart = br.ReadSingle();
                marshalZone.zoneFlag = br.ReadSByte();
            }
            catch (EndOfStreamException)
            {
                Console.WriteLine("Warning: Incomplete MarshalZone packet. Skipping...");
            }
            return marshalZone;
        }
        private static WeatherForecastSample ReadWeatherForecastSample(BinaryReader br)
        {
            WeatherForecastSample weatherForecastSample = new();
            try
            {
                weatherForecastSample.sessionType = br.ReadByte();
                weatherForecastSample.timeOffset = br.ReadByte();
                weatherForecastSample.weather = br.ReadByte();
                weatherForecastSample.trackTemperature = br.ReadSByte();
                weatherForecastSample.trackTemperatureChange = br.ReadSByte();
                weatherForecastSample.airTemperature = br.ReadSByte();
                weatherForecastSample.airTemperatureChange = br.ReadSByte();
                weatherForecastSample.rainPercentage = br.ReadByte();
            }
            catch (EndOfStreamException)
            {
                Console.WriteLine("Warning: Incomplete WeatherForecastSample packet. Skipping...");
            }
            return weatherForecastSample;
        }
        private static PacketSessionData ReadPacketSessionData(BinaryReader br)
        {//id=1, Packets sent on Frame 1, Packets sent on Frame 31
            PacketSessionData packetSessionData = new();
            try
            {
                packetSessionData.header = ReadPacketHeader(br);
                packetSessionData.weather = br.ReadByte();
                packetSessionData.trackTemperature = br.ReadSByte();
                packetSessionData.airTemperature = br.ReadSByte();
                packetSessionData.totalLaps = br.ReadByte();
                packetSessionData.trackLength = br.ReadUInt16();
                packetSessionData.sessionType = br.ReadByte();
                packetSessionData.trackId = br.ReadSByte();
                packetSessionData.formula = br.ReadByte();
                packetSessionData.sessionTimeLeft = br.ReadUInt16();
                packetSessionData.sessionDuration = br.ReadUInt16();
                packetSessionData.pitSpeedLimit = br.ReadByte();
                packetSessionData.gamePaused = br.ReadByte();
                packetSessionData.isSpectating = br.ReadByte();
                packetSessionData.spectatorCarIndex = br.ReadByte();
                packetSessionData.sliProNativeSupport = br.ReadByte();
                packetSessionData.numMarshalZones = br.ReadByte();
                packetSessionData.marshalZones = new MarshalZone[21];
                for (int i = 0; i < 21; i++) packetSessionData.marshalZones[i] = ReadMarshalZone(br);
                packetSessionData.safetyCarStatus = br.ReadByte();
                packetSessionData.networkGame = br.ReadByte();
                packetSessionData.numWeatherForecastSamples = br.ReadByte();
                packetSessionData.weatherForecastSamples = new WeatherForecastSample[64];
                for (int i = 0; i < 64; i++) packetSessionData.weatherForecastSamples[i] = ReadWeatherForecastSample(br);
                packetSessionData.forecastAccuracy = br.ReadByte();
                packetSessionData.aiDifficulty = br.ReadByte();
                packetSessionData.seasonLinkIdentifier = br.ReadUInt32();
                packetSessionData.weekendLinkIdentifier = br.ReadUInt32();
                packetSessionData.sessionLinkIdentifier = br.ReadUInt32();
                packetSessionData.pitStopWindowIdealLap = br.ReadByte();
                packetSessionData.pitStopWindowLatestLap = br.ReadByte();
                packetSessionData.pitStopRejoinPosition = br.ReadByte();
                packetSessionData.steeringAssist = br.ReadByte();
                packetSessionData.brakingAssist = br.ReadByte();
                packetSessionData.gearboxAssist = br.ReadByte();
                packetSessionData.pitAssist = br.ReadByte();
                packetSessionData.pitReleaseAssist = br.ReadByte();
                packetSessionData.ERSAssist = br.ReadByte();
                packetSessionData.DRSAssist = br.ReadByte();
                packetSessionData.dynamicRacingLine = br.ReadByte();
                packetSessionData.dynamicRacingLineType = br.ReadByte();
                packetSessionData.gameMode = br.ReadByte();
                packetSessionData.ruleSet = br.ReadByte();
                packetSessionData.timeOfDay = br.ReadUInt32();
                packetSessionData.sessionLength = br.ReadByte();
                packetSessionData.speedUnitsLeadPlayer = br.ReadByte();
                packetSessionData.temperatureUnitsLeadPlayer = br.ReadByte();
                packetSessionData.speedUnitsSecondaryPlayer = br.ReadByte();
                packetSessionData.temperatureUnitsSecondaryPlayer = br.ReadByte();
                packetSessionData.numSafetyCarPeriods = br.ReadByte();
                packetSessionData.numVirtualSafetyCarPeriods = br.ReadByte();
                packetSessionData.numRedFlagPeriods = br.ReadByte();
                packetSessionData.equalCarPerformance = br.ReadByte();
                packetSessionData.recoveryMode = br.ReadByte();
                packetSessionData.flashbackLimit = br.ReadByte();
                packetSessionData.surfaceType = br.ReadByte();
                packetSessionData.lowFuelMode = br.ReadByte();
                packetSessionData.raceStarts = br.ReadByte();
                packetSessionData.tyreTemperature = br.ReadByte();
                packetSessionData.pitLaneTyreSim = br.ReadByte();
                packetSessionData.carDamage = br.ReadByte();
                packetSessionData.carDamageRate = br.ReadByte();
                packetSessionData.collisions = br.ReadByte();
                packetSessionData.collisionsOffForFirstLapOnly = br.ReadByte();
                packetSessionData.mpUnsafePitRelease = br.ReadByte();
                packetSessionData.mpOffForGriefing = br.ReadByte();
                packetSessionData.cornerCuttingStringency = br.ReadByte();
                packetSessionData.parcFermeRules = br.ReadByte();
                packetSessionData.pitStopExperience = br.ReadByte();
                packetSessionData.safetyCar = br.ReadByte();
                packetSessionData.safetyCarExperience = br.ReadByte();
                packetSessionData.formationLap = br.ReadByte();
                packetSessionData.formationLapExperience = br.ReadByte();
                packetSessionData.redFlags = br.ReadByte();
                packetSessionData.affectsLicenceLevelSolo = br.ReadByte();
                packetSessionData.affectsLicenceLevelMP = br.ReadByte();
                packetSessionData.numSessionsInWeekend = br.ReadByte();
                packetSessionData.weekendStructure = br.ReadBytes(12);
                packetSessionData.sector2LapDistanceStart = br.ReadSingle();
                packetSessionData.sector3LapDistanceStart = br.ReadSingle();
            }
            catch (EndOfStreamException)
            {
                Console.WriteLine("Warning: Incomplete PacketSessionData packet. Skipping...");
            }
            return packetSessionData;
        }
//-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private static LapData ReadLapData(BinaryReader br)
        {
            LapData lapData = new();
            try
            {
                lapData.lastLapTimeInMS = br.ReadUInt32();
                lapData.currentLapTimeInMS = br.ReadUInt32();
                lapData.sector1TimeMSPart = br.ReadUInt16();
                lapData.sector1TimeMinutesPart = br.ReadByte();
                lapData.sector2TimeMSPart = br.ReadUInt16();
                lapData.sector2TimeMinutesPart = br.ReadByte();
                lapData.deltaToCarInFrontMSPart = br.ReadUInt16();
                lapData.deltaToCarInFrontMinutesPart = br.ReadByte();
                lapData.deltaToRaceLeaderMSPart = br.ReadUInt16();
                lapData.deltaToRaceLeaderMinutesPart = br.ReadByte();
                lapData.lapDistance = br.ReadSingle();
                lapData.totalDistance = br.ReadSingle();
                lapData.safetyCarDelta = br.ReadSingle();
                lapData.carPosition = br.ReadByte();
                lapData.currentLapNum = br.ReadByte();
                lapData.pitStatus = br.ReadByte();
                lapData.numPitStops = br.ReadByte();
                lapData.sector = br.ReadByte();
                lapData.currentLapInvalid = br.ReadByte();
                lapData.penalties = br.ReadByte();
                lapData.totalWarnings = br.ReadByte();
                lapData.cornerCuttingWarnings = br.ReadByte();
                lapData.numUnservedDriveThroughPens = br.ReadByte();
                lapData.numUnservedStopGoPens = br.ReadByte();
                lapData.gridPosition = br.ReadByte();
                lapData.driverStatus = br.ReadByte();
                lapData.resultStatus = br.ReadByte();
                lapData.pitLaneTimerActive = br.ReadByte();
                lapData.pitLaneTimeInLaneInMS = br.ReadUInt16();
                lapData.pitStopTimerInMS = br.ReadUInt16();
                lapData.pitStopShouldServePen = br.ReadByte();
                lapData.speedTrapFastestSpeed = br.ReadSingle();
                lapData.speedTrapFastestLap = br.ReadByte();
            }
            catch (EndOfStreamException)
            {
                Console.WriteLine("Warning: Incomplete LapData packet. Skipping...");
            }
            return lapData;
        }
        private static PacketLapData ReadPacketLapData(BinaryReader br)
        {//id=2, Packets sent on Frame 1, Packets sent on Frame 2, Packets sent on Frame 31
            PacketLapData packetLapData = new();
            try
            {
                packetLapData.header = ReadPacketHeader(br);
                packetLapData.lapData = new LapData[22];
                for (int i = 0; i < 22; i++) packetLapData.lapData[i] = ReadLapData(br);
                packetLapData.timeTrialPBCarIdx = br.ReadByte();
                packetLapData.timeTrialRivalCarIdx = br.ReadByte();
            }
            catch (EndOfStreamException)
            {
                Console.WriteLine("Warning: Incomplete PacketLapData packet. Skipping...");
            }
            return packetLapData;
        }
//-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private static PacketEventData ReadPacketEventData(BinaryReader br)
        {//id=3, 
            PacketEventData packetEventData = new();
            try
            {
                packetEventData.header = ReadPacketHeader(br);
                packetEventData.eventStringCode = br.ReadBytes(4);
                string eventCode = System.Text.Encoding.ASCII.GetString(packetEventData.eventStringCode);
                switch (eventCode)
                {
                    case "FTLP":
                        packetEventData.eventDetails.FastestLap.vehicleIdx = br.ReadByte();
                        packetEventData.eventDetails.FastestLap.lapTime = br.ReadSingle();
                        break;
                    case "RTMT":
                        packetEventData.eventDetails.Retirement.vehicleIdx = br.ReadByte();
                        break;
                    case "TMPT":
                        packetEventData.eventDetails.TeamMateInPits.vehicleIdx = br.ReadByte();
                        break;
                    case "RCWN":
                        packetEventData.eventDetails.RaceWinner.vehicleIdx = br.ReadByte();
                        break;
                    case "PENA":
                        packetEventData.eventDetails.Penalty.penaltyType = br.ReadByte();
                        packetEventData.eventDetails.Penalty.infringementType = br.ReadByte();
                        packetEventData.eventDetails.Penalty.vehicleIdx = br.ReadByte();
                        packetEventData.eventDetails.Penalty.otherVehicleIdx = br.ReadByte();
                        packetEventData.eventDetails.Penalty.time = br.ReadByte();
                        packetEventData.eventDetails.Penalty.lapNum = br.ReadByte();
                        packetEventData.eventDetails.Penalty.placesGained = br.ReadByte();
                        break;
                    case "SPTP":
                        packetEventData.eventDetails.SpeedTrap.vehicleIdx = br.ReadByte();
                        packetEventData.eventDetails.SpeedTrap.speed = br.ReadSingle();
                        packetEventData.eventDetails.SpeedTrap.isOverallFastestInSession = br.ReadByte();
                        packetEventData.eventDetails.SpeedTrap.isDriverFastestInSession = br.ReadByte();
                        packetEventData.eventDetails.SpeedTrap.fastestVehicleIdxInSession = br.ReadByte();
                        packetEventData.eventDetails.SpeedTrap.fastestSpeedInSession = br.ReadSingle();
                        break;
                    case "STLG":
                        packetEventData.eventDetails.StartLights.numLights = br.ReadByte();
                        break;
                    case "DTSV":
                        packetEventData.eventDetails.DriveThroughPenaltyServed.vehicleIdx = br.ReadByte();
                        break;
                    case "SGSV":
                        packetEventData.eventDetails.StopGoPenaltyServed.vehicleIdx = br.ReadByte();
                        break;
                    case "FLBK":
                        packetEventData.eventDetails.Flashback.flashbackFrameIdentifier = br.ReadUInt32();
                        packetEventData.eventDetails.Flashback.flashbackSessionTime = br.ReadSingle();
                        break;
                    case "BUTN":
                        packetEventData.eventDetails.Buttons.buttonStatus = br.ReadUInt32();
                        break;
                    case "OVTK":
                        packetEventData.eventDetails.Overtake.overtakingVehicleIdx = br.ReadByte();
                        packetEventData.eventDetails.Overtake.beingOvertakenVehicleIdx = br.ReadByte();
                        break;
                    case "SCAR":
                        packetEventData.eventDetails.SafetyCar.safetyCarType = br.ReadByte();
                        packetEventData.eventDetails.SafetyCar.eventType = br.ReadByte();
                        break;
                    case "COLL":
                        packetEventData.eventDetails.Collision.vehicle1Idx = br.ReadByte();
                        packetEventData.eventDetails.Collision.vehicle2Idx = br.ReadByte();
                        break;
                    default:
                        // Other events like SSTA, SEND, DRSE, DRSD, CHQF, LGOT, RDFL don't carry extra data
                        break;
                }
            }
            catch (EndOfStreamException)
            {
                Console.WriteLine("Warning: Incomplete PacketEventData packet. Skipping...");
            }
            return packetEventData;
        }
//-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private static ParticipantData ReadParticipantData(BinaryReader br)
        {
            ParticipantData participantData = new();
            try
            {
                participantData.aiControlled = br.ReadByte();
                participantData.driverId = br.ReadByte();
                participantData.networkId = br.ReadByte();
                participantData.teamId = br.ReadByte();
                participantData.myTeam = br.ReadByte();
                participantData.raceNumber = br.ReadByte();
                participantData.nationality = br.ReadByte();
                participantData.name = br.ReadBytes(48);
                participantData.yourTelemetry = br.ReadByte();
                participantData.showOnlineNames = br.ReadByte();
                participantData.techLevel = br.ReadUInt16();
                participantData.platform = br.ReadByte();
            }
            catch (EndOfStreamException)
            {
                Console.WriteLine("Warning: Incomplete ParticipantData packet. Skipping...");
            }
            return participantData;
        }
        private static PacketParticipantsData ReadPacketParticipantsData(BinaryReader br)
        {//id=4,//id=1, Packets sent on Frame 1
            PacketParticipantsData packetParticipantsData = new();
            try
            {
                packetParticipantsData.header = ReadPacketHeader(br);
                packetParticipantsData.numActiveCars = br.ReadByte();
                packetParticipantsData.participants = new ParticipantData[22];
                for (int i = 0; i < 22; i++) packetParticipantsData.participants[i] = ReadParticipantData(br);
            }
            catch (EndOfStreamException)
            {
                Console.WriteLine("Warning: Incomplete PacketParticipantsData packet. Skipping...");
            }
            return packetParticipantsData;
        }
//-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private static CarSetupData ReadCarSetupData(BinaryReader br)
        {
            CarSetupData carSetupData = new();
            try
            {
                carSetupData.frontWing = br.ReadByte();
                carSetupData.rearWing = br.ReadByte();
                carSetupData.onThrottle = br.ReadByte();
                carSetupData.offThrottle = br.ReadByte();
                carSetupData.frontCamber = br.ReadSingle();
                carSetupData.rearCamber = br.ReadSingle();
                carSetupData.frontToe = br.ReadSingle();
                carSetupData.rearToe = br.ReadSingle();
                carSetupData.frontSuspension = br.ReadByte();
                carSetupData.rearSuspension = br.ReadByte();
                carSetupData.frontAntiRollBar = br.ReadByte();
                carSetupData.rearAntiRollBar = br.ReadByte();
                carSetupData.frontSuspensionHeight = br.ReadByte();
                carSetupData.rearSuspensionHeight = br.ReadByte();
                carSetupData.brakePressure = br.ReadByte();
                carSetupData.brakeBias = br.ReadByte();
                carSetupData.engineBraking = br.ReadByte();
                carSetupData.rearLeftTyrePressure = br.ReadSingle();
                carSetupData.rearRightTyrePressure = br.ReadSingle();
                carSetupData.frontLeftTyrePressure = br.ReadSingle();
                carSetupData.frontRightTyrePressure = br.ReadSingle();
                carSetupData.ballast = br.ReadByte();
                carSetupData.fuelLoad = br.ReadSingle();
            }
            catch (EndOfStreamException)
            {
                Console.WriteLine("Warning: Incomplete CarSetupData packet. Skipping...");
            }
            return carSetupData;
        }
        private static PacketCarSetupData ReadPacketCarSetupData(BinaryReader br)
        {//id=5,Packets sent on Frame 1, Packets sent on Frame 31
            PacketCarSetupData packetCarSetupData = new();
            try
            {
                packetCarSetupData.header = ReadPacketHeader(br);
                packetCarSetupData.carSetups = new CarSetupData[22];
                for (int i = 0; i < 22; i++) packetCarSetupData.carSetups[i] = ReadCarSetupData(br);
                packetCarSetupData.nextFrontWingValue = br.ReadSingle();
            }
            catch (EndOfStreamException)
            {
                Console.WriteLine("Warning: Incomplete PacketCarSetupData packet. Skipping...");
            }
            return packetCarSetupData;
        }
//-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private static CarTelemetryData ReadCarTelemetryData(BinaryReader br)
        {
            CarTelemetryData carTelemetrydata = new();
            try
            {
                carTelemetrydata.speed = br.ReadUInt16();
                carTelemetrydata.throttle = br.ReadSingle();
                carTelemetrydata.steer = br.ReadSingle();
                carTelemetrydata.brake = br.ReadSingle();
                carTelemetrydata.clutch = br.ReadByte();
                carTelemetrydata.gear = br.ReadSByte();
                carTelemetrydata.engineRPM = br.ReadUInt16();
                carTelemetrydata.drs = br.ReadByte();
                carTelemetrydata.revLightsPercent = br.ReadByte();
                carTelemetrydata.revLightsBitValue = br.ReadUInt16();
                carTelemetrydata.brakesTemperature = new ushort[4];
                carTelemetrydata.tyresSurfaceTemperature = new byte[4];
                carTelemetrydata.tyresInnerTemperature = new byte[4];
                carTelemetrydata.tyresPressure = new float[4];
                carTelemetrydata.surfaceType = new byte[4];
                for (int i = 0; i < 4; i++) carTelemetrydata.brakesTemperature[i] = br.ReadUInt16();
                for (int i = 0; i < 4; i++) carTelemetrydata.tyresSurfaceTemperature[i] = br.ReadByte();
                for (int i = 0; i < 4; i++) carTelemetrydata.tyresInnerTemperature[i] = br.ReadByte();
                carTelemetrydata.engineTemperature = br.ReadUInt16();
                for (int i = 0; i < 4; i++) carTelemetrydata.tyresPressure[i] = br.ReadSingle();
                for (int i = 0; i < 4; i++) carTelemetrydata.surfaceType[i] = br.ReadByte();
            }
            catch (EndOfStreamException)
            {
                Console.WriteLine("Warning: Incomplete CarTelemetryData packet. Skipping...");
            }
            return carTelemetrydata;
        }
        private static PacketCarTelemetryData ReadPacketCarTelemetryData(BinaryReader br)
        {//id=6, Packets sent on Frame 1, Packets sent on Frame 2, Packets sent on Frame 31
            PacketCarTelemetryData packetCarTelemetryData = new();
            try
            {
                packetCarTelemetryData.header = ReadPacketHeader(br);
                packetCarTelemetryData.carTelemetryData = new CarTelemetryData[22];
                for (int i = 0; i < 22; i++) packetCarTelemetryData.carTelemetryData[i] = ReadCarTelemetryData(br);
                packetCarTelemetryData.mfdPanelIndex = br.ReadByte();
                packetCarTelemetryData.mfdPanelIndexSecondaryPlayer = br.ReadByte();
                packetCarTelemetryData.suggestedGear = br.ReadSByte();
            }
            catch (EndOfStreamException)
            {
                Console.WriteLine("Warning: Incomplete PacketCarTelemetryData packet. Skipping...");
            }
            return packetCarTelemetryData;
        }
//-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private static CarStatusData ReadCarStatusData(BinaryReader br)
        {
            CarStatusData carStatusData = new();
            try
            {
                carStatusData.tractionControl = br.ReadByte();
                carStatusData.antiLockBrakes = br.ReadByte();
                carStatusData.fuelMix = br.ReadByte();
                carStatusData.frontBrakeBias = br.ReadByte();
                carStatusData.pitLimiterStatus = br.ReadByte();
                carStatusData.fuelInTank = br.ReadSingle();
                carStatusData.fuelCapacity = br.ReadSingle();
                carStatusData.fuelRemainingLaps = br.ReadSingle();
                carStatusData.maxRPM = br.ReadUInt16();
                carStatusData.idleRPM = br.ReadUInt16();
                carStatusData.maxGears = br.ReadByte();
                carStatusData.drsAllowed = br.ReadByte();
                carStatusData.drsActivationDistance = br.ReadUInt16();
                carStatusData.actualTyreCompound = br.ReadByte();
                carStatusData.visualTyreCompound = br.ReadByte();
                carStatusData.tyresAgeLaps = br.ReadByte();
                carStatusData.vehicleFiaFlags = br.ReadSByte();
                carStatusData.enginePowerICE = br.ReadSingle();
                carStatusData.enginePowerMGUK = br.ReadSingle();
                carStatusData.ersStoreEnergy = br.ReadSingle();
                carStatusData.ersDeployMode = br.ReadByte();
                carStatusData.ersHarvestedThisLapMGUK = br.ReadSingle();
                carStatusData.ersHarvestedThisLapMGUH = br.ReadSingle();
                carStatusData.ersDeployedThisLap = br.ReadSingle();
                carStatusData.networkPaused = br.ReadByte();
            }
            catch (EndOfStreamException)
            {
                Console.WriteLine("Warning: Incomplete CarStatusData packet. Skipping...");
            }
            return carStatusData;
        }
        private static PacketCarStatusData ReadPacketCarStatusData(BinaryReader br)
        {//id=7, Packets sent on Frame 1, Packets sent on Frame 2, Packets sent on Frame 31
            PacketCarStatusData packetCarStatusData = new();
            try
            {
                packetCarStatusData.header = ReadPacketHeader(br);
                packetCarStatusData.carStatusData = new CarStatusData[22];
                for (int i = 0; i < 22; i++) packetCarStatusData.carStatusData[i] = ReadCarStatusData(br);
            }
            catch (EndOfStreamException)
            {
                Console.WriteLine("Warning: Incomplete PacketCarStatusData packet. Skipping...");
            }
            return packetCarStatusData;
        }
//-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private static FinalClassificationData ReadFinalClassificationData(BinaryReader br)
        {
            FinalClassificationData finalClassificationData = new();
            try
            {
                finalClassificationData.position = br.ReadByte();
                finalClassificationData.numLaps = br.ReadByte();
                finalClassificationData.gridPosition = br.ReadByte();
                finalClassificationData.points = br.ReadByte();
                finalClassificationData.numPitStops = br.ReadByte();
                finalClassificationData.resultStatus = br.ReadByte();
                finalClassificationData.bestLapTimeInMS = br.ReadUInt32();
                finalClassificationData.totalRaceTime = br.ReadDouble();
                finalClassificationData.penaltiesTime = br.ReadByte();
                finalClassificationData.numPenalties = br.ReadByte();
                finalClassificationData.numTyreStints = br.ReadByte();
                finalClassificationData.tyreStintsActual = br.ReadBytes(8);
                finalClassificationData.tyreStintsVisual = br.ReadBytes(8);
                finalClassificationData.tyreStintsEndLaps = br.ReadBytes(8);
            }
            catch (EndOfStreamException)
            {
                Console.WriteLine("Warning: Incomplete FinalClassificationData packet. Skipping...");
            }
            return finalClassificationData;
        }
        private static PacketFinalClassificationData ReadPacketFinalClassificationData(BinaryReader br)
        {//id=8, 
            PacketFinalClassificationData packetFinalClassificationData = new();
            try
            {
                packetFinalClassificationData.header = ReadPacketHeader(br);
                packetFinalClassificationData.numCars = br.ReadByte();
                packetFinalClassificationData.classificationData = new FinalClassificationData[22];
                for (int i = 0; i < 22; i++) packetFinalClassificationData.classificationData[i] = ReadFinalClassificationData(br);
            }
            catch (EndOfStreamException)
            {
                Console.WriteLine("Warning: Incomplete PacketFinalClassificationData packet. Skipping...");
            }
            return packetFinalClassificationData;
        }
//-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private static LobbyInfoData ReadLobbyInfoData(BinaryReader br)
        {
            LobbyInfoData lobbyInfoData = new();
            try
            {
                lobbyInfoData.aiControlled = br.ReadByte();
                lobbyInfoData.teamId = br.ReadByte();
                lobbyInfoData.nationality = br.ReadByte();
                lobbyInfoData.platform = br.ReadByte();
                lobbyInfoData.name = br.ReadBytes(48);
                lobbyInfoData.carNumber = br.ReadByte();
                lobbyInfoData.yourTelemetry = br.ReadByte();
                lobbyInfoData.showOnlineNames = br.ReadByte();
                lobbyInfoData.techLevel = br.ReadUInt16();
                lobbyInfoData.readyStatus = br.ReadByte();
            }
            catch (EndOfStreamException)
            {
                Console.WriteLine("Warning: Incomplete LobbyInfoData packet. Skipping...");
            }
            return lobbyInfoData;
        }
        private static PacketLobbyInfoData ReadPacketLobbyInfoData(BinaryReader br)
        {//id=9, 
            PacketLobbyInfoData packetLobbyInfoData = new();
            try
            {
                packetLobbyInfoData.header = ReadPacketHeader(br);
                packetLobbyInfoData.numPlayers = br.ReadByte();
                packetLobbyInfoData.lobbyPlayers = new LobbyInfoData[22];
                for (int i = 0; i < 22; i++) packetLobbyInfoData.lobbyPlayers[i] = ReadLobbyInfoData(br);
            }
            catch (EndOfStreamException)
            {
                Console.WriteLine("Warning: Incomplete PacketLobbyInfoData packet. Skipping...");
            }
            return packetLobbyInfoData;
        }
//-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private static CarDamageData ReadCarDamageData(BinaryReader br)
        {
            CarDamageData carDamageData = new();
            try
            {
                carDamageData.tyresWear = new float[4];
                for (int i = 0; i < 4; i++) carDamageData.tyresWear[i] = br.ReadSingle();
                carDamageData.tyresDamage = br.ReadBytes(4);
                carDamageData.brakesDamage = br.ReadBytes(4);
                carDamageData.frontLeftWingDamage = br.ReadByte();
                carDamageData.frontRightWingDamage = br.ReadByte();
                carDamageData.rearWingDamage = br.ReadByte();
                carDamageData.floorDamage = br.ReadByte();
                carDamageData.diffuserDamage = br.ReadByte();
                carDamageData.sidepodDamage = br.ReadByte();
                carDamageData.drsFault = br.ReadByte();
                carDamageData.ersFault = br.ReadByte();
                carDamageData.gearBoxDamage = br.ReadByte();
                carDamageData.engineDamage = br.ReadByte();
                carDamageData.engineMGUHWear = br.ReadByte();
                carDamageData.engineESWear = br.ReadByte();
                carDamageData.engineCEWear = br.ReadByte();
                carDamageData.engineICEWear = br.ReadByte();
                carDamageData.engineMGUKWear = br.ReadByte();
                carDamageData.engineTCWear = br.ReadByte();
                carDamageData.engineBlown = br.ReadByte();
                carDamageData.engineSeized = br.ReadByte();
            }
            catch (EndOfStreamException)
            {
                Console.WriteLine("Warning: Incomplete CarDamageData packet. Skipping...");
            }
            return carDamageData;
        }
        private static PacketCarDamageData ReadPacketCarDamageData(BinaryReader br)
        {//id=10, Packets sent on Frame 1, Packets sent on Frame 31
            PacketCarDamageData packetCarDamageData = new();
            try
            {
                packetCarDamageData.header = ReadPacketHeader(br);
                packetCarDamageData.carDamageData = new CarDamageData[22];
                for (int i = 0; i < 22; i++) packetCarDamageData.carDamageData[i] = ReadCarDamageData(br);
            }
            catch (EndOfStreamException)
            {
                Console.WriteLine("Warning: Incomplete PacketCarDamageData packet. Skipping...");
            }
            return packetCarDamageData;
        }
//-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private static LapHistoryData ReadLapHistoryData(BinaryReader br)
        {
            LapHistoryData lapHistoryData = new();
            try
            {
                lapHistoryData.lapTimeInMS = br.ReadUInt32();
                lapHistoryData.sector1TimeInMS = br.ReadUInt16();
                lapHistoryData.sector1TimeMinutes = br.ReadByte();
                lapHistoryData.sector2TimeInMS = br.ReadUInt16();
                lapHistoryData.sector2TimeMinutes = br.ReadByte();
                lapHistoryData.sector3TimeInMS = br.ReadUInt16();
                lapHistoryData.sector3TimeMinutes = br.ReadByte();
                lapHistoryData.lapValidBitFlags = br.ReadByte();
            }
            catch (EndOfStreamException)
            {
                Console.WriteLine("Warning: Incomplete LapHistoryData. Skipping...");
            }
            return lapHistoryData;
        }
        private static TyreStintHistoryData ReadTyreStintHistoryData(BinaryReader br)
        {
            TyreStintHistoryData tyreStintHistoryData = new();
            try
            {
                tyreStintHistoryData.endLap = br.ReadByte();
                tyreStintHistoryData.tyreActualCompound = br.ReadByte();
                tyreStintHistoryData.tyreVisualCompound = br.ReadByte();
            }
            catch (EndOfStreamException)
            {
                Console.WriteLine("Warning: Incomplete TyreStintHistoryData. Skipping...");
            }
            return tyreStintHistoryData;
        }
        private static PacketSessionHistoryData ReadPacketSessionHistoryData(BinaryReader br)
        {//id=11, 
            PacketSessionHistoryData packetSessionHistoryData = new();
            try
            {
                packetSessionHistoryData.header = ReadPacketHeader(br);
                packetSessionHistoryData.carIdx = br.ReadByte();
                packetSessionHistoryData.numLaps = br.ReadByte();
                packetSessionHistoryData.numTyreStints = br.ReadByte();
                packetSessionHistoryData.bestLapTimeLapNum = br.ReadByte();
                packetSessionHistoryData.bestSector1LapNum = br.ReadByte();
                packetSessionHistoryData.bestSector2LapNum = br.ReadByte();
                packetSessionHistoryData.bestSector3LapNum = br.ReadByte();
                packetSessionHistoryData.lapHistoryData = new LapHistoryData[100];
                for (int i = 0; i < 100; i++) packetSessionHistoryData.lapHistoryData[i] = ReadLapHistoryData(br);
                packetSessionHistoryData.tyreStintsHistoryData = new TyreStintHistoryData[8];
                for (int i = 0; i < 8; i++) packetSessionHistoryData.tyreStintsHistoryData[i] = ReadTyreStintHistoryData(br);
            }
            catch (EndOfStreamException)
            {
                Console.WriteLine("Warning: Incomplete PacketSessionHistoryData packet. Skipping...");
            }
            return packetSessionHistoryData;
        }
//-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private static TyreSetData ReadTyreSetData(BinaryReader br)
        {
            TyreSetData tyreSetData = new();
            try
            {
                tyreSetData.actualTyreCompound = br.ReadByte();
                tyreSetData.visualTyreCompound = br.ReadByte();
                tyreSetData.wear = br.ReadByte();
                tyreSetData.available = br.ReadByte();
                tyreSetData.recommendedSession = br.ReadByte();
                tyreSetData.lifeSpan = br.ReadByte();
                tyreSetData.usableLife = br.ReadByte();
                tyreSetData.lapDeltaTime = br.ReadInt16();
                tyreSetData.fitted = br.ReadByte();
            }
            catch (EndOfStreamException)
            {
                Console.WriteLine("Warning: Incomplete TyreSetData. Skipping...");
            }
            return tyreSetData;
        }
        private static PacketTyreSetsData ReadPacketTyreSetsData(BinaryReader br)
        {//id=12, 
            PacketTyreSetsData packetTyreSetsData = new();
            try
            {
                packetTyreSetsData.header = ReadPacketHeader(br);
                packetTyreSetsData.carIdx = br.ReadByte();
                packetTyreSetsData.tyreSetData = new TyreSetData[20];
                for (int i = 0; i < 20; i++) packetTyreSetsData.tyreSetData[i] = ReadTyreSetData(br);
                packetTyreSetsData.fittedIdx = br.ReadByte();
            }
            catch (EndOfStreamException)
            {
                Console.WriteLine("Warning: Incomplete PacketTyreSetsData packet. Skipping...");
            }
            return packetTyreSetsData;
        }
//-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private static PacketMotionExData ReadPacketMotionExData(BinaryReader br)
        {//id=13, Packets sent on Frame 1, Packets sent on Frame 2, Packets sent on Frame 31
            PacketMotionExData packetMotionExData = new();
            try
            {
                packetMotionExData.header = ReadPacketHeader(br);
                packetMotionExData.suspensionPosition = new float[4];
                for (int i = 0; i < 4; i++) packetMotionExData.suspensionPosition[i] = br.ReadSingle();
                packetMotionExData.suspensionVelocity = new float[4];
                for (int i = 0; i < 4; i++) packetMotionExData.suspensionVelocity[i] = br.ReadSingle();
                packetMotionExData.suspensionAcceleration = new float[4];
                for (int i = 0; i < 4; i++) packetMotionExData.suspensionAcceleration[i] = br.ReadSingle();
                packetMotionExData.wheelSpeed = new float[4];
                for (int i = 0; i < 4; i++) packetMotionExData.wheelSpeed[i] = br.ReadSingle();
                packetMotionExData.wheelSlipRatio = new float[4];
                for (int i = 0; i < 4; i++) packetMotionExData.wheelSlipRatio[i] = br.ReadSingle();
                packetMotionExData.wheelSlipAngle = new float[4];
                for (int i = 0; i < 4; i++) packetMotionExData.wheelSlipAngle[i] = br.ReadSingle();
                packetMotionExData.wheelLatForce = new float[4];
                for (int i = 0; i < 4; i++) packetMotionExData.wheelLatForce[i] = br.ReadSingle();
                packetMotionExData.wheelLongForce = new float[4];
                for (int i = 0; i < 4; i++) packetMotionExData.wheelLongForce[i] = br.ReadSingle();
                packetMotionExData.heightOfCOGAboveGround = br.ReadSingle();
                packetMotionExData.localVelocityX = br.ReadSingle();
                packetMotionExData.localVelocityY = br.ReadSingle();
                packetMotionExData.localVelocityZ = br.ReadSingle();
                packetMotionExData.angularVelocityX = br.ReadSingle();
                packetMotionExData.angularVelocityY = br.ReadSingle();
                packetMotionExData.angularVelocityZ = br.ReadSingle();
                packetMotionExData.angularAccelerationX = br.ReadSingle();
                packetMotionExData.angularAccelerationY = br.ReadSingle();
                packetMotionExData.angularAccelerationZ = br.ReadSingle();
                packetMotionExData.frontWheelsAngle = br.ReadSingle();
                packetMotionExData.wheelVertForce = new float[4];
                for (int i = 0; i < 4; i++) packetMotionExData.wheelVertForce[i] = br.ReadSingle();
                packetMotionExData.frontAeroHeight = br.ReadSingle();
                packetMotionExData.rearAeroHeight = br.ReadSingle();
                packetMotionExData.frontRollAngle = br.ReadSingle();
                packetMotionExData.rearRollAngle = br.ReadSingle();
                packetMotionExData.chassisYaw = br.ReadSingle();
            }
            catch (EndOfStreamException)
            {
                Console.WriteLine("Warning: Incomplete PacketMotionExData packet. Skipping...");
            }
            return packetMotionExData;
        }
//-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private static TimeTrialDataSet ReadTimeTrialDataSet(BinaryReader br)
        {
            TimeTrialDataSet timeTrialDataSet = new();
            try
            {
                timeTrialDataSet.carIdx = br.ReadByte();
                timeTrialDataSet.teamId = br.ReadByte();
                timeTrialDataSet.lapTimeInMS = br.ReadUInt32();
                timeTrialDataSet.sector1TimeInMS = br.ReadUInt32();
                timeTrialDataSet.sector2TimeInMS = br.ReadUInt32();
                timeTrialDataSet.sector3TimeInMS = br.ReadUInt32();
                timeTrialDataSet.tractionControl = br.ReadByte();
                timeTrialDataSet.gearboxAssist = br.ReadByte();
                timeTrialDataSet.antiLockBrakes = br.ReadByte();
                timeTrialDataSet.equalCarPerformance = br.ReadByte();
                timeTrialDataSet.customSetup = br.ReadByte();
                timeTrialDataSet.valid = br.ReadByte();
            }
            catch (EndOfStreamException)
            {
                Console.WriteLine("Warning: Incomplete TimeTrialDataSet packet. Skipping...");
            }
            return timeTrialDataSet;
        }
        private static PacketTimeTrialData ReadPacketTimeTrialData(BinaryReader br)
        {//id=14, 
            PacketTimeTrialData packetTimeTrialData = new();
            try
            {
                packetTimeTrialData.header = ReadPacketHeader(br);
                packetTimeTrialData.playerSessionBestDataSet = ReadTimeTrialDataSet(br);
                packetTimeTrialData.personalBestDataSet = ReadTimeTrialDataSet(br);
                packetTimeTrialData.rivalDataSet = ReadTimeTrialDataSet(br);
            }
            catch (EndOfStreamException)
            {
                Console.WriteLine("Warning: Incomplete PacketTimeTrialData packet. Skipping...");
            }
            return packetTimeTrialData;
        }
    }
}