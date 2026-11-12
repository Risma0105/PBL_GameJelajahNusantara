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

    [Header("Pengaturan Timer (3 Hari)")]
    public int jumlahHariPerSekat = 3; // Batas waktu pengerjaan 1 scene adalah 3 hari dalam game

    private float waktuBerjalan = 0f;

    // Jumlah checkpoint yang sudah berhasil diselesaikan
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

        // Periksa scene pertama saat game dimulai
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

        // --- TAMBAHAN: OTOMATIS CATAT SCENE TERAKHIR UNTUK FITUR RESUME ---
        string namaSceneIni = scene.name;
        
        // Jangan catat kalau sedang di Main Menu atau Prolog
        if (namaSceneIni != "00_MainMenu" && namaSceneIni != "08_Prolog")
        {
            PlayerPrefs.SetString("SceneTerakhir", namaSceneIni);
            PlayerPrefs.Save();
            Debug.Log("📌 Scene terakhir berhasil dicatat oleh GameProgress: " + namaSceneIni);
        }
        // -----------------------------------------------------------------
    }


    // =====================================
    // PERIKSA SCENE
    // =====================================

    void PeriksaSceneAktif(Scene scene)
    {
        timerAktif = false;
        sceneAktif = -1;

        // Cari scene alat musik yang sedang dibuka
        for (int i = 0; i < sceneAlatMusik.Length; i++)
        {
            if (scene.name == sceneAlatMusik[i])
            {
                sceneAktif = i;
                break;
            }
        }

        // Update tampilan progress
        UpdateProgressBar();

        // Jika bukan scene alat musik, timer tidak berjalan
        if (sceneAktif == -1)
        {
            Debug.Log("Bukan scene alat musik. Timer berhenti.");
            return;
        }

        // Pastikan scene sesuai checkpoint berikutnya
        if (sceneAktif != checkpointSelesai)
        {
            Debug.Log("Scene belum sesuai urutan checkpoint.");
            return;
        }

        // Timer dimulai saat masuk scene alat musik
        waktuBerjalan = 0f;
        timerAktif = true;

        float totalDurasiTigaHari = HitungDurasiTigaHari();
        UpdateTeksSisaWaktu(totalDurasiTigaHari);

        Debug.Log("Timer dimulai (3 Hari): " + sceneAlatMusik[sceneAktif]);
    }


    // =====================================
    // MENCARI UI
    // =====================================

    void BindUI()
    {
        if (progressBar == null)
        {
            GameObject sliderObj = GameObject.Find(namaSlider);

            if (sliderObj != null)
            {
                progressBar = sliderObj.GetComponent<Slider>();
            }
        }

        if (progressText == null)
        {
            GameObject textObj = GameObject.Find(namaTeks);

            if (textObj != null)
            {
                progressText = textObj.GetComponent<TextMeshProUGUI>();
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
    float HitungDurasiTigaHari()
    {
        if (GameManager.Instance != null)
        {
            // 1 hari = durasiSiang + durasiMalam, dikali jumlah hari per sekat (3 hari)
            return (GameManager.Instance.durasiSiang + GameManager.Instance.durasiMalam) * jumlahHariPerSekat;
        }
        return 600f; // Nilai cadangan jika GameManager belum siap (misal 10 menit)
    }


    // =====================================
    // UPDATE TIMER DAN PROGRESS
    // =====================================

    void Update()
    {
        if (!timerAktif)
            return;

        if (sceneAktif != checkpointSelesai)
            return;

        if (checkpointSelesai >= 4)
            return;

        waktuBerjalan += Time.deltaTime;

        float totalDurasiTigaHari = HitungDurasiTigaHari();
        float sisaWaktu = totalDurasiTigaHari - waktuBerjalan;

        // Jika waktu 3 hari habis
        if (sisaWaktu <= 0f)
        {
            PlayerGagalDanResetSekatIni();
            return;
        }

        // Hitung posisi progress
        float progressAwal = checkpointSelesai == 0 ? 0f : posisiCheckpoint[checkpointSelesai - 1];
        float progressAkhir = posisiCheckpoint[checkpointSelesai];
        float persentase = waktuBerjalan / totalDurasiTigaHari;

        float nilaiProgress = Mathf.Lerp(progressAwal, progressAkhir, persentase);

        if (progressBar != null)
        {
            progressBar.value = nilaiProgress;
        }

        // Efek warna (peringatan jika waktu sisa sedikit)
        AturWarnaProgress(sisaWaktu);

        // Update timer
        UpdateTeksSisaWaktu(sisaWaktu);
    }


    // =====================================
    // EFEK KEDIP MERAH
    // =====================================

    void AturWarnaProgress(float sisaWaktu)
    {
        if (fillBarImage == null)
            return;

        // Peringatan jika waktu tersisa di bawah 20% dari total durasi 3 hari
        float batasPeringatan = HitungDurasiTigaHari() * 0.2f;

        if (sisaWaktu <= batasPeringatan)
        {
            float kedip = Mathf.PingPong(Time.time * 6f, 1f);

            fillBarImage.color = Color.Lerp(
                warnaPeringatan,
                warnaBahaya,
                kedip
            );
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
    // PENGUMPULAN ALAT MUSIK
    // =====================================

    public void CollectInstrument(string namaAlat)
    {
        if (!timerAktif)
            return;

        if (checkpointSelesai >= 4)
            return;

        bool alatSesuai = false;

        switch (checkpointSelesai)
        {
            case 0:
                alatSesuai = namaAlat == "Angklung";
                break;

            case 1:
                alatSesuai = namaAlat == "GesoGeso";
                break;

            case 2:
                alatSesuai = namaAlat == "Sarone";
                break;

            case 3:
                alatSesuai = namaAlat == "AlatKeempat" || namaAlat == "Suling";
                break;
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

        timerAktif = false;
        checkpointSelesai++;
        waktuBerjalan = 0f;

        UpdateProgressBar();

        Debug.Log(namaAlat + " berhasil dikumpulkan!");

        if (checkpointSelesai >= 4)
        {
            checkpointSelesai = 4;

            if (progressBar != null)
                progressBar.value = 1f;

            if (progressText != null)
                progressText.text = "Selesai!";

            Debug.Log("SEMUA ALAT MUSIK SELESAI!");
            return;
        }

        Debug.Log("Menunggu perpindahan ke scene berikutnya.");
    }


    // =====================================
    // JIKA WAKTU HABIS
    // =====================================

    void PlayerGagalDanResetSekatIni()
    {
        timerAktif = false;
        waktuBerjalan = 0f;
        UpdateProgressBar();

        Debug.Log("⏰ Waktu 3 hari habis! Mengulang kembali ke museum awal.");
        SceneManager.LoadScene(namaSceneMuseumAwal);
    }


    // =====================================
    // TEKS TIMER
    // =====================================

    void UpdateTeksSisaWaktu(float sisaDetik)
    {
        if (progressText == null)
            return;

        int totalDetik = Mathf.FloorToInt(sisaDetik);
        int hari = totalDetik / 200; // Contoh estimasi jika 1 hari total 200 detik (siang+malam), atau tampilkan format jam/menit
        int menit = Mathf.FloorToInt(sisaDetik / 60f);
        int detik = Mathf.FloorToInt(sisaDetik % 60f);

        progressText.text = $"Sisa: {menit:00}:{detik:00}";
    }


    // =====================================
    // FUNGSI TAMBAHAN
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