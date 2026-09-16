using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.XR.CoreUtils;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.XR.Interaction.Toolkit.UI;

/// <summary>
/// Builds the scenes of the Extended Immersive Tour:
/// MainMenuScene, IntranetTourScene (the original Intranet project with scene navigation added)
/// and CustomCampusTourScene (generated from Assets/CampusMedia/campus_tour.json).
/// Every step can be re-run safely.
/// </summary>
public static class ExtendedTourBuilder
{
    // Scene paths
    private const string OldIntranetScenePath = "Assets/Scenes/360VideoTour.unity";
    private const string IntranetScenePath = "Assets/Scenes/IntranetTourScene.unity";
    private const string MenuScenePath = "Assets/Scenes/MainMenuScene.unity";
    private const string CampusScenePath = "Assets/Scenes/CustomCampusTourScene.unity";

    // Shared assets
    private const string XROriginPrefabPath = "Assets/Samples/XR Interaction Toolkit/3.4.1/Starter Assets/Prefabs/XR Origin (XR Rig).prefab";
    private const string HotspotIconPath = "Assets/Sprites/HotspotIcon.png";
    private const string InfoIconPath = "Assets/Sprites/InfoIcon.png";
    private const string PanelSpritePath = "Assets/UI/RoundedPanel.png";
    private const string BackdropPath = "Assets/UI/MenuBackdrop.jpg";
    private const string IntranetIconPath = "Assets/UI/IntranetIcon.png";
    private const string CampusIconPath = "Assets/UI/CampusIcon.png";
    private const string HomeIconPath = "Assets/UI/HomeIcon.png";
    private const string ProgressSpritePath = "Assets/UI/ProgressFill.png";
    private const string MusicPath = "Assets/audio/freemusicbg.com-Tech Live.mp3";
    private const string MixerPath = "Assets/audio/BGMMixer.mixer";

    // Custom campus media
    private const string CampusConfigPath = "Assets/CampusMedia/campus_tour.json";
    private const string CapturesFolder = "Assets/CampusMedia/Captures";
    private const string PlaceholdersFolder = "Assets/CampusMedia/Placeholders";
    private const string GeneratedFolder = "Assets/CampusMedia/Generated";
    private const string MenuMaterialPath = "Assets/Materials/MenuBackdrop_Mat.mat";

    private static readonly string[] VideoExtensions = { ".mp4", ".mov", ".webm", ".m4v" };
    private static readonly string[] ImageExtensions = { ".jpg", ".jpeg", ".png", ".tif", ".tiff", ".exr", ".hdr" };

    // Colors of the UI theme
    private static readonly Color PanelColor = new Color(0.04f, 0.06f, 0.12f, 0.88f);
    private static readonly Color CardColor = new Color(0.12f, 0.16f, 0.28f, 0.95f);
    private static readonly Color AccentColor = new Color(0.36f, 0.72f, 1f, 1f);
    private static readonly Color SubtleText = new Color(0.78f, 0.84f, 0.95f, 1f);

    /// <summary>
    /// One stop of the custom campus tour as written in campus_tour.json.
    /// </summary>
    [Serializable]
    public class CampusStop
    {
        /// <summary>Unique object name of the stop, e.g. Stop1_Entrance.</summary>
        public string id;

        /// <summary>Name shown on hotspots and in the info box.</summary>
        public string title;

        /// <summary>Text shown in the stop's info box.</summary>
        public string info;

        /// <summary>File name (without extension) of the capture in Assets/CampusMedia/Captures.</summary>
        public string media;

        /// <summary>Yaw in degrees of the hotspot that leads to the next stop.</summary>
        public float nextYaw = 0f;

        /// <summary>Yaw in degrees of the hotspot that leads back to the previous stop.</summary>
        public float backYaw = 180f;

        /// <summary>Yaw in degrees of the info button.</summary>
        public float infoYaw = -60f;

        /// <summary>Extra yaw rotation applied to the capture so its front lines up with the route.</summary>
        public float mediaYaw = 0f;
    }

