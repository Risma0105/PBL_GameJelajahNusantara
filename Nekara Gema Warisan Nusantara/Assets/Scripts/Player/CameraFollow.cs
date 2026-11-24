using UnityEngine;
using UnityEngine.Tilemaps;

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

    [Header("Batas Map (isi TEPI MAP, bukan batas tengah kamera)")]
    public bool gunakanBatas = true;
    [Tooltip("Opsional: kalau diisi, tepi map dihitung otomatis dari tilemap ini dan 4 angka di bawah diabaikan.")]
    public Tilemap tilemapBatas;
    public float tepiKiri = -10f;
    public float tepiKanan = 10f;
    public float tepiBawah = -6f;
    public float tepiAtas = 6f;

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
        AmbilBatasDariTilemap();
    }

    void AmbilBatasDariTilemap()
    {
        if (tilemapBatas == null) return;

        tilemapBatas.CompressBounds();
        Bounds lb = tilemapBatas.localBounds;
        Vector3 min = tilemapBatas.transform.TransformPoint(lb.min);
        Vector3 max = tilemapBatas.transform.TransformPoint(lb.max);

        tepiKiri = Mathf.Min(min.x, max.x);
        tepiKanan = Mathf.Max(min.x, max.x);
        tepiBawah = Mathf.Min(min.y, max.y);
        tepiAtas = Mathf.Max(min.y, max.y);
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

    // Batasi posisi kamera supaya TEPI tampilannya tidak keluar map
    Vector3 KunciKeBatas(Vector3 p)
    {
        float halfH = cam.orthographicSize;
        float halfW = halfH * cam.aspect;

        // kalau map lebih kecil dari layar, taruh di tengah map
        if (tepiKanan - tepiKiri <= halfW * 2f)
            p.x = (tepiKiri + tepiKanan) * 0.5f;
        else
            p.x = Mathf.Clamp(p.x, tepiKiri + halfW, tepiKanan - halfW);

        if (tepiAtas - tepiBawah <= halfH * 2f)
            p.y = (tepiBawah + tepiAtas) * 0.5f;
        else
            p.y = Mathf.Clamp(p.y, tepiBawah + halfH, tepiAtas - halfH);

        return p;
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 targetPosisi;
        float ukuranTarget = ukuranAwal;
        bool dialogAktif = modeDialog && fokusDialog != null;
        bool pakaiBatas = gunakanBatas && !(dialogAktif && abaikanBatasSaatDialog);

        if (dialogAktif)
        {
            Vector3 tengah = (target.position + fokusDialog.position) * 0.5f;
            targetPosisi = tengah + (Vector3)dialogOffset + offset;
            ukuranTarget = ukuranAwal * zoomDialog;
        }
        else
        {
            targetPosisi = target.position + offset;
        }

        if (pakaiBatas) targetPosisi = KunciKeBatas(targetPosisi);
        targetPosisi.z = offset.z;

        transform.position = Vector3.SmoothDamp(transform.position, targetPosisi, ref currentVelocity, smoothTime);

        // Zoom halus (hanya kalau kamera orthographic & tidak diatur Pixel Perfect Camera)
        if (cam.orthographic && Mathf.Abs(zoomDialog - 1f) > 0.001f)
        {
            cam.orthographicSize = Mathf.SmoothDamp(cam.orthographicSize, ukuranTarget, ref zoomVelocity, zoomSmoothTime);
        }

        // Pengaman terakhir: ukuran kamera bisa berubah karena zoom, jadi kunci lagi
        if (pakaiBatas)
        {
            Vector3 p = KunciKeBatas(transform.position);
            p.z = offset.z;
            transform.position = p;
        }
    }

    // Kotak kuning di Scene view = tepi map. Pilih Main Camera untuk melihatnya.
    void OnDrawGizmosSelected()
    {
        if (!gunakanBatas) return;
        Gizmos.color = Color.yellow;
        Vector3 c = new Vector3((tepiKiri + tepiKanan) * 0.5f, (tepiBawah + tepiAtas) * 0.5f, 0f);
        Vector3 s = new Vector3(tepiKanan - tepiKiri, tepiAtas - tepiBawah, 0f);
        Gizmos.DrawWireCube(c, s);
    }
}