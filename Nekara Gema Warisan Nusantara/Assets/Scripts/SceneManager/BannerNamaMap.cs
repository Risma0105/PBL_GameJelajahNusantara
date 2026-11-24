using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Taruh di Canvas (mis. TransisiCanvas) di SETIAP scene map.
// Saat scene mulai: layar hitam + banner nama map muncul sebentar, lalu hilang.
// Kosongkan "Banner Map" di scene yang tidak butuh banner (mis. museum).
public class BannerNamaMap : MonoBehaviour
{
    [Header("Isi per scene")]
    public Sprite bannerMap;          // sprite nama map untuk scene ini

    [Header("Referensi UI")]
    public Image imageBanner;         // Image banner di tengah layar
    public Image layarHitam;          // opsional: latar hitam di belakang banner

    [Header("Pengaturan")]
    public float durasiTampil = 1.5f; // lama banner terlihat penuh
    public float durasiFade = 0.4f;   // lama fade masuk / keluar
    public float durasiBannerSetelahHitam = 1f; // banner masih tampil sebentar setelah layar hitam hilang
    public bool bekukanWaktu = true;  // player & jam game berhenti selama banner tampil

    private float timeScaleAwal = 1f;
    private bool membekukan;

    private void Start()
    {
        if (imageBanner != null) imageBanner.raycastTarget = false;
        if (layarHitam != null) layarHitam.raycastTarget = false;

        if (bannerMap == null || imageBanner == null)
        {
            SetAlpha(imageBanner, 0f);
            SetAlpha(layarHitam, 0f);
            return;
        }

        imageBanner.sprite = bannerMap;
        imageBanner.preserveAspect = true;
        StartCoroutine(Putar());
    }

    private IEnumerator Putar()
    {
        SetAlpha(layarHitam, 1f);
        SetAlpha(imageBanner, 0f);

        if (bekukanWaktu)
        {
            timeScaleAwal = Time.timeScale;
            Time.timeScale = 0f;
            membekukan = true;
        }

        // Banner fade in di atas layar hitam
        yield return Fade(imageBanner, 0f, 1f, durasiFade);
        yield return new WaitForSecondsRealtime(durasiTampil);

        // 1. Layar hitam hilang dulu, banner tetap tampil
        yield return Fade(layarHitam, 1f, 0f, durasiFade);
        PulihkanWaktu(); // game jalan lagi, banner masih menempel di layar

        // 2. Banner bertahan sebentar di atas scene
        yield return new WaitForSecondsRealtime(durasiBannerSetelahHitam);

        // 3. Banner fade out
        yield return Fade(imageBanner, 1f, 0f, durasiFade);
    }

    private IEnumerator Fade(Image img, float dari, float ke, float durasi)
    {
        float t = 0f;
        float dur = Mathf.Max(0.01f, durasi);
        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            SetAlpha(img, Mathf.Lerp(dari, ke, t / dur));
            yield return null;
        }
        SetAlpha(img, ke);
    }

    private void PulihkanWaktu()
    {
        if (!membekukan) return;
        membekukan = false;
        Time.timeScale = timeScaleAwal > 0f ? timeScaleAwal : 1f;
    }

    private void OnDestroy()
    {
        PulihkanWaktu(); // jaga-jaga kalau scene berpindah saat banner masih tampil
    }

    private void SetAlpha(Image img, float a)
    {
        if (img == null) return;
        Color c = img.color;
        c.a = a;
        img.color = c;
    }
}