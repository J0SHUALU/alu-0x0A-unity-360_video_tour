using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Provides gaze-based raycasting from the center of the camera to drive UI interactions.
/// Allows headsets without physical controllers to trigger hotspots by looking at them.
/// </summary>
public class GazeRaycaster : MonoBehaviour
{
    /// <summary>
    /// The maximum raycast distance for gaze detection.
    /// </summary>
    [SerializeField]
    public float raycastDistance = 50.0f;

    /// <summary>
    /// An optional reticle UI image positioned in front of the camera.
    /// </summary>
    [SerializeField]
    public Image reticleImage;

    // The current UI object being hit by the gaze raycast
    private GameObject currentGazeTarget;

    // Reusable pointer event data container
    private PointerEventData pointerEventData;

    // Reusable list of raycast results
    private List<RaycastResult> raycastResults = new List<RaycastResult>();

    private void Start()
    {
        // Initialize pointer event data with current EventSystem
        if (EventSystem.current != null)
        {
            pointerEventData = new PointerEventData(EventSystem.current);
        }
    }

    private void Update()
    {
        if (EventSystem.current == null)
            return;

        if (pointerEventData == null)
        {
            pointerEventData = new PointerEventData(EventSystem.current);
        }

        // Cast ray from center of screen (gaze center)
        pointerEventData.position = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        raycastResults.Clear();
        EventSystem.current.RaycastAll(pointerEventData, raycastResults);

        GameObject hitObject = null;
        if (raycastResults.Count > 0)
        {
            hitObject = raycastResults[0].gameObject;
        }

        // Handle pointer enter / exit events
        if (hitObject != currentGazeTarget)
        {
            if (currentGazeTarget != null)
            {
                ExecuteEvents.Execute(currentGazeTarget, pointerEventData, ExecuteEvents.pointerExitHandler);
            }

            currentGazeTarget = hitObject;

            if (currentGazeTarget != null)
            {
                ExecuteEvents.Execute(currentGazeTarget, pointerEventData, ExecuteEvents.pointerEnterHandler);
            }
        }
    }
}
