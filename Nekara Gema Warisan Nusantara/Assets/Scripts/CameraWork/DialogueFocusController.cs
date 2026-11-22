using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Ngatur "mode fokus dialog": kamera geser, HUD fade out, vignette fade in.
/// Taruh di satu GameObject di scene (misal "GameManager").
/// </summary>
public class DialogueFocusController : MonoBehaviour
{
    public static DialogueFocusController Instance { get; private set; }

    [Header("Referensi")]
    public CameraFollow cameraFollow;

    [Tooltip("Semua CanvasGroup HUD yang mau disembunyikan saat dialog (minimap, jam, inventory, tombol, dll).")]
    public CanvasGroup[] hudGroups;

    [Header("Vignette")]
    [Tooltip("Image fullscreen (stretch) di Canvas. Kalau Sprite kosong, vignette dibuat otomatis lewat kode.")]
    public Image vignetteImage;
    public Color vignetteWarna = Color.black;
    [Range(0f, 1f)] public float vignetteKegelapan = 0.7f;
    [Range(0f, 1f)] public float vignetteMulaiDari = 0.35f; // makin kecil, area gelap makin luas

    [Header("Fade")]
    public float durasiFade = 0.3f;

    public bool SedangDialog { get; private set; }

    private float progres; // 0 = normal, 1 = mode fokus penuh

    void Awake()
    {
        Instance = this;

        if (vignetteImage != null)
        {
            if (vignetteImage.sprite == null)
                vignetteImage.sprite = BuatSpriteVignette(256, vignetteMulaiDari);

            vignetteImage.raycastTarget = false;
        }

        Terapkan(0f);
    }

    public void Mulai(Transform npc)
    {
        SedangDialog = true;
        if (cameraFollow != null) cameraFollow.MulaiFokusDialog(npc);
    }

    public void Selesai()
    {
        SedangDialog = false;
        if (cameraFollow != null) cameraFollow.SelesaiFokusDialog();
    }

    void Update()
    {
        // unscaledDeltaTime supaya tetap jalan kalau game di-pause saat dialog
        float tujuan = SedangDialog ? 1f : 0f;
        if (!Mathf.Approximately(progres, tujuan))
        {
            progres = Mathf.MoveTowards(progres, tujuan, Time.unscaledDeltaTime / Mathf.Max(0.01f, durasiFade));
            Terapkan(progres);
        }
    }

    void Terapkan(float t)
    {
        float halus = Mathf.SmoothStep(0f, 1f, t);

        foreach (var g in hudGroups)
        {
            if (g == null) continue;
            g.alpha = 1f - halus;
            bool aktif = t < 0.01f;
            g.interactable = aktif;
            g.blocksRaycasts = aktif;
        }

        if (vignetteImage != null)
        {
            Color c = vignetteWarna;
            c.a = halus * vignetteKegelapan;
            vignetteImage.color = c;
        }
    }

    // Bikin sprite radial: tengah transparan, pinggir gelap
    static Sprite BuatSpriteVignette(int ukuran, float mulai)
    {
        var tex = new Texture2D(ukuran, ukuran, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;

        float pusat = (ukuran - 1) * 0.5f;
        for (int y = 0; y < ukuran; y++)
        {
            for (int x = 0; x < ukuran; x++)
            {
                float dx = (x - pusat) / pusat;
                float dy = (y - pusat) / pusat;
                float jarak = Mathf.Sqrt(dx * dx + dy * dy); // 0 di tengah, ~1.41 di pojok
                float a = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(mulai, 1.2f, jarak));
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
        }
        tex.Apply();

        return Sprite.Create(tex, new Rect(0, 0, ukuran, ukuran), new Vector2(0.5f, 0.5f), 100f);
    }
}