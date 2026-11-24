using UnityEngine;
using UnityEngine.UI;

// Taruh di GameObject mana saja di scene map yang mau punya banner nama.
// Scene tanpa banner (mis. museum): ga perlu dipasang, atau kosongkan "Banner Map".
// Fade hitam & tampilan banner sekarang diurus MapFadeManager (persisten).
public class BannerNamaMap : MonoBehaviour
{
    [Header("Isi per scene")]
    public Sprite bannerMap;

    [Header("Sudah tidak dipakai (boleh dikosongkan)")]
    public Image imageBanner;
    public Image layarHitam;

    private void Awake()
    {
        // Matikan UI lama biar ga numpuk dengan transisi baru
        MatikanJikaBukanDiriSendiri(imageBanner);
        MatikanJikaBukanDiriSendiri(layarHitam);
    }

    private void Start()
    {
        if (bannerMap != null) MapFadeManager.TampilkanBanner(bannerMap);
    }

    // Tetap ada supaya PintuPindahScene lama (BannerNamaMap.Pindah) ga perlu diubah
    public static void Pindah(string namaScene)
    {
        MapFadeManager.Pindah(namaScene);
    }

    private void MatikanJikaBukanDiriSendiri(Image img)
    {
        if (img != null && img.gameObject != gameObject)
            img.gameObject.SetActive(false);
    }
}