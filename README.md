# BikeRepairXR

BikeRepairXR is a Unity XR bicycle repair experience built for Meta Quest. It combines hands-on interaction (tools, grabbing, locomotion) with an adaptive UI/optimization system called [AUIT](https://github.com/joaobelo92/auit) to place and update instruction canvases at runtime.

Core areas to look at:
- AUIT runtime/optimization logic lives in [Assets/AUIT/](Assets/AUIT/)
- Gameplay/AR interaction scripts live in [Assets/Scripts/](Assets/Scripts/)
- Scenes for the final experiments live in [Assets/Scenes/Finals/](Assets/Scenes/Finals/)
- Project/package dependencies are defined in [Packages/manifest.json](Packages/manifest.json)

Some glimpses of the interaction experiments are available in [Assets/InstructionVideos/](Assets/InstructionVideos/).

#### Pick up wrench

![Pick up wrench](Assets/InstructionVideos/Pick_up_wrench.gif)

#### Wrench–wheel interaction

![Wrench–wheel interaction](Assets/InstructionVideos/Wrench_Wheel_Interaction.gif)

#### Wrench–pedal interaction

![Wrench–pedal interaction](Assets/InstructionVideos/pedal-unscrew-wrench.gif)

#### Wheel grabbing interaction

![Wheel grabbing interaction](Assets/InstructionVideos/wheel-grab-interaction.gif)


## Requirements
- Unity Editor `6000.3.8f1` (see [ProjectSettings/ProjectVersion.txt](ProjectSettings/ProjectVersion.txt))
- Android build support installed via Unity Hub (Android SDK/NDK + OpenJDK)
- Target device: Meta Quest (Quest / Quest 2 / Quest Pro / Quest 3 / Quest 3S), as declared in [Assets/Plugins/Android/AndroidManifest.xml](Assets/Plugins/Android/AndroidManifest.xml)

## Usage Instructions

Run in Editor:
- Open [Assets/Scenes/Finals/IntroScene_Final.unity](Assets/Scenes/Finals/IntroScene_Final.unity) and press Play.
- To jump straight into a final experiment scene, open one of:
	- [Assets/Scenes/Finals/Experiment_1_Final.unity](Assets/Scenes/Finals/Experiment_1_Final.unity)
	- [Assets/Scenes/Finals/Experiment_2_Final.unity](Assets/Scenes/Finals/Experiment_2_Final.unity)
	- [Assets/Scenes/Finals/Experiment_3_Final.unity](Assets/Scenes/Finals/Experiment_3_Final.unity)

Build & run on Quest:
- Switch platform to Android (`File > Build Settings...`).
- Connect the headset with USB debugging enabled, then use `Build And Run`.


## Dependencies

Unity packages (see [Packages/manifest.json](Packages/manifest.json)):
- Meta XR SDK `85.0.0` (Interaction SDK, Platform, Audio/Haptics): https://developer.oculus.com/documentation/unity/
- Unity OpenXR (`com.unity.xr.openxr`): https://docs.unity3d.com/Packages/com.unity.xr.openxr@latest

NuGet .NET packages (see [Assets/packages.config](Assets/packages.config); restore settings in [Assets/NuGet.config](Assets/NuGet.config)):
- NetMQ: https://github.com/zeromq/netmq
- AsyncIO (NetMQ dependency): https://www.nuget.org/packages/AsyncIO
- Newtonsoft.Json (Json.NET): https://www.newtonsoft.com/json
- Numpy: https://github.com/SciSharp/Numpy.NET
- pythonnet: https://github.com/pythonnet/pythonnet
- Python.Included / Python.Deployment (embedded Python runtime for .NET workflows): https://www.nuget.org/packages/Python.Included and https://www.nuget.org/packages/Python.Deployment


## Project structure

```text
├─ README.md
├─ Assets/
|  ├─ allen_wrench/
│  ├─ AUIT/
│  ├─ Editor/
│  ├─ InstructionVideos/
│  ├─ InteractionSDK/
│  ├─ Models/
│  ├─ NuGet/
│  ├─ Oculus/
│  ├─ Packages/
│  ├─ Plugins/
│  ├─ Prefabs/
│  ├─ Resources/
│  ├─ Scenes/
│  │  └─ Finals/
│  ├─ Scripts/
│  ├─ Settings/
│  ├─ Simple Garage/
|  ├─ UserStudyDocuments/
│  ├─ XR/
├─ Packages/
├─ ProjectSettings/
└─ UserSettings/
```

