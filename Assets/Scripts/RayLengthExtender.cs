using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactors.Casters;
using UnityEngine.XR.Interaction.Toolkit.Interactors.Visuals;

/// <summary>
/// Extends the controller ray length so hotspots on the inside of the
/// 360 spheres can be reached and pointed at from the center.
/// </summary>
public class RayLengthExtender : MonoBehaviour
{
    /// <summary>
    /// How far, in meters, the controller rays reach and are drawn.
    /// </summary>
    [SerializeField]
    public float rayLength = 50f;

    // Creates an extender in the first scene and in every scene loaded afterwards
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
        EnsureInScene();
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // Adds an extender to a newly loaded scene
    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        EnsureInScene();
    }

    // Creates the extender if the active scene does not have one
    private static void EnsureInScene()
    {
        if (FindFirstObjectByType<RayLengthExtender>() == null)
        {
            new GameObject("RayLengthExtender").AddComponent<RayLengthExtender>();
        }
    }

    private void Start()
    {
        Apply();
    }

    /// <summary>
    /// Applies the ray length to every ray caster and ray visual in the scene.
    /// </summary>
    public void Apply()
    {
        // Far casters used by the Near-Far interactors
        foreach (var caster in FindObjectsByType<CurveInteractionCaster>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            caster.castDistance = rayLength;
        }

        // Line visuals drawn for the Near-Far interactors
        foreach (var visual in FindObjectsByType<CurveVisualController>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            visual.maxVisualCurveDistance = rayLength;
        }

        // Classic ray interactors, if any are present
        foreach (var ray in FindObjectsByType<XRRayInteractor>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            ray.maxRaycastDistance = rayLength;
        }
    }
}
