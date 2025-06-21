# 🏁 Racing Telemetry Analysis & Coaching System

A full-stack, machine learning-enabled telemetry web application that records real-time driving data and provides feedback on racing technique using insights from professional e-sports drivers across multiple racing classes. Built with C#, .NET Worker Services, PHP, MySQL, and Python, and deployed via Docker containers on Microsoft Azure.

---

## 🔍 Problem Statement

As a motorsport enthusiast, I wanted to **track and analyze** my performance during simulated racing sessions. Commercial tools often lack customization or are limited to specific games or hardware. I needed a **self-hosted solution** that could:
- Record telemetry data from my races
- Store and process the data securely
- Train and serve a machine learning model to give feedback based on elite e-sports drivers
- Run efficiently and scalably via **Dockerized deployment on Azure**

---

## 🎯 Project Goals

- Create a full-featured telemetry **web app server** for racing simulation data
- Train a **machine learning model** to detect driving inefficiencies and recommend improvements
- Compare personal performance with data from **Formula 1, Le Mans Hypercar, GT3, and Rally e-sports drivers**
- Deploy the system using **Docker** on **Microsoft Azure** for real-world performance testing
- Improve expertise in ML pipelines, server architecture, and cloud-native deployment

---

## 🛠️ Technologies Used

### 🧩 Architecture & Backend
- **.NET Worker Service** — Continuous background telemetry service
- **C#** — Core backend server logic and telemetry ingestion
- **MySQL** — Relational database for storing lap/session data
- **PHP** — Frontend interface for user interaction and analytics display
- **F#** — Used in backend to call ML prediction functions when needed

### 📊 Machine Learning
- **Python** — Training and testing of ML models
- **Libraries:** TensorFlow, scikit-learn, XGBoost, pandas, NumPy, Matplotlib

### 🐳 DevOps & Deployment
- **Docker** — Containerized the full stack for portability
- **YAML** — Docker Compose and Azure Container configuration
- **Azure Repos** — Version control and CI/CD integration
- **Azure Container Apps** — Scalable, managed container hosting

---

## 🧠 Machine Learning Model

- **Input Features:**
  - Throttle/brake percentages
  - Steering angles
  - G-forces
  - Lap times, sector times
  - Gear shifts and RPM
- **Training Data:**
  - Personal lap data
  - Open-source professional e-sports datasets
- **Output:**
  - Driving performance ratings
  - Targeted feedback on:
    - Braking zones
    - Corner entry/exit speeds
    - Acceleration curves

---

## 📈 Challenges Faced

- **Language Dilemma:** Choosing between Python and F# for ML training. Python won due to its mature ecosystem and extensive support (TensorFlow, PyTorch, XGBoost, pandas, etc.).
- **Data Management:** Normalizing telemetry from different racing classes and formats
- **Integration:** Ensuring smooth interoperability between C#, PHP, MySQL, and Python
- **Cloud Deployment:** Learning the best practices for deploying microservices on Azure using Docker and Container Apps

---

## 📚 Lessons Learned

### ⚙️ Software & Cloud Engineering
- Built and managed a **.NET-based hosted service**
- Used **Docker Compose** to streamline local and cloud deployments
- Learned to integrate multi-language services across **C#, F#, Python, and PHP**
- Managed infrastructure and deployment pipelines using **Azure Repos** and **Azure Container Apps**

### 🧠 Machine Learning
- Trained a model on complex time-series telemetry data
- Gained fluency with **model lifecycle**, from training to serving
- Understood how to align ML insights with actionable driver feedback

### 🚗 Domain Knowledge
- Deepened understanding of racing telemetry standards
- Learned how to derive **coaching insights** from raw telemetry
- Benchmarked against real e-sports telemetry from **Formula 1**, **WEC**, and **Rally**

---

## 🚀 Future Enhancements

- Expand support to live WebSocket-based telemetry streaming
- Integrate OpenF1 API for additional pro racing benchmark data
- Add an interactive driving heatmap using JavaScript + D3.js
- Explore using **ONNX** for more efficient ML model serving with C#
- Introduce user profiles with driving history and skill tracking

---

> **Note:** This system is for educational and training purposes. It is not affiliated with FIA, WEC, or Codemasters F1.
