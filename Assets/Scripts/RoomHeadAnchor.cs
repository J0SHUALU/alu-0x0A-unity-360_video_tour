using UnityEngine;

/// <summary>
/// Keeps every 360 video sphere centered on the viewer's head.
/// 360 video is filmed from a single point, so moving the head away from the
/// sphere center makes the room look warped and the viewer feel like they are floating.
/// </summary>
public class RoomHeadAnchor : MonoBehaviour
{
    // Room spheres that follow the head
    private Transform[] rooms;

    // Camera used as the head position
    private Camera head;

    // Creates the anchor automatically once the scene has loaded
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void CreateOnLoad()
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
        var navigator = FindFirstObjectByType<RoomNavigator>();
        if (navigator != null)
        {
            rooms = new[]
            {
                Get(navigator.livingRoom),
                Get(navigator.cantina),
                Get(navigator.cube),
                Get(navigator.mezzanine)
            };
        }
    }

    private void LateUpdate()
    {
        Follow();
    }

    // Returns the transform of a room, or null if it is not assigned
    private static Transform Get(GameObject room)
    {
        return room != null ? room.transform : null;
    }

    // Moves every room sphere to the current head position
    [BeforeRenderOrder(100)]
    private void Follow()
    {
        if (rooms == null)
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
