using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Transisi PERSISTEN: dibuat otomatis saat game mulai, hidup lintas scene.
// Ga perlu dipasang di scene mana pun.
//
// Alur:
//   Pindah("X")  : fade ke hitam -> load scene -> (banner kalau scene tujuan punya) -> hitam fade out
//
// Pindah scene pakai:  MapFadeManager.Pindah("NamaScene");   (atau BannerNamaMap.Pindah, sama aja)
public class MapFadeManager : MonoBehaviour
{
    // ---------- Pengaturan (ubah di sini) ----------
    public static float durasiFadeKeluar = 0.4f;          // scene lama -> hitam
    public static float durasiFadeMasuk = 0.5f;           // hitam -> scene baru (tanpa banner)
    public static float durasiFadeBanner = 0.5f;          // fade in/out banner
    public static float durasiBannerTampil = 1.2f;        // banner terlihat penuh di atas hitam
    public static float durasiBannerSetelahHitam = 0.8f;  // banner nempel setelah hitam hilang
    public static float skalaAwalBanner = 0.92f;          // banner "pop" dari skala ini ke 1
    public static float lebarBanner = 1500f;              // lebar banner di layar (referensi 1920x1080). Tinggi ngikutin aspek sprite
    public static bool bekukanWaktu = true;               // game berhenti selama layar hitam + banner

    // ---------- Singleton ----------
    private static MapFadeManager _inst;

