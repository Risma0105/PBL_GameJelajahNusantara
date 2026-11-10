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
    public float durasiSiang = 100f; 
    public float durasiMalam = 100f; 
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
    private bool isSiang = true;

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

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        GameObject globalLightObj = GameObject.Find("Global Light 2D");
        if (globalLightObj != null)
        {
            currentGlobalLight = globalLightObj.GetComponent<Light2D>();
        }
        else
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
        
        AturSemuaLampuMap(!isSiang);
    }

    void Update()
    {
        timerHari += Time.deltaTime;

        if (isSiang)
        {
            float t = timerHari / durasiSiang;
            t = Mathf.Clamp01(t);

            if (currentGlobalLight != null)
            {
                currentGlobalLight.color = Color.Lerp(warnaSiang, warnaMalam, t);
                currentGlobalLight.intensity = Mathf.Lerp(intensitasSiang, intensitasMalam, t);
            }

            if (timerHari >= durasiSiang)
            {
                isSiang = false;
                timerHari = 0f; 
                AturSemuaLampuMap(true); // Nyalakan cahaya lampu saat malam
                Debug.Log("🌙 Malam tiba: Lampu map dinyalakan.");
            }
        }
        else
        {
            float t = timerHari / durasiMalam;
            t = Mathf.Clamp01(t);

            if (currentGlobalLight != null)
            {
                currentGlobalLight.color = Color.Lerp(warnaMalam, warnaSiang, t);
                currentGlobalLight.intensity = Mathf.Lerp(intensitasMalam, intensitasMalam, t); // Bisa disesuaikan transisinya
            }

            if (timerHari >= durasiMalam)
            {
                isSiang = true;
                timerHari = 0f; 
                hariSekarang++;
                AturSemuaLampuMap(false); // Matikan cahaya lampu saat siang
                Debug.Log($"☀️ Hari baru: Masuk ke Hari ke-{hariSekarang}");

                if (hariSekarang > totalMaksimalHari)
                {
                    Debug.Log("🏁 Batas 10 Hari Selesai!");
                }
            }
        }

        if (teksHariDanWaktu != null)
        {
            string statusWaktu = isSiang ? "Siang" : "Malam";
            teksHariDanWaktu.text = $"Hari {hariSekarang} ({statusWaktu})";
        }
    }

    // Fungsi paling aman: Tiang lampu dibiarkan utuh, hanya komponen Light2D yang diaktifkan/dimatikan
    void AturSemuaLampuMap(bool nyala)
    {
        GameObject[] daftarLampu = GameObject.FindGameObjectsWithTag("LampuMap");
        foreach (GameObject lampu in daftarLampu)
        {
            if (lampu != null)
            {
                // Ambil komponen Light2D langsung dari objek atau anak-anaknya tanpa mengubah SetActive objek utamanya
                Light2D compLight = lampu.GetComponentInChildren<Light2D>();
                if (compLight != null)
                {
                    compLight.enabled = nyala; // True saat malam (nyala), False saat siang (mati)
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