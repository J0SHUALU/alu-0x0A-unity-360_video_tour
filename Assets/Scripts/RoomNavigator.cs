using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

/// <summary>
/// Manages navigation between 360-degree video rooms.
/// Handles switching the active room sphere, fading the screen to black, and controlling video playback.
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

    // The currently active room GameObject
    private GameObject currentRoom;

    // Maps room names to their GameObjects for lookup
    private Dictionary<string, GameObject> roomMap;

    // Flag indicating whether a room transition is currently in progress
    private bool isTransitioning = false;

    private void Start()
    {
        // Build the room lookup table
        roomMap = new Dictionary<string, GameObject>
        {
            { "LivingRoom", livingRoom },
            { "Cantina", cantina },
            { "Cube", cube },
            { "Mezzanine", mezzanine }
        };

        // Deactivate all rooms first
        foreach (var room in roomMap.Values)
        {
            if (room != null)
                room.SetActive(false);
        }

        // Start in the LivingRoom
        currentRoom = livingRoom;
        ActivateRoom(currentRoom);
    }

    /// <summary>
    /// Switches from the current room to the specified target room with a fade transition.
    /// Fades out to black, switches room spheres, and fades in from black.
    /// </summary>
    /// <param name="roomName">The name of the target room to switch to.</param>
    public void SwitchRoom(string roomName)
    {
        if (isTransitioning)
            return;

        if (!roomMap.ContainsKey(roomName))
            return;

        GameObject targetRoom = roomMap[roomName];
        if (targetRoom == null || targetRoom == currentRoom)
            return;

        StartCoroutine(TransitionRoutine(targetRoom));
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