    public static MapFadeManager Instance
    {
        get
        {
            if (_inst == null)
            {
                var go = new GameObject("MapFadeManager");
                _inst = go.AddComponent<MapFadeManager>();
            }
            return _inst;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init()
    {
        var _ = Instance; // buat sebelum scene pertama dimuat
    }

    // ---------- API ----------
    public static void Pindah(string namaScene)
    {
        Instance.StartCoroutine(Instance.RutinPindah(namaScene));
    }

    // Dipanggil BannerNamaMap di scene tujuan
    public static void TampilkanBanner(Sprite sprite)
    {
        Instance.TerimaBanner(sprite);
    }

    // ---------- State ----------
    private Image hitam;
    private Image banner;
    private bool sedangPindah;
    private bool adaBannerPending;
    private Sprite bannerPending;
    private float timeScaleAwal = 1f;
    private bool membekukan;

    private void Awake()
    {
        if (_inst != null && _inst != this)
        {
            Destroy(gameObject);
            return;
        }
        _inst = this;
        DontDestroyOnLoad(gameObject);
        BuatUI();
    }

    private void OnDestroy()
    {
        PulihkanWaktu();
        if (_inst == this) _inst = null;
    }

    private void BuatUI()
    {
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 32767; // di atas semua UI

        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        gameObject.AddComponent<GraphicRaycaster>();

        // Layar hitam (full screen)
        var goHitam = new GameObject("Hitam", typeof(RectTransform), typeof(Image));
        goHitam.transform.SetParent(transform, false);
        hitam = goHitam.GetComponent<Image>();
        hitam.color = new Color(0, 0, 0, 0);
        hitam.raycastTarget = false;
        var rh = hitam.rectTransform;
        rh.anchorMin = Vector2.zero;
        rh.anchorMax = Vector2.one;
        rh.offsetMin = Vector2.zero;
        rh.offsetMax = Vector2.zero;

        // Banner (tengah layar, di atas hitam)
        var goBanner = new GameObject("Banner", typeof(RectTransform), typeof(Image));
        goBanner.transform.SetParent(transform, false);
        banner = goBanner.GetComponent<Image>();
        banner.color = new Color(1, 1, 1, 0);
        banner.raycastTarget = false;
        banner.preserveAspect = true;
        var rb = banner.rectTransform;
        rb.anchorMin = rb.anchorMax = rb.pivot = new Vector2(0.5f, 0.5f);
        rb.anchoredPosition = Vector2.zero;
        rb.sizeDelta = new Vector2(lebarBanner, lebarBanner);
    }

    // ---------- Alur ----------
    private void TerimaBanner(Sprite sprite)
    {
        if (sedangPindah)
        {
            // Datang lewat Pindah(): hitam sudah nutup, banner dijalankan oleh RutinPindah
            bannerPending = sprite;
            adaBannerPending = true;
        }
        else
        {
            // Scene dimuat tanpa Pindah() (mis. tekan Play langsung di scene map)
            StartCoroutine(RutinBanner(sprite));
        }
    }

    private IEnumerator RutinPindah(string namaScene)
    {
        if (sedangPindah) yield break;
        sedangPindah = true;
        adaBannerPending = false;
        bannerPending = null;

        hitam.raycastTarget = true; // blok klik selama transisi
        yield return Fade(hitam, hitam.color.a, 1f, durasiFadeKeluar);

        AsyncOperation op = SceneManager.LoadSceneAsync(namaScene);
        while (!op.isDone) yield return null;

        // Kasih 2 frame supaya Start() BannerNamaMap di scene baru sempat jalan
        yield return null;
        yield return null;

        if (adaBannerPending)
            yield return RutinBanner(bannerPending);
        else
            yield return Fade(hitam, 1f, 0f, durasiFadeMasuk);

        hitam.raycastTarget = false;
        sedangPindah = false;
    }

    private IEnumerator RutinBanner(Sprite sprite)
    {
        SetAlpha(hitam, 1f);
        SetAlpha(banner, 0f);
        banner.sprite = sprite;

        // Ukuran banner ngikutin aspek sprite, lebarnya diatur lewat lebarBanner
        float aspek = sprite.rect.width / sprite.rect.height;
        banner.rectTransform.sizeDelta = new Vector2(lebarBanner, lebarBanner / aspek);
        Bekukan();

        yield return null; // 1 frame hitam penuh

        StartCoroutine(Skala(banner.rectTransform, skalaAwalBanner, 1f, durasiFadeBanner * 1.5f));
        yield return Fade(banner, 0f, 1f, durasiFadeBanner);
        yield return new WaitForSecondsRealtime(durasiBannerTampil);

        // Hitam hilang, banner masih nempel
        yield return Fade(hitam, 1f, 0f, durasiFadeMasuk * 1.2f);
        hitam.raycastTarget = false;
        PulihkanWaktu();

        yield return new WaitForSecondsRealtime(durasiBannerSetelahHitam);
        yield return Fade(banner, 1f, 0f, durasiFadeBanner);
    }

    // ---------- Helper ----------
    private IEnumerator Fade(Image img, float dari, float ke, float durasi)
    {
        float t = 0f;
        float dur = Mathf.Max(0.01f, durasi);
        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.SmoothStep(0f, 1f, t / dur);
            SetAlpha(img, Mathf.Lerp(dari, ke, k));
            yield return null;
        }
        SetAlpha(img, ke);
    }

    private IEnumerator Skala(RectTransform rt, float dari, float ke, float durasi)
    {
        float t = 0f;
        float dur = Mathf.Max(0.01f, durasi);
        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            float k = 1f - Mathf.Pow(1f - Mathf.Clamp01(t / dur), 3f);
            rt.localScale = Vector3.one * Mathf.Lerp(dari, ke, k);
            yield return null;
        }
        rt.localScale = Vector3.one * ke;
    }

    private void Bekukan()
    {
        if (!bekukanWaktu || membekukan) return;
        timeScaleAwal = Time.timeScale;
        Time.timeScale = 0f;
        membekukan = true;
    }

    private void PulihkanWaktu()
    {
        if (!membekukan) return;
        membekukan = false;
        Time.timeScale = timeScaleAwal > 0f ? timeScaleAwal : 1f;
    }

    private static void SetAlpha(Image img, float a)
    {
        if (img == null) return;
        Color c = img.color;
        c.a = a;
        img.color = c;
    }
}