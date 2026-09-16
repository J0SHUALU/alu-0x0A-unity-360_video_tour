using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Keyboard and mouse shortcuts for testing the tour in the Unity Editor without a headset.
/// 1, 2 and 3 open the main menu, Intranet tour and campus tour. N opens the next room,
/// I toggles the info boxes of the current room, the arrow keys turn 30 degrees,
/// and holding the right mouse button looks around.
/// Only active in the Editor.
/// </summary>
public class DesktopTestKeys : MonoBehaviour
{
    /// <summary>
    /// Degrees turned per pixel of mouse movement while looking around.
    /// </summary>
    [SerializeField]
    public float lookSpeed = 0.2f;

    // Accumulated look angles
    private float yaw;
    private float pitch;

    // Creates the helper once when play mode starts in the Editor
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
        if (!Application.isEditor)
            return;

        var go = new GameObject("DesktopTestKeys");
        DontDestroyOnLoad(go);
        go.AddComponent<DesktopTestKeys>();
    }

    private void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.digit1Key.wasPressedThisFrame)
                SceneTransition.Load(SceneTransition.MainMenuScene);
            if (keyboard.digit2Key.wasPressedThisFrame)
                SceneTransition.Load(SceneTransition.IntranetTourScene);
            if (keyboard.digit3Key.wasPressedThisFrame)
                SceneTransition.Load(SceneTransition.CustomCampusTourScene);
            if (keyboard.nKey.wasPressedThisFrame)
                OpenNextRoom();
            if (keyboard.iKey.wasPressedThisFrame)
                ToggleInfoBoxes();
            if (keyboard.leftArrowKey.wasPressedThisFrame)
                yaw -= 30f;
            if (keyboard.rightArrowKey.wasPressedThisFrame)
                yaw += 30f;
            if (keyboard.upArrowKey.wasPressedThisFrame)
                pitch = Mathf.Clamp(pitch - 15f, -80f, 80f);
            if (keyboard.downArrowKey.wasPressedThisFrame)
                pitch = Mathf.Clamp(pitch + 15f, -80f, 80f);
        }

        var mouse = Mouse.current;
        if (mouse != null && mouse.rightButton.isPressed)
        {
            Vector2 delta = mouse.delta.ReadValue();
            yaw += delta.x * lookSpeed;
            pitch = Mathf.Clamp(pitch - delta.y * lookSpeed, -80f, 80f);
        }

        // Turn the rig's camera offset so the XR camera keeps its own tracked pose
        var head = Camera.main;
        if (head != null && head.transform.parent != null)
        {
            head.transform.parent.localRotation = Quaternion.Euler(pitch, yaw, 0f);
        }
    }

    // Moves to the next room of the current tour
    private static void OpenNextRoom()
    {
        var navigator = FindFirstObjectByType<RoomNavigator>();
        if (navigator == null)
            return;

        var rooms = new List<GameObject>(navigator.AllRooms);
        int index = rooms.IndexOf(navigator.CurrentRoom);
        var next = rooms[(index + 1) % rooms.Count];
        navigator.SwitchRoom(next.name);
    }

    // Opens or closes every info box in the current room
    private static void ToggleInfoBoxes()
    {
        var navigator = FindFirstObjectByType<RoomNavigator>();
        if (navigator == null || navigator.CurrentRoom == null)
            return;

        foreach (var toggle in navigator.CurrentRoom.GetComponentsInChildren<InfoPanelToggle>(true))
        {
            toggle.TogglePanel();
        }
    }
}
