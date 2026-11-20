using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

// =====================================================================
// GameProgress
//
// Aturan:
//  - Tiap sekat (alat musik) punya jatah 'jumlahHariPerSekat' hari game.
//  - Sumber waktu TUNGGAL = jam & hari di GameManager. Bar, teks sisa waktu,
//    peringatan, dan batas gagal semuanya dihitung dari sana.
//  - Bar jalan dari checkpoint sebelumnya menuju checkpoint sekat ini
//    dan TEPAT sampai di sana saat jatah hari habis.
//  - Timer mulai saat pemain masuk scene alat musik sesuai urutan, lalu
//    terus jalan di scene mana pun (museum luar, museum dalam, dll)
//    kecuali Main Menu & Prolog. Berhenti saat game di-pause.
//  - Kalau alat belum ditaruh di display saat jatah habis -> gagal,
//    balik ke museum awal dan jatah diulang.
//  - Pintu alat musik terkunci sampai urutannya tiba (lihat BolehMasuk).
// =====================================================================
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
        "01_Angklung",
        "03_GesoGeso",
        "02_Sarone",
        "07_Pertanyaan"
    };

    [Header("Scene Museum")]
    public string namaSceneMuseumAwal = "05_MuseumDalam";

    [Header("Scene Tanpa Timer")]
    public string[] sceneTanpaTimer =
    {
        "00_MainMenu",
        "08_Prolog"
    };

    [Header("Jatah Waktu per Sekat (ikut sistem hari GameManager)")]
    public int jumlahHariPerSekat = 3;

    // true  = jatah habis tepat tengah malam akhir hari ke-3
    // false = jatah habis setelah 3 x 24 jam penuh
    public bool akhirDiTengahMalam = true;

    // Dipakai hanya kalau GameManager tidak ada di scene
    public float durasiCadanganDetik = 600f;

    [Header("Posisi Checkpoint Progress Bar (0-1, sesuai garis di frame)")]
    [Range(0f, 1f)]
    public float[] posisiCheckpoint =
    {
        0.26f,
        0.51f,
        0.77f,
        0.9f
    };

    [Header("Warna Progress Bar & Peringatan")]
    public Color warnaNormal = Color.green;
    public Color warnaPeringatan = Color.yellow;
    public Color warnaBahaya = Color.red;

    [Tooltip("Sisa waktu (0-1 dari jatah) di bawah ini -> fill jadi KUNING")]
    [Range(0f, 1f)] public float batasKuning = 0.30f;

    [Tooltip("Sisa waktu (0-1 dari jatah) di bawah ini -> fill KEDIP kuning-merah")]
    [Range(0f, 1f)] public float batasMerah = 0.15f;

    public float kecepatanKedip = 4f;

    [Tooltip("Bar ditahan penuh + teks 'Waktu Habis!' sebelum balik ke museum")]
    public float jedaSebelumGagal = 1.5f;

    // Status alat musik
    public static bool hasAngklung = false;
    public static bool hasGesoGeso = false;
    public static bool hasSarone = false;
    public static bool hasAlatKeempat = false;

    // ----- state internal -----
    private int checkpointSelesai = 0;   // sekat yang sudah BERHASIL ditaruh di display
    private int sceneAktif = -1;
    private bool timerAktif = false;
    private bool sekatSudahMulai = false;
    private bool sedangGagal = false;
    private bool pakaiFillAmount = false;
    private float waktuCadangan = 0f;
    private float jadwalBindUlang = 0f;


    // =====================================
    // AWAKE / START / DESTROY
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

    void Start()
    {
        BindUI();
        PeriksaSceneAktif(SceneManager.GetActiveScene());
    }

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
        // Batalkan rencana "gagal" yang tertunda kalau scene keburu berganti
        CancelInvoke(nameof(KembaliKeMuseum));
        sedangGagal = false;

        progressBar = null;
        progressText = null;
        fillBarImage = null;
        pakaiFillAmount = false;

        BindUI();
        PeriksaSceneAktif(scene);

        string namaSceneIni = scene.name;

        // Catat scene terakhir untuk fitur Resume (kecuali Main Menu & Prolog)
        if (!SceneTanpaTimer(namaSceneIni))
        {
            PlayerPrefs.SetString("SceneTerakhir", namaSceneIni);
            PlayerPrefs.Save();
        }
    }

    bool SceneTanpaTimer(string namaScene)
    {
        if (sceneTanpaTimer == null) return false;

        foreach (string s in sceneTanpaTimer)
        {
            if (namaScene == s)
                return true;
        }
        return false;
    }

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

        bool bebasTimer = SceneTanpaTimer(scene.name);
        bool masihAdaSekat = checkpointSelesai < 4;

        // ----- Bukan scene alat musik -----
        if (sceneAktif == -1)
        {
            // Kalau sekat sudah dimulai, timer terus jalan di scene mana pun (kecuali menu/prolog)
            if (!bebasTimer && sekatSudahMulai && masihAdaSekat)
            {
                MulaiAtauLanjutkanSekat();
                Debug.Log($"⏱️ Timer sekat ke-{checkpointSelesai + 1} dilanjutkan di {scene.name}.");
                return;
            }

            Debug.Log("Scene menu/prolog atau sekat belum mulai. Timer berhenti.");
            return;
        }

        // ----- Scene alat musik yang bukan gilirannya -----
        if (sceneAktif != checkpointSelesai)
        {
            if (sekatSudahMulai && masihAdaSekat && !bebasTimer)
                MulaiAtauLanjutkanSekat();
            else
                Debug.Log("Scene belum sesuai urutan checkpoint.");
            return;
        }

        // ----- Scene alat musik sesuai giliran -----
        MulaiAtauLanjutkanSekat();
        Debug.Log("Timer aktif di scene: " + sceneAlatMusik[sceneAktif]);
    }


    // =====================================
    // MENCARI & MENYIAPKAN UI
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

        if (progressBar == null)
            return;

        // Bar murni tampilan: pemain tidak boleh bisa klik/drag
        progressBar.interactable = false;
        progressBar.minValue = 0f;
        progressBar.maxValue = 1f;
        progressBar.wholeNumbers = false;

        // Cari Image "Fill"
        fillBarImage = null;
        RectTransform fr = progressBar.fillRect;
        if (fr != null) fillBarImage = fr.GetComponent<Image>();
        if (fillBarImage == null)
        {
            Transform t = progressBar.transform.Find("Fill");
            if (t != null) fillBarImage = t.GetComponent<Image>();
        }

        if (fillBarImage == null)
            return;

        // Satu mekanisme saja yang mengisi bar:
        //  - Image tipe Filled  -> diisi lewat fillAmount, lebar Fill dilepas dari Slider
        //  - Image tipe lain    -> diisi lewat Slider (lebar Fill yang berubah)
        pakaiFillAmount = fillBarImage.type == Image.Type.Filled;

        if (pakaiFillAmount)
        {
            fillBarImage.fillMethod = Image.FillMethod.Horizontal;
            fillBarImage.fillOrigin = (int)Image.OriginHorizontal.Left;

            if (progressBar.fillRect != null)
                progressBar.fillRect = null;

            RectTransform rt = fillBarImage.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
        }
        else if (progressBar.fillRect == null)
        {
            progressBar.fillRect = fillBarImage.rectTransform;
        }
    }


    // =====================================
    // SATU-SATUNYA TEMPAT NILAI BAR DITULIS
    // =====================================

    void SetNilaiBar(float nilai)
    {
        nilai = Mathf.Clamp01(nilai);

        // normalizedValue = aman walau Min/Max Slider di Inspector bukan 0-1
        if (progressBar != null)
            progressBar.normalizedValue = nilai;

        if (pakaiFillAmount && fillBarImage != null)
            fillBarImage.fillAmount = nilai;
    }

    float PosisiCheckpoint(int index)
    {
        if (posisiCheckpoint == null || posisiCheckpoint.Length == 0) return 1f;
        return posisiCheckpoint[Mathf.Clamp(index, 0, posisiCheckpoint.Length - 1)];
    }

    float PosisiAwalSekat()
    {
        return checkpointSelesai <= 0 ? 0f : PosisiCheckpoint(checkpointSelesai - 1);
    }

    float PosisiAkhirSekat()
    {
        return PosisiCheckpoint(checkpointSelesai);
    }

    // Bar diam di garis checkpoint terakhir yang sudah terkunci
    void UpdateProgressBar()
    {
        bool selesai = checkpointSelesai >= 4;

        SetNilaiBar(selesai ? 1f : PosisiAwalSekat());

        if (fillBarImage != null)
            fillBarImage.color = warnaNormal;

        if (selesai && progressText != null)
            progressText.text = "Selesai!";
    }


    // =====================================
    // WAKTU SEKAT (DARI JAM GAMEMANAGER)
    // =====================================

    // fraksi    : 0 (baru mulai) -> 1 (jatah habis)
    // sisaDetik : sisa waktu NYATA (detik) sampai jatah habis
    void HitungWaktuSekat(out float fraksi, out float sisaDetik)
    {
        GameManager gm = GameManager.Instance;

        if (gm != null)
        {
            float detikPerHari = Mathf.Max(1f, gm.durasiSiang + gm.durasiMalam);

            // Total jam game dalam satu sekat
            float totalJam = jumlahHariPerSekat * 24f;
            if (akhirDiTengahMalam)
                totalJam -= gm.jamMulaiGame;   // mulai 09.00 -> habis 24.00 hari terakhir
            totalJam = Mathf.Max(1f, totalJam);

            // Jam game yang sudah lewat sejak sekat dimulai
            float jamBerlalu = (gm.hariSekarang - 1) * 24f + gm.JamSekarang - gm.jamMulaiGame;
            jamBerlalu = Mathf.Max(0f, jamBerlalu);

            fraksi = Mathf.Clamp01(jamBerlalu / totalJam);
            sisaDetik = (totalJam - jamBerlalu) / 24f * detikPerHari;
            return;
        }

        // Cadangan kalau GameManager tidak ada
        float durasi = Mathf.Max(1f, durasiCadanganDetik);
        fraksi = Mathf.Clamp01(waktuCadangan / durasi);
        sisaDetik = durasi - waktuCadangan;
    }

    // Mulai hitung sekat baru dari nol, jam game ikut kembali ke Hari 1 jam mulai
    void MulaiHitungWaktuBaru()
    {
        sekatSudahMulai = true;
        waktuCadangan = 0f;

        if (GameManager.Instance != null)
            GameManager.Instance.ResetWaktu();
    }

    // Aktifkan timer; jam hanya di-reset kalau sekat ini memang belum pernah mulai
    void MulaiAtauLanjutkanSekat()
    {
        timerAktif = true;

        if (!sekatSudahMulai)
            MulaiHitungWaktuBaru();

        HitungWaktuSekat(out float fraksi, out float sisaDetik);
        TampilkanProgress(fraksi, sisaDetik);
    }


    // =====================================
    // UPDATE
    // =====================================

    void Update()
    {
        // Game di-pause: bar dan timer berhenti
        if (Time.timeScale == 0f)
            return;

        GameManager gm = GameManager.Instance;

        // Jam game hanya jalan saat timer sekat aktif -> selalu sinkron dengan bar
        if (gm != null)
            gm.AturWaktuJalan(timerAktif || checkpointSelesai >= 4);

        if (!timerAktif || checkpointSelesai >= 4)
            return;

        // UI mungkin baru muncul belakangan (mis. UI_Game diaktifkan setelah cutscene)
        if (progressBar == null && Time.unscaledTime >= jadwalBindUlang)
        {
            jadwalBindUlang = Time.unscaledTime + 0.5f;
            BindUI();
        }

        if (gm == null)
            waktuCadangan += Time.deltaTime;

        HitungWaktuSekat(out float fraksi, out float sisaDetik);

        if (sisaDetik <= 0f)
        {
            PlayerGagal();
            return;
        }

        TampilkanProgress(fraksi, sisaDetik);
    }

    void TampilkanProgress(float fraksi, float sisaDetik)
    {
        // Bar merayap dari checkpoint sebelumnya ke checkpoint sekat ini
        SetNilaiBar(Mathf.Lerp(PosisiAwalSekat(), PosisiAkhirSekat(), fraksi));
        AturWarnaProgress(fraksi);
        UpdateTeksSisaWaktu(sisaDetik);
    }


    // =====================================
    // WARNA: HIJAU -> KUNING -> KEDIP KUNING-MERAH
    // =====================================

    void AturWarnaProgress(float fraksi)
    {
        if (fillBarImage == null)
            return;

        float sisa = 1f - fraksi;   // 1 = baru mulai, 0 = habis

        if (sisa <= batasMerah)
        {
            float kedip = Mathf.PingPong(Time.time * kecepatanKedip, 1f);
            fillBarImage.color = Color.Lerp(warnaPeringatan, warnaBahaya, kedip);
        }
        else if (sisa <= batasKuning)
        {
            fillBarImage.color = warnaPeringatan;
        }
        else
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
    // BERHASIL TARUH DISPLAY DI MUSEUM
    // =====================================

    // namaAlat opsional: kalau diisi, dicek harus sesuai urutan sekat
    public void BerhasilTaruhDisplay(string namaAlat = null)
    {
        if (checkpointSelesai >= 4 || sedangGagal)
            return;

        string[] urutan = { "Angklung", "GesoGeso", "Sarone", "AlatKeempat" };
        if (!string.IsNullOrEmpty(namaAlat))
        {
            bool sesuai = namaAlat == urutan[checkpointSelesai]
                          || (checkpointSelesai == 3 && namaAlat == "Suling");

            if (!sesuai)
            {
                Debug.LogWarning("Urutan display salah, bar tidak naik.");
                return;
            }
        }

        // Kunci bar tepat di garis checkpoint sekat ini
        SetNilaiBar(PosisiAkhirSekat());

        // Lanjut ke sekat berikutnya, jatah hari baru mulai dari nol
        checkpointSelesai++;
        MulaiHitungWaktuBaru();

        if (checkpointSelesai >= 4)
        {
            checkpointSelesai = 4;
            timerAktif = false;
            UpdateProgressBar();   // bar penuh + "Selesai!"
            Debug.Log("🏆 SEMUA ALAT MUSIK SELESAI DITARUH DI DISPLAY!");
            return;
        }

        UpdateProgressBar();       // bar tetap di checkpoint yang baru dikunci, warna balik normal
        Debug.Log("🎉 Alat musik ditaruh di display! Progress sekat ini terkunci.");
    }


    // =====================================
    // KUNCI PINTU (DIPAKAI PintuMuseum)
    // =====================================

    // Pintu terbuka kalau urutannya sudah tiba.
    // Kalau pintu yang sudah selesai tidak boleh dimasuki lagi, ganti ">=" jadi "==".
    public bool BolehMasuk(string namaAlat)
    {
        switch (namaAlat)
        {
            case "Angklung":    return checkpointSelesai >= 0;
            case "GesoGeso":    return checkpointSelesai >= 1;
            case "Sarone":      return checkpointSelesai >= 2;
            case "AlatKeempat":
            case "Suling":      return checkpointSelesai >= 3;
        }
        return false;
    }


    // =====================================
    // JATAH HARI HABIS (GAGAL)
    // =====================================

    void PlayerGagal()
    {
        timerAktif = false;   // jam game ikut berhenti (lihat Update)
        sedangGagal = true;

        // Bar ditahan penuh & merah sebentar supaya pemain sadar waktunya habis
        SetNilaiBar(PosisiAkhirSekat());
        if (fillBarImage != null) fillBarImage.color = warnaBahaya;
        if (progressText != null) progressText.text = "Waktu Habis!";

        Debug.Log("⏰ Jatah hari habis sebelum ditaruh di display! Kembali ke museum awal.");
        Invoke(nameof(KembaliKeMuseum), Mathf.Max(0f, jedaSebelumGagal));
    }

    void KembaliKeMuseum()
    {
        sedangGagal = false;
        MulaiHitungWaktuBaru();
        UpdateProgressBar();
        SceneManager.LoadScene(namaSceneMuseumAwal);
    }


    // =====================================
    // TEKS TIMER
    // =====================================

    void UpdateTeksSisaWaktu(float sisaDetik)
    {
        if (progressText == null)
            return;

        sisaDetik = Mathf.Max(0f, sisaDetik);

        int menit = Mathf.FloorToInt(sisaDetik / 60f);
        int detik = Mathf.FloorToInt(sisaDetik % 60f);

        progressText.text = $"Sisa Waktu: {menit:00}:{detik:00}";
    }


    // =====================================
    // FUNGSI YANG DIPANGGIL SCRIPT LAIN (NPCCrafting, dll)
    // =====================================

    public void MulaiTimerBar()
    {
        if (!sedangGagal && sceneAktif == checkpointSelesai && checkpointSelesai < 4)
        {
            timerAktif = true;

            if (!sekatSudahMulai)
                MulaiHitungWaktuBaru();
        }
    }

    public void MulaiProgressOtomatis(float durasi)
    {
        MulaiTimerBar();
    }
}