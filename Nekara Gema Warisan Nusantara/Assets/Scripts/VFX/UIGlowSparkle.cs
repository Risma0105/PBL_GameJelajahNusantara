using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// =====================================================================
// UIGlowSparkle
//
// Pasang di objek ikon alat musik (yang punya komponen Image), mis. "alat-musik".
// Script ini membuat sendiri:
//   - GLOW bulat yang berdenyut di BELAKANG ikon
//   - KILAU bintang kecil yang muncul-hilang di sekitar ikon
// Semua gambar dibuat lewat kode, jadi tidak butuh sprite, material, atau VFX Graph.
// Memakai unscaledTime, jadi tetap jalan walau Time.timeScale = 0 saat panel tampil.
//
// Catatan: ikon sebaiknya anchor-nya satu titik (Min = Max), seperti alat-musik (0.5, 0.5).
// =====================================================================
[RequireComponent(typeof(RectTransform))]
public class UIGlowSparkle : MonoBehaviour
{
    [Header("Glow")]
    public Color warnaGlow = new Color(1f, 0.82f, 0.3f, 1f);
    [Tooltip("Ukuran glow = sisi terbesar ikon x angka ini")]
    public float ukuranGlow = 1.9f;
    public float kecepatanDenyut = 2.5f;
    [Range(0f, 1f)] public float alphaMin = 0.35f;
    [Range(0f, 1f)] public float alphaMax = 0.75f;
    [Tooltip("Seberapa besar glow membesar-mengecil saat berdenyut")]
    public float skalaDenyut = 0.08f;

    [Header("Kilau")]
    public Color warnaKilau = new Color(1f, 0.97f, 0.75f, 1f);
    public int jumlahKilau = 10;
    [Tooltip("Ukuran bintang (px) terkecil - terbesar")]
    public Vector2 rentangUkuran = new Vector2(10f, 26f);
    [Tooltip("Lama hidup satu kilau (detik) tercepat - terlama")]
    public Vector2 rentangDurasi = new Vector2(0.8f, 1.6f);
    [Tooltip("Sebaran posisi kilau, x ukuran ikon. 0.5 = pas di badan ikon")]
    public float sebaranArea = 0.75f;
    public float kecepatanPutar = 40f;

    class Kilau
    {
        public RectTransform rt;
        public Image img;
        public float umur, durasi, putar;
    }

    RectTransform ikon;
    RectTransform glowRT;
    Image glowImg;
    readonly List<Kilau> daftarKilau = new List<Kilau>();
    Texture2D texGlow, texBintang;
    Sprite sprGlow, sprBintang;
    bool siap;


    void Awake()
    {
        Bangun();
    }

    void OnEnable()
    {
        AturAktif(true);
    }

    void OnDisable()
    {
        // Glow & kilau adalah saudara ikon, jadi ikut dimatikan kalau ikon dimatikan
        AturAktif(false);
    }

    void OnDestroy()
    {
        if (glowRT != null) Destroy(glowRT.gameObject);
        foreach (var k in daftarKilau)
            if (k.rt != null) Destroy(k.rt.gameObject);

        if (sprGlow != null) Destroy(sprGlow);
        if (sprBintang != null) Destroy(sprBintang);
        if (texGlow != null) Destroy(texGlow);
        if (texBintang != null) Destroy(texBintang);
    }


    // =====================================
    // MEMBANGUN GLOW & KILAU
    // =====================================

    void Bangun()
    {
        ikon = (RectTransform)transform;
        Transform induk = ikon.parent;
        if (induk == null) return;

        texGlow = BuatTexturGlow(64);
        texBintang = BuatTexturBintang(48);
        sprGlow = BuatSprite(texGlow);
        sprBintang = BuatSprite(texBintang);

        // --- Glow: ditaruh TEPAT DI BAWAH ikon (index ikon dipakai glow, ikon geser naik) ---
        float sisi = Mathf.Max(ikon.rect.width * Mathf.Abs(ikon.localScale.x),
                               ikon.rect.height * Mathf.Abs(ikon.localScale.y)) * ukuranGlow;

        glowRT = BuatGambar("Glow", induk, sprGlow, warnaGlow, out glowImg);
        glowRT.anchorMin = ikon.anchorMin;
        glowRT.anchorMax = ikon.anchorMax;
        glowRT.pivot = ikon.pivot;
        glowRT.anchoredPosition = ikon.anchoredPosition;
        glowRT.sizeDelta = new Vector2(sisi, sisi);
        glowRT.SetSiblingIndex(ikon.GetSiblingIndex());

        // --- Kilau: tepat di atas ikon ---
        for (int i = 0; i < Mathf.Max(0, jumlahKilau); i++)
        {
            var k = new Kilau();
            k.rt = BuatGambar("Kilau", induk, sprBintang, warnaKilau, out k.img);
            k.rt.anchorMin = ikon.anchorMin;
            k.rt.anchorMax = ikon.anchorMax;
            k.rt.pivot = new Vector2(0.5f, 0.5f);
            k.rt.sizeDelta = Vector2.one * Random.Range(rentangUkuran.x, rentangUkuran.y);
            k.rt.SetSiblingIndex(ikon.GetSiblingIndex() + 1);

            AcakKilau(k, true);
            daftarKilau.Add(k);
        }

        siap = true;
        AturAktif(isActiveAndEnabled);
    }

