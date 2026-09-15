using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Loads scenes with a fade to black and a matching audio fade,
/// so every scene change stays comfortable in VR.
/// </summary>
public class SceneTransition : MonoBehaviour
{
    /// <summary>
    /// Scene name of the welcome menu.
    /// </summary>
    public const string MainMenuScene = "MainMenuScene";

    /// <summary>
    /// Scene name of the original Intranet 360 video tour.
    /// </summary>
    public const string IntranetTourScene = "IntranetTourScene";

    /// <summary>
    /// Scene name of the custom campus tour.
    /// </summary>
    public const string CustomCampusTourScene = "CustomCampusTourScene";

    // Persistent object that runs the transition coroutine across scene loads
    private static SceneTransition runner;

    // Whether a scene change is currently running
    private static bool isLoading;

    /// <summary>
    /// True while a scene change is in progress.
    /// </summary>
    public static bool IsLoading
    {
        get { return isLoading; }
    }

    /// <summary>
    /// Fades out, loads the named scene, then fades the audio back in.
    /// The new scene fades its view in through its own ScreenFader.
    /// </summary>
    /// <param name="sceneName">Name of the scene to open. It must be in the build settings.</param>
    public static void Load(string sceneName)
    {
        if (isLoading || string.IsNullOrEmpty(sceneName))
            return;

        if (runner == null)
        {
            var go = new GameObject("SceneTransition");
            DontDestroyOnLoad(go);
            runner = go.AddComponent<SceneTransition>();
        }

        runner.StartCoroutine(runner.LoadRoutine(sceneName));
    }

    // Fades picture and sound out, swaps the scene, and brings the sound back
    private IEnumerator LoadRoutine(string sceneName)
    {
        isLoading = true;

        var fader = ScreenFader.Instance;
        float duration = fader != null ? fader.fadeDuration : 0.5f;
        float startVolume = AudioListener.volume;

        if (fader != null)
        {
            StartCoroutine(fader.FadeOut());
        }

        for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
        {
            AudioListener.volume = Mathf.Lerp(startVolume, 0f, t / duration);
            yield return null;
        }

        AudioListener.volume = 0f;

        // Let the fully black frame render before the load hitch
        yield return null;

        var operation = SceneManager.LoadSceneAsync(sceneName);
        while (operation != null && !operation.isDone)
        {
            yield return null;
        }

        for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
        {
            AudioListener.volume = Mathf.Lerp(0f, 1f, t / duration);
            yield return null;
        }

        AudioListener.volume = 1f;
        isLoading = false;
    }
}
