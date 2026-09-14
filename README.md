 Smart-X IoT Mesh Ecosystem — Part 1

A hybrid Internet of Things (IoT) telemetry ingestion platform built with .NET 10, ASP.NET Core Minimal API, and Blazor WebAssembly.

Project Overview

This is Part 1 of the Smart-X PoE. It implements the Sensor Data Ingestion and Telemetry pillar. Future parts will add the Real-Time Command Stream and Network Topology modules.

Architecture

- Backend API: `SmartX.Api` — ASP.NET Core Minimal API (.NET 10)
- Frontend: `SmartX.Client` — Standalone Blazor WebAssembly
- Communication: REST via HTTP + CORS

 Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Git



