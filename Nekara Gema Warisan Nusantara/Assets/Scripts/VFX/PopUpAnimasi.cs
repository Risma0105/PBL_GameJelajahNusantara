using System.Collections;
using UnityEngine;

// =====================================================================
// PopUpAnimasi
//
// Pasang di objek panel yang di-SetActive(true) saat popup muncul
// (mis. Panel_BadgeUnlock atau Panel prompt). Tiap panel aktif, 'target' (box) akan:
//   MUNCUL : membesar dengan efek membal + fade in + naik sedikit
//   TUTUP  : mengecil + fade out, lalu panel dimatikan (panggil Tutup() dari tombol X)
//   BUKA   : dipanggil kalau panel mau dimunculkan lagi (batalkan animasi tutup kalau masih jalan)
//
// Syarat: semua elemen popup (ikon, teks, tombol X) harus jadi ANAK 'target',
// supaya ikut membesar-mengecil bersama box.
// Memakai unscaledTime, jadi tetap jalan walau Time.timeScale = 0.
// =====================================================================
public class PopUpAnimasi : MonoBehaviour
{
    [Header("Target (isi dengan 'box'; elemen lain harus jadi anak box)")]
    public RectTransform target;

    [Tooltip("Objek yang dimatikan setelah animasi tutup. Kosong = objek ini sendiri")]
    public GameObject panelYangDitutup;

    [Header("Muncul")]
    public float durasiMuncul = 0.35f;
    [Range(0f, 1f)] public float skalaAwal = 0.6f;
    [Tooltip("Mulai dari sekian piksel di bawah posisi akhir (negatif = dari bawah)")]
    public float geserAwalY = -40f;
    [Tooltip("Seberapa membal. 0 = tanpa membal, 1.7 = standar, 3 = sangat membal")]
    public float membal = 1.7f;

    [Header("Tutup")]
    public float durasiTutup = 0.2f;
    [Range(0f, 1f)] public float skalaAkhir = 0.8f;

    CanvasGroup grup;
    Vector3 skalaAsli = Vector3.one;
    Vector2 posAsli;
    bool awalTersimpan;
    bool sedangTutup;
    Coroutine jalan;


    void Awake()
    {
        SimpanKeadaanAsli();
    }

    void OnEnable()
    {
        SimpanKeadaanAsli();
        if (!awalTersimpan) return;

        sedangTutup = false;
        TerapkanMuncul(0f);   // langsung ke keadaan awal supaya tidak ada kedipan 1 frame penuh
        jalan = StartCoroutine(Muncul());
    }

    void OnDisable()
    {
        // Pulihkan supaya tidak ada yang tertinggal mengecil/transparan di aktivasi berikutnya
        if (awalTersimpan) Pulihkan();
        jalan = null;
    }

    void SimpanKeadaanAsli()
    {
        if (awalTersimpan) return;

        if (target == null) target = transform as RectTransform;
        if (target == null)
        {
            Debug.LogWarning("PopUpAnimasi: isi 'Target' dengan RectTransform box.", this);
            return;
        }

        grup = target.GetComponent<CanvasGroup>();
        if (grup == null) grup = target.gameObject.AddComponent<CanvasGroup>();

        skalaAsli = target.localScale;
        posAsli = target.anchoredPosition;
        awalTersimpan = true;
    }


    // =====================================
    // MUNCUL
    // =====================================

    IEnumerator Muncul()
    {
        float t = 0f;
        float durasi = Mathf.Max(0.01f, durasiMuncul);

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / durasi;
            TerapkanMuncul(Mathf.Clamp01(t));
            yield return null;
        }

        Pulihkan();   // pastikan berhenti tepat di ukuran & posisi asli
        jalan = null;
    }

    void TerapkanMuncul(float t)
    {
        float e = EaseOutBack(t, membal);

        target.localScale = skalaAsli * Mathf.LerpUnclamped(skalaAwal, 1f, e);
        target.anchoredPosition = posAsli + new Vector2(0f, Mathf.Lerp(geserAwalY, 0f, EaseOutCubic(t)));

        grup.alpha = Mathf.Clamp01(t * 2f);   // fade in selesai di separuh durasi
        grup.interactable = false;            // tombol belum bisa diklik selama animasi
        grup.blocksRaycasts = false;
    }


    // =====================================
    // BUKA (dipanggil dari script lain, mis. ProximityPrompt)
    // =====================================

    public void Buka()
    {
        GameObject p = panelYangDitutup != null ? panelYangDitutup : gameObject;

        if (!p.activeSelf)
        {
            p.SetActive(true);   // OnEnable otomatis memutar animasi muncul
            return;
        }

        if (sedangTutup)         // batalkan animasi tutup, muncul lagi
        {
            sedangTutup = false;
            if (jalan != null) StopCoroutine(jalan);
            TerapkanMuncul(0f);
            jalan = StartCoroutine(Muncul());
        }
    }


    // =====================================
    // TUTUP (panggil dari OnClick tombol X)
    // =====================================

    public void Tutup()
    {
        if (!isActiveAndEnabled || !awalTersimpan || sedangTutup)
            return;

        sedangTutup = true;
        if (jalan != null) StopCoroutine(jalan);
        jalan = StartCoroutine(TutupRoutine());
    }

    IEnumerator TutupRoutine()
    {
        grup.interactable = false;
        grup.blocksRaycasts = false;

        Vector3 skalaMulai = target.localScale;
        float alphaMulai = grup.alpha;
        float t = 0f;
        float durasi = Mathf.Max(0.01f, durasiTutup);

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / durasi;
            float e = EaseInCubic(Mathf.Clamp01(t));

            target.localScale = Vector3.LerpUnclamped(skalaMulai, skalaAsli * skalaAkhir, e);
            grup.alpha = Mathf.Lerp(alphaMulai, 0f, e);
            yield return null;
        }

        jalan = null;
        GameObject yangDitutup = panelYangDitutup != null ? panelYangDitutup : gameObject;
        yangDitutup.SetActive(false);   // OnDisable memulihkan keadaan asli
    }


    // =====================================
    // BANTUAN
    // =====================================

    void Pulihkan()
    {
        target.localScale = skalaAsli;
        target.anchoredPosition = posAsli;
        grup.alpha = 1f;
        grup.interactable = true;
        grup.blocksRaycasts = true;
    }

    static float EaseOutBack(float t, float c1)
    {
        float c3 = c1 + 1f;
        float x = t - 1f;
        return 1f + c3 * x * x * x + c1 * x * x;
    }

    static float EaseOutCubic(float t)
    {
        float x = 1f - t;
        return 1f - x * x * x;
    }

    static float EaseInCubic(float t)
    {
        return t * t * t;
    }
}