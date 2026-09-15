using UnityEngine;

/// <summary>
/// Keeps a world-space panel at a fixed offset from the viewer's head.
/// It follows head position every frame but only turns to the viewer's new
/// facing direction after they look away past a threshold, so it never jitters.
/// </summary>
public class HeadFollower : MonoBehaviour
{
    /// <summary>
    /// Horizontal distance in meters from the head to the panel.
    /// </summary>
    [SerializeField]
    public float distance = 2.2f;

    /// <summary>
    /// Vertical offset in meters from eye height (negative is below the eyes).
    /// </summary>
    [SerializeField]
    public float height = -0.9f;

    /// <summary>
    /// Degrees the viewer must turn away before the panel follows.
    /// </summary>
    [SerializeField]
    public float angleThreshold = 45f;

    /// <summary>
    /// How quickly the panel swings round once it starts following.
    /// </summary>
    [SerializeField]
    public float turnSpeed = 3f;

    // Camera used as the head
    private Camera head;

    // Yaw angle the panel currently sits at
    private float yaw;

    // Whether the yaw has been set from the head yet
    private bool hasYaw = false;

    // Whether the panel is currently swinging toward the head direction
    private bool isTurning = false;

    private void LateUpdate()
    {
        if (head == null)
        {
            head = Camera.main;
            if (head == null)
                return;
        }

        float headYaw = head.transform.eulerAngles.y;
        if (!hasYaw)
        {
            yaw = headYaw;
            hasYaw = true;
        }

        float delta = Mathf.DeltaAngle(yaw, headYaw);
        if (Mathf.Abs(delta) > angleThreshold)
        {
            isTurning = true;
        }

        if (isTurning)
        {
            yaw = Mathf.LerpAngle(yaw, headYaw, Time.deltaTime * turnSpeed);
            if (Mathf.Abs(Mathf.DeltaAngle(yaw, headYaw)) < 2f)
            {
                isTurning = false;
            }
        }

        Vector3 headPosition = head.transform.position;
        transform.position = headPosition + Quaternion.Euler(0f, yaw, 0f) * new Vector3(0f, height, distance);
        transform.rotation = Quaternion.LookRotation(transform.position - headPosition, Vector3.up);
    }
}
