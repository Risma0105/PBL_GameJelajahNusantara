using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; // Pakai TMP untuk teks angka timer

public class RoomTimer : MonoBehaviour
{
    [Header("Pengaturan Waktu")]
    public float sisaWaktu = 180f; // Disamakan jadi 180 detik (sesuai GameProgress)
    public string sceneLobby = "Scene_MuseumDalam"; // Diperbarui ke scene museum yang benar

    [Header("UI")]
    public TMP_Text teksTimer;

    private bool timerBerjalan = true;

    void Update()
    {
        if (!timerBerjalan) return;

        if (sisaWaktu > 0)
        {
            sisaWaktu -= Time.deltaTime;
            UpdateTampilanWaktu(sisaWaktu);
        }
        else
        {
            // Waktu habis -> Gagal kembali ke museum
            sisaWaktu = 0;
            timerBerjalan = false;
            Debug.Log("Waktu habis! Kembali ke Museum Dalam.");
            SceneManager.LoadScene(sceneLobby);
        }
    }

    void UpdateTampilanWaktu(float waktu)
    {
        // Hitung menit dan detik dari total sisa waktu
        int menit = Mathf.FloorToInt(waktu / 60f);
        int detik = Mathf.FloorToInt(waktu % 60f);

        if (teksTimer != null)
        {
            // Format jadi MM:SS (contoh: 03:05) dengan tambahan angka 0 di depannya
            teksTimer.text = $"Sisa Waktu: {menit:00}:{detik:00}";
        }
    }

    // Dipanggil saat crafting berhasil agar timer berhenti
    public void BerhentiTimer()
    {
        timerBerjalan = false;
    }
}