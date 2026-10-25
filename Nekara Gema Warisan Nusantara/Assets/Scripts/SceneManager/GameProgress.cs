using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using UnityEngine.SceneManagement; // Diperlukan untuk pindah scene

public class GameProgress : MonoBehaviour
{
    public static GameProgress Instance;

    [Header("UI References")]
    public Slider progressBar;
    public TextMeshProUGUI progressText;
    public Image fillBarImage;

    [Header("Pengaturan Waktu & Checkpoint")]
    public float totalDurasiGame = 180f; // Total durasi 3 menit (180 detik)
    private float waktuBerjalan = 0f;
    private bool timerAktif = false;

    [Header("Pengaturan Scene Gagal / Museum Awal")]
    public string namaSceneMuseumAwal = "Scene_MuseumAwal"; // Sesuaikan dengan nama scene museum awal kamu di Unity

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        if (progressBar != null)
        {
            progressBar.minValue = 0f;
            progressBar.maxValue = 4f; // 4 titik checkpoint
            progressBar.value = 0f;
        }
        MulaiTimerBar();
    }

    void Update()
    {
        if (timerAktif)
        {
            if (waktuBerjalan < totalDurasiGame)
            {
                waktuBerjalan += Time.deltaTime;
                
                // Bar bergerak otomatis dari nilai 0 sampai 4 secara proporsional dengan waktu
                float nilaiSekarang = Mathf.Lerp(0f, 4f, waktuBerjalan / totalDurasiGame);
                
                if (progressBar != null)
                {
                    progressBar.value = nilaiSekarang;
                }

                CekEfekKedipMerah(nilaiSekarang);
                UpdateTeksWaktu();
            }
            else
            {
                // Waktu Habis / Gagal Mencapai Checkpoint!
                waktuBerjalan = totalDurasiGame;
                timerAktif = false;
                
                Debug.Log("⏰ Waktu habis! Gagal mengumpulkan alat musik. Kembali ke museum dan reset progress.");
                PlayerGagalDanReset();
            }
        }
    }

    public void MulaiTimerBar()
    {
        timerAktif = true;
    }

    // Fungsi yang dipanggil saat waktu habis
    void PlayerGagalDanReset()
    {
        // 1. Reset status progres alat musik kembali ke awal
        hasAngklung = false;
        hasGesoGeso = false;
        hasSarone = false;
        hasAlatKeempat = false;

        // 2. Reset nilai slider bar menjadi 0
        if (progressBar != null)
        {
            progressBar.value = 0f;
        }

        // 3. Pindahkan player kembali ke scene museum awal
        SceneManager.LoadScene(namaSceneMuseumAwal);
    }

    public static bool hasAngklung = false;
    public static bool hasGesoGeso = false;
    public static bool hasSarone = false;
    public static bool hasAlatKeempat = false;

    public void MulaiProgressOtomatis(float durasi) { }

    public void CollectInstrument(string namaAlat)
    {
        if (namaAlat == "Angklung") hasAngklung = true;
        if (namaAlat == "GesoGeso") hasGesoGeso = true;
        if (namaAlat == "Sarone") hasSarone = true;
        if (namaAlat == "AlatKeempat" || namaAlat == "Suling") hasAlatKeempat = true;

        Debug.Log(namaAlat + " berhasil dikumpulkan!");
    }

    void CekEfekKedipMerah(float nilaiBar)
    {
        float sisaKeCheckpoint = 1f - (nilaiBar % 1f);
        
        if (sisaKeCheckpoint <= 0.25f && nilaiBar > 0.1f)
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
    }

    void UpdateTeksWaktu()
    {
        if (progressText != null)
        {
            float sisaDetik = totalDurasiGame - waktuBerjalan;
            int menit = Mathf.FloorToInt(sisaDetik / 60f);
            int detik = Mathf.FloorToInt(sisaDetik % 60f);
            progressText.text = $"Waktu: {menit:00}:{detik:00}";
        }
    }
}