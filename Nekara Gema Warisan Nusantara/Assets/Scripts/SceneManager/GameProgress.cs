using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using UnityEngine.SceneManagement;

public class GameProgress : MonoBehaviour
{
    public static GameProgress Instance;

    [Header("UI References")]
    public Slider progressBar;
    public TextMeshProUGUI progressText;
    public Image fillBarImage;

    [Header("Pengaturan Waktu & Checkpoint")]
    public float durasiPerSekat = 180f; // Bisa diubah jadi 20 untuk tes cepat
    private float waktuBerjalanDiSekatIni = 0f;
    private int sekatAktif = 0; // 0: Angklung, 1: Alat 2, 2: Sarone, 3: Alat 4
    private bool timerAktif = false;

    [Header("Pengaturan Scene Gagal / Museum Awal")]
    public string namaSceneMuseumAwal = "Scene_MuseumDalam";

    void Awake()
    {
        if (Instance == null) 
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (progressBar != null)
        {
            progressBar.minValue = 0f;
            progressBar.maxValue = 4f; 
            progressBar.value = (float)sekatAktif;
        }
        MulaiTimerBar();
    }

    void Update()
    {
        if (timerAktif)
        {
            if (waktuBerjalanDiSekatIni < durasiPerSekat)
            {
                waktuBerjalanDiSekatIni += Time.deltaTime;
                
                // BAR BERGERAK PENUH DI SETIAP SEKAT:
                // Menghitung persentase kemajuan (0 sampai 1) di sekat yang sedang aktif saja
                float progressDalamSekat = waktuBerjalanDiSekatIni / durasiPerSekat;
                
                // Posisi slider di Unity (dari sekatAktif sampai sekatAktif + 1)
                float nilaiSliderTotal = (float)sekatAktif + progressDalamSekat;
                
                if (progressBar != null)
                {
                    progressBar.value = nilaiSliderTotal;
                }

                // Cek efek kedip merah saat sisa 25% waktu di sekat ini
                float sisaWaktuSekat = durasiPerSekat - waktuBerjalanDiSekatIni;
                if (sisaWaktuSekat <= (durasiPerSekat * 0.25f))
                {
                    if (fillBarImage != null)
                    {
                        float kedip = Mathf.PingPong(Time.time * 6f, 1f);
                        fillBarImage.color = Color.Lerp(Color.yellow, Color.red, kedip);
                    }
                }
                else
                {
                    if (fillBarImage != null)
                    {
                        fillBarImage.color = Color.green;
                    }
                }

                UpdateTeksSisaWaktu(sisaWaktuSekat);
            }
            else
            {
                // Waktu di sekat ini habis! Gagal dan reset sekat ini.
                timerAktif = false;
                Debug.Log($"⏰ Waktu sekat ke-{sekatAktif + 1} habis! Kembali ke museum.");
                PlayerGagalDanResetSekatIni();
            }
        }
    }

    public void MulaiTimerBar()
    {
        timerAktif = true;
    }

    void PlayerGagalDanResetSekatIni()
    {
        waktuBerjalanDiSekatIni = 0f;

        if (progressBar != null)
        {
            progressBar.value = (float)sekatAktif;
        }

        SceneManager.LoadScene(namaSceneMuseumAwal);
    }

    public void LanjutKeSekatBerikutnya()
    {
        sekatAktif++;
        waktuBerjalanDiSekatIni = 0f; // Reset waktu untuk sekat baru

        if (sekatAktif >= 4)
        {
            sekatAktif = 4;
            timerAktif = false;
            Debug.Log("🎉 Selamat! Semua alat musik berhasil diselesaikan!");
        }
    }

    public void MulaiProgressOtomatis(float durasi)
    {
        durasiPerSekat = durasi;
        MulaiTimerBar();
    }

    public static bool hasAngklung = false;
    public static bool hasGesoGeso = false;
    public static bool hasSarone = false;
    public static bool hasAlatKeempat = false;

    public void CollectInstrument(string namaAlat)
    {
        if (namaAlat == "Angklung") { hasAngklung = true; }
        if (namaAlat == "GesoGeso") { hasGesoGeso = true; }
        if (namaAlat == "Sarone") { hasSarone = true; }
        if (namaAlat == "AlatKeempat" || namaAlat == "Suling") { hasAlatKeempat = true; }

        LanjutKeSekatBerikutnya();
        Debug.Log(namaAlat + " berhasil dikumpulkan! Meluncur ke sekat berikutnya.");
    }

    void UpdateTeksSisaWaktu(float sisaDetik)
    {
        if (progressText != null)
        {
            int menit = Mathf.FloorToInt(sisaDetik / 60f);
            int detik = Mathf.FloorToInt(sisaDetik % 60f);
            progressText.text = $"Waktu: {menit:00}:{detik:00}";
        }
    }
}