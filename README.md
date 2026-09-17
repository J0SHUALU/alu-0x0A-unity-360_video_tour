# Extended Immersive Tour

A 360° VR tour for Meta Quest 2, made in Unity 6. It extends my Unity 360 Video Tour project from the ALU Intranet.

The app opens on a main menu. From there you can take the original Intranet tour or a tour of my own campus that I shot with a 360 camera.

## Scenes

- **MainMenuScene**: the start screen, with buttons for both tours.
- **IntranetTourScene**: the original Intranet project. It has the four 360 video rooms (LivingRoom, Cantina, Cube, Mezzanine), hotspots, info boxes, background music and fades.
- **CustomCampusTourScene**: three 360 photos I took on campus, linked in walking order: Main Hall → Enterprise Courtyard → Brick Walkway.

## Navigation

- **Main menu**: Intranet Tour or Custom Campus Tour.
- **Intranet tour**: a bar just below eye level has Main Menu and Campus Tour buttons.
- **Campus tour**: the same bar has Main Menu and Intranet Tour buttons.

Inside each tour, hotspots move you between rooms. Every room change and scene change fades to black and back, so nothing jumps suddenly.

## Controls

- Point a controller at a button and pull the trigger.
- You can also look at a button for a couple of seconds to select it.
- Info buttons open and close a short description of the room.

## How it works

- **XR setup**: OpenXR with Meta Quest support, and the XR Interaction Toolkit rig for the headset and controller rays.
- **360 rooms**: each room is an inside-out sphere. Intranet rooms play their video through a Video Player and Render Texture. Campus stops use my photos as textures.
- **Scripts** (in `Assets/Scripts`):
  - `RoomNavigator`: switches rooms inside a scene.
  - `SceneTransition`: fades out, loads the next scene, then fades back in.
  - `SceneNavButton`, `HotspotInteraction` and `InfoPanelToggle`: handle the buttons.
  - `ScreenFader`: the fade overlay.
  - `RoomHeadAnchor`: keeps the sphere centred on your head so the view doesn't warp.
- **Campus tour data**: stop names, info text and hotspot directions are set in `Assets/CampusMedia/campus_tour.json`. The scene is rebuilt from that file with **Tools → Extended Tour**.
- **My photos**: `Assets/CampusMedia/Captures`.

## Running it

1. Open the project in Unity 6000.4.7f1 with Android Build Support.
2. Plug in a Quest 2 with USB debugging allowed.
3. Use **File → Build And Run**.

## Credits

Music: "Tech Live" by Kevin MacLeod (incompetech.com), licensed under Creative Commons: By Attribution 4.0 (http://creativecommons.org/licenses/by/4.0/).
