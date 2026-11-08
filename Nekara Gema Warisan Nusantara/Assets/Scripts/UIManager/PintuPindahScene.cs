using UnityEngine;
using UnityEngine.SceneManagement;


public class PintuPindahScene : MonoBehaviour
{
    [Header("Tujuan Ruangan")]
    public string namaSceneTujuan;
    [Header("Pengaturan SFX Transisi")]
    public string namaSFXPintu;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Begitu objek ber-tag "Player" menyentuh trigger pintu, langsung pindah scene
        if (other.CompareTag("Player"))
        {
            if (!string.IsNullOrEmpty(namaSceneTujuan))
            {
                // 1. Putar SFX pintu terlebih dahulu
                if (AudioManager.Instance != null && !string.IsNullOrEmpty(namaSFXPintu))
                {
                    AudioManager.Instance.PlaySFX(namaSFXPintu);
                }
                SceneManager.LoadScene(namaSceneTujuan);
            }
            else
            {
                Debug.LogWarning("Nama scene tujuan belum diisi di Inspector!");
            }
        }
    }
}