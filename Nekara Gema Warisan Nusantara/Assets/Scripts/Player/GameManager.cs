using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal; // Wajib untuk Light2D
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Data Progres Alat Musik")]
    public HashSet<string> alatMusikSelesai = new HashSet<string>();
    
    [Header("Debug Inspector (Jangan Diubah Manual)")]
    [Tooltip("Daftar alat musik yang sudah selesai (hanya untuk dilihat di Inspector)")]
    public List<string> daftarAlatMusikDebug = new List<string>();

    [Header("Pengaturan Siklus Waktu (Detik)")]
    // 1 hari game = 3,34 menit = 200,4 detik (1,67 menit siang, 1,67 menit malam)
    public float durasiSatuHariNyata = 200.4f; 
    public int hariSekarang = 1;
    public int totalMaksimalHari = 10;

    [Header("Pengaturan Warna & Cahaya Global")]
    public Color warnaSiang = Color.white;
    public Color warnaMalam = new Color(0.12f, 0.15f, 0.32f, 1f);
    public float intensitasSiang = 1.0f;
    public float intensitasMalam = 0.2f;

    [Header("UI Feedback (Opsional)")]
    public TMP_Text teksHariDanWaktu;

    private float timerHari = 0f;
    private Light2D currentGlobalLight;
    private bool statusMalamAktif = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Tiap kali player pindah scene, otomatis deteksi Light2D dan sinkronkan lampu di scene baru
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        currentGlobalLight = Object.FindFirstObjectByType<Light2D>();
        
        // Sinkronkan status lampu (nyala/mati) segera begitu masuk scene baru
        AturSemuaLampuMap(statusMalamAktif);
    }

    void Update()
    {
        // Jalankan timer waktu global
        timerHari += Time.deltaTime;

        // Cek pergantian hari (setiap 3,34 menit)
        if (timerHari >= durasiSatuHariNyata)
        {
            timerHari -= durasiSatuHariNyata;
            hariSekarang++;
            Debug.Log($"☀️ Masuk ke Hari ke-{hariSekarang}!");

            if (hariSekarang > totalMaksimalHari)
            {
                Debug.Log("🏁 Batas 10 Hari Selesai!");
            }
        }

        // Hitung siklus terang-gelap (1,67 menit siang, 1,67 menit malam)
        float progress = (Mathf.Sin((timerHari / durasiSatuHariNyata) * Mathf.PI * 2 + (Mathf.PI / 2)) + 1f) / 2f;
        
        // Tentukan apakah sekarang sudah masuk waktu malam (progress di bawah 0.5)
        bool apakahMalamIni = progress <= 0.5f;
        if (apakahMalamIni != statusMalamAktif)
        {
            statusMalamAktif = apakahMalamIni;
            // Nyalakan lampu saat malam tiba, matikan saat siang
            AturSemuaLampuMap(statusMalamAktif);
            Debug.Log(statusMalamAktif ? "🌙 Malam tiba: Lampu map dinyalakan." : "☀️ Siang tiba: Lampu map dimatikan.");
        }

        // Perbarui warna dan intensitas cahaya jika objek lampu ditemukan di scene aktif
        if (currentGlobalLight != null)
        {
            currentGlobalLight.color = Color.Lerp(warnaMalam, warnaSiang, progress);
            currentGlobalLight.intensity = Mathf.Lerp(intensitasMalam, intensitasSiang, progress);
        }

        // Tampilkan teks hari dan status siang/malam jika UI dihubungkan
        if (teksHariDanWaktu != null)
        {
            string statusWaktu = progress > 0.5f ? "Siang" : "Malam";
            teksHariDanWaktu.text = $"Hari {hariSekarang} ({statusWaktu})";
        }
    }

    // Fungsi otomatis untuk menyalakan/mematikan semua objek ber-Tag "LampuMap"
    void AturSemuaLampuMap(bool nyala)
    {
        GameObject[] daftarLampu = GameObject.FindGameObjectsWithTag("LampuMap");
        foreach (GameObject lampu in daftarLampu)
        {
            if (lampu != null)
            {
                lampu.SetActive(nyala);
            }
        }
    }

    public bool CekSudahSelesai(string namaAlat)
    {
        return alatMusikSelesai.Contains(namaAlat);
    }

    public void TandaiSelesai(string namaAlat)
    {
        if (!alatMusikSelesai.Contains(namaAlat))
        {
            alatMusikSelesai.Add(namaAlat);
            
            if (!daftarAlatMusikDebug.Contains(namaAlat))
            {
                daftarAlatMusikDebug.Add(namaAlat);
            }
            
            Debug.Log($"✅ [GameManager] Alat musik '{namaAlat}' berhasil ditandai selesai!");
        }
    }
}