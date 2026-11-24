using UnityEngine;
using UnityEngine.UI;

public class PrologFokusUI : MonoBehaviour
{
    [Header("Vignette")]
    [Tooltip("Image fullscreen (stretch). Kalau Sprite kosong, dibuat otomatis lewat kode.")]
    public Image vignetteImage;
    public Color vignetteWarna = Color.black;
    [Range(0f, 1f)] public float vignetteKegelapan = 0.7f;
    [Range(0f, 1f)] public float vignetteMulaiDari = 0.35f; // makin kecil, area gelap makin luas
    public float durasiFade = 0.3f;

    [Header("Instruksi (sprite klik kiri)")]
    [Tooltip("Objek Instruksi_Lanjut yang ada BlinkInstruksi (di dirinya atau anaknya). Jangan taruh script ini di objek itu.")]
    public GameObject instruksiObject;

    float progres;      // 0 = vignette hilang, 1 = penuh
    bool vignetteOn;
    BlinkInstruksi blink;

    void Awake()
    {
        if (vignetteImage != null)
        {
            if (vignetteImage.sprite == null)
                vignetteImage.sprite = BuatSpriteVignette(256, vignetteMulaiDari);
            vignetteImage.raycastTarget = false; // jangan blokir klik
        }

        if (instruksiObject != null)
        {
            blink = instruksiObject.GetComponentInChildren<BlinkInstruksi>(true);
            if (blink != null)
            {
                // objek dibiarkan aktif, yang di-fade cuma alpha-nya
                instruksiObject.SetActive(true);
                blink.SetTampil(false, true);
            }
            else
            {
                // cadangan kalau BlinkInstruksi nggak ketemu: tanpa fade
                instruksiObject.SetActive(false);
            }
        }

        TerapkanVignette();
    }

    // Dipanggil dari DialogueManager / PrologueDialogSystem
    public void SetVignette(bool aktif) => vignetteOn = aktif;

    public void SetInstruksi(bool aktif)
    {
        if (instruksiObject == null) return;

        if (blink != null)
            blink.SetTampil(aktif);
        else if (instruksiObject.activeSelf != aktif)
            instruksiObject.SetActive(aktif);
    }

    void Update()
    {
        float tujuan = vignetteOn ? 1f : 0f;
        if (Mathf.Approximately(progres, tujuan)) return;

        // unscaledDeltaTime supaya tetap jalan walau game di-pause
        progres = Mathf.MoveTowards(progres, tujuan, Time.unscaledDeltaTime / Mathf.Max(0.01f, durasiFade));
        TerapkanVignette();
    }

    void TerapkanVignette()
    {
        if (vignetteImage == null) return;
        Color c = vignetteWarna;
        c.a = Mathf.SmoothStep(0f, 1f, progres) * vignetteKegelapan;
        vignetteImage.color = c;
    }

    // Sprite radial: tengah transparan, pinggir gelap
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
                float jarak = Mathf.Sqrt(dx * dx + dy * dy);
                float a = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(mulai, 1.2f, jarak));
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
        }
        tex.Apply();

        return Sprite.Create(tex, new Rect(0, 0, ukuran, ukuran), new Vector2(0.5f, 0.5f), 100f);
    }
}