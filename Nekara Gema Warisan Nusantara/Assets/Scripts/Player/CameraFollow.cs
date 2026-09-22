using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target & Offset")]
    public Transform target;
    public Vector3 offset = new Vector3(0f, 0f, -10f); // -10 wajib di 2D agar kamera tidak menembus layar

    [Header("Kehalusan Gerak")]
    [Range(0.01f, 1f)]
    public float smoothSpeed = 0.125f;

    void FixedUpdate()
    {
        if (target == null) return;

        // Posisi tujuan kamera
        Vector3 desiredPosition = target.position + offset;

        // Transisi halus dari posisi sekarang ke posisi tujuan
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        transform.position = smoothedPosition;
    }
}