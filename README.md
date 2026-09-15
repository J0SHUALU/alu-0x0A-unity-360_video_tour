# alu-0x0A-unity-360_video_tour

**Extended Immersive Tour** is a 360° VR experience for Meta Quest 2, built with Unity.

The app opens on a welcome menu with two tours:

- **Intranet Tour**: the Unity 360 Video Tour project from the ALU Intranet. It has four Holberton rooms filmed in 360° video.
- **Custom Campus Tour**: a walk through my campus, made from my own 360° captures.

Every scene can open the others. Each scene change fades to black.

## Scenes

| Scene | Path | Purpose |
|---|---|---|
| `MainMenuScene` | `Assets/Scenes/MainMenuScene.unity` | Entry point. Shows the title and two large buttons (Intranet Tour, Custom Campus Tour) over a 360° backdrop |
| `IntranetTourScene` | `Assets/Scenes/IntranetTourScene.unity` | The original Intranet project, with scene navigation added |
| `CustomCampusTourScene` | `Assets/Scenes/CustomCampusTourScene.unity` | Three or more original 360° captures, linked in walking order |

The build loads the scenes in this order, with `MainMenuScene` first.

## Navigation flow

```
                 ┌──────────────────┐
                 │  MainMenuScene   │
                 └───┬──────────┬───┘
      Intranet Tour  │          │  Custom Campus Tour
                     ▼          ▼
  ┌───────────────────┐  ◄──►  ┌────────────────────────┐
  │ IntranetTourScene │        │ CustomCampusTourScene  │
  └───────────────────┘        └────────────────────────┘
        │  Main Menu                     │  Main Menu
        └──────────────► MainMenuScene ◄─┘
```

- **Main menu**: Intranet Tour or Custom Campus Tour.
- **Intranet tour**: the navigation bar has **Main Menu** and **Campus Tour**.
- **Campus tour**: the navigation bar has **Main Menu** and **Intranet Tour**.

Inside a tour, you move between rooms with the hotspots in each 360° sphere.

### Intranet room map

- **LivingRoom** (start) → Cantina, Cube
- **Cantina** → LivingRoom, Cube
- **Cube** → LivingRoom, Cantina, Mezzanine
- **Mezzanine** → Cube

### Custom campus route

The stops are linked in the order you walk them: **Stop 1 ↔ Stop 2 ↔ Stop 3**. Each stop has a hotspot forward, a hotspot back, and an info button, so the tour never jumps between unconnected places.

## Controls (Meta Quest 2)

- **Controllers**: point a Touch controller ray at a hotspot, info button or menu button, then pull the trigger.
- **Gaze**: look at a hotspot or info button for about 2 seconds, or at a menu or navigation button for 3 seconds. A bar under each menu button fills while you look at it.
- **Hover feedback**: buttons grow and light up while you point at them.
- **Navigation bar**: it sits just below your line of sight. When you turn more than 45°, it swings round to follow you, so you can always find it by looking down.

## Comfort and readability

- **Fades**: room changes and scene changes both fade to black. Scene changes also fade the music out and back in.
- **Head-centred spheres**: each 360° sphere stays centred on your head, so leaning or walking never warps the view or makes you feel like you're floating.
- **No locomotion**: the XR rig's movement is turned off. You turn with your head, never with the stick.
- **Spacing**: hotspots and info buttons sit on a ring 10 m away and are spaced so they never overlap. Info boxes open directly above their button and fit in your field of view.
- **Hit areas**: every button has a large invisible hit area, so the ray catches the whole icon and label.
- **Text**: all text is large, light-on-dark and high-contrast.

## Project structure

```
Assets/
├── Scenes/
│   ├── MainMenuScene.unity
│   ├── IntranetTourScene.unity
│   └── CustomCampusTourScene.unity
├── Scripts/
│   ├── RoomNavigator.cs         switches rooms inside a scene (fades, video playback)
│   ├── SceneTransition.cs       loads scenes with a picture and sound fade
│   ├── SceneNavButton.cs        VR button that opens a scene (ray click or gaze)
│   ├── ScreenFader.cs           fade overlay in front of the eyes
│   ├── HotspotInteraction.cs    room hotspot (ray click or gaze)
│   ├── InfoPanelToggle.cs       opens and closes an info box
│   ├── GazeRaycaster.cs         gaze pointer from the center of view
│   ├── HeadFollower.cs          keeps the menu and navigation bar near the viewer
│   ├── RoomHeadAnchor.cs        keeps 360 spheres centered on the head
│   └── RayLengthExtender.cs     lengthens controller rays to reach the spheres
├── Editor/
│   ├── ExtendedTourBuilder.cs   Tools > Extended Tour: builds and rebuilds the scenes
│   ├── TourLayout.cs            places hotspots and info buttons on the sphere
│   ├── TourAssetImporter.cs     import settings for 360 photos and UI images
│   └── TourBuilder.cs           Tools > Extended Tour > Build APK (Meta Quest 2)
├── CampusMedia/
│   ├── campus_tour.json         stops, titles, info text and hotspot angles
│   ├── Captures/                my original 360 captures (Capture1, Capture2, Capture3)
│   ├── Placeholders/            shown until a capture is added
│   └── Generated/               materials and render textures made by the builder
├── UI/                          menu backdrop, panel sprite and icons
├── Videos/                      LivingRoom, Cantina, Cube, Mezzanine (4096x2048 H.264)
├── RenderTextures/              one 4096x2048 render texture per Intranet room
├── Materials/
├── Sprites/                     hotspot and info icons
├── audio/                       BGMMixer and the "Tech Live" track
└── XR/, XRI/, Samples/          OpenXR and XR Interaction Toolkit settings
```

## Adding your 360 captures

1. **Export equirectangular files from the camera app.** Use 2:1 photos (JPG) or videos (H.264 MP4, 4096×2048 or smaller, which Quest 2 can decode).
2. **Copy them into `Assets/CampusMedia/Captures/`.** Name them `Capture1`, `Capture2` and `Capture3`, in walking order (for example `Capture1.jpg`, `Capture2.mp4`). If a stop has both a video and a photo, the video is used.
3. **Edit `Assets/CampusMedia/campus_tour.json`.**
   - Fill in each stop's `title` and `info` text.
   - `nextYaw` and `backYaw` are the directions of the forward and back hotspots, in degrees (0 = straight ahead at start, 90 = right).
   - `mediaYaw` turns the whole capture so that its doorway or path lines up with the hotspot.
   - To add a stop, add another entry and another `CaptureN` file.
4. **Run Tools → Extended Tour → Steps → 4 Build Custom Campus Tour Scene** to regenerate the scene.

Photos are imported at 4096×2048 with ASTC compression for Quest 2 (see `TourAssetImporter`).

## Setup and build

1. **Open the project in Unity 6000.4.7f1.** Android Build Support must be installed.
2. **Build the scenes.** Run **Tools → Extended Tour → Build All Scenes** once. It will:
   - rename the Intranet scene to `IntranetTourScene`
   - add its navigation bar
   - build `MainMenuScene` and `CustomCampusTourScene`
   - set the build scene order
3. **Build for the headset.** Connect the Quest 2 with USB debugging allowed, then either:
   - use **File → Build And Run**, or
   - run **Tools → Extended Tour → Build APK (Meta Quest 2)**. The APK goes to `Builds/MetaQuest2/`.

## Build target

- **Platform**: Android (ARM64, IL2CPP, Vulkan)
- **Device**: Meta Quest 2
- **XR**: OpenXR with Meta Quest Support and the Oculus Touch controller profile
- **Rig**: XR Interaction Toolkit XR Origin (XR Rig), with device tracking and locomotion turned off

## Audio attribution

"Tech Live" Kevin MacLeod (incompetech.com)
Licensed under Creative Commons: By Attribution 4.0 License
http://creativecommons.org/licenses/by/4.0/
