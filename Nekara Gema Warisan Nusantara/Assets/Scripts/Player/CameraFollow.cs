using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target & Offset")]
    public Transform target;
    public Vector3 offset = new Vector3(0, 0, -10f);

    [Header("Kehalusan Gerak")]
    [Range(0.01f, 1f)]
    public float smoothSpeed = 0.125f;

    [Header("Batas Pergerakan Kamera (Bounds)")]
    public bool gunakanBatas = true;
    public float minX = -5f; // Batas mentok kiri
    public float maxX = 5f;  // Batas mentok kanan
    public float minY = -3f; // Batas mentok bawah
    public float maxY = 3f;  // Batas mentok atas

    void LateUpdate()
    {
        if (target == null) return;

        // Posisi ideal mengikuti pemain
        Vector3 targetPosisi = target.position + offset;

        // Jika fitur batas aktif, kunci koordinatnya agar tidak tembus map
        if (gunakanBatas)
        {
            targetPosisi.x = Mathf.Clamp(targetPosisi.x, minX, maxX);
            targetPosisi.y = Mathf.Clamp(targetPosisi.y, minY, maxY);
        }

        // Gerakan kamera halus (lerp)
        Vector3 posisiHalus = Vector3.Lerp(transform.position, targetPosisi, smoothSpeed);
        transform.position = posisiHalus;
    }
}