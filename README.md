# Automated Variable-Density CAD Ingestion & Real-Time Digital Twin Pipeline

An enterprise-grade cyber-physical digital twin framework. This system ingests raw, continuous mathematical industrial engineering files (`.STEP`) via an open-source geometric kernel, optimizes them programmatically into variable-density LOD meshes, and bridges them to an active asynchronous mock IoT telemetry dashboard running on Unity's modern UI Toolkit.

---

## 🏗️ System Architecture & Data Routing

The application utilizes a decoupled microservice layout to ensure that heavy geometric calculations never block real-time rendering or tracking view loops:


[ Raw CAD Data (.STEP) ]
│
▼
[ Open CASCADE C++ Worker (CLI) ] ──► Computes Tessellation & Surface Triangulation
│
▼ (Outputs Web-Ready .gltf Mesh File)
[ Unity Frontend UI Ingestion ]  ──► Streams geometry into Universal Render Pipeline (URP)
│
▼
[ Asynchronous Telemetry Feed ]  ──► Binds live mock IoT data variables to UI Toolkit Panel

---

## 🛠️ Tech Stack & Dependencies

- **Visual Computing Engine:** Unity 6 / Universal Render Pipeline (URP)
- **Geometric Translation Kernel:** Open CASCADE Technology (OCCT) 8.0.1 SDK (Native C++)
- **C++ Build Chain:** Microsoft Visual C++ Compiler (MSVC) / Visual Studio 2022
- **Data Model Serialization:** glTFast (`com.unity.cloud.gltfast`)
- **UI Architecture:** Unity UI Toolkit (UXML data mapping layouts / USS stylesheet formatting)

---

## 🚀 Implemented Features

### 1. Headless CAD Tessellation Engine
- Compiled a custom C++ command-line utility linked to modular Open CASCADE 8.0 libraries (`TKCAF`, `TKDESTEP`, `TKDEGLTF`, `TKernel`).
- Triangulates infinitely smooth NURBS/B-Rep surfaces into explicit coordinate point vertex grids and triangular index arrays cleanly.
- Refactored legacy Open CASCADE data types to native C++ primitives (`double`, `bool`, `int`) for warning-free modern memory allocation.

### 2. Multi-Density Parallel LOD Generation Pipeline
- Engineered an asynchronous Unity C# manager script utilizing multi-threaded operations (`Task.Run`) to isolate heavy geometry calculations from the main thread graphics loop.
- Automatically executes the C++ background worker three times sequentially using varying linear deflection limits (`0.02` smooth close-up, `0.2` mid-poly, `0.8` blocky far-away mesh).
- Instantiates the structural array outputs dynamically at runtime into a native Unity `LODGroup` component, reducing GPU vertex shader draw call loads by up to 90% at a distance.

### 3. Reactive Cyber-Physical IoT Dashboard
- Constructed an industrial monitor panel using Unity’s modern web-style UI Toolkit layout systems.
- Runs an independent coroutine polling loop (500ms cycles) that simulates real-world underwater XLUUV vessel diagnostics (*Depth scaling metrics, proportional Hull Pressure curves, and fluctuating Core Temperatures*).
- Implemented an automated tracking lifecycle listener that activates emergency blinking alert statuses across the visual tree if operational values cross critical security thresholds.

---

## 📈 Engineering Case Study: Memory & Performance Optimization

### The Profiler Bottleneck
During initial milestone deployments of the reactive warning loop, the Unity Profiler identified extreme spikes in the **"Others" CPU category**, locking frame times at a sub-30 FPS boundary (**32.53ms**). 

Deep analysis traced the leak directly to standard material array calls (`renderer.materials`), which forced the native C++ engine to repeatedly instantiate duplicate material asset array clones in RAM every half-second, overloading the Garbage Collector.

### The Refactoring Solution
By refactoring the update cycle to utilize **`MaterialPropertyBlock`** structures and referencing assets via `sharedMaterials`, the application completely bypassed RAM allocation loops. The flashing warning vector values are now pushed directly into the GPU's native instance data channel without mutating the original mesh files. 

**Result:** The "Others" processing spike was completely flattened, restoring fluid, high-performance rendering targets mandatory for safe spatial VR tracking.
