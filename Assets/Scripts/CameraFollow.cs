using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform carTarget;

    [Header("Movement")]
    [Range(1f, 20f)]
    public float moveSmoothness = 10f;

    public Vector3 moveOffset = new Vector3(0f, 5f, -10f);

    [Header("Rotation")]
    [Range(1f, 20f)]
    public float rotSmoothness = 8f;

    public Vector3 rotOffset;

    [Header("Options")]
    public bool followOnStart = true;

    private void Start()
    {
        // Langsung pindah ke posisi awal
        if (followOnStart && carTarget != null)
        {
            transform.position = carTarget.TransformPoint(moveOffset);
            transform.LookAt(carTarget);
        }
    }

    private void LateUpdate()
    {
        if (carTarget == null)
            return;

        FollowTarget();
    }

    void FollowTarget()
    {
        HandleMovement();
        HandleRotation();
    }

    void HandleMovement()
    {
        // Posisi target kamera
        Vector3 targetPosition = carTarget.TransformPoint(moveOffset);

        // Smooth follow
        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            moveSmoothness * Time.deltaTime
        );
    }

    void HandleRotation()
    {
        // Arah ke mobil
        Vector3 direction = carTarget.position - transform.position;

        // Rotasi target
        Quaternion targetRotation = Quaternion.LookRotation(
            direction + rotOffset,
            Vector3.up
        );

        // Smooth rotation
        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            targetRotation,
            rotSmoothness * Time.deltaTime
        );
    }

    // Visual debugging
    private void OnDrawGizmosSelected()
    {
        if (carTarget == null)
            return;

        Gizmos.color = Color.yellow;

        Vector3 targetPosition = carTarget.TransformPoint(moveOffset);

        Gizmos.DrawLine(carTarget.position, targetPosition);
        Gizmos.DrawSphere(targetPosition, 0.4f);
    }
}