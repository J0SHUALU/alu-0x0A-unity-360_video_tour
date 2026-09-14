using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Handles gaze and pointer interactions for VR hotspots with visual feedback.
/// Supports both pointer interaction and dwell-based gaze interaction.
/// </summary>
public class HotspotInteraction : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    /// <summary>
    /// The target room name to navigate to when clicked or gazed.
    /// </summary>
    [SerializeField]
    public string targetRoomName;

    /// <summary>
    /// The time in seconds needed to gaze at the hotspot to trigger navigation.
    /// </summary>
    [SerializeField]
    public float gazeDuration = 2.0f;

    /// <summary>
    /// The image element whose scale or color indicates dwell progress.
    /// </summary>
    [SerializeField]
    public Image fillIndicator;

    // Reference to the scene's RoomNavigator
    private RoomNavigator navigator;

    // Whether the pointer or gaze reticle is currently over the hotspot
    private bool isHovered = false;

    // Timer tracking the duration of continuous gaze
    private float gazeTimer = 0f;

    // Original scale of the button for hover feedback
    private Vector3 originalScale;

    private void Start()
    {
        // Cache original transform scale
        originalScale = transform.localScale;

        // Find RoomNavigator in the scene
        navigator = Object.FindFirstObjectByType<RoomNavigator>();
    }

    private void Update()
    {
        // Handle gaze dwelling logic
        if (isHovered)
        {
            gazeTimer += Time.deltaTime;

            if (fillIndicator != null)
            {
                fillIndicator.fillAmount = Mathf.Clamp01(gazeTimer / gazeDuration);
            }

            // Visual feedback - slight pulse or enlargement
            transform.localScale = originalScale * (1f + 0.1f * Mathf.Clamp01(gazeTimer / gazeDuration));

            if (gazeTimer >= gazeDuration)
            {
                TriggerNavigation();
            }
        }
        else
        {
            gazeTimer = 0f;
            if (fillIndicator != null)
            {
                fillIndicator.fillAmount = 0f;
            }
            transform.localScale = originalScale;
        }
    }

    /// <summary>
    /// Called when the pointer enters the UI element.
    /// </summary>
    /// <param name="eventData">Pointer event data.</param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
    }

    /// <summary>
    /// Called when the pointer exits the UI element.
    /// </summary>
    /// <param name="eventData">Pointer event data.</param>
    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
    }

    /// <summary>
    /// Called when the pointer or controller clicks the UI element.
    /// </summary>
    /// <param name="eventData">Pointer event data.</param>
    public void OnPointerClick(PointerEventData eventData)
    {
        TriggerNavigation();
    }

    // Navigates to the designated room and resets interaction states
    private void TriggerNavigation()
    {
        isHovered = false;
        gazeTimer = 0f;
        transform.localScale = originalScale;

        if (navigator != null && !string.IsNullOrEmpty(targetRoomName))
        {
            navigator.SwitchRoom(targetRoomName);
        }
    }
}
