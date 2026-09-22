using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; // Pakai TMP untuk teks angka timer

public class RoomTimer : MonoBehaviour
{
    [Header("Pengaturan Waktu")]
    public float sisaWaktu = 30f;
    public string sceneLobby = "Scene_Museum";

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
            // Waktu habis -> Gagal
            sisaWaktu = 0;
            timerBerjalan = false;
            Debug.Log("Waktu habis! Kembali ke Museum.");
            SceneManager.LoadScene(sceneLobby);
        }
    }

    void UpdateTampilanWaktu(float waktu)
    {
        int detik = Mathf.CeilToInt(waktu);
        if (teksTimer != null)
        {
            teksTimer.text = $"Sisa Waktu: {detik}s";
        }
    }

    // Dipanggil saat crafting berhasil agar timer berhenti
    public void BerhentiTimer()
    {
        timerBerjalan = false;
    }
}