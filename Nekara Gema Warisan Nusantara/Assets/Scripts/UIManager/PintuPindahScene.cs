using UnityEngine;

public class PintuPindahScene : MonoBehaviour
{
    [Header("Tujuan Ruangan")]
    public string namaSceneTujuan;

    [Header("Pengaturan SFX Transisi")]
    public string namaSFXPintu;

    private bool sudahTerpicu;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Cegah pintu kepicu berkali-kali selama fade berjalan
        if (sudahTerpicu) return;
        if (!other.CompareTag("Player")) return;

        if (string.IsNullOrEmpty(namaSceneTujuan))
        {
            Debug.LogWarning("Nama scene tujuan belum diisi di Inspector!");
            return;
        }

        sudahTerpicu = true;

        // 1. Putar SFX pintu
        if (AudioManager.Instance != null && !string.IsNullOrEmpty(namaSFXPintu))
        {
            AudioManager.Instance.PlaySFX(namaSFXPintu);
        }

        // 2. Fade ke hitam lalu pindah scene (fallback ke LoadScene biasa kalau canvas transisi ga ada)
        BannerNamaMap.Pindah(namaSceneTujuan);
    }
}