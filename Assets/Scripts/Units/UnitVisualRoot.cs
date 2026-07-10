using UnityEngine;

/// <summary>
/// Moves the visual root's Y position to match the ground beneath the logical unit (parent).
/// The parent always sits at the floor plane (flat per-floor Y), while this visual root
/// smoothly follows stairs, slopes, and uneven terrain on top of that.
/// </summary>
public class UnitVisualRoot : MonoBehaviour
{
    [Header("Raycasting")]
    // Must be taller than the highest stair/slope the unit can walk over,
    // so the ray origin always starts above any terrain bump above the parent's floor plane.
    [SerializeField] float raycastOriginHeight = 2f;

    // Total downward distance the ray travels. Should cover:
    // raycastOriginHeight (back down to parent's feet) + a small buffer below.
    // Keep this tight to avoid punching through to a lower floor in multi-floor maps.
    [SerializeField] float raycastMaxDistance = 3f;

    [SerializeField] LayerMask walkableLayers;

    [Header("Smoothing")]
    [SerializeField] float snapSpeed = 12f;

    [Header("Offset")]
    // Fine-tune how close the visual root sits to the ground surface.
    [SerializeField] float footOffset = 0f;

    float targetY;
    bool hasValidGround = false;

    private void Start()
    {
        // On startup, snap immediately to ground rather than lerping up from 0.
        if (CastToGround(out RaycastHit hit))
        {
            targetY = hit.point.y + footOffset;
            SetYImmediate(targetY);
            hasValidGround = true;
        }
    }

    private void Update()
    {
        // Always update targetY when ground is found beneath the parent.
        if (CastToGround(out RaycastHit hit))
        {
            targetY = hit.point.y + footOffset;
            hasValidGround = true;
        }

        // Don't move at all until we have a valid ground sample —
        // avoids snapping to world origin (Y=0) on the first frame.
        if (!hasValidGround)
            return;

        // First valid frame: snap instantly.
        // Subsequent frames: lerp smoothly toward the target.
        float newY = hasValidGround
            ? Mathf.Lerp(transform.position.y, targetY, Time.deltaTime * snapSpeed)
            : targetY;

        SetYImmediate(newY);
    }

    /// <summary>
    /// Raycasts downward from above the parent's position (not the visual root's position).
    /// Using the parent keeps the origin stable at the floor plane, so:
    ///   - The ray reliably clears stairs/slopes above the parent
    ///   - The ray never reaches far enough to hit a lower floor in multi-story maps
    /// </summary>
    private bool CastToGround(out RaycastHit hit)
    {
        // Always read position from the parent (logical unit), never from this transform.
        // This visual root's Y drifts with terrain; the parent's Y is the stable floor plane.
        Transform source = transform.parent != null ? transform.parent : transform;

        Vector3 origin = source.position + Vector3.up * raycastOriginHeight;

        return Physics.Raycast(origin, Vector3.down, out hit, raycastMaxDistance, walkableLayers);
    }

    private void SetYImmediate(float y)
    {
        Vector3 pos = transform.position;
        pos.y = y;
        transform.position = pos;
    }
}