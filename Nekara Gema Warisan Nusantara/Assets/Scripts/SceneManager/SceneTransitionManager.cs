using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance; // Singleton global
    public static event Action OnLayarTerbuka;     // didengarkan oleh PortalKeluar
    public static string SpawnIdTerakhir = "";     // ID PortalKeluar yang harus dipakai di scene tujuan

    [Header("Fade")]
    public Image imageFade;            // PanelFade (hitam, stretch penuh)
    public float kecepatanFade = 1.5f; // makin besar makin cepat

    [Header("Banner Nama Map")]
    public Image imageBanner;          // Image di tengah layar, urutan Hierarchy di BAWAH PanelFade
    [Tooltip("Kelipatan bulat dari ukuran pixel sprite asli. Coba 3 atau 4.")]
    public int skalaBanner = 4;
    public float durasiBanner = 2f;

    private bool sedangTransisi;
    public bool SedangTransisi => sedangTransisi;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // pastikan skrip ini ada di ROOT Canvas
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        SetAlpha(imageFade, 0f);
        SetAlpha(imageBanner, 0f);
        imageFade.gameObject.SetActive(false);
    }

    // Tanpa banner
    public void PindahScene(string namaScene)
    {
        PindahScene(namaScene, null, "");
    }

    public void PindahScene(string namaScene, Sprite banner)
    {
        PindahScene(namaScene, banner, "");
    }

    // Dengan banner dan ID titik muncul di scene tujuan
    public void PindahScene(string namaScene, Sprite banner, string spawnId)
    {
        if (sedangTransisi) return;
        SpawnIdTerakhir = spawnId ?? "";
        StartCoroutine(ProsesTransisi(namaScene, banner));
    }

    IEnumerator ProsesTransisi(string namaScene, Sprite banner)
    {
        sedangTransisi = true;
        imageFade.gameObject.SetActive(true);

        // 1. Fade to black
        yield return FadeImage(imageFade, 0f, 1f);

        // 2. Load scene di belakang layar hitam
        AsyncOperation op = SceneManager.LoadSceneAsync(namaScene);
        while (!op.isDone) yield return null;

        // 3. Banner nama map
        if (banner != null && imageBanner != null)
        {
            imageBanner.sprite = banner;
            imageBanner.preserveAspect = true;

            // Ukuran = pixel asli sprite x kelipatan bulat (tajam, tidak kegedean)
            int k = Mathf.Max(1, skalaBanner);
            imageBanner.rectTransform.sizeDelta = new Vector2(banner.rect.width * k, banner.rect.height * k);
            imageBanner.rectTransform.localScale = Vector3.one;

            yield return FadeImage(imageBanner, 0f, 1f);
            yield return new WaitForSecondsRealtime(durasiBanner);
            yield return FadeImage(imageBanner, 1f, 0f);
        }

        // 4. Kabari PortalKeluar, lalu buka layar
        OnLayarTerbuka?.Invoke();
        yield return FadeImage(imageFade, 1f, 0f);

        imageFade.gameObject.SetActive(false);
        sedangTransisi = false;
    }

    IEnumerator FadeImage(Image img, float dari, float ke)
    {
        float timer = 0f;
        while (timer < 1f)
        {
            timer += Time.unscaledDeltaTime * kecepatanFade;
            SetAlpha(img, Mathf.Lerp(dari, ke, Mathf.Clamp01(timer)));
            yield return null;
        }
        SetAlpha(img, ke);
    }

    void SetAlpha(Image img, float a)
    {
        if (img == null) return;
        Color c = img.color;
        c.a = a;
        img.color = c;
    }
}