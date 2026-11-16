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

    [Header("Pengaturan Durasi Waktu (Detik)")]
    [Tooltip("Satu hari penuh (00.00-23.59) = durasiSiang + durasiMalam")]
    public float durasiSiang = 100f;
    public float durasiMalam = 100f;
    public int hariSekarang = 1;
    public int totalMaksimalHari = 10;

    [Header("Jam Dalam Game")]
    [Tooltip("Jam saat game dimulai (di scene pertama / museum dalam)")]
    [Range(0f, 24f)] public float jamMulaiGame = 9f;
    [Tooltip("Lampu map menyala mulai jam ini")]
    [Range(0f, 24f)] public float jamLampuNyala = 18f;
    [Tooltip("Lampu map mati mulai jam ini")]
    [Range(0f, 24f)] public float jamLampuMati = 6f;

    [Header("Pengaturan Warna & Cahaya Global")]
    public Color warnaSiang = Color.white;
    public Color warnaSore = new Color(1f, 0.62f, 0.38f, 1f);   // dipakai juga saat fajar
    public Color warnaMalam = new Color(0.12f, 0.15f, 0.32f, 1f);
    public float intensitasSiang = 1.0f;
    public float intensitasMalam = 0.2f;

    [Tooltip("X = jam (0-24), Y: 0 = siang, 0.5 = sore/fajar (oranye), 1 = gelap")]
    public AnimationCurve kurvaCahaya24Jam = new AnimationCurve(
        new Keyframe(0f, 1f),     // tengah malam: gelap
        new Keyframe(5f, 1f),     // masih gelap
        new Keyframe(6.5f, 0.5f), // fajar (oranye)
        new Keyframe(8f, 0f),     // pagi: terang
        new Keyframe(15f, 0f),    // siang terang
        new Keyframe(17f, 0.5f),  // sore (oranye)
        new Keyframe(19f, 0.5f),
        new Keyframe(21f, 1f),    // gelap
        new Keyframe(24f, 1f));

    [Header("Scene Indoor (waktu tetap jalan, cahaya tetap terang)")]
    [Tooltip("Isi dengan nama scene persis seperti di Build Profiles, mis. museum dalam")]
    public List<string> sceneIndoor = new List<string> { "NAMA_SCENE_MUSEUM_DALAM" };

    [Header("UI Feedback (Opsional)")]
    public TMP_Text teksHariDanWaktu;

    private float jamGame;   // 0-24
    private Light2D currentGlobalLight;
    private bool isSiang = true;
    private bool waktuJalan = true;
    private bool cahayaAktif = true; // false = scene indoor (cahaya dikunci terang)

    public bool IsSiang => isSiang;
    public float JamSekarang => jamGame;
    public bool WaktuJalan => waktuJalan;
    public bool CahayaAktif => cahayaAktif;

    float DetikPerHari => Mathf.Max(1f, durasiSiang + durasiMalam);

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            jamGame = jamMulaiGame;
            isSiang = HitungSiang(jamGame);
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

    void Start()
    {
        // Pastikan scene pertama (tempat GameManager dibuat) juga ikut dicek
        if (Instance == this)
        {
            OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Abaikan GameManager duplikat yang akan dihancurkan
        if (Instance != this) return;

        cahayaAktif = !sceneIndoor.Contains(scene.name);

        currentGlobalLight = null;
        GameObject globalLightObj = GameObject.Find("Global Light 2D");
        if (globalLightObj != null)
        {
            currentGlobalLight = globalLightObj.GetComponent<Light2D>();
        }

        if (currentGlobalLight == null)
        {
            Light2D[] allLights = Object.FindObjectsByType<Light2D>(FindObjectsSortMode.None);
            foreach (var light in allLights)
            {
                if (light.lightType == Light2D.LightType.Global)
                {
                    currentGlobalLight = light;
                    break;
                }
            }
        }

        // Lampu map hanya menyala kalau scene outdoor dan sedang malam
        AturSemuaLampuMap(cahayaAktif && !isSiang);
    }

    void Update()
    {
        // Jam hanya maju kalau waktuJalan aktif (diatur oleh GameProgress)
        // Tetap jalan di semua scene, termasuk indoor
        if (waktuJalan)
        {
            // Satu jam penuh 24 jam berjalan selama DetikPerHari
            jamGame += Time.deltaTime / DetikPerHari * 24f;

            if (jamGame >= 24f)
            {
                jamGame -= 24f;
                hariSekarang++;
                Debug.Log($"☀️ Hari baru: Masuk ke Hari ke-{hariSekarang}");

                if (hariSekarang > totalMaksimalHari)
                {
                    Debug.Log("🏁 Batas 10 Hari Selesai!");
                }
            }
        }

        // Status siang/malam tetap dihitung (supaya sinkron saat kembali ke outdoor)
        bool siangSekarang = HitungSiang(jamGame);
        if (siangSekarang != isSiang)
        {
            isSiang = siangSekarang;

            // Lampu map hanya diganti di scene outdoor
            if (cahayaAktif)
            {
                AturSemuaLampuMap(!isSiang);
            }

            Debug.Log(isSiang ? "☀️ Pagi tiba: lampu map dimatikan." : "🌙 Malam tiba: lampu map dinyalakan.");
        }

        TerapkanCahaya();

        if (teksHariDanWaktu != null)
        {
            string statusWaktu = isSiang ? "Siang" : "Malam";
            teksHariDanWaktu.text = $"Hari {hariSekarang} ({statusWaktu})";
        }
    }

    bool HitungSiang(float jam)
    {
        return jam >= jamLampuMati && jam < jamLampuNyala;
    }

    void TerapkanCahaya()
    {
        if (currentGlobalLight == null) return;

        if (!cahayaAktif)
        {
            // Indoor: selalu terang, tidak ikut siklus siang-malam
            currentGlobalLight.color = warnaSiang;
            currentGlobalLight.intensity = intensitasSiang;
            return;
        }

        float gelap = kurvaCahaya24Jam.Evaluate(jamGame);
        currentGlobalLight.color = WarnaDariGelap(gelap);
        currentGlobalLight.intensity = Mathf.Lerp(intensitasSiang, intensitasMalam, gelap);
    }

    Color WarnaDariGelap(float g)
    {
        return g < 0.5f
            ? Color.Lerp(warnaSiang, warnaSore, g * 2f)
            : Color.Lerp(warnaSore, warnaMalam, (g - 0.5f) * 2f);
    }

    // Jeda / jalankan jam game (dipanggil GameProgress supaya sinkron dengan progress bar)
    public void AturWaktuJalan(bool jalan)
    {
        waktuJalan = jalan;
    }

    // Panggil saat mulai Game Baru / mulai sekat baru supaya waktu kembali ke awal
    public void ResetWaktu()
    {
        jamGame = jamMulaiGame;
        hariSekarang = 1;
        isSiang = HitungSiang(jamGame);
        AturSemuaLampuMap(cahayaAktif && !isSiang);
    }

    // Tiang lampu dibiarkan utuh, hanya komponen Light2D yang diaktifkan/dimatikan
    void AturSemuaLampuMap(bool nyala)
    {
        GameObject[] daftarLampu = GameObject.FindGameObjectsWithTag("LampuMap");
        foreach (GameObject lampu in daftarLampu)
        {
            if (lampu != null)
            {
                Light2D compLight = lampu.GetComponentInChildren<Light2D>();
                if (compLight != null)
                {
                    compLight.enabled = nyala;
                }
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