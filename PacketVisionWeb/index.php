<html lang="en">
    <head>
        <title>PacketVision</title>
        <meta charset="UTF-8">
        <meta name="viewport" content ="initial-scale=1.0">
        <link rel="stylesheet" href="includes/stylesheet.css">
    </head>
    <body>
        <h1 id="header">Telemetry Dashboard</h1>
        <canvas id="staticCanvas"></canvas>
        <canvas id="dynamicCanvas"></canvas>
        <div id="speed"></div>
        <div id="kmh">KMH</div>
        <div id="engineRPM"></div>
        <div id="rpm">RPM</div>
        <div id="gear">Gear</div>

        <div class="drs-container" id="drsContainer">
            <div class="arrow left1">〙</div>
            <div class="arrow left2">〙</div>
            <!-- 〗 | 〕 | 》 | 〙 | 〛 | 〉 -->
            <div class="drs-label" id="drs">DRS</div>
            <div class="arrow right2">〘</div>
            <div class="arrow right1">〘</div>
            <!-- 〖 | 〔 | 《 | 〘 | 〚 | 〈 -->
        </div>

        <div id="steeringBarContainer">
            <div id="steeringBar"></div>
        </div>
        <div id="steeringLabel">Steering</div>
        
        <div class="sessionDataDisplay">
            <div class="sessionDataRouletteContainer">
                <div id="sessionDataRouletteTable" class="sessionDataRouletteTable">
                    <div class="sessionDataRouletteRow">
                        <strong>Weather</strong>
                        <strong id="weather"></strong>
                        <span class="sessionDataScrollButtons">
                            <button class="sessionDataScrollUp">⇑</button>
                            <button class="sessionDataScrollDown">⇓</button>
                        </span>
                    </div>
                    <div class="sessionDataRouletteRow">
                        <strong>Track Temp</strong>
                        <strong id="trackTemperature"></strong>
                        <span class="sessionDataScrollButtons">
                            <button class="sessionDataScrollUp">⇑</button>
                            <button class="sessionDataScrollDown">⇓</button>
                        </span>
                    </div>
                    <div class="sessionDataRouletteRow">
                        <strong>Air Temp</strong>
                        <strong id="airTemperature"></strong>
                        <span class="sessionDataScrollButtons">
                            <button class="sessionDataScrollUp">⇑</button>
                            <button class="sessionDataScrollDown">⇓</button>
                        </span>
                    </div>
                    <div class="sessionDataRouletteRow">
                        <strong>Total Laps</strong>
                        <strong id="totalLaps"></strong>
                        <span class="sessionDataScrollButtons">
                            <button class="sessionDataScrollUp">⇑</button>
                            <button class="sessionDataScrollDown">⇓</button>
                        </span>
                    </div>
                    <div class="sessionDataRouletteRow">
                        <strong>Track Length</strong>
                        <strong id="trackLength"></strong>
                        <span class="sessionDataScrollButtons">
                            <button class="sessionDataScrollUp">⇑</button>
                            <button class="sessionDataScrollDown">⇓</button>
                        </span>
                    </div>
                    <div class="sessionDataRouletteRow">
                        <strong>Session Type</strong>
                        <strong id="sessionType"></strong>
                        <span class="sessionDataScrollButtons">
                            <button class="sessionDataScrollUp">⇑</button>
                            <button class="sessionDataScrollDown">⇓</button>
                        </span>
                    </div>
                    <div class="sessionDataRouletteRow">
                        <strong>Track</strong>
                        <strong id="trackId"></strong>
                        <span class="sessionDataScrollButtons">
                            <button class="sessionDataScrollUp">⇑</button>
                            <button class="sessionDataScrollDown">⇓</button>
                        </span>
                    </div>
                    <div class="sessionDataRouletteRow">
                        <strong>Formula Car</strong>
                        <strong id="formula"></strong>
                        <span class="sessionDataScrollButtons">
                            <button class="sessionDataScrollUp">⇑</button>
                            <button class="sessionDataScrollDown">⇓</button>
                        </span>
                    </div>
                    <div class="sessionDataRouletteRow">
                        <strong>Time Left</strong>
                        <strong id="timeLeft"></strong>
                        <span class="sessionDataScrollButtons">
                            <button class="sessionDataScrollUp">⇑</button>
                            <button class="sessionDataScrollDown">⇓</button>
                        </span>
                    </div>
                    <div class="sessionDataRouletteRow">
                        <strong>Duration</strong>
                        <strong id="duration"></strong>
                        <span class="sessionDataScrollButtons">
                            <button class="sessionDataScrollUp">⇑</button>
                            <button class="sessionDataScrollDown">⇓</button>
                        </span>
                    </div>
                    <div class="sessionDataRouletteRow">
                        <strong>Pit Limit</strong>
                        <strong id="pitSpeedLimit"></strong>
                        <span class="sessionDataScrollButtons">
                            <button class="sessionDataScrollUp">⇑</button>
                            <button class="sessionDataScrollDown">⇓</button>
                        </span>
                    </div>
                </div>
            </div>
        </div>
        <div class="marshalZoneDataDisplay">
            <div class="marshalZoneRouletteContainer">
                <div id="marshalZoneRouletteTable" class="marshalZoneRouletteTable">
                    <div class="marshalZoneRouletteRow">
                        <strong>Marshal Zone 1</strong>
                        <strong id="marshalZone1"></strong>
                        <span class="marshalZoneScrollButtons">
                            <button class="marshalZoneScrollUp">⇑</button>
                            <button class="marshalZoneScrollDown">⇓</button>
                        </span>
                    </div>
                    <div class="marshalZoneRouletteRow">
                        <strong>Marshal Zone 2</strong>
                        <strong id="marshalZone2"></strong>
                        <span class="marshalZoneScrollButtons">
                            <button class="marshalZoneScrollUp">⇑</button>
                            <button class="marshalZoneScrollDown">⇓</button>
                        </span>
                    </div>
                    <div class="marshalZoneRouletteRow">
                        <strong>Marshal Zone 3</strong>
                        <strong id="marshalZone3"></strong>
                        <span class="marshalZoneScrollButtons">
                            <button class="marshalZoneScrollUp">⇑</button>
                            <button class="marshalZoneScrollDown">⇓</button>
                        </span>
                    </div>
                    <div class="marshalZoneRouletteRow">
                        <strong>Marshal Zone 4</strong>
                        <strong id="marshalZone4"></strong>
                        <span class="marshalZoneScrollButtons">
                            <button class="marshalZoneScrollUp">⇑</button>
                            <button class="marshalZoneScrollDown">⇓</button>
                        </span>
                    </div>
                    <div class="marshalZoneRouletteRow">
                        <strong>Marshal Zone 5</strong>
                        <strong id="marshalZone5"></strong>
                        <span class="marshalZoneScrollButtons">
                            <button class="marshalZoneScrollUp">⇑</button>
                            <button class="marshalZoneScrollDown">⇓</button>
                        </span>
                    </div>
                    <div class="marshalZoneRouletteRow">
                        <strong>Marshal Zone 6</strong>
                        <strong id="marshalZone6"></strong>
                        <span class="marshalZoneScrollButtons">
                            <button class="marshalZoneScrollUp">⇑</button>
                            <button class="marshalZoneScrollDown">⇓</button>
                        </span>
                    </div>
                    <div class="marshalZoneRouletteRow">
                        <strong>Marshal Zone 7</strong>
                        <strong id="marshalZone7"></strong>
                        <span class="marshalZoneScrollButtons">
                            <button class="marshalZoneScrollUp">⇑</button>
                            <button class="marshalZoneScrollDown">⇓</button>
                        </span>
                    </div>
                    <div class="marshalZoneRouletteRow">
                        <strong>Marshal Zone 8</strong>
                        <strong id="marshalZone8"></strong>
                        <span class="marshalZoneScrollButtons">
                            <button class="marshalZoneScrollUp">⇑</button>
                            <button class="marshalZoneScrollDown">⇓</button>
                        </span>
                    </div>
                    <div class="marshalZoneRouletteRow">
                        <strong>Marshal Zone 9</strong>
                        <strong id="marshalZone9"></strong>
                        <span class="marshalZoneScrollButtons">
                            <button class="marshalZoneScrollUp">⇑</button>
                            <button class="marshalZoneScrollDown">⇓</button>
                        </span>
                    </div>
                    <div class="marshalZoneRouletteRow">
                        <strong>Marshal Zone 10</strong>
                        <strong id="marshalZone10"></strong>
                        <span class="marshalZoneScrollButtons">
                            <button class="marshalZoneScrollUp">⇑</button>
                            <button class="marshalZoneScrollDown">⇓</button>
                        </span>
                    </div>
                    <div class="marshalZoneRouletteRow">
                        <strong>Marshal Zone 11</strong>
                        <strong id="marshalZone11"></strong>
                        <span class="marshalZoneScrollButtons">
                            <button class="marshalZoneScrollUp">⇑</button>
                            <button class="marshalZoneScrollDown">⇓</button>
                        </span>
                    </div>
                    <div class="marshalZoneRouletteRow">
                        <strong>Marshal Zone 12</strong>
                        <strong id="marshalZone12"></strong>
                        <span class="marshalZoneScrollButtons">
                            <button class="marshalZoneScrollUp">⇑</button>
                            <button class="marshalZoneScrollDown">⇓</button>
                        </span>
                    </div>
                    <div class="marshalZoneRouletteRow">
                        <strong>Marshal Zone 13</strong>
                        <strong id="marshalZone13"></strong>
                        <span class="marshalZoneScrollButtons">
                            <button class="marshalZoneScrollUp">⇑</button>
                            <button class="marshalZoneScrollDown">⇓</button>
                        </span>
                    </div>
                    <div class="marshalZoneRouletteRow">
                        <strong>Marshal Zone 14</strong>
                        <strong id="marshalZone14"></strong>
                        <span class="marshalZoneScrollButtons">
                            <button class="marshalZoneScrollUp">⇑</button>
                            <button class="marshalZoneScrollDown">⇓</button>
                        </span>
                    </div>
                    <div class="marshalZoneRouletteRow">
                        <strong>Marshal Zone 15</strong>
                        <strong id="marshalZone15"></strong>
                        <span class="marshalZoneScrollButtons">
                            <button class="marshalZoneScrollUp">⇑</button>
                            <button class="marshalZoneScrollDown">⇓</button>
                        </span>
                    </div>
                    <div class="marshalZoneRouletteRow">
                        <strong>Marshal Zone 16</strong>
                        <strong id="marshalZone16"></strong>
                        <span class="marshalZoneScrollButtons">
                            <button class="marshalZoneScrollUp">⇑</button>
                            <button class="marshalZoneScrollDown">⇓</button>
                        </span>
                    </div>
                    <div class="marshalZoneRouletteRow">
                        <strong>Marshal Zone 17</strong>
                        <strong id="marshalZone17"></strong>
                        <span class="marshalZoneScrollButtons">
                            <button class="marshalZoneScrollUp">⇑</button>
                            <button class="marshalZoneScrollDown">⇓</button>
                        </span>
                    </div>
                    <div class="marshalZoneRouletteRow">
                        <strong>Marshal Zone 18</strong>
                        <strong id="marshalZone18"></strong>
                        <span class="marshalZoneScrollButtons">
                            <button class="marshalZoneScrollUp">⇑</button>
                            <button class="marshalZoneScrollDown">⇓</button>
                        </span>
                    </div>
                    <div class="marshalZoneRouletteRow">
                        <strong>Marshal Zone 19</strong>
                        <strong id="marshalZone19"></strong>
                        <span class="marshalZoneScrollButtons">
                            <button class="marshalZoneScrollUp">⇑</button>
                            <button class="marshalZoneScrollDown">⇓</button>
                        </span>
                    </div>
                    <div class="marshalZoneRouletteRow">
                        <strong>Marshal Zone 20</strong>
                        <strong id="marshalZone20"></strong>
                        <span class="marshalZoneScrollButtons">
                            <button class="marshalZoneScrollUp">⇑</button>
                            <button class="marshalZoneScrollDown">⇓</button>
                        </span>
                    </div>
                    <div class="marshalZoneRouletteRow">
                        <strong>Marshal Zone 21</strong>
                        <strong id="marshalZone21"></strong>
                        <span class="marshalZoneScrollButtons">
                            <button class="marshalZoneScrollUp">⇑</button>
                            <button class="marshalZoneScrollDown">⇓</button>
                        </span>
                    </div>
                </div>
            </div>
        </div>
        <script src="includes/script.js"></script>
    </body>
</html>