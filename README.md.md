# VR Interaction Module: Shape-Socket Puzzle Experience

An immersive VR spatial interaction puzzle built with **Unity 6 LTS** and the **Meta XR Interaction SDK**. The module demonstrates precision hand tracking, controller input, physics-based spatial manipulation, audio-visual state feedback, dynamic UI dashboard tracking, and wrist-anchored menus.

---

## 🚀 Key Features

* **Physics & Spatial Interaction**: Grabbable 3D primitives (Cube, Sphere, Cylinder) leveraging Meta's `Grabbable` and `HandGrabInteractable` pipelines with continuous collision detection.
* **Dual-Validation Sockets**: Shape-matching sockets combining Unity physics triggers with a non-allocating proximity fallback (`Physics.OverlapSphereNonAlloc`) to guarantee docking even during rapid hand movement.
* **Audio-Visual Feedback**:
  * Material transitions on sockets from default blue to success green upon correct placement.
  * Spatialized, debounced 3D audio cues (`Success_SFX` and `Wrong_SFX`).
  * Dynamic error messaging displaying shape mismatch warnings.
* **Auto-Recovery & Safety Loops**:
  * Out-of-bounds recovery that returns dropped or misplaced shapes back to their starting table coordinates.
  * 60-second task countdown timer with automated task reset loops upon timeout (`00:00`).
* **Dashboard & Wrist-Mounted UI**:
  * Central world-space Dashboard tracking active remaining objects and live countdown.
  * Left wrist-anchored menu providing one-click options to **Reset Objects**, **Restart Task**, and **Toggle Hand Tracking Visuals**.
* **Task Completion Gate**: Smooth animated opening of the exit door pivot once all three sockets are solved.
* **Targeted Optimization**: Framerate-capped (72 FPS), zero-allocation physics queries, and disabled real-time shadow cascades to prevent thermal throttling on standalone chipsets.

---

## 🛠️ Technical Specifications

| Specification | Target / Value |
| :--- | :--- |
| **Engine Version** | Unity 6 LTS (6000.0.x) |
| **Render Pipeline** | Universal Render Pipeline (URP) |
| **Target Platform** | Android (Meta Quest 2, Quest 3, Quest Pro) |
| **XR Plugin** | OpenXR with Meta XR Feature Group |
| **SDK** | Meta XR Interaction SDK Core & Hand Tracking |
| **Scripting Backend** | IL2CPP |
| **Target Architecture** | ARM64 |
| **Color Space** | Linear |
| **Graphics API** | Vulkan (Primary) |

---

## 📂 Scene Hierarchy

```text
MainScene
├── Directional Light             (Realtime, Shadows: Disabled)
├── OVRCameraRigInteraction       (Camera Rig & Synthetic Hand/Controller Pipeline)
│   └── OVRInteraction
│       └── InteractionRig
│           └── Tracking
│               └── LeftHand / LeftHandAnchor
│                   └── WristAnchor
│                       └── Wrist_Canvas (World-Space, HandVisualToggle)
├── Environment                   (Walls, Floor, Room Bounds)
├── Table                         (Static Box Collider, IsTrigger: false)
├── Door                          (Exit frame)
│   └── DoorPivot                 (Animated Transform)
├── INTERACTABLES
│   ├── InteractableCube          (BoxCollider, Rigidbody, DraggableShape: Cube)
│   ├── InteractableSphere        (SphereCollider, Rigidbody, DraggableShape: Sphere)
│   └── InteractableCylinder      (CapsuleCollider, Rigidbody, DraggableShape: Cylinder)
├── SOCKETINTERACTORS
│   ├── SocketCube                (Trigger Collider, AudioSource, MetaSnapFeedback: Cube)
│   ├── SocketSphere              (Trigger Collider, AudioSource, MetaSnapFeedback: Sphere)
│   └── SocketCylinder            (Trigger Collider, AudioSource, MetaSnapFeedback: Cylinder)
├── PuzzleManager                 (Coordinator: timer, state tracking, completion logic)
├── DashBoardCanvas               (World-Space Canvas, Ray Interactable)
└── EventSystem                   (PointableCanvasModule)

💻 Editor & Simulator Setup
1. Prerequisites
Unity Hub installed with Unity 6 LTS.

Build modules installed: Android Build Support, OpenJDK, and Android SDK & NDK Tools.

Meta XR Core SDK and Interaction SDK installed.

2. Loading the Project
Open Unity Hub and click Add > Add project from disk.

Select the repository root folder.

Open the project and navigate to Assets/Scenes/MainScene.unity.

3. Testing in Meta XR Simulator
Open the simulator via Meta XR SDK > Meta XR Simulator > Show Simulator.

Recommended simulator settings for smooth performance:

Viewport: Left eye (mono mode to minimize GPU draw calls).

Refresh rate: 72 fps.

In the Unity Editor, keep the Console tab active over the Game view to prevent duplicate rendering stalls.

Press Play:

Look Around: Hold Right Mouse Button + WASD.

Interact: Use synthetic controller/hand hotkeys (Space, Shift, Left Mouse Click) to grab objects and target UI rays.

📦 Standalone Quest Build Instructions (APK)
Open File > Build Profiles (or File > Build Settings).

Select Android and click Switch Platform if not already active.

Verify Project Settings:

XR Plug-in Management: Under the Android tab, ensure OpenXR is selected with the Meta XR feature group enabled.

Player > Other Settings:

Graphics API: Vulkan

Scripting Backend: IL2CPP

Target Architecture: ARM64 (uncheck ARMv7)

Minimum API Level: Android 10.0 (API Level 29) or Android 12.0 (API Level 32)

Ensure Assets/Scenes/MainScene.unity is included in Scenes in Build.

Set Texture Compression to ASTC.

Connect your Quest headset via USB-C with Developer Mode enabled.

Click Build and Run (or Build to generate the standalone .apk for SideQuest or Meta Quest Developer Hub installation).