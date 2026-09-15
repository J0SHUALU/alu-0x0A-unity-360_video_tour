using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Editor tool that lays out every hotspot and info button in the 360VideoTour scene.
/// Each button sits on a ring around the viewer, faces the center, has a large
/// invisible hit area and is spaced so no two elements overlap.
/// </summary>
public static class TourLayout
{
    // Distance in meters from the viewer to each button
    private const float Distance = 10f;

    // World size in meters of one canvas unit
    private const float UnitSize = 0.008f;

    // Canvas sizes in canvas units
    private static readonly Vector2 HotspotCanvasSize = new Vector2(420f, 120f);
    private static readonly Vector2 InfoCanvasSize = new Vector2(620f, 400f);

    // One layout entry: room sphere, canvas name and yaw angle in degrees
    private struct Slot
    {
        public string room;
        public string canvas;
        public float yaw;

        public Slot(string room, string canvas, float yaw)
        {
            this.room = room;
            this.canvas = canvas;
            this.yaw = yaw;
        }
    }

    // Where every canvas goes around the viewer
    private static readonly Slot[] Slots =
    {
        new Slot("LivingRoom", "Canvas_CantinaHotspot", -45f),
        new Slot("LivingRoom", "Canvas_InfoHotspot", 0f),
        new Slot("LivingRoom", "Canvas_CubeHotspot", 45f),
        new Slot("LivingRoom", "Canvas_InfoHotspot_Library", 110f),

        new Slot("Cantina", "Canvas_LivingRoomHotspot", -45f),
        new Slot("Cantina", "Canvas_InfoHotspot_Cantina", 0f),
        new Slot("Cantina", "Canvas_CubeHotspot", 45f),

        new Slot("Cube", "Canvas_LivingRoomHotspot", -50f),
        new Slot("Cube", "Canvas_InfoHotspot_Cube", 0f),
        new Slot("Cube", "Canvas_CantinaHotspot", 50f),
        new Slot("Cube", "Canvas_MezzanineHotspot", 110f),

        new Slot("Mezzanine", "Canvas_InfoHotspot_Booth", -55f),
        new Slot("Mezzanine", "Canvas_CubeHotspot", 0f),
        new Slot("Mezzanine", "Canvas_InfoHotspot_iMacs", 55f),
    };

    /// <summary>
    /// Applies the layout to the open scene, saves it and writes a report to Logs/TourLayout.txt.
    /// </summary>
    [MenuItem("Tools/Lay Out Tour Hotspots")]
    public static void Apply()
    {
        var report = new StringBuilder();
        var rooms = new Dictionary<string, Transform>();
        foreach (var root in EditorSceneManager.GetActiveScene().GetRootGameObjects())
        {
            rooms[root.name] = root.transform;
        }

        var placed = new Dictionary<string, List<KeyValuePair<float, float>>>();

        foreach (var slot in Slots)
        {
            Transform room;
            if (!rooms.TryGetValue(slot.room, out room))
            {
                report.AppendLine("MISSING room " + slot.room);
                continue;
            }

            var canvas = room.Find(slot.canvas);
            if (canvas == null)
            {
                report.AppendLine("MISSING canvas " + slot.room + "/" + slot.canvas);
                continue;
            }

            float halfWidth;
            if (canvas.GetComponentInChildren<InfoPanelToggle>(true) != null)
            {
                LayoutInfo(canvas);
                halfWidth = HalfAngle(600f);
            }
            else
            {
                LayoutHotspot(canvas);
                halfWidth = HalfAngle(HotspotCanvasSize.x);
            }

            Place(room, canvas, slot.yaw);

            bool mirrored = canvas.localToWorldMatrix.determinant < 0f;
            report.AppendLine(string.Format("{0}/{1} yaw={2} halfWidth={3:F1} forwardDot={4:F3} mirrored={5}",
                slot.room, slot.canvas, slot.yaw, halfWidth,
                Vector3.Dot(canvas.forward, canvas.position.normalized), mirrored));

            if (!placed.ContainsKey(slot.room))
            {
                placed[slot.room] = new List<KeyValuePair<float, float>>();
            }

            foreach (var other in placed[slot.room])
            {
                float gap = Mathf.Abs(Mathf.DeltaAngle(other.Key, slot.yaw)) - other.Value - halfWidth;
                if (gap <= 0f)
                {
                    report.AppendLine("  OVERLAP in " + slot.room + " at yaw " + slot.yaw + " gap " + gap);
                }
            }

            placed[slot.room].Add(new KeyValuePair<float, float>(slot.yaw, halfWidth));
        }

        var scene = EditorSceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        Directory.CreateDirectory("Logs");
        File.WriteAllText("Logs/TourLayout.txt", report.ToString());
        Debug.Log("Tour layout applied.\n" + report);
    }

    // Returns the half angular width in degrees of a canvas width given in canvas units
    private static float HalfAngle(float width)
    {
        return Mathf.Atan(width * UnitSize * 0.5f / Distance) * Mathf.Rad2Deg;
    }

    // Positions a canvas on the ring, facing away from the viewer so its front is readable,
    // compensating for the room sphere's mirrored scale
    private static void Place(Transform room, Transform canvas, float yaw)
    {
        Vector3 dir = Quaternion.Euler(0f, yaw, 0f) * Vector3.forward;
        Quaternion worldRot = Quaternion.LookRotation(dir, Vector3.up);
        Vector3 parentScale = room.localScale;

        canvas.localPosition = room.InverseTransformPoint(dir * Distance);

        // The sphere is flipped on X, so mirror the rotation and the X scale to cancel it out
        if (parentScale.x < 0f)
        {
            worldRot = new Quaternion(worldRot.x, -worldRot.y, -worldRot.z, worldRot.w);
        }

        canvas.localRotation = Quaternion.Inverse(room.rotation) * worldRot;
        canvas.localScale = new Vector3(
            UnitSize / parentScale.x,
            UnitSize / parentScale.y,
            UnitSize / parentScale.z);
    }

    // Arranges a hotspot canvas: icon on the left, label on the right, one hit area covering both
    private static void LayoutHotspot(Transform canvas)
    {
        ((RectTransform)canvas).sizeDelta = HotspotCanvasSize;

        var hotspot = canvas.GetComponentInChildren<HotspotInteraction>(true);
        var button = (RectTransform)hotspot.transform;
        Center(button, new Vector2(-150f, 0f), new Vector2(100f, 100f));

        foreach (var label in canvas.GetComponentsInChildren<Text>(true))
        {
            var rt = (RectTransform)label.transform;
            Center(rt, new Vector2(60f, 0f), new Vector2(280f, 100f));
            label.alignment = TextAnchor.MiddleLeft;
            label.fontSize = 48;
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            label.raycastTarget = false;
        }

        AddHitArea(button, new Vector2(150f, 0f), HotspotCanvasSize);
    }

    // Arranges an info canvas: button below eye level, panel directly above it
    private static void LayoutInfo(Transform canvas)
    {
        ((RectTransform)canvas).sizeDelta = InfoCanvasSize;

        var toggle = canvas.GetComponentInChildren<InfoPanelToggle>(true);
        var button = (RectTransform)toggle.transform;
        Center(button, new Vector2(0f, -140f), new Vector2(100f, 100f));
        AddHitArea(button, Vector2.zero, new Vector2(170f, 170f));

        if (toggle.infoPanel == null)
            return;

        var panel = (RectTransform)toggle.infoPanel.transform;
        Center(panel, new Vector2(0f, 60f), new Vector2(600f, 220f));

        var panelImage = panel.GetComponent<Image>();
        if (panelImage != null)
        {
            panelImage.raycastTarget = false;
        }

        foreach (var text in panel.GetComponentsInChildren<Text>(true))
        {
            var rt = (RectTransform)text.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.offsetMin = new Vector2(24f, 16f);
            rt.offsetMax = new Vector2(-24f, -16f);
            text.alignment = TextAnchor.MiddleCenter;
            text.fontSize = 30;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.raycastTarget = false;
        }
    }

    // Anchors a rect to its parent's center at the given position and size
    private static void Center(RectTransform rt, Vector2 position, Vector2 size)
    {
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = position;
        rt.sizeDelta = size;
        rt.localRotation = Quaternion.identity;
        rt.localScale = Vector3.one;
    }

    // Adds or updates a transparent child image that catches rays for its button
    private static void AddHitArea(RectTransform button, Vector2 offset, Vector2 size)
    {
        var existing = button.Find("HitArea");
        GameObject hit = existing != null ? existing.gameObject : new GameObject("HitArea", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        hit.layer = button.gameObject.layer;
        var rt = (RectTransform)hit.transform;
        rt.SetParent(button, false);
        rt.SetAsFirstSibling();
        Center(rt, offset, size);

        var image = hit.GetComponent<Image>();
        image.color = new Color(1f, 1f, 1f, 0f);
        image.raycastTarget = true;
    }
}
