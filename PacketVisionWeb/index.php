<html lang="en">
    <head>
        <title>PacketVision</title>
        <style>
            @font-face {
                font-family: 'Digital';
                src: url('F1Wide.woff') format('woff');
                font-weight: normal;
                font-style: normal;
            }
            body {
                font-family: 'Digital';
                position: relative;
                align-content: center;
                text-align: center;
                background: #222;
                color: #eee;
                margin: 0;
            }
            #header {
                font-family: 'Digital';
                position: relative;
                align-content: center;
                top: 10px;
            }
            canvas {
                font-family: 'Digital';
                position: relative;
                align-content: center;
                height: 500px;
                width: 800px;
                background: transparent;
                border-radius: 50%;
            }
            #staticCanvas {
                left : 400px;
                z-index: 0;
            }
            #dynamicCanvas {
                top : -1px;
                left : -406px;
                z-index: 1;
            }
            #speed {
                font-family: 'Digital';
                position: relative;
                align-content: center;
                top: -330px;
                font-size: 40px;
            }
            .kmh {
                font-family: 'Digital';
                position: relative;
                align-content: center;
                top: -320px;
            }
            #engineRPM {
                font-family: 'Digital';
                position: relative;
                align-content: center;
                top: -310px;
                font-size: 32px;
            }
            .rpm {
                font-family: 'Digital';
                position: relative;
                align-content: center;
                top: -302px;
            }
            #gear {
                font-family: 'Digital';
                position: relative;
                align-content: center;
                top: -355px;
                height: 23px;
                width: 110px;
                font-size: 20px;
                margin: 70px auto;
                font-weight: bold;
                padding: 5px 20px;
                border-radius: 5px;
            }
            #drs {
                font-family: 'Digital';
                position: relative;
                align-content: center;
                height: 23px;
                width: 100px;
                font-size: 20px;
                font-weight: bold;
                margin: -70px auto;
                padding: 5px 20px;
                border-radius: 5px;
            }
            .drs-container {
                position: absolute;
                top: 540px;
                left: 912px;
                width: 10px;
                height: 10px;
                display: flex;
                justify-content: center;
                align-items: center;
                pointer-events: none;
                z-index: 10;
            }
            .arrow {
                position: relative;
                font-size: 48px;
                font-weight: bold;
                color: #42DD42;
                opacity: 0;
                transition: transform 0.3s ease, opacity 0.3s ease;
            }
            .left1 {
                transform: translateX(-300%);
            }
            .left2 {
                transform: translateX(-200%);
            }
            .right1 {
                transform: translateX(300%);
            }
            .right2 {
                transform: translateX(200%);
            }
            .drs-label {
                font-size: 18px;
                font-weight: bold;
                margin: 0 20px;
                transition: opacity 0.3s ease;
            }
            .active .left1,
            .active .left2 {
                transform: translateX(600%);
                opacity: 1;
            }
            .active .right1,
            .active .right2 {
                transform: translateX(-600%);
                opacity: 1;
            }
            .active .drs-label {
                opacity: 1;
            }
            #steeringBarContainer {
                position: relative;
                align-content: center;
                top: -120px;
                height: 20px;
                width: 400px;
                background: #333;
                margin: 40px auto;
                border-radius: 10px;
            }
            #steeringBar {
                position: absolute;
                align-content: center;
                top: 0;
                height: 100%;
                background: orange;
            }
            #steeringLabel {
                font-family: 'Digital';
                position: relative;
                align-content: center;
                top: -130px;
                font-size: 20px;
                margin-top: -20px;
                color: orange;
            }
            .sessionDataDisplay {
                position: relative;
                top: -800px;
            }
            .rouletteContainer {
                position: relative;
                width: 600px;
                height: 59px;
                overflow: hidden;
                border-radius: 8px;
                box-shadow: 0 0 15px #000;
                background-color: #222;
            }
            .rouletteTable {
                transition: transform 0.5s ease-in-out;
                position: absolute;
                width: 100%;
            }
            .rouletteRow {
                justify-content: space-between;
                display: flex;
                padding: 16px 24px;
                box-sizing: border-box;
                width: 100%;
                height: 60px;
                border-bottom: 1px solid #333;
                background: #222;
                color: #eee;
            }
            button {
                position: relative;
                background: #444;
                color: #fff;
                border: none;
                padding: 8px 16px;
                border-radius: 4px;
                cursor: pointer;
            }
            button:hover {
                color: #666;
            }
            .buttons {
                position: absolute;
                align-items: center;
                gap: 12px;
                margin: 0 220px;
            }
            .scrollUp {
                background: transparent;
                height: 29px;
                color: #eee;
                top: -11px;
                left: 90px;
                font-size: 20px;
            }
            .scrollDown {
                background: transparent;
                height: 29px;
                color: #eee;
                top: -11px;
                left: 80px;
                font-size: 20px;
            }
        </style>
    </head>
    <body>
        <h1 id="header">Telemetry Dashboard</h1>
        <canvas id="staticCanvas"></canvas>
        <canvas id="dynamicCanvas"></canvas>
        <div id="speed"></div>
        <div class="kmh">KMH</div>
        <div id="engineRPM"></div>
        <div class="rpm">RPM</div>
        <div id="gear">Gear</div>

        <div class="drs-container" id="drsContainer">
            <div class="arrow left1">〙</div>
            <div class="arrow left2">〙</div>                <!-- 〗 | 〕 | 》 | 〙 | 〛 | 〉 -->
            <div class="drs-label" id="drs">DRS</div>
            <div class="arrow right2">〘</div>               <!-- 〖 | 〔 | 《 | 〘 | 〚 | 〈 -->
            <div class="arrow right1">〘</div>
        </div>

        <div id="steeringBarContainer">
            <div id="steeringBar"></div>
        </div>
        <div id="steeringLabel">Steering</div>
        
        <div class="sessionDataDisplay">
            <div class="rouletteContainer">
                <div id="rouletteTable" class="rouletteTable">
                    <div class="rouletteRow">
                        <strong>Weather</strong>
                        <span id="weather"></span>
                        <span class="buttons">
                            <button id="scrollUpArrow" class="scrollUp" onclick="scrollUp()">⇑</button>
                            <button id="scrollDownArrow" class="scrollDown" onclick="scrollDown()">⇓</button>
                        </span>
                    </div>
                    <div class="rouletteRow">
                        <strong>Track Temp</strong>
                        <span id="trackTemperature"></span>
                        <span class="buttons">
                            <button id="scrollUpArrow" class="scrollUp" onclick="scrollUp()">⇑</button>
                            <button id="scrollDownArrow" class="scrollDown" onclick="scrollDown()">⇓</button>
                        </span>
                    </div>
                    <div class="rouletteRow">
                        <strong>Air Temp</strong>
                        <span id="airTemperature"></span>
                        <span class="buttons">
                            <button id="scrollUpArrow" class="scrollUp" onclick="scrollUp()">⇑</button>
                            <button id="scrollDownArrow" class="scrollDown" onclick="scrollDown()">⇓</button>
                        </span>
                    </div>
                    <div class="rouletteRow">
                        <strong>Total Laps</strong>
                        <span id="totalLaps"></span>
                        <span class="buttons">
                            <button id="scrollUpArrow" class="scrollUp" onclick="scrollUp()">⇑</button>
                            <button id="scrollDownArrow" class="scrollDown" onclick="scrollDown()">⇓</button>
                        </span>
                    </div>
                    <div class="rouletteRow">
                        <strong>Track Length</strong>
                        <span id="trackLength"></span>
                        <span class="buttons">
                            <button id="scrollUpArrow" class="scrollUp" onclick="scrollUp()">⇑</button>
                            <button id="scrollDownArrow" class="scrollDown" onclick="scrollDown()">⇓</button>
                        </span>
                    </div>
                    <div class="rouletteRow">
                        <strong>Session</strong>
                        <span id="sessionType"></span>
                        <span class="buttons">
                            <button id="scrollUpArrow" class="scrollUp" onclick="scrollUp()">⇑</button>
                            <button id="scrollDownArrow" class="scrollDown" onclick="scrollDown()">⇓</button>
                        </span>
                    </div>
                    <div class="rouletteRow">
                        <strong>Track</strong>
                        <span id="trackId"></span>
                        <span class="buttons">
                            <button id="scrollUpArrow" class="scrollUp" onclick="scrollUp()">⇑</button>
                            <button id="scrollDownArrow" class="scrollDown" onclick="scrollDown()">⇓</button>
                        </span>
                    </div>
                    <div class="rouletteRow">
                        <strong>Formula Car</strong>
                        <span id="formula"></span>
                        <span class="buttons">
                            <button id="scrollUpArrow" class="scrollUp" onclick="scrollUp()">⇑</button>
                            <button id="scrollDownArrow" class="scrollDown" onclick="scrollDown()">⇓</button>
                        </span>
                    </div>
                    <div class="rouletteRow">
                        <strong>Time Left</strong>
                        <span id="timeLeft"></span>
                        <span class="buttons">
                            <button id="scrollUpArrow" class="scrollUp" onclick="scrollUp()">⇑</button>
                            <button id="scrollDownArrow" class="scrollDown" onclick="scrollDown()">⇓</button>
                        </span>
                    </div>
                    <div class="rouletteRow">
                        <strong>Duration</strong>
                        <span id="duration"></span>
                        <span class="buttons">
                            <button id="scrollUpArrow" class="scrollUp" onclick="scrollUp()">⇑</button>
                            <button id="scrollDownArrow" class="scrollDown" onclick="scrollDown()">⇓</button>
                        </span>
                    </div>
                    <div class="rouletteRow">
                        <strong>Pit Limit</strong>
                        <span id="pitSpeedLimit"></span>
                        <span class="buttons">
                            <button id="scrollUpArrow" class="scrollUp" onclick="scrollUp()">⇑</button>
                            <button id="scrollDownArrow" class="scrollDown" onclick="scrollDown()">⇓</button>
                        </span>
                    </div>
                    <div class="rouletteRow">
                        <strong>Marshal Zones</strong>
                        <span id="numMarshalZones"></span>
                        <span class="buttons">
                            <button id="scrollUpArrow" class="scrollUp" onclick="scrollUp()">⇑</button>
                            <button id="scrollDownArrow" class="scrollDown" onclick="scrollDown()">⇓</button>
                        </span>
                    </div>
                </div>
            </div>
        </div>
        <script>
            const table = document.getElementById('rouletteTable');
            const rowHeight = 60;
            let currentIndex = 0;
            const totalRows = table.children.length;
            const interval = setInterval(function() {
                    if (currentIndex === totalRows - 1) {
                        currentIndex = -1;
                    }
                    if (currentIndex < totalRows - 1) {
                        currentIndex++;
                    }
                    updateScroll();
                }, 3000
            );
            function updateScroll() {
                table.style.transform = `translateY(-${currentIndex * rowHeight}px)`;
            }
            function scrollDown() {
                if (currentIndex === totalRows - 1) {
                    currentIndex = -1;
                }
                if (currentIndex < totalRows - 1) {
                    currentIndex++;
                }
                updateScroll();
            }
            function scrollUp() {
                if (currentIndex === 0) {
                    currentIndex = totalRows;
                }
                if (currentIndex > 0) {
                    currentIndex--;
                }
                updateScroll();
            }
            const socket = new WebSocket('ws://localhost:3000');
            const dpr = window.devicePixelRatio || 1;
            const logicalWidth = 800;
            const logicalHeight = 500;
            const staticCanvas = document.getElementById('staticCanvas');
            const ctxStatic = staticCanvas.getContext('2d');
            staticCanvas.height = logicalHeight * dpr;
            staticCanvas.width = logicalWidth * dpr;
            ctxStatic.scale(dpr, dpr);

            const dynamicCanvas = document.getElementById('dynamicCanvas');
            const ctxDynamic = dynamicCanvas.getContext('2d');
            dynamicCanvas.height = logicalHeight * dpr;
            dynamicCanvas.width = logicalWidth * dpr;
            ctxDynamic.scale(dpr, dpr);

            const centerX = logicalWidth / 2;
            const centerY = logicalHeight / 2;
            const radiusSpeed = 200;
            const radiusThrottle = 150;
            const radiusBrake = 150;
            // statics in the dynamic
            const degToRad = d => d * Math.PI / 180;
            const baseAngle = Math.PI * 0.66;
            const fullCircle = Math.PI * 1.68;
            const fullCircle_tb = Math.PI * 0.97;
            const thick = 40;
            const size = 40;
            const brakeStart = degToRad(60);
            const throttleStart = degToRad(120);
            const drs = document.getElementById('drs');
            const bar = document.getElementById('steeringBar');
            const label = document.getElementById('steeringLabel');
            const halfWidth = 200;
            // end
            const speedArcs = new Map();
            for (let speed = 0; speed <= 360; speed++) {
                const offCanvas = document.createElement('canvas');
                const offCtx = offCanvas.getContext('2d');
                offCanvas.height = logicalHeight * dpr;
                offCanvas.width = logicalWidth * dpr;
                offCtx.translate(-3, -6);
                offCtx.beginPath();
                offCtx.arc(centerX, centerY, radiusSpeed, Math.PI * 0.66, Math.PI * 0.66 + (speed / 360) * Math.PI * 1.68);
                offCtx.strokeStyle = '#005eed';
                offCtx.lineWidth = thick;
                offCtx.lineCap = 'round';
                offCtx.stroke();
                speedArcs.set(speed, offCanvas);
            }
            const drawSpeedArc = (currentSpeed) => {
                const speed = Math.floor(currentSpeed);
                const canvasSpeedArc = speedArcs.get(speed);
                const speedAngle = baseAngle + (speed / 360) * fullCircle;
                ctxDynamic.drawImage(canvasSpeedArc, baseAngle, speedAngle)

            };
            const drawSpeedBackgroundArc = (start, end, color, thickness) => {
                ctxStatic.beginPath();
                ctxStatic.arc(centerX, centerY, radiusSpeed, start, end);
                ctxStatic.strokeStyle = color;
                ctxStatic.lineWidth = thickness;
                ctxStatic.lineCap = 'round';
                ctxStatic.stroke();
            };
            const drawThrottleArc = (start, end, color, thickness) => {
                ctxDynamic.beginPath();
                ctxDynamic.arc(centerX, centerY, radiusThrottle, start, end);
                ctxDynamic.strokeStyle = color;
                ctxDynamic.lineWidth = thickness;
                ctxDynamic.lineCap = 'round';
                ctxDynamic.stroke();
            };
            const drawThrottleBackgroundArc = (start, end, color, thickness) => {
                ctxStatic.beginPath();
                ctxStatic.arc(centerX, centerY, radiusThrottle, start, end);
                ctxStatic.strokeStyle = color;
                ctxStatic.lineWidth = thickness;
                ctxStatic.lineCap = 'round';
                ctxStatic.stroke();
            };
            const drawBrakeArc = (start, end, color, thickness) => {
                ctxDynamic.beginPath();
                ctxDynamic.arc(centerX, centerY, radiusBrake, start, end);
                ctxDynamic.strokeStyle = color;
                ctxDynamic.lineWidth = thickness;
                ctxDynamic.lineCap = 'round';
                ctxDynamic.stroke();
            };
            const drawBrakeBackgroundArc = (start, end, color, thickness) => {
                ctxStatic.beginPath();
                ctxStatic.arc(centerX, centerY, radiusBrake, start, end);
                ctxStatic.strokeStyle = color;
                ctxStatic.lineWidth = thickness;
                ctxStatic.lineCap = 'round';
                ctxStatic.stroke();
            };
            drawSpeedBackgroundArc(baseAngle, baseAngle + fullCircle, '#0a6efd22', thick);
            drawThrottleBackgroundArc(throttleStart, throttleStart + fullCircle_tb, '#32CD3222', thick);
            drawBrakeBackgroundArc(brakeStart - fullCircle_tb + degToRad(70), brakeStart, '#DD222222', thick);

            function renderCarTelemetry(dataCARTELEMETRY) {
                ctxDynamic.clearRect(0, 0, dynamicCanvas.width, dynamicCanvas.height)

                drawSpeedArc(dataCARTELEMETRY.speed);
                document.getElementById('speed').textContent = `${Math.floor(dataCARTELEMETRY.speed)}`;

                const throttleEnd = throttleStart + dataCARTELEMETRY.throttle * degToRad(174.5);
                drawThrottleArc(throttleStart, throttleEnd, '#22BD22', thick);

                const brakeEnd = brakeStart - dataCARTELEMETRY.brake * degToRad(104.5);
                drawBrakeArc(brakeEnd, brakeStart, '#CD1212', thick);

                document.getElementById('engineRPM').textContent = `${dataCARTELEMETRY.engineRPM}`;

                if (dataCARTELEMETRY.drs === 1) {
                    drs.style.background = '#42DD42';
                    drs.style.color = '#000';
                    const container = document.getElementById('drsContainer');
                    container.classList.add('active');
                } else {
                    const container = document.getElementById('drsContainer');
                    container.classList.remove('active');
                    drs.style.background = 'transparent';
                    drs.style.color = '#eee';
                }
                let gearText = dataCARTELEMETRY.gear === 0 ? 'N' : dataCARTELEMETRY.gear === -1 ? 'R' : dataCARTELEMETRY.gear;
                document.getElementById('gear').textContent = `Gear ${gearText}`;

                const steerNormalized = Math.max(-1, Math.min(1, dataCARTELEMETRY.steer));
                if (steerNormalized >= 0) {
                    bar.style.left = halfWidth + 'px';
                    bar.style.width = (halfWidth * steerNormalized) + 'px';
                } else {
                    bar.style.left = (halfWidth + steerNormalized * halfWidth) + 'px';
                    bar.style.width = (-halfWidth * steerNormalized) + 'px';
                }
            }
            function displaySessionData(dataSESSION) {
                document.getElementById('weather').textContent = `${dataSESSION.weather}`

                document.getElementById('trackTemperature').textContent = `${dataSESSION.trackTemperature} °C`

                //document.getElementById('');
            }
            socket.onmessage = (event) => {
                const data = JSON.parse(event.data);
                //console.log(data);
                renderCarTelemetry(data.CARTELEMETRY);
                displaySessionData(data.SESSION);
            };
            setInterval(() => { if (socket.readyState === WebSocket.OPEN) { socket.send('__ping__'); } }, 17);
        </script>
    </body>
</html>