using UnityEngine;

[RequireComponent(typeof(Camera))]
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

    [Header("Mode Dialog")]
    [Tooltip("Geser kamera dari titik tengah (player + NPC). Y positif = karakter turun di layar, ruang atas buat bubble makin lega.")]
    public Vector2 dialogOffset = new Vector2(0f, 1.5f);
    [Tooltip("Pengali orthographic size saat dialog. 1 = tanpa zoom, 0.8 = zoom in 20%.")]
    public float zoomDialog = 0.85f;
    public float zoomSmoothTime = 0.25f;
    [Tooltip("Kalau aktif, batas map diabaikan saat dialog (berguna kalau NPC dekat tepi map).")]
    public bool abaikanBatasSaatDialog = false;

    private Camera cam;
    private float ukuranAwal;
    private float zoomVelocity;
    private Transform fokusDialog;
    private bool modeDialog;

    void Awake()
    {
        cam = GetComponent<Camera>();
        ukuranAwal = cam.orthographicSize;
    }

    // Dipanggil saat tekan E di NPC
    public void MulaiFokusDialog(Transform npc)
    {
        fokusDialog = npc;
        modeDialog = true;
    }

    // Dipanggil saat dialog selesai
    public void SelesaiFokusDialog()
    {
        modeDialog = false;
        fokusDialog = null;
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 targetPosisi;
        float ukuranTarget = ukuranAwal;
        bool dialogAktif = modeDialog && fokusDialog != null;

        if (dialogAktif)
        {
            // Titik tengah antara player & NPC, lalu digeser ke atas
            Vector3 tengah = (target.position + fokusDialog.position) * 0.5f;
            targetPosisi = tengah + (Vector3)dialogOffset + offset;
            ukuranTarget = ukuranAwal * zoomDialog;
        }
        else
        {
            targetPosisi = target.position + offset;
        }

        // Kunci batas map jika aktif
        if (gunakanBatas && !(dialogAktif && abaikanBatasSaatDialog))
        {
            targetPosisi.x = Mathf.Clamp(targetPosisi.x, minX, maxX);
            targetPosisi.y = Mathf.Clamp(targetPosisi.y, minY, maxY);
        }

        // Pastikan Z selalu tepat terkunci di kedalaman kamera
        targetPosisi.z = offset.z;

        transform.position = Vector3.SmoothDamp(transform.position, targetPosisi, ref currentVelocity, smoothTime);

        // Zoom halus (hanya kalau kamera orthographic & tidak diatur Pixel Perfect Camera)
        if (cam.orthographic && Mathf.Abs(zoomDialog - 1f) > 0.001f)
        {
            cam.orthographicSize = Mathf.SmoothDamp(cam.orthographicSize, ukuranTarget, ref zoomVelocity, zoomSmoothTime);
        }
    }
}