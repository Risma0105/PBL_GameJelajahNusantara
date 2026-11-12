using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameProgress : MonoBehaviour
{
    public static GameProgress Instance;

    [Header("UI References")]
    public Slider progressBar;
    public TextMeshProUGUI progressText;
    public Image fillBarImage;

    [Header("Nama UI")]
    public string namaSlider = "ProgressBar";
    public string namaTeks = "ProgressText";

    [Header("Nama Scene Alat Musik")]
    public string[] sceneAlatMusik =
    {
        "Scene_Angklung",
        "Scene_GesoGeso",
        "Scene_Sarone",
        "Scene_AlatKeempat"
    };

    [Header("Scene Museum")]
    public string namaSceneMuseumAwal = "Scene_MuseumDalam";

    [Header("Pengaturan Timer (3 Hari per Sekat)")]
    public int jumlahHariPerSekat = 3; 

    private float waktuBerjalan = 0f;

    // Jumlah checkpoint / sekat yang sudah BERHASIL DITARUH DI DISPLAY MUSEUM
    private int checkpointSelesai = 0;

    // Scene alat musik yang sedang dikerjakan
    private int sceneAktif = -1;

    private bool timerAktif = false;

    [Header("Posisi Checkpoint Progress Bar")]
    [Range(0f, 1f)]
    public float[] posisiCheckpoint =
    {
        0.27f,
        0.51f,
        0.77f,
        1f
    };

    [Header("Warna Progress Bar")]
    public Color warnaNormal = Color.green;
    public Color warnaPeringatan = Color.yellow;
    public Color warnaBahaya = Color.red;

    // Status alat musik
    public static bool hasAngklung = false;
    public static bool hasGesoGeso = false;
    public static bool hasSarone = false;
    public static bool hasAlatKeempat = false;


    // =====================================
    // AWAKE
    // =====================================

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }


    // =====================================
    // START
    // =====================================

    void Start()
    {
        BindUI();
        PeriksaSceneAktif(SceneManager.GetActiveScene());
    }


    // =====================================
    // DESTROY
    // =====================================

    void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Instance = null;
        }
    }


    // =====================================
    // SAAT PINDAH SCENE
    // =====================================

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        progressBar = null;
        progressText = null;
        fillBarImage = null;

        BindUI();
        PeriksaSceneAktif(scene);

        string namaSceneIni = scene.name;
        
        // Catat scene terakhir untuk fitur Resume (kecuali Main Menu & Prolog)
        if (namaSceneIni != "00_MainMenu" && namaSceneIni != "08_Prolog")
        {
            PlayerPrefs.SetString("SceneTerakhir", namaSceneIni);
            PlayerPrefs.Save();
        }
    }


    // =====================================
    // PERIKSA SCENE
    // =====================================

    void PeriksaSceneAktif(Scene scene)
    {
        timerAktif = false;
        sceneAktif = -1;

        for (int i = 0; i < sceneAlatMusik.Length; i++)
        {
            if (scene.name == sceneAlatMusik[i])
            {
                sceneAktif = i;
                break;
            }
        }

        UpdateProgressBar();

        // Jika bukan scene alat musik
        if (sceneAktif == -1)
        {
            // Jika berada di scene museum awal, timer 3 hari untuk sekat aktif kembali berjalan mengejar batas waktu
            if (scene.name == namaSceneMuseumAwal && checkpointSelesai < 4)
            {
                timerAktif = true;
                Debug.Log($"🏛️ Kembali ke Museum. Timer 3 hari untuk sekat ke-{checkpointSelesai + 1} dilanjutkan.");
                return;
            }

            Debug.Log("Bukan scene alat musik/museum. Timer berhenti.");
            return;
        }

        if (sceneAktif != checkpointSelesai)
        {
            Debug.Log("Scene belum sesuai urutan checkpoint.");
            return;
        }

        // Masuk ke scene alat musik, timer 3 hari aktif berjalan
        timerAktif = true;
        UpdateTeksSisaWaktu(HitungSisaWaktuTotal());

        Debug.Log("Timer 3 hari aktif di scene: " + sceneAlatMusik[sceneAktif]);
    }


    // =====================================
    // MENCARI UI (MENEMBUS PARENT SEPERTI UI_Game 1)
    // =====================================

    void BindUI()
    {
        if (progressBar == null)
        {
            Slider[] semuaSlider = FindObjectsByType<Slider>(FindObjectsSortMode.None);
            foreach (var s in semuaSlider)
            {
                if (s.gameObject.name == namaSlider)
                {
                    progressBar = s;
                    break;
                }
            }
        }

        if (progressText == null)
        {
            TextMeshProUGUI[] semuaTeks = FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None);
            foreach (var t in semuaTeks)
            {
                if (t.gameObject.name == namaTeks)
                {
                    progressText = t;
                    break;
                }
            }
        }

        if (progressBar != null)
        {
            progressBar.minValue = 0f;
            progressBar.maxValue = 1f;
            progressBar.wholeNumbers = false;

            if (progressBar.fillRect != null)
            {
                fillBarImage = progressBar.fillRect.GetComponent<Image>();
            }
        }
    }


    // =====================================
    // HITUNG DURASI 3 HARI DARI GAMEMANAGER
    // =====================================
    float HitungDurasiTigaHariTotal()
    {
        if (GameManager.Instance != null)
        {
            // 1 hari = durasiSiang + durasiMalam, dikalikan 3 hari dari GameManager
            return (GameManager.Instance.durasiSiang + GameManager.Instance.durasiMalam) * jumlahHariPerSekat;
        }
        return 600f; // Nilai cadangan jika GameManager belum siap
    }

    float HitungSisaWaktuTotal()
    {
        return HitungDurasiTigaHariTotal() - waktuBerjalan;
    }


    // =====================================
    // UPDATE TIMER DAN PROGRESS (BAR MAJU TERUS SAMPAI TARUH DISPLAY)
    // =====================================

    void Update()
    {
        if (!timerAktif)
            return;

        if (checkpointSelesai >= 4)
            return;

        waktuBerjalan += Time.deltaTime;

        float totalDurasi = HitungDurasiTigaHariTotal();
        float sisaWaktu = totalDurasi - waktuBerjalan;

        // Jika waktu 3 hari habis sebelum sempat menaruh ke display museum
        if (sisaWaktu <= 0f)
        {
            PlayerGagalDanResetSekatIni();
            return;
        }

        // Progress bar merayap maju dari checkpoint sebelumnya menuju checkpoint saat ini seiring waktu
        float progressAwal = checkpointSelesai == 0 ? 0f : posisiCheckpoint[checkpointSelesai - 1];
        float progressAkhir = posisiCheckpoint[checkpointSelesai];
        float persentase = Mathf.Clamp01(waktuBerjalan / totalDurasi);

        float nilaiProgress = Mathf.Lerp(progressAwal, progressAkhir, persentase);

        if (progressBar != null)
        {
            progressBar.value = nilaiProgress;
        }

        AturWarnaProgress(sisaWaktu);
        UpdateTeksSisaWaktu(sisaWaktu);
    }


    // =====================================
    // EFEK KEDIP MERAH
    // =====================================

    void AturWarnaProgress(float sisaWaktu)
    {
        if (fillBarImage == null)
            return;

        float batasPeringatan = HitungDurasiTigaHariTotal() * 0.2f;

        if (sisaWaktu <= batasPeringatan)
        {
            float kedip = Mathf.PingPong(Time.time * 6f, 1f);
            fillBarImage.color = Color.Lerp(warnaPeringatan, warnaBahaya, kedip);
        }
        else
        {
            fillBarImage.color = warnaNormal;
        }
    }


    // =====================================
    // UPDATE POSISI PROGRESS
    // =====================================

    void UpdateProgressBar()
    {
        if (progressBar == null)
            return;

        float nilaiProgress = checkpointSelesai == 0 ? 0f : posisiCheckpoint[checkpointSelesai - 1];
        progressBar.value = nilaiProgress;

        if (fillBarImage != null)
        {
            fillBarImage.color = warnaNormal;
        }
    }


    // =====================================
    // PENGUMPULAN ALAT MUSIK (SAAT DIAMBIL DARI NPC)
    // =====================================

    public void CollectInstrument(string namaAlat)
    {
        if (checkpointSelesai >= 4)
            return;

        bool alatSesuai = false;

        switch (checkpointSelesai)
        {
            case 0: alatSesuai = namaAlat == "Angklung"; break;
            case 1: alatSesuai = namaAlat == "GesoGeso"; break;
            case 2: alatSesuai = namaAlat == "Sarone"; break;
            case 3: alatSesuai = namaAlat == "AlatKeempat" || namaAlat == "Suling"; break;
        }

        if (!alatSesuai)
        {
            Debug.LogWarning("Alat musik tidak sesuai checkpoint!");
            return;
        }

        if (checkpointSelesai == 0) hasAngklung = true;
        if (checkpointSelesai == 1) hasGesoGeso = true;
        if (checkpointSelesai == 2) hasSarone = true;
        if (checkpointSelesai == 3) hasAlatKeempat = true;

        Debug.Log(namaAlat + " berhasil diambil dari NPC! Bawa kembali dan taruh di display museum.");
    }


    // =====================================
    // KETIKA BERHASIL TARUH DISPLAY DI MUSEUM (BAR LANGSUNG FULL PENUH)
    // =====================================

    public void BerhasilTaruhDisplay()
    {
        if (checkpointSelesai >= 4)
            return;

        // Kunci progress bar tepat penuh di garis checkpoint sekat ini
        if (progressBar != null)
        {
            progressBar.value = posisiCheckpoint[checkpointSelesai];
        }

        // Naikkan checkpoint ke sekat/alat musik berikutnya
        checkpointSelesai++;
        waktuBerjalan = 0f; // Reset timer untuk jatah 3 hari sekat berikutnya

        Debug.Log("🎉 Alat musik berhasil ditaruh di display museum! Progress sekat ini terkunci penuh.");

        if (checkpointSelesai >= 4)
        {
            checkpointSelesai = 4;

            if (progressBar != null) progressBar.value = 1f;
            if (progressText != null) progressText.text = "Selesai!";

            timerAktif = false;
            Debug.Log("🏆 SEMUA ALAT MUSIK SELESAI DITARRUH DI DISPLAY!");
        }
    }


    // =====================================
    // JIKA WAKTU 3 HARI HABIS (GAGAL)
    // =====================================

    void PlayerGagalDanResetSekatIni()
    {
        timerAktif = false;
        waktuBerjalan = 0f;
        UpdateProgressBar();

        Debug.Log("⏰ Waktu 3 hari habis sebelum ditaruh display! Mengulang kembali ke museum awal.");
        SceneManager.LoadScene(namaSceneMuseumAwal);
    }


    // =====================================
    // TEKS TIMER
    // =====================================

    void UpdateTeksSisaWaktu(float sisaDetik)
    {
        if (progressText == null)
            return;

        int menit = Mathf.FloorToInt(sisaDetik / 60f);
        int detik = Mathf.FloorToInt(sisaDetik % 60f);

        progressText.text = $"Sisa Waktu: {menit:00}:{detik:00}";
    }


    // =====================================
    // FUNGSI TAMBAHAN (YANG DIPANGGIL OLEH NPCCrafting)
    // =====================================

    public void MulaiTimerBar()
    {
        if (sceneAktif == checkpointSelesai && checkpointSelesai < 4)
        {
            timerAktif = true;
        }
    }

    public void MulaiProgressOtomatis(float durasi)
    {
        MulaiTimerBar();
    }
}