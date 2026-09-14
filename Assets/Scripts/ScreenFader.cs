using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls screen fade transitions between black and transparent.
/// Used during room navigation to prevent visual disorientation.
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
            // Ensure fade image starts fully transparent and non-blocking
            fadeImage.color = new Color(0f, 0f, 0f, 0f);
            fadeImage.raycastTarget = false;
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

        fadeImage.raycastTarget = true;
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Clamp01(timer / fadeDuration);
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

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = 1f - Mathf.Clamp01(timer / fadeDuration);
            fadeImage.color = new Color(0f, 0f, 0f, alpha);
            yield return null;
        }

        fadeImage.color = new Color(0f, 0f, 0f, 0f);
        fadeImage.raycastTarget = false;
    }
}
