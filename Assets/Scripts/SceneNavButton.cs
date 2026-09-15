using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// A VR button that opens another scene. It works with controller rays
/// (point and pull the trigger) and with gaze dwell, and shows hover feedback.
/// </summary>
public class SceneNavButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    /// <summary>
    /// Name of the scene this button opens.
    /// </summary>
    [SerializeField]
    public string sceneName;

    /// <summary>
    /// Seconds of continuous gaze needed to open the scene without a controller.
    /// </summary>
    [SerializeField]
    public float gazeDuration = 3f;

    /// <summary>
    /// Filled image that shows gaze progress.
    /// </summary>
    [SerializeField]
    public Image progressBar;

    /// <summary>
    /// Background graphic that changes color on hover.
    /// </summary>
    [SerializeField]
    public Graphic background;

    /// <summary>
    /// Background color while idle.
    /// </summary>
    [SerializeField]
    public Color normalColor = new Color(0.12f, 0.16f, 0.28f, 0.95f);

    /// <summary>
    /// Background color while hovered.
    /// </summary>
    [SerializeField]
    public Color hoverColor = new Color(0.16f, 0.42f, 0.86f, 1f);

    /// <summary>
    /// Scale multiplier applied while hovered.
    /// </summary>
    [SerializeField]
    public float hoverScale = 1.06f;

    // Number of pointers (rays or gaze) currently over the button
    private int hoverCount = 0;

    // Seconds of continuous hover
    private float gazeTimer = 0f;

    // Scale of the button when idle
    private Vector3 baseScale;

    private void Start()
    {
        baseScale = transform.localScale;
        UpdateVisuals(0f);
    }

    private void OnDisable()
    {
        hoverCount = 0;
        gazeTimer = 0f;
    }

    private void Update()
    {
        bool hovered = hoverCount > 0 && !SceneTransition.IsLoading;

        if (hovered)
        {
            gazeTimer += Time.deltaTime;
            if (gazeTimer >= gazeDuration)
            {
                Open();
            }
        }
        else
        {
            gazeTimer = 0f;
        }

        Vector3 target = baseScale * (hovered ? hoverScale : 1f);
        transform.localScale = Vector3.Lerp(transform.localScale, target, Time.deltaTime * 12f);

        if (background != null)
        {
            background.color = Color.Lerp(background.color, hovered ? hoverColor : normalColor, Time.deltaTime * 12f);
        }

        UpdateVisuals(gazeDuration > 0f ? gazeTimer / gazeDuration : 0f);
    }

    /// <summary>
    /// Called when a ray or the gaze pointer enters the button.
    /// </summary>
    /// <param name="eventData">Pointer event data.</param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        hoverCount++;
    }

    /// <summary>
    /// Called when a ray or the gaze pointer leaves the button.
    /// </summary>
    /// <param name="eventData">Pointer event data.</param>
    public void OnPointerExit(PointerEventData eventData)
    {
        hoverCount = Mathf.Max(0, hoverCount - 1);
    }

    /// <summary>
    /// Called when a controller trigger clicks the button.
    /// </summary>
    /// <param name="eventData">Pointer event data.</param>
    public void OnPointerClick(PointerEventData eventData)
    {
        Open();
    }

    /// <summary>
    /// Opens the target scene with a fade transition.
    /// </summary>
    public void Open()
    {
        gazeTimer = 0f;
        hoverCount = 0;
        SceneTransition.Load(sceneName);
    }

    // Updates the gaze progress bar
    private void UpdateVisuals(float progress)
    {
        if (progressBar != null)
        {
            progressBar.fillAmount = Mathf.Clamp01(progress);
        }
    }
}
