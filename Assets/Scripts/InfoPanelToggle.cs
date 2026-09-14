using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Controls toggling the visibility of an associated informational text panel.
/// Supports both click/pointer interaction and gaze dwell interaction.
/// </summary>
public class InfoPanelToggle : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    /// <summary>
    /// The target informational panel GameObject to toggle.
    /// </summary>
    [SerializeField]
    public GameObject infoPanel;

    /// <summary>
    /// Duration in seconds of continuous gaze required to trigger the toggle.
    /// </summary>
    [SerializeField]
    public float gazeDuration = 1.5f;

    // Whether the pointer or gaze reticle is hovering over the icon
    private bool isHovered = false;

    // Timer tracking the continuous gaze duration
    private float gazeTimer = 0f;

    // Original transform scale for visual hover feedback
    private Vector3 originalScale;

    private void Start()
    {
        // Cache original scale
        originalScale = transform.localScale;

        // Ensure info panel starts closed if assigned
        if (infoPanel != null)
        {
            infoPanel.SetActive(false);
        }
    }

    private void Update()
    {
        // Handle gaze dwelling logic
        if (isHovered)
        {
            gazeTimer += Time.deltaTime;
            transform.localScale = originalScale * (1f + 0.1f * Mathf.Clamp01(gazeTimer / gazeDuration));

            if (gazeTimer >= gazeDuration)
            {
                TogglePanel();
                gazeTimer = 0f;
            }
        }
        else
        {
            gazeTimer = 0f;
            transform.localScale = originalScale;
        }
    }

    /// <summary>
    /// Called when the pointer or gaze enters the UI element.
    /// </summary>
    /// <param name="eventData">Pointer event data.</param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
    }

    /// <summary>
    /// Called when the pointer or gaze exits the UI element.
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
        TogglePanel();
    }

    /// <summary>
    /// Toggles the active state of the informational panel.
    /// </summary>
    public void TogglePanel()
    {
        if (infoPanel != null)
        {
            infoPanel.SetActive(!infoPanel.activeSelf);
        }
    }
}
