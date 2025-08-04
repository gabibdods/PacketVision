document.addEventListener('DOMContentLoaded', () => {
    const sessionDataRouletteTable = document.getElementById('sessionDataRouletteTable');
    const sessionDataRouletteRow = sessionDataRouletteTable.getElementsByClassName('sessionDataRouletteRow');
    let sessionDataCurrentIndex = 0;
    const rowHeight = 93;
    const totalRows = sessionDataRouletteRow.length;
    let sessionDataScrollTimeout;
    let sessionDataAutoScrollInterval;
    if (sessionDataRouletteTable && totalRows > 0) {
        const sessionDataScrollUpButtons = document.querySelectorAll('.sessionDataScrollUp');
        const sessionDataScrollDownButtons = document.querySelectorAll('.sessionDataScrollDown');

        function sessionDataAutomaticScroll() {
            sessionDataAutoScrollInterval = setInterval(function () {
                if (sessionDataCurrentIndex === totalRows - 1) {
                    sessionDataCurrentIndex = -1;
                }
                sessionDataCurrentIndex++;
                sessionDataUpdateScroll();
            }, 2000);
        }
        sessionDataScrollUpButtons.forEach(button => {
            button.addEventListener('click', function () {
                clearTimeout(sessionDataScrollTimeout);
                clearInterval(sessionDataAutoScrollInterval);
                sessionDataScrollUp();
                sessionDataScrollTimeout = setTimeout(function () {
                    sessionDataAutomaticScroll();
                }, 6000);
            });
        });
        sessionDataScrollDownButtons.forEach(button => {
            button.addEventListener('click', sessionDataScrollDown);
        });
        function sessionDataUpdateScroll() {
            for (let i = 0; i < totalRows; i++) {
                if (sessionDataRouletteRow[i]) {
                    sessionDataRouletteRow[i].querySelectorAll('button').forEach(btn => {
                        btn.style.display = 'none';
                    });
                }
            }
            if (sessionDataRouletteRow[sessionDataCurrentIndex]) {
                sessionDataRouletteRow[sessionDataCurrentIndex].querySelectorAll('button').forEach(btn => {
                    btn.style.display = 'inline-block';
                });
            }
            sessionDataRouletteTable.style.transform = 'transform 0.3s ease';
            sessionDataRouletteTable.style.transform = `translateY(-${sessionDataCurrentIndex * rowHeight + 1}px)`;
        }
        function sessionDataScrollUp() {
            if (sessionDataCurrentIndex === 0) {
                sessionDataCurrentIndex = totalRows;
            }
            sessionDataCurrentIndex--;
            sessionDataUpdateScroll();
        }
        function sessionDataScrollDown() {
            if (sessionDataCurrentIndex === totalRows - 1) {
                sessionDataCurrentIndex = -1;
            }
            sessionDataCurrentIndex++;
            sessionDataUpdateScroll();
        }
        sessionDataAutomaticScroll();
    }
    const marshalZoneRouletteTable = document.getElementById('marshalZoneRouletteTable');
    const marshalZoneRouletteRow = marshalZoneRouletteTable.getElementsByClassName('marshalZoneRouletteRow');
    let marshalZoneCurrentIndex = 0;
    let marshalZoneScrollTimeout;
    let marshalZoneAutoScrollInterval;
    if (marshalZoneRouletteTable) {
        const marshalZoneScrollUpButtons = document.querySelectorAll('.marshalZoneScrollUp');
        const marshalZoneScrollDownButtons = document.querySelectorAll('.marshalZoneScrollDown');

        function marshalZoneAutomaticScroll() {
            marshalZoneAutoScrollInterval = setInterval(function () {
                if (marshalZoneCurrentIndex === 20) {
                    marshalZoneCurrentIndex = -1;
                }
                marshalZoneCurrentIndex++;
                marshalZoneUpdateScroll();
            }, 2000);
        }
        marshalZoneScrollUpButtons.forEach(button => {
            button.addEventListener('click', function () {
                clearTimeout(marshalZoneScrollTimeout);
                clearInterval(marshalZoneAutoScrollInterval);
                marshalZoneScrollUp();
                marshalZoneScrollTimeout = setTimeout(function () {
                    marshalZoneAutomaticScroll();
                }, 6000);
            });
        });
        marshalZoneScrollDownButtons.forEach(button => {
            button.addEventListener('click', marshalZoneScrollDown);
        });
        function marshalZoneUpdateScroll() {
            for (let i = 0; i < 21; i++) {
                if (marshalZoneRouletteRow[i]) {
                    marshalZoneRouletteRow[i].querySelectorAll('button').forEach(btn => {
                        btn.style.display = 'none';
                    });
                }
            }
            if (marshalZoneRouletteRow[marshalZoneCurrentIndex]) {
                marshalZoneRouletteRow[marshalZoneCurrentIndex].querySelectorAll('button').forEach(btn => {
                    btn.style.display = 'inline-block';
                });
            }
            marshalZoneRouletteTable.style.transform = 'transform 0.3s ease';
            marshalZoneRouletteTable.style.transform = `translateY(-${marshalZoneCurrentIndex * rowHeight + 1}px)`;
        }
        function marshalZoneScrollUp() {
            if (marshalZoneCurrentIndex === 0) {
                marshalZoneCurrentIndex = 21;
            }
            marshalZoneCurrentIndex--;
            marshalZoneUpdateScroll();
        }
        function marshalZoneScrollDown() {
            if (marshalZoneCurrentIndex === 20) {
                marshalZoneCurrentIndex = -1;
            }
            marshalZoneCurrentIndex++;
            marshalZoneUpdateScroll();
        }
        marshalZoneAutomaticScroll();
    }
    const degToRad = d => d * Math.PI / 180;
    const baseAngle = Math.PI * 0.66;
    const fullCircleS = Math.PI * 1.68;
    const fullCircleTB = Math.PI * 0.97;
    const thick = 40;
    const throttleStart = degToRad(120);
    const brakeStart = degToRad(60);
    const drs = document.getElementById('drs');
    const bar = document.getElementById('steeringBar');
    const halfWidth = 200;
    const container = document.getElementById('drsContainer');
    const sessionTypeMap = {
        0: "Unknown",
        1: "Practice 1",
        2: "Practice 2",
        3: "Practice 3",
        4: "Short Practice",
        5: "Qualifying 1",
        6: "Qualifying 2",
        7: "Qualifying 3",
        8: "Short Qualifying",
        9: "One-Shot Qualifying",
        10: "Sprint Shootout 1",
        11: "Sprint Shootout 2",
        12: "Sprint Shootout 3",
        13: "Short Sprint Shootout",
        14: "One-Shot Sprint Shootout",
        15: "Race",
        16: "Race 2",
        17: "Race 3",
        18: "Time Trial"
    };
    const sessionTrackId = {
        0: "Melbourne",
        1: "Paul Ricard",
        2: "Shanghai",
        3: "Sakhir",
        4: "Catalunya",
        5: "Monaco",
        6: "Montreal",
        7: "Silverstone",
        8: "Hockenheim",
        9: "Hungaroring",
        10: "Spa",
        11: "Monza",
        12: "Singapore",
        13: "Suzuka",
        14: "Abu Dhabi",
        15: "Texas",
        16: "Brazil",
        17: "Austria",
        18: "Sochi",
        19: "Mexico",
        20: "Baku",
        21: "Sakhir Short",
        22: "Silverstone Short",
        23: "Texas Short",
        24: "Suzuka Short",
        25: "Hanoi",
        26: "Zandvoort",
        27: "Imola",
        28: "Portimão",
        29: "Jeddah",
        30: "Miami",
        31: "Las Vegas",
        32: "Losail"
    };
    const sessionFormula = {
        0: "F1 Modern",
        1: "F1 Classic",
        2: "F2",
        3: "F1 Generic",
        4: "Beta",
        6: "Esports",
        8: "F1 World",
        9: "F1 Elimination"
    };
    const sessionMarshalZoneFlag = {
        0: "Unknown",
        1: "None",
        2: "Green",
        3: "Blue",
        4: "Yellow"
    };
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
    
    const speedArcs = new Map();
    const throttleArcs = new Map();
    const brakeArcs = new Map();
    for (let speed = 0; speed <= 360; speed++) {
        const offCanvasSpeed = document.createElement('canvas');
        const offCtxSpeed = offCanvasSpeed.getContext('2d');
        offCanvasSpeed.height = logicalHeight * dpr;
        offCanvasSpeed.width = logicalWidth * dpr;
        offCtxSpeed.translate(-1, -6);
        offCtxSpeed.beginPath();
        offCtxSpeed.arc(centerX, centerY, radiusSpeed, Math.PI * 0.66, Math.PI * 0.66 + (speed / 360) * Math.PI * 1.68);
        offCtxSpeed.strokeStyle = '#005eed';
        offCtxSpeed.lineWidth = thick;
        offCtxSpeed.lineCap = 'round';
        offCtxSpeed.stroke();
        speedArcs.set(speed, offCanvasSpeed);
    }
    for (let value = 0; value <= 100; value++) {
        const offCanvasThrottle = document.createElement('canvas');
        const offCtxThrottle = offCanvasThrottle.getContext('2d');
        offCanvasThrottle.height = logicalHeight * dpr;
        offCanvasThrottle.width = logicalWidth * dpr;
        offCtxThrottle.translate(-1, -6);
        offCtxThrottle.beginPath();
        offCtxThrottle.arc(centerX, centerY, radiusThrottle, throttleStart, throttleStart + (value / 100) * degToRad(174.5));
        offCtxThrottle.strokeStyle = '#32cd32';
        offCtxThrottle.lineWidth = thick;
        offCtxThrottle.lineCap = 'round';
        offCtxThrottle.stroke();
        throttleArcs.set((value / 100), offCanvasThrottle);

        const offCanvasBrake = document.createElement('canvas');
        const offCtxBrake = offCanvasBrake.getContext('2d');
        offCanvasBrake.height = logicalHeight * dpr;
        offCanvasBrake.width = logicalWidth * dpr;
        offCtxBrake.translate(-1, -2);
        offCtxBrake.beginPath();
        offCtxBrake.arc(centerX, centerY, radiusBrake, brakeStart - (value / 100) * degToRad(104.5), brakeStart);
        offCtxBrake.strokeStyle = '#cd1212';
        offCtxBrake.lineWidth = thick;
        offCtxBrake.lineCap = 'round';
        offCtxBrake.stroke();
        brakeArcs.set((value / 100), offCanvasBrake);
    }
    const drawSpeedArc = (currentSpeed) => {
        const speed = Math.floor(currentSpeed);
        const canvasSpeedArc = speedArcs.get(speed);
        const speedAngle = baseAngle + (speed / 360) * fullCircleS;
        ctxDynamic.drawImage(canvasSpeedArc, baseAngle, speedAngle)
    };
    const drawThrottleArc = (currentThrottle) => {
        const throttle = parseFloat(currentThrottle.toFixed(2));
        const canvasThrottleArc = throttleArcs.get(throttle);
        const throttleAngle = throttleStart + throttle * degToRad(174.5);
        ctxDynamic.drawImage(canvasThrottleArc, throttleStart, throttleAngle);
    };
    const drawBrakeArc = (currentBrake) => {
        const brake = parseFloat(currentBrake.toFixed(2));
        const canvasBrakeArc = brakeArcs.get(brake);
        const brakeAngle = brakeStart - brake * degToRad(104.5);
        ctxDynamic.drawImage(canvasBrakeArc, brakeAngle, brakeStart);
    };
    const drawSpeedBackgroundArc = (start, end, color, thickness) => {
        ctxStatic.beginPath();
        ctxStatic.arc(centerX, centerY, radiusSpeed, start, end);
        ctxStatic.strokeStyle = color;
        ctxStatic.lineWidth = thickness;
        ctxStatic.lineCap = 'round';
        ctxStatic.stroke();
    };
    const drawThrottleBackgroundArc = (start, end, color, thickness) => {
        ctxStatic.beginPath();
        ctxStatic.arc(centerX, centerY, radiusThrottle, start, end);
        ctxStatic.strokeStyle = color;
        ctxStatic.lineWidth = thickness;
        ctxStatic.lineCap = 'round';
        ctxStatic.stroke();
    };
    const drawBrakeBackgroundArc = (start, end, color, thickness) => {
        ctxStatic.beginPath();
        ctxStatic.arc(centerX, centerY, radiusBrake, start, end);
        ctxStatic.strokeStyle = color;
        ctxStatic.lineWidth = thickness;
        ctxStatic.lineCap = 'round';
        ctxStatic.stroke();
    };
    drawSpeedBackgroundArc(baseAngle, baseAngle + fullCircleS, '#0a6efd22', thick);
    drawThrottleBackgroundArc(throttleStart, throttleStart + fullCircleTB, '#32CD3222', thick);
    drawBrakeBackgroundArc(brakeStart - fullCircleTB + degToRad(70), brakeStart, '#DD222222', thick);
    function renderCarTelemetry(dataCARTELEMETRY) {
        ctxDynamic.clearRect(0, 0, dynamicCanvas.width, dynamicCanvas.height)

        drawSpeedArc(dataCARTELEMETRY.speed);
        document.getElementById('speed').textContent = `${Math.floor(dataCARTELEMETRY.speed)}`;

        drawThrottleArc(dataCARTELEMETRY.throttle);

        drawBrakeArc(dataCARTELEMETRY.brake);

        document.getElementById('engineRPM').textContent = `${dataCARTELEMETRY.engineRPM}`;

        if (dataCARTELEMETRY.drs === 1) {
            drs.style.color = '#42DD42';
            container.classList.add('active');
        } else {
            container.classList.remove('active');
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
    function renderSessionTelemetry(dataSESSION) {
        document.getElementById('weather').textContent = `${dataSESSION.weather}`

        document.getElementById('trackTemperature').textContent = `${dataSESSION.trackTemperature} °C`

        document.getElementById('airTemperature').textContent = `${dataSESSION.airTemperature} °C`;

        document.getElementById('totalLaps').textContent = `${dataSESSION.totalLaps}`;

        document.getElementById('trackLength').textContent = `${dataSESSION.trackLength} m`;

        document.getElementById('sessionType').textContent = sessionTypeMap[dataSESSION.sessionType] || "Unknowm";

        document.getElementById('trackId').textContent = sessionTrackId[dataSESSION.trackId];

        document.getElementById('formula').textContent = sessionFormula[dataSESSION.formula];

        document.getElementById('timeLeft').textContent = `${dataSESSION.timeLeft}`;

        document.getElementById('duration').textContent = `${dataSESSION.duration}`;

        document.getElementById('pitSpeedLimit').textContent = `${dataSESSION.pitSpeedLimit}`;

        document.getElementById('marshalZone1').textContent = sessionMarshalZoneFlag[dataSESSION.marshalZones0_zoneFlag + 1];
        document.getElementById('marshalZone2').textContent = sessionMarshalZoneFlag[dataSESSION.marshalZones1_zoneFlag + 1];
        document.getElementById('marshalZone3').textContent = sessionMarshalZoneFlag[dataSESSION.marshalZones2_zoneFlag + 1];
        document.getElementById('marshalZone4').textContent = sessionMarshalZoneFlag[dataSESSION.marshalZones3_zoneFlag + 1];
        document.getElementById('marshalZone5').textContent = sessionMarshalZoneFlag[dataSESSION.marshalZones4_zoneFlag + 1];
        document.getElementById('marshalZone6').textContent = sessionMarshalZoneFlag[dataSESSION.marshalZones5_zoneFlag + 1];
        document.getElementById('marshalZone7').textContent = sessionMarshalZoneFlag[dataSESSION.marshalZones6_zoneFlag + 1];
        document.getElementById('marshalZone8').textContent = sessionMarshalZoneFlag[dataSESSION.marshalZones7_zoneFlag + 1];
        document.getElementById('marshalZone9').textContent = sessionMarshalZoneFlag[dataSESSION.marshalZones8_zoneFlag + 1];
        document.getElementById('marshalZone10').textContent = sessionMarshalZoneFlag[dataSESSION.marshalZones9_zoneFlag + 1];
        document.getElementById('marshalZone11').textContent = sessionMarshalZoneFlag[dataSESSION.marshalZones10_zoneFlag + 1];
        document.getElementById('marshalZone12').textContent = sessionMarshalZoneFlag[dataSESSION.marshalZones11_zoneFlag + 1];
        document.getElementById('marshalZone13').textContent = sessionMarshalZoneFlag[dataSESSION.marshalZones12_zoneFlag + 1];
        document.getElementById('marshalZone14').textContent = sessionMarshalZoneFlag[dataSESSION.marshalZones13_zoneFlag + 1];
        document.getElementById('marshalZone15').textContent = sessionMarshalZoneFlag[dataSESSION.marshalZones14_zoneFlag + 1];
        document.getElementById('marshalZone16').textContent = sessionMarshalZoneFlag[dataSESSION.marshalZones15_zoneFlag + 1];
        document.getElementById('marshalZone17').textContent = sessionMarshalZoneFlag[dataSESSION.marshalZones16_zoneFlag + 1];
        document.getElementById('marshalZone18').textContent = sessionMarshalZoneFlag[dataSESSION.marshalZones17_zoneFlag + 1];
        document.getElementById('marshalZone19').textContent = sessionMarshalZoneFlag[dataSESSION.marshalZones18_zoneFlag + 1];
        document.getElementById('marshalZone20').textContent = sessionMarshalZoneFlag[dataSESSION.marshalZones19_zoneFlag + 1];
        document.getElementById('marshalZone21').textContent = sessionMarshalZoneFlag[dataSESSION.marshalZones20_zoneFlag + 1];
    }
    socket.onopen = () => {
        console.log("Socket Connected");
    }
    socket.onmessage = (event) => {
        const data = JSON.parse(event.data);
        renderCarTelemetry(data.CARTELEMETRY);
        renderSessionTelemetry(data.SESSION);
    }
    socket.onerror = (err) => {
        console.error("Socket Disconnected", err);
    }
    setInterval(() => {
        if (socket.readyState === WebSocket.OPEN) socket.send('__ping__');
    }, 17);
});