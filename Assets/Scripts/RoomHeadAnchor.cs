using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Keeps every 360 room sphere centered on the viewer's head.
/// 360 media is captured from a single point, so moving the head away from the
/// sphere center makes the room look warped and the viewer feel like they are floating.
/// </summary>
public class RoomHeadAnchor : MonoBehaviour
{
    // Room spheres that follow the head
    private readonly List<Transform> rooms = new List<Transform>();

    // Camera used as the head position
    private Camera head;

    // Creates an anchor in the first scene and in every scene loaded afterwards
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
        EnsureInScene();
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // Adds an anchor to a newly loaded scene
    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        EnsureInScene();
    }

    // Creates the anchor if the active scene does not have one
    private static void EnsureInScene()
    {
        if (FindFirstObjectByType<RoomHeadAnchor>() == null)
        {
            new GameObject("RoomHeadAnchor").AddComponent<RoomHeadAnchor>();
        }
    }

    private void OnEnable()
    {
        Application.onBeforeRender += Follow;
    }

    private void OnDisable()
    {
        Application.onBeforeRender -= Follow;
    }

    private void Start()
    {
        rooms.Clear();
        var navigator = FindFirstObjectByType<RoomNavigator>();
        if (navigator == null)
            return;

        foreach (var room in navigator.AllRooms)
        {
            rooms.Add(room.transform);
        }
    }

    private void LateUpdate()
    {
        Follow();
    }

    // Moves every room sphere to the current head position
    [BeforeRenderOrder(100)]
    private void Follow()
    {
        if (rooms.Count == 0)
            return;

        if (head == null)
        {
            head = Camera.main;
            if (head == null)
                return;
        }

        Vector3 center = head.transform.position;
        foreach (var room in rooms)
        {
            if (room != null)
            {
                room.position = center;
            }
        }
    }
}
