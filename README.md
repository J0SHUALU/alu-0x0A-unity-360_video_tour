# alu-0x0A-unity-360_video_tour

A 360-degree video VR tour built with Unity, targeting Meta Quest.

## Description

This project lets users explore four different 360-degree video environments (LivingRoom, Cantina, Cube, and Mezzanine) by navigating between them through interactive hotspots placed inside each scene.

The tour starts in the **LivingRoom**. From there, users can move to connected rooms using on-screen navigation buttons. Each room plays a looping 360-degree video rendered on the inside of a sphere using Video Player and Render Textures.

## Navigation Map

- **LivingRoom** (Start) -> Cantina, Cube
- **Cantina** -> LivingRoom, Cube
- **Cube** -> LivingRoom, Cantina, Mezzanine
- **Mezzanine** -> Cube

## Setup

1. Clone this repository
2. Open the project in Unity 6000.4.7f1 or later
3. Open the `360VideoTour` scene from `Assets/Scenes/`
4. Press Play to preview in the Editor, or build for Android (Meta Quest)

## Build Target

- **Platform**: Android
- **Device**: Meta Quest
- **XR**: OpenXR with Meta Quest feature profile

## Project Structure

```
Assets/
├── audio/
│   ├── BGMMixer.mixer
│   └── freemusicbg.com-Tech Live.mp3
├── Videos/
│   ├── LivingRoom.mp4
│   ├── Cantina.mp4
│   ├── Cube.mp4
│   └── Mezzanine.mp4
├── RenderTextures/
├── Materials/
├── Scripts/
│   ├── GazeRaycaster.cs
│   ├── HotspotInteraction.cs
│   ├── InfoPanelToggle.cs
│   ├── RoomNavigator.cs
│   └── ScreenFader.cs
├── Sprites/
│   ├── HotspotIcon.png
│   └── InfoIcon.png
└── Scenes/
    └── 360VideoTour.unity
```

## Audio Attribution

"Tech Live" Kevin MacLeod (incompetech.com)
Licensed under Creative Commons: By Attribution 4.0 License
http://creativecommons.org/licenses/by/4.0/
