using UnityEngine;
using UnityEngine.SceneManagement;

public class PintuPindahScene : MonoBehaviour
{
    [Header("Tujuan Ruangan")]
    public string namaSceneTujuan;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Begitu objek ber-tag "Player" menyentuh trigger pintu, langsung pindah scene
        if (other.CompareTag("Player"))
        {
            if (!string.IsNullOrEmpty(namaSceneTujuan))
            {
                SceneManager.LoadScene(namaSceneTujuan);
            }
            else
            {
                Debug.LogWarning("Nama scene tujuan belum diisi di Inspector!");
            }
        }
    }
}