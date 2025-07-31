# Racing Telemetry

# Real-Time Data Ingestion, Storage, and Visualization System

### Description

- A high-performance, modular telemetry platform that ingests real-time data from racing simulations (e.g., F1 24), processes it using UDP and WebSocket protocols, and presents insights via a structured MySQL database and an optional web frontend
- Engineered with Docker support for ease of deployment

---

## NOTICE

- Please read through this `README.md` to better understand the project's source code and setup instructions
- Also, make sure to review the contents of the `License/` directory
- Your attention to these details is appreciated — enjoy exploring the project!

---

## Problem Statement

- Formula racing simulations generate large volumes of real-time telemetry data, which is invaluable for performance analysis, driver behavior monitoring, and strategic decisions
- This project provides an efficient and scalable way to capture, structure, and visualize that data for both offline analysis and live dashboards

---

## Project Goals

### Real-Time Data Capture from Simulation

- Capture telemetry packets over UDP from games like Codemasters F1 24 and decode them using a packet reader

### Multi-Protocol Forwarding and Storage

- Forward parsed data to WebSocket clients for real-time display and to a MySQL database for persistent storage and analytics

---

## Tools, Materials & Resources

### Tools

- Docker, Docker Compose, MySQL 8, Python, WebSockets, Codemasters UDP telemetry protocol

### Materials

- Game telemetry stream (F1 24), Custom WebSocket server/client

### Resources

- OpenF1 API (for enriching or validating telemetry data), Codemasters F1 UDP specification documentation

---

## Design Decision

### Dockerized Architecture

- All core services are containerized to allow simple, portable deployment

### Separation of Concerns

- Worker handles parsing and routing, while storage and live visual feedback are decoupled via WebSockets and SQL layers

### Scalable Design

- Structured for horizontal scaling of ingestion, processing, and database operations via clustering or connection pooling

---

## Features

### UDP Telemetry Capture

- Parses Codemasters telemetry packets in real-time using structured packet definitions

### WebSocket Forwarding

- Broadcasts processed data to a central WebSocket server for integration with web clients or dashboards

### SQL Data Logging

- Inserts telemetry records into a MySQL database for long-term storage and analytical processing

---

## Block Diagram

```plaintext
┌─────────────────────┐   UDP  ┌──────────────────────┐WebSocket┌─────────────────────┐   PHP   ┌─────────────────────┐
│ Codemasters F1 Game ├── → ───┤  Telemetry Worker    ├─── → ───┤    Proxy Server     ├─── → ───┤  Web UI / Dashboard │
│    (UDP Output)     │        │ (UDP → JSON Parser)  │         │                     │         │    (Live View)      │
└─────────────────────┘        └──────────────────────┘         └─────────────────────┘         └─────────────────────┘
                                                   │
                                                   │ SQL INSERT
                                                   ↓
                                           ┌───────▼──────────┐
                                           │   MySQL Storage  │
                                           │  (Telemetry DB)  │
                                           └──────────────────┘
```

---

## Functional Overview

- The telemetry worker listens for UDP packets, decodes them into structured JSON, forwards them over WebSocket, and logs selected data to a MySQL instance
- The architecture supports containerized deployment via Docker Compose

---

## Challenges & Solutions

### Codemasters telemetry formats vary by game version

- Abstracted parsing logic to adapt to multiple formats

### Frequent UDP packets risk overwhelming system

- Optimized async I/O and optional data throttling

---

## Lessons Learned

### Protocol Debugging is Critical

- Packet mismatches led to data corruption, validating against the official spec was essential

### Modular Services Enable Growth

- Splitting responsibilities into services helped add database and WebSocket support independently

---

## Project Structure

```plaintext
root/
├── License/
│   ├── LICENSE.md
│   │
│   └── NOTICE.md
│
├── .gitattributes
│
├── .gitignore
│
├── README.md
│
├── PacketVisionListener/
│   ├── Dockerfile
│   │
│   ├── PacketVisionListener.csproj
│   │
│   ├── Program.cs
│   │
│   └── Worker.cs
│
├── PacketVisionWeb/
│   ├── Dockerfile
│   │
│   ├── F1Wide.otf
│   │
│   ├── api.php
│   │
│   ├── composer.json
│   │
│   ├── composer.lock
│   │
│   ├── db.php
│   │
│   ├── favicon.png
│   │
│   ├── functions.php
│   │
│   ├── index.php
│   │
│   ├── script.js
│   │
│   ├── stylesheet.css
│   │
│   └── wait-for-it.sh
│
├── PacketVisionWebSocket/
│   ├── Dockerfile
│   │
│   ├── package-lock.json
│   │
│   ├── package.json
│   │
│   └── websocket.js
│
├── Dockerfile
│
├── docker-compose.yml
│
├── init.sql
│
├── officialPacketDoc.docx
│
└── publicPacketDoc.md

```

---

## Future Enhancements

- Add OpenF1 API integration for data enrichment
- Create configurable telemetry dashboards using React
- Implement message queue (e.g., Redis or Kafka) for buffering
- Add support for other racing games (iRacing, LeMans Ultimate, DiRT Rally 2.0)