    /// <summary>
    /// The full campus tour definition.
    /// </summary>
    [Serializable]
    public class CampusTour
    {
        /// <summary>Title shown in the navigation bar.</summary>
        public string title = "Custom Campus Tour";

        /// <summary>Stops in walking order.</summary>
        public List<CampusStop> stops = new List<CampusStop>();
    }

    /// <summary>
    /// Runs every step: renames the Intranet scene, adds its scene navigation,
    /// builds the main menu and campus scenes, and sets the build scene list.
    /// </summary>
    [MenuItem("Tools/Extended Tour/Build All Scenes", priority = 0)]
    public static void BuildAll()
    {
        EditorSceneManager.SaveOpenScenes();
        RenameIntranetScene();
        AddNavigationToIntranet();
        BuildMainMenu();
        BuildCampusTour();
        ConfigureBuildScenes();
        EditorSceneManager.OpenScene(MenuScenePath);
        Debug.Log("Extended Immersive Tour: all scenes built.");
    }

    /// <summary>
    /// Renames the original 360VideoTour scene to IntranetTourScene, keeping its asset GUID.
    /// </summary>
    [MenuItem("Tools/Extended Tour/Steps/1 Rename Intranet Scene")]
    public static void RenameIntranetScene()
    {
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(OldIntranetScenePath) == null)
            return;

        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(IntranetScenePath) != null)
        {
            Debug.LogWarning("IntranetTourScene already exists, leaving 360VideoTour untouched.");
            return;
        }

        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        string error = AssetDatabase.MoveAsset(OldIntranetScenePath, IntranetScenePath);
        if (!string.IsNullOrEmpty(error))
        {
            Debug.LogError("Could not rename the Intranet scene: " + error);
        }
    }

    /// <summary>
    /// Adds the Main Menu and Campus Tour buttons to the Intranet scene.
    /// </summary>
    [MenuItem("Tools/Extended Tour/Steps/2 Add Navigation To Intranet Scene")]
    public static void AddNavigationToIntranet()
    {
        var scene = EditorSceneManager.OpenScene(IntranetScenePath, OpenSceneMode.Single);

        RemoveRoot(scene, "SceneNavigation");
        var nav = CreateNavigationBar("Intranet Tour", new[]
        {
            new NavTarget("Main Menu", SceneTransition.MainMenuScene, HomeIconPath),
            new NavTarget("Campus Tour", SceneTransition.CustomCampusTourScene, CampusIconPath)
        });
        SetCanvasCamera(nav, Camera.main);

        foreach (var fader in UnityEngine.Object.FindObjectsByType<ScreenFader>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            fader.fadeInOnStart = true;
            EditorUtility.SetDirty(fader);
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    /// <summary>
    /// Builds the MainMenuScene from scratch.
    /// </summary>
    [MenuItem("Tools/Extended Tour/Steps/3 Build Main Menu Scene")]
    public static void BuildMainMenu()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var camera = CreateXRRig();
        CreateEventSystem();
        CreateMusic();

        // 360 backdrop
        var backdrop = CreateSphere("Backdrop", null);
        backdrop.GetComponent<MeshRenderer>().sharedMaterial = GetOrCreateMaterial(MenuMaterialPath, Load<Texture>(BackdropPath));

        // Menu panel that stays in front of the viewer
        var menu = CreateWorldCanvas("MainMenu", null, new Vector2(1600f, 1000f), 0.0015f);
        var follower = menu.AddComponent<HeadFollower>();
        follower.distance = 3f;
        follower.height = 0f;
        follower.angleThreshold = 60f;
        menu.transform.position = new Vector3(0f, 0f, 3f);

        var panel = CreateImage("Panel", menu.transform, Load<Sprite>(PanelSpritePath), PanelColor, false);
        Stretch(panel.rectTransform);

        var accent = CreateImage("AccentBar", menu.transform, Load<Sprite>(PanelSpritePath), AccentColor, false);
        SetRect(accent.rectTransform, new Vector2(0f, 400f), new Vector2(220f, 10f));

        CreateText("Title", menu.transform, "Extended Immersive Tour", 92, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter,
            new Vector2(0f, 320f), new Vector2(1500f, 120f));
        CreateText("Subtitle", menu.transform, "Choose where you want to go", 40, FontStyle.Normal, SubtleText, TextAnchor.MiddleCenter,
            new Vector2(0f, 230f), new Vector2(1500f, 60f));

        CreateMenuCard(menu.transform, "IntranetTourButton", "Intranet Tour",
            "Holberton campus in four rooms,\nfilmed in 360 video",
            IntranetIconPath, SceneTransition.IntranetTourScene, new Vector2(-370f, -60f));
        CreateMenuCard(menu.transform, "CampusTourButton", "Custom Campus Tour",
            "A walk through my campus,\ncaptured with a 360 camera",
            CampusIconPath, SceneTransition.CustomCampusTourScene, new Vector2(370f, -60f));

        CreateText("Hint", menu.transform, "Point a controller and pull the trigger, or look at a card for 3 seconds",
            28, FontStyle.Italic, SubtleText, TextAnchor.MiddleCenter, new Vector2(0f, -395f), new Vector2(1500f, 44f));
        CreateText("Credit", menu.transform, "LayersbyJ  |  AR/VR Development with Unity", 24, FontStyle.Normal,
            new Color(1f, 1f, 1f, 0.5f), TextAnchor.MiddleCenter, new Vector2(0f, -455f), new Vector2(1500f, 40f));

        SetCanvasCamera(menu, camera);
        EditorSceneManager.SaveScene(scene, MenuScenePath);
    }

    /// <summary>
    /// Builds the CustomCampusTourScene from Assets/CampusMedia/campus_tour.json.
    /// Captures are read from Assets/CampusMedia/Captures; missing ones use placeholders.
    /// </summary>
    [MenuItem("Tools/Extended Tour/Steps/4 Build Custom Campus Tour Scene")]
    public static void BuildCampusTour()
    {
        var tour = LoadCampusTour();
        if (tour == null || tour.stops.Count == 0)
        {
            Debug.LogError("campus_tour.json has no stops.");
            return;
        }

        Directory.CreateDirectory(GeneratedFolder);
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var camera = CreateXRRig();
        CreateEventSystem();
        CreateMusic();

        var stopsRoot = new GameObject("CampusStops").transform;
        var stopObjects = new List<GameObject>();

        for (int i = 0; i < tour.stops.Count; i++)
        {
            var stop = tour.stops[i];
            var room = CreateSphere(stop.id, stopsRoot);
            room.transform.localRotation = Quaternion.Euler(0f, stop.mediaYaw, 0f);
            ApplyMedia(room, stop);
            stopObjects.Add(room);

            if (i + 1 < tour.stops.Count)
            {
                var next = tour.stops[i + 1];
                AddHotspot(room.transform, next, stop.nextYaw);
            }

            if (i > 0)
            {
                var previous = tour.stops[i - 1];
                AddHotspot(room.transform, previous, stop.backYaw);
            }

            AddInfo(room.transform, stop);
        }

        var manager = new GameObject("RoomManager");
        var navigator = manager.AddComponent<RoomNavigator>();
        navigator.rooms = stopObjects;
        navigator.startRoom = tour.stops[0].id;

        var nav = CreateNavigationBar(tour.title, new[]
        {
            new NavTarget("Main Menu", SceneTransition.MainMenuScene, HomeIconPath),
            new NavTarget("Intranet Tour", SceneTransition.IntranetTourScene, IntranetIconPath)
        });

        foreach (var canvas in UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            canvas.worldCamera = camera;
        }

        SetCanvasCamera(nav, camera);
        EditorSceneManager.SaveScene(scene, CampusScenePath);
    }

    /// <summary>
    /// Puts the three scenes into the build settings with the main menu first.
    /// </summary>
    [MenuItem("Tools/Extended Tour/Steps/5 Configure Build Scenes")]
    public static void ConfigureBuildScenes()
    {
        EditorBuildSettings.scenes = TourBuilder.Scenes
            .Where(path => AssetDatabase.LoadAssetAtPath<SceneAsset>(path) != null)
            .Select(path => new EditorBuildSettingsScene(path, true))
            .ToArray();
    }

    // A button in a navigation bar
    private struct NavTarget
    {
        public string label;
        public string scene;
        public string icon;

        public NavTarget(string label, string scene, string icon)
        {
            this.label = label;
            this.scene = scene;
            this.icon = icon;
        }
    }

    // Reads the campus tour definition
    private static CampusTour LoadCampusTour()
    {
        if (!File.Exists(CampusConfigPath))
        {
            Debug.LogError("Missing " + CampusConfigPath);
            return null;
        }

        return JsonUtility.FromJson<CampusTour>(File.ReadAllText(CampusConfigPath));
    }

    // Finds the capture for a stop: a video first, then a photo, then the placeholder
    private static UnityEngine.Object FindMedia(string mediaName, out bool isVideo)
    {
        isVideo = false;
        foreach (var ext in VideoExtensions)
        {
            var clip = AssetDatabase.LoadAssetAtPath<VideoClip>(CapturesFolder + "/" + mediaName + ext);
            if (clip != null)
            {
                isVideo = true;
                return clip;
            }
        }

        foreach (var ext in ImageExtensions)
        {
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(CapturesFolder + "/" + mediaName + ext);
            if (texture != null)
                return texture;
        }

        Debug.LogWarning("No capture found for " + mediaName + ", using the placeholder.");
        return AssetDatabase.LoadAssetAtPath<Texture2D>(PlaceholdersFolder + "/" + mediaName + "_Placeholder.png");
    }

    // Shows a stop's photo or video on its sphere
    private static void ApplyMedia(GameObject room, CampusStop stop)
    {
        bool isVideo;
        var media = FindMedia(stop.media, out isVideo);
        var materialPath = GeneratedFolder + "/" + stop.id + "_Mat.mat";

        if (isVideo)
        {
            var clip = (VideoClip)media;
            var rtPath = GeneratedFolder + "/" + stop.id + "_RT.renderTexture";
            var rt = AssetDatabase.LoadAssetAtPath<RenderTexture>(rtPath);
            if (rt == null)
            {
                rt = new RenderTexture(4096, 2048, 0, RenderTextureFormat.ARGB32);
                AssetDatabase.CreateAsset(rt, rtPath);
            }

            var player = room.AddComponent<VideoPlayer>();
            player.clip = clip;
            player.renderMode = VideoRenderMode.RenderTexture;
            player.targetTexture = rt;
            player.isLooping = true;
            player.playOnAwake = true;
            player.audioOutputMode = VideoAudioOutputMode.Direct;
            if (clip.audioTrackCount > 0)
            {
                player.SetDirectAudioVolume(0, 0.6f);
            }

            room.GetComponent<MeshRenderer>().sharedMaterial = GetOrCreateMaterial(materialPath, rt);
        }
        else
        {
            room.GetComponent<MeshRenderer>().sharedMaterial = GetOrCreateMaterial(materialPath, (Texture)media);
        }
    }

    // Adds a hotspot that leads to another stop
    private static void AddHotspot(Transform room, CampusStop target, float yaw)
    {
        var canvas = CreateWorldCanvas("Canvas_" + target.id + "Hotspot", room, new Vector2(420f, 120f), 0.008f);
        canvas.AddComponent<TrackedDeviceGraphicRaycaster>();

        var icon = CreateImage(target.id + "Hotspot", canvas.transform, Load<Sprite>(HotspotIconPath), Color.white, true);
        icon.gameObject.AddComponent<Button>().targetGraphic = icon;
        var hotspot = icon.gameObject.AddComponent<HotspotInteraction>();
        hotspot.targetRoomName = target.id;

        var label = CreateText("Text", canvas.transform, target.title, 48, FontStyle.Bold, Color.white, TextAnchor.MiddleLeft,
            Vector2.zero, new Vector2(280f, 100f));
        label.gameObject.AddComponent<Outline>().effectColor = new Color(0f, 0f, 0f, 0.8f);

        TourLayout.LayoutHotspot(canvas.transform);
        if (target.title.Length > 12)
        {
            label.fontSize = 36;
        }

        TourLayout.Place(room, canvas.transform, yaw);
    }

    // Adds the info button and info box of a stop
    private static void AddInfo(Transform room, CampusStop stop)
    {
        var canvas = CreateWorldCanvas("Canvas_Info_" + stop.id, room, new Vector2(620f, 400f), 0.008f);
        canvas.AddComponent<TrackedDeviceGraphicRaycaster>();

        var button = CreateImage("InfoButton", canvas.transform, Load<Sprite>(InfoIconPath), Color.white, true);
        button.gameObject.AddComponent<Button>().targetGraphic = button;
        var toggle = button.gameObject.AddComponent<InfoPanelToggle>();

        var panel = CreateImage("InfoPanel", canvas.transform, Load<Sprite>(PanelSpritePath), PanelColor, false);
        CreateText("Text", panel.transform, "<b>" + stop.title + "</b>\n" + stop.info, 30, FontStyle.Normal, Color.white,
            TextAnchor.MiddleCenter, Vector2.zero, new Vector2(560f, 190f));
        toggle.infoPanel = panel.gameObject;

        TourLayout.LayoutInfo(canvas.transform);
        panel.gameObject.SetActive(false);
        TourLayout.Place(room, canvas.transform, stop.infoYaw);
    }

    // Creates the XR rig used by the generated scenes and returns its camera
    private static Camera CreateXRRig()
    {
        var prefab = Load<GameObject>(XROriginPrefabPath);
        var rig = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        rig.transform.position = Vector3.zero;

        var origin = rig.GetComponent<XROrigin>();
        origin.RequestedTrackingOriginMode = XROrigin.TrackingOriginMode.Device;
        origin.CameraYOffset = 0f;

        foreach (var child in rig.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == "Locomotion")
            {
                child.gameObject.SetActive(false);
            }
        }

        var camera = origin.Camera;
        camera.nearClipPlane = 0.01f;
        camera.farClipPlane = 100f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = Color.black;
        camera.gameObject.AddComponent<GazeRaycaster>();

        // Fade overlay just in front of the eyes
        var faderCanvas = new GameObject("ScreenFaderCanvas", typeof(RectTransform), typeof(Canvas));
        var canvasRect = (RectTransform)faderCanvas.transform;
        canvasRect.SetParent(camera.transform, false);
        var canvas = faderCanvas.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = camera;
        canvas.sortingOrder = 100;
        canvasRect.localPosition = new Vector3(0f, 0f, 0.3f);
        canvasRect.sizeDelta = new Vector2(1000f, 1000f);
        canvasRect.localScale = Vector3.one * 0.003f;

        var overlay = CreateImage("FadeOverlay", faderCanvas.transform, null, Color.black, false);
        Stretch(overlay.rectTransform);

        var fader = faderCanvas.AddComponent<ScreenFader>();
        fader.fadeImage = overlay;
        fader.fadeInOnStart = true;

        return camera;
    }

    // Creates the event system that turns XR rays and gaze into UI events
    private static void CreateEventSystem()
    {
        new GameObject("EventSystem", typeof(EventSystem), typeof(XRUIInputModule));
    }

    // Creates the looping background music routed through the Music mixer group
    private static void CreateMusic()
    {
        var go = new GameObject("BGM");
        var source = go.AddComponent<AudioSource>();
        source.clip = Load<AudioClip>(MusicPath);
        source.loop = true;
        source.playOnAwake = true;
        source.spatialBlend = 0f;

        var mixer = Load<AudioMixer>(MixerPath);
        if (mixer != null)
        {
            var groups = mixer.FindMatchingGroups("Music");
            if (groups.Length > 0)
            {
                source.outputAudioMixerGroup = groups[0];
            }
        }
    }

    // Creates a 360 sphere seen from the inside
    private static GameObject CreateSphere(string name, Transform parent)
    {
        var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.name = name;
        UnityEngine.Object.DestroyImmediate(sphere.GetComponent<Collider>());
        sphere.transform.SetParent(parent, false);
        sphere.transform.localScale = new Vector3(-50f, 50f, 50f);

        var renderer = sphere.GetComponent<MeshRenderer>();
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        return sphere;
    }

    // Creates or updates an unlit material showing the given texture
    private static Material GetOrCreateMaterial(string path, Texture texture)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            AssetDatabase.CreateAsset(material, path);
        }

        // Draw both faces so the sphere is visible from the inside, like the Intranet room materials
        material.SetFloat("_Cull", 0f);
        material.SetTexture("_BaseMap", texture);
        material.SetColor("_BaseColor", Color.white);
        material.mainTexture = texture;
        EditorUtility.SetDirty(material);
        AssetDatabase.SaveAssets();
        return material;
    }

    // Creates the navigation bar shown below the viewer's line of sight
    private static GameObject CreateNavigationBar(string title, NavTarget[] targets)
    {
        var bar = CreateWorldCanvas("SceneNavigation", null, new Vector2(960f, 260f), 0.0022f);
        var follower = bar.AddComponent<HeadFollower>();
        follower.distance = 2.2f;
        follower.height = -0.95f;
        follower.angleThreshold = 45f;
        bar.transform.position = new Vector3(0f, -0.95f, 2.2f);

        var panel = CreateImage("Panel", bar.transform, Load<Sprite>(PanelSpritePath), PanelColor, false);
        Stretch(panel.rectTransform);

        CreateText("Title", bar.transform, "You are in: " + title, 30, FontStyle.Bold, SubtleText, TextAnchor.MiddleCenter,
            new Vector2(0f, 95f), new Vector2(900f, 44f));

        float spacing = 460f;
        float start = -spacing * (targets.Length - 1) * 0.5f;
        for (int i = 0; i < targets.Length; i++)
        {
            var target = targets[i];
            CreateNavButton(bar.transform, target, new Vector2(start + spacing * i, -25f), new Vector2(420f, 150f));
        }

        return bar;
    }

    // Creates one wide button of the navigation bar
    private static void CreateNavButton(Transform parent, NavTarget target, Vector2 position, Vector2 size)
    {
        var background = CreateImage(target.label.Replace(" ", "") + "Button", parent, Load<Sprite>(PanelSpritePath), CardColor, true);
        SetRect(background.rectTransform, position, size);

        var icon = CreateImage("Icon", background.transform, Load<Sprite>(target.icon), Color.white, false);
        SetRect(icon.rectTransform, new Vector2(-size.x * 0.5f + 70f, 8f), new Vector2(80f, 80f));

        CreateText("Label", background.transform, target.label, 42, FontStyle.Bold, Color.white, TextAnchor.MiddleLeft,
            new Vector2(55f, 8f), new Vector2(size.x - 150f, 70f));

        var bar = CreateProgressBar(background.transform, new Vector2(0f, -size.y * 0.5f + 16f), new Vector2(size.x - 60f, 8f));

        var button = background.gameObject.AddComponent<SceneNavButton>();
        button.sceneName = target.scene;
        button.background = background;
        button.normalColor = CardColor;
        button.progressBar = bar;
        button.gazeDuration = 3f;
    }

    // Creates one large card of the main menu
    private static void CreateMenuCard(Transform parent, string name, string title, string description, string iconPath,
        string scene, Vector2 position)
    {
        var size = new Vector2(660f, 520f);
        var background = CreateImage(name, parent, Load<Sprite>(PanelSpritePath), CardColor, true);
        SetRect(background.rectTransform, position, size);

        var icon = CreateImage("Icon", background.transform, Load<Sprite>(iconPath), AccentColor, false);
        SetRect(icon.rectTransform, new Vector2(0f, 120f), new Vector2(180f, 180f));

        CreateText("Title", background.transform, title, 58, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter,
            new Vector2(0f, -30f), new Vector2(620f, 80f));
        CreateText("Description", background.transform, description, 32, FontStyle.Normal, SubtleText, TextAnchor.MiddleCenter,
            new Vector2(0f, -135f), new Vector2(620f, 110f));

        var bar = CreateProgressBar(background.transform, new Vector2(0f, -size.y * 0.5f + 24f), new Vector2(size.x - 80f, 10f));

        var button = background.gameObject.AddComponent<SceneNavButton>();
        button.sceneName = scene;
        button.background = background;
        button.normalColor = CardColor;
        button.progressBar = bar;
        button.gazeDuration = 3f;
    }

    // Creates a thin bar that fills while the viewer gazes at a button
    private static Image CreateProgressBar(Transform parent, Vector2 position, Vector2 size)
    {
        var track = CreateImage("ProgressTrack", parent, null, new Color(1f, 1f, 1f, 0.12f), false);
        SetRect(track.rectTransform, position, size);

        // A filled image needs a sprite, otherwise it always draws full
        var fill = CreateImage("ProgressFill", track.transform, Load<Sprite>(ProgressSpritePath), AccentColor, false);
        Stretch(fill.rectTransform);
        fill.type = Image.Type.Filled;
        fill.fillMethod = Image.FillMethod.Horizontal;
        fill.fillAmount = 0f;
        return fill;
    }

    // Creates a world-space canvas that works with XR rays and gaze
    private static GameObject CreateWorldCanvas(string name, Transform parent, Vector2 size, float scale)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var rect = (RectTransform)go.transform;
        rect.SetParent(parent, false);

        var canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        go.GetComponent<CanvasScaler>().dynamicPixelsPerUnit = 3f;

        rect.sizeDelta = size;
        rect.localScale = Vector3.one * scale;

        if (parent == null)
        {
            go.AddComponent<TrackedDeviceGraphicRaycaster>();
        }

        return go;
    }

    // Creates an image element
    private static Image CreateImage(string name, Transform parent, Sprite sprite, Color color, bool raycastTarget)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        var image = go.GetComponent<Image>();
        image.sprite = sprite;
        image.color = color;
        image.raycastTarget = raycastTarget;
        if (sprite != null && sprite.border != Vector4.zero)
        {
            image.type = Image.Type.Sliced;
        }

        return image;
    }

    // Creates a legacy UI text element
    private static Text CreateText(string name, Transform parent, string content, int size, FontStyle style, Color color,
        TextAnchor alignment, Vector2 position, Vector2 rectSize)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        go.transform.SetParent(parent, false);
        var text = go.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.text = content;
        text.fontSize = size;
        text.fontStyle = style;
        text.color = color;
        text.alignment = alignment;
        text.supportRichText = true;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.raycastTarget = false;
        SetRect(text.rectTransform, position, rectSize);
        return text;
    }

    // Anchors a rect to its parent's center
    private static void SetRect(RectTransform rect, Vector2 position, Vector2 size)
    {
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }

    // Makes a rect fill its parent
    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    // Points every canvas under a root at the given camera
    private static void SetCanvasCamera(GameObject root, Camera camera)
    {
        foreach (var canvas in root.GetComponentsInChildren<Canvas>(true))
        {
            canvas.worldCamera = camera;
        }
    }

    // Deletes a root object of a scene if it exists
    private static void RemoveRoot(Scene scene, string name)
    {
        foreach (var root in scene.GetRootGameObjects())
        {
            if (root.name == name)
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }
    }

    // Loads an asset and reports a clear error when it is missing
    private static T Load<T>(string path) where T : UnityEngine.Object
    {
        var asset = AssetDatabase.LoadAssetAtPath<T>(path);
        if (asset == null)
        {
            Debug.LogError("Missing asset: " + path);
        }

        return asset;
    }
}
