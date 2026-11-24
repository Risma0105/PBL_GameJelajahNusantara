using UnityEngine;

[RequireComponent(typeof(Camera))]
public class MiniMapCameraController : MonoBehaviour
{
    [SerializeField] private Transform player;

    [Header("Isi dari kamera lama yang masih kotak")]
    [SerializeField] private float mapCenterY;     // Position Y kamera lama
    [SerializeField] private float mapFullHeight;  // Orthographic Size lama x 2

    [Header("Pengaturan")]
    [SerializeField, Range(0.3f, 1f)] private float visibleRatio = 2f / 3f;
    [SerializeField] private float edgeMargin = 0.5f;  // jarak aman dari tepi sebelum kamera geser
    [SerializeField] private float smoothTime = 0.15f;

    private Camera cam;
    private bool showBottom;
    private float velY;
    private float visibleH, topY, bottomY;

    private void Awake()
    {
        cam = GetComponent<Camera>();

        visibleH = mapFullHeight * visibleRatio;
        cam.orthographicSize = visibleH * 0.5f;

        float mapTop = mapCenterY + mapFullHeight * 0.5f;
        float mapBottom = mapCenterY - mapFullHeight * 0.5f;

        topY = mapTop - visibleH * 0.5f;        // posisi kamera saat nampilin 2/3 atas
        bottomY = mapBottom + visibleH * 0.5f;  // posisi kamera saat nampilin 2/3 bawah

        // mulai dari posisi sesuai letak pemain
        showBottom = player != null && player.position.y < mapCenterY;
        Vector3 p = transform.position;
        p.y = showBottom ? bottomY : topY;
        transform.position = p;
    }

    private void LateUpdate()
    {
        if (player == null) return;

        float py = player.position.y;
        float topWindowLowerEdge = topY - visibleH * 0.5f;      // tepi bawah jendela atas
        float bottomWindowUpperEdge = bottomY + visibleH * 0.5f; // tepi atas jendela bawah

        if (!showBottom && py < topWindowLowerEdge + edgeMargin)
            showBottom = true;
        else if (showBottom && py > bottomWindowUpperEdge - edgeMargin)
            showBottom = false;

        float targetY = showBottom ? bottomY : topY;
        Vector3 pos = transform.position;
        pos.y = Mathf.SmoothDamp(pos.y, targetY, ref velY, smoothTime);
        transform.position = pos;
    }
}