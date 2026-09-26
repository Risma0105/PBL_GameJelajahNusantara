using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target & Offset")]
    public Transform target;
    public Vector3 offset = new Vector3(0, 0, -10f);

    [Header("Kehalusan Gerak")]
    [Tooltip("Waktu redam kamera (makin kecil makin menempel rapat, 0.15f - 0.2f sangat ideal)")]
    public float smoothTime = 0.15f;
    private Vector3 currentVelocity = Vector3.zero;

    [Header("Batas Pergerakan Kamera (Bounds)")]
    public bool gunakanBatas = true;
    public float minX = -5f; // Batas mentok kiri
    public float maxX = 5f;  // Batas mentok kanan
    public float minY = -3f; // Batas mentok bawah
    public float maxY = 3f;  // Batas mentok atas

    void LateUpdate()
    {
        if (target == null) return;

        // Hitung posisi ideal
        Vector3 targetPosisi = target.position + offset;

        // Kunci batas map jika aktif
        if (gunakanBatas)
        {
            targetPosisi.x = Mathf.Clamp(targetPosisi.x, minX, maxX);
            targetPosisi.y = Mathf.Clamp(targetPosisi.y, minY, maxY);
        }

        // Pastikan Z selalu tepat terkunci di kedalaman kamera (-10f)
        targetPosisi.z = offset.z;

        // SmoothDamp menghasilkan luncuran kamera yang konsisten tanpa getaran frame
        transform.position = Vector3.SmoothDamp(transform.position, targetPosisi, ref currentVelocity, smoothTime);
    }
}