using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

/// <summary>
/// Manages navigation between 360-degree rooms inside one scene.
/// Handles switching the active room sphere, fading the screen to black, and controlling video playback.
/// The Intranet tour uses the four named room fields; the custom campus tour uses the rooms list.
/// </summary>
public class RoomNavigator : MonoBehaviour
{
    /// <summary>
    /// The LivingRoom sphere GameObject.
    /// </summary>
    [SerializeField]
    public GameObject livingRoom;

    /// <summary>
    /// The Cantina sphere GameObject.
    /// </summary>
    [SerializeField]
    public GameObject cantina;

    /// <summary>
    /// The Cube sphere GameObject.
    /// </summary>
    [SerializeField]
    public GameObject cube;

    /// <summary>
    /// The Mezzanine sphere GameObject.
    /// </summary>
    [SerializeField]
    public GameObject mezzanine;

    /// <summary>
    /// Additional rooms, looked up by their GameObject name.
    /// </summary>
    [SerializeField]
    public List<GameObject> rooms = new List<GameObject>();

    /// <summary>
    /// Name of the room shown first. When empty, LivingRoom or the first listed room is used.
    /// </summary>
    [SerializeField]
    public string startRoom = "";

    // The currently active room GameObject
    private GameObject currentRoom;

    // Maps room names to their GameObjects for lookup
    private Dictionary<string, GameObject> roomMap;

    // Flag indicating whether a room transition is currently in progress
    private bool isTransitioning = false;

    /// <summary>
    /// Every room this navigator controls.
    /// </summary>
    public IEnumerable<GameObject> AllRooms
    {
        get
        {
            EnsureRoomMap();
            return roomMap.Values;
        }
    }

    /// <summary>
    /// The room that is currently shown.
    /// </summary>
    public GameObject CurrentRoom
    {
        get { return currentRoom; }
    }

    private void Start()
    {
        EnsureRoomMap();

        // Deactivate all rooms first
        foreach (var room in roomMap.Values)
        {
            room.SetActive(false);
        }

        // Pick the starting room
        GameObject first = null;
        if (!string.IsNullOrEmpty(startRoom))
        {
            roomMap.TryGetValue(startRoom, out first);
        }

        if (first == null)
        {
            first = livingRoom;
        }

        if (first == null)
        {
            foreach (var room in rooms)
            {
                if (room != null)
                {
                    first = room;
                    break;
                }
            }
        }

        currentRoom = first;
        ActivateRoom(currentRoom);
    }

    /// <summary>
    /// Switches from the current room to the specified target room with a fade transition.
    /// Fades out to black, switches room spheres, and fades in from black.
    /// </summary>
    /// <param name="roomName">The name of the target room to switch to.</param>
    public void SwitchRoom(string roomName)
    {
        if (isTransitioning || SceneTransition.IsLoading)
            return;

        EnsureRoomMap();
        GameObject targetRoom;
        if (!roomMap.TryGetValue(roomName, out targetRoom))
            return;

        if (targetRoom == null || targetRoom == currentRoom)
            return;

        StartCoroutine(TransitionRoutine(targetRoom));
    }

    // Builds the room lookup table once
    private void EnsureRoomMap()
    {
        if (roomMap != null)
            return;

        roomMap = new Dictionary<string, GameObject>();
        AddRoom("LivingRoom", livingRoom);
        AddRoom("Cantina", cantina);
        AddRoom("Cube", cube);
        AddRoom("Mezzanine", mezzanine);

        foreach (var room in rooms)
        {
            if (room != null)
            {
                AddRoom(room.name, room);
            }
        }
    }

    // Adds a room to the lookup table if it is assigned and not already present
    private void AddRoom(string key, GameObject room)
    {
        if (room != null && !roomMap.ContainsKey(key))
        {
            roomMap.Add(key, room);
        }
    }

    // Executes the fade-out, room switch, and fade-in sequence
    private IEnumerator TransitionRoutine(GameObject targetRoom)
    {
        isTransitioning = true;

        if (ScreenFader.Instance != null)
        {
            yield return StartCoroutine(ScreenFader.Instance.FadeOut());
        }

        DeactivateRoom(currentRoom);
        currentRoom = targetRoom;
        ActivateRoom(currentRoom);

        if (ScreenFader.Instance != null)
        {
            yield return StartCoroutine(ScreenFader.Instance.FadeIn());
        }

        isTransitioning = false;
    }

    // Activates a room and starts its video playback
    private void ActivateRoom(GameObject room)
    {
        if (room == null)
            return;

        room.SetActive(true);

        var videoPlayer = room.GetComponent<VideoPlayer>();
        if (videoPlayer != null)
        {
            videoPlayer.Play();
        }
    }

    // Deactivates a room and stops its video playback
    private void DeactivateRoom(GameObject room)
    {
        if (room == null)
            return;

        var videoPlayer = room.GetComponent<VideoPlayer>();
        if (videoPlayer != null)
        {
            videoPlayer.Stop();
        }

        room.SetActive(false);
    }
}
