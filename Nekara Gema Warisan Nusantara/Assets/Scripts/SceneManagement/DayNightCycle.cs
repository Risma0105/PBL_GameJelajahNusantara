using UnityEngine;
using UnityEngine.Rendering.Universal; // Wajib untuk Light2D
using TMPro;

public class DayNightCycle : MonoBehaviour
{
    public static DayNightCycle Instance;

    [Header("Referensi Cahaya Global")]
    public Light2D globalLight; // Slot Global Light 2D (otomatis dicari jika kosong)

    [Header("Warna Cahaya")]
    public Color warnaSiang = Color.white;
    public Color warnaMalam = new Color(0.12f, 0.15f, 0.35f, 1f); 

    [Header("Pengaturan Waktu")]
    public float durasiSiang = 80f; // 1.33 menit
    public float durasiMalam = 80f; // 1.33 menit
    public int maxHari = 10;

    [Header("Status")]
    public static int hariSekarang = 1;
    private float timerDetik = 0f;
    private bool isSiang = true;

    [Header("UI Hari (Opsional)")]
    public TextMeshProUGUI teksHari;

    [Header("Daftar Lampu di Map")]
    public GameObject[] daftarLampu; // Masukkan Point Light 2D / lentera di sini

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
        }
    }

    void Start()
    {
        CariGlobalLight();
        UpdateTampilanUI();

        // Mulai di kondisi siang: lampu dimatikan
        SetSemuaLampu(!isSiang);
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
        if (daftarLampu != null)
        {
            foreach (GameObject lampu in daftarLampu)
            {
                if (lampu != null)
                {
                    lampu.SetActive(status);
                }
            }
        }
    }

    void UpdateTampilanUI()
    {
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