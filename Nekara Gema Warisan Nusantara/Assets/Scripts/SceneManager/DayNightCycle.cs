using UnityEngine;
using UnityEngine.Rendering.Universal; // Wajib untuk Light2D
using UnityEngine.SceneManagement;      // Wajib untuk mendeteksi perpindahan scene
using TMPro;

public class DayNightCycle : MonoBehaviour
{
    public static DayNightCycle Instance;

    [Header("Referensi Cahaya Global")]
    public Light2D globalLight; 

    [Header("Warna Cahaya")]
    public Color warnaSiang = Color.white;
    public Color warnaMalam = new Color(0.12f, 0.15f, 0.35f, 1f); 

    [Header("Pengaturan Waktu")]
    public float durasiSiang = 80f; 
    public float durasiMalam = 80f; 
    public int maxHari = 10;

    [Header("Status")]
    public static int hariSekarang = 1;
    private float timerDetik = 0f;
    private bool isSiang = true;

    [Header("UI Hari (Opsional)")]
    public TextMeshProUGUI teksHari;

    [Header("Daftar Lampu di Map")]
    public GameObject[] daftarLampu; 

    void Awake()
    {
        // Pola Singleton untuk mencegah duplikasi objek saat pindah scene
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded; // Daftarkan Event pindah scene
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void OnDestroy()
    {
        // Bersihkan event saat objek dihancurkan
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    void Start()
    {
        InisialisasiSceneBaru();
    }

    void Update()
    {
        if (globalLight == null)
        {
            CariGlobalLight();
            if (globalLight == null) return;
        }

        timerDetik += Time.deltaTime;

        if (isSiang)
        {
            float t = timerDetik / durasiSiang;
            globalLight.color = Color.Lerp(warnaSiang, warnaMalam, t);

            if (timerDetik >= durasiSiang)
            {
                isSiang = false;
                timerDetik = 0f;
                Debug.Log("🌙 Malam telah tiba!");
                SetSemuaLampu(true);
            }
        }
        else
        {
            float t = timerDetik / durasiMalam;
            globalLight.color = Color.Lerp(warnaMalam, warnaSiang, t);

            if (timerDetik >= durasiMalam)
            {
                isSiang = true;
                timerDetik = 0f;
                hariSekarang++;
                Debug.Log("☀️ Hari baru: Hari ke-" + hariSekarang);
                UpdateTampilanUI();
                SetSemuaLampu(false);
            }
        }
    }

    // Dipanggil otomatis setiap kali scene baru selesai dimuat
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        InisialisasiSceneBaru();
    }

    void InisialisasiSceneBaru()
    {
        CariGlobalLight();
        UpdateTampilanUI();

        // Sinkronkan warna cahaya & status lampu dengan kondisi saat ini (siang/malam)
        if (globalLight != null)
        {
            float t = timerDetik / (isSiang ? durasiSiang : durasiMalam);
            // Sesuaikan warna langsung agar tidak kaget/berubah drastis
            globalLight.color = isSiang ? Color.Lerp(warnaSiang, warnaMalam, t) : Color.Lerp(warnaMalam, warnaSiang, t);
        }

        // Jika pindah scene pas malam, nyalakan lampunya. Kalau siang, matikan.
        SetSemuaLampu(!isSiang);
    }

    void CariGlobalLight()
    {
        if (globalLight != null) return;

        Light2D[] allLights = Object.FindObjectsByType<Light2D>(FindObjectsSortMode.None);
        foreach (Light2D l in allLights)
        {
            if (l.gameObject.name.ToLower().Contains("global"))
            {
                globalLight = l;
                break;
            }
        }
    }

    void SetSemuaLampu(bool status)
    {
        // Cari otomatis lampu ber-tag "LampuMap" jika array kosong di scene baru
        if (daftarLampu == null || daftarLampu.Length == 0)
        {
            GameObject[] lampuObjek = GameObject.FindGameObjectsWithTag("LampuMap");
            foreach (GameObject l in lampuObjek)
            {
                l.SetActive(status);
            }
            return;
        }

        foreach (GameObject lampu in daftarLampu)
        {
            if (lampu != null)
            {
                lampu.SetActive(status);
            }
        }
    }

    void UpdateTampilanUI()
    {
        // Cari otomatis teks hari jika referensinya terputus karena pindah scene
        if (teksHari == null)
        {
            GameObject teksObj = GameObject.FindWithTag("TeksHariUI");
            if (teksObj != null) teksHari = teksObj.GetComponent<TextMeshProUGUI>();
        }

        if (teksHari != null)
        {
            teksHari.text = "Hari: " + hariSekarang + " / " + maxHari;
        }
    }

    public bool IsSiangHari()
    {
        return isSiang;
    }
}