    RectTransform BuatGambar(string nama, Transform induk, Sprite spr, Color warna, out Image img)
    {
        var go = new GameObject(nama, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.layer = induk.gameObject.layer;
        go.transform.SetParent(induk, false);

        img = go.GetComponent<Image>();
        img.sprite = spr;
        img.color = warna;
        img.raycastTarget = false;   // jangan menghalangi klik tombol

        return go.GetComponent<RectTransform>();
    }

    Sprite BuatSprite(Texture2D tex)
    {
        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height),
                             new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
    }

    void AturAktif(bool aktif)
    {
        if (!siap) return;

        if (glowRT != null) glowRT.gameObject.SetActive(aktif);
        foreach (var k in daftarKilau)
            if (k.rt != null) k.rt.gameObject.SetActive(aktif);
    }


    // =====================================
    // UPDATE
    // =====================================

    void Update()
    {
        if (!siap) return;

        // Glow berdenyut
        float s = (Mathf.Sin(Time.unscaledTime * kecepatanDenyut) + 1f) * 0.5f;   // 0-1
        Color cg = warnaGlow;
        cg.a = warnaGlow.a * Mathf.Lerp(alphaMin, alphaMax, s);
        glowImg.color = cg;
        glowRT.localScale = Vector3.one * (1f + skalaDenyut * (s - 0.5f) * 2f);

        // Kilau muncul - membesar - hilang, lalu pindah posisi
        float dt = Time.unscaledDeltaTime;
        foreach (var k in daftarKilau)
        {
            k.umur += dt;
            float p = k.umur / k.durasi;

            if (p >= 1f)
            {
                AcakKilau(k, false);
                p = 0f;
            }

            float kurva = Mathf.Sin(p * Mathf.PI);   // 0 -> 1 -> 0

            k.rt.localScale = Vector3.one * kurva;
            k.rt.Rotate(0f, 0f, k.putar * dt);

            Color c = warnaKilau;
            c.a = warnaKilau.a * kurva;
            k.img.color = c;
        }
    }

    void AcakKilau(Kilau k, bool awal)
    {
        k.durasi = Random.Range(rentangDurasi.x, rentangDurasi.y);
        k.umur = awal ? Random.Range(0f, k.durasi) : 0f;   // awal: diacak supaya tidak muncul serempak
        k.putar = Random.Range(-kecepatanPutar, kecepatanPutar);

        Vector2 area = new Vector2(ikon.rect.width * Mathf.Abs(ikon.localScale.x),
                                   ikon.rect.height * Mathf.Abs(ikon.localScale.y)) * sebaranArea;
        Vector2 p = Random.insideUnitCircle;

        k.rt.anchoredPosition = ikon.anchoredPosition + new Vector2(p.x * area.x, p.y * area.y);
        k.rt.localEulerAngles = new Vector3(0f, 0f, Random.Range(0f, 90f));
    }


    // =====================================
    // TEKSTUR DIBUAT LEWAT KODE
    // =====================================

    // Lingkaran lembut: putih di tengah, memudar ke tepi
    static Texture2D BuatTexturGlow(int n)
    {
        var tex = new Texture2D(n, n, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;

        var px = new Color[n * n];
        float r = (n - 1) * 0.5f;

        for (int y = 0; y < n; y++)
        {
            for (int x = 0; x < n; x++)
            {
                float dx = (x - r) / r;
                float dy = (y - r) / r;
                float d = Mathf.Sqrt(dx * dx + dy * dy);
                float a = Mathf.Clamp01(1f - d);
                px[y * n + x] = new Color(1f, 1f, 1f, a * a);
            }
        }

        tex.SetPixels(px);
        tex.Apply();
        return tex;
    }

    // Bintang 4 sudut (kilau)
    static Texture2D BuatTexturBintang(int n)
    {
        var tex = new Texture2D(n, n, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;

        var px = new Color[n * n];
        float r = (n - 1) * 0.5f;

        for (int y = 0; y < n; y++)
        {
            for (int x = 0; x < n; x++)
            {
                float dx = Mathf.Abs((x - r) / r);
                float dy = Mathf.Abs((y - r) / r);
                float v = Mathf.Sqrt(dx) + Mathf.Sqrt(dy);   // bentuk astroid = bintang 4 sudut
                float a = Mathf.Pow(Mathf.Clamp01(1f - v), 1.5f);
                px[y * n + x] = new Color(1f, 1f, 1f, a);
            }
        }

        tex.SetPixels(px);
        tex.Apply();
        return tex;
    }
}