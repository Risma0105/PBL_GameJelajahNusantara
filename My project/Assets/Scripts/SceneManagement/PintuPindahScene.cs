using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PintuPindahScene : MonoBehaviour
{
    [Header("Tujuan Ruangan")]
    public string namaSceneTujuan;

    [Header("UI / Petunjuk")]
    public GameObject teksPetunjuk;

    private bool playerDekat = false;

    void Update()
    {
        // Deteksi klik kiri mouse saat player berada di dekat pintu
        if (playerDekat && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
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

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerDekat = true;
            if (teksPetunjuk != null) teksPetunjuk.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerDekat = false;
            if (teksPetunjuk != null) teksPetunjuk.SetActive(false);
        }
    }
}