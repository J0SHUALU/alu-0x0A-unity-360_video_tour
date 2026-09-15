using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls screen fade transitions between black and transparent.
/// Used during room navigation and scene changes to prevent visual disorientation.
/// </summary>
public class ScreenFader : MonoBehaviour
{
    /// <summary>
    /// Duration of the fade in and fade out effects in seconds.
    /// </summary>
    [SerializeField]
    public float fadeDuration = 0.5f;

    /// <summary>
    /// The UI Image used as an overlay for screen fading.
    /// </summary>
    [SerializeField]
    public Image fadeImage;

    /// <summary>
    /// When true the view starts black and fades in as the scene opens.
    /// </summary>
    [SerializeField]
    public bool fadeInOnStart = true;

    // Singleton instance of the screen fader
    private static ScreenFader instance;

    /// <summary>
    /// Public static accessor for the ScreenFader instance.
    /// </summary>
    public static ScreenFader Instance
    {
        get { return instance; }
    }

    private void Awake()
    {
        // Enforce singleton pattern
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        if (fadeImage != null)
        {
            // Start black when fading in, otherwise fully transparent, and never block rays
            fadeImage.color = new Color(0f, 0f, 0f, fadeInOnStart ? 1f : 0f);
            fadeImage.raycastTarget = false;
        }
    }

    private void Start()
    {
        if (fadeInOnStart && fadeImage != null)
        {
            StartCoroutine(FadeIn());
        }
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    /// <summary>
    /// Fades the screen to solid black.
    /// </summary>
    /// <returns>Coroutine IEnumerator.</returns>
    public IEnumerator FadeOut()
    {
        if (fadeImage == null)
            yield break;

        float timer = 0f;
        float start = fadeImage.color.a;

        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(start, 1f, Mathf.Clamp01(timer / fadeDuration));
            fadeImage.color = new Color(0f, 0f, 0f, alpha);
            yield return null;
        }

        fadeImage.color = new Color(0f, 0f, 0f, 1f);
    }

    /// <summary>
    /// Fades the screen back from solid black to transparent.
    /// </summary>
    /// <returns>Coroutine IEnumerator.</returns>
    public IEnumerator FadeIn()
    {
        if (fadeImage == null)
            yield break;

        float timer = 0f;
        float start = fadeImage.color.a;

        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(start, 0f, Mathf.Clamp01(timer / fadeDuration));
            fadeImage.color = new Color(0f, 0f, 0f, alpha);
            yield return null;
        }

        fadeImage.color = new Color(0f, 0f, 0f, 0f);
    }
}
