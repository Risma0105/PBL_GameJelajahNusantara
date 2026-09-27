using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DayNightCycle : MonoBehaviour
{
    public static DayNightCycle Instance;

    [Header("Referensi Layar Gelap")]
    public Image nightOverlay; // Masukkan objek NightOverlay ke sini

    [Header("Kepekatan Gelap Malam")]
    [Range(0f, 1f)] public float kegelapanMaksimal = 0.5f; // 0.6 = biru malam pas, tidak terlalu gelap pekat

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
    public GameObject[] daftarLampu; // Masukkan semua objek pendaran lampu ke sini

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
        UpdateTampilanUI();

        // Kondisi awal (karena mulai di siang hari, lampu dimatikan dulu)
        SetSemuaLampu(!isSiang);
    }

    void Update()
    {
        // Cari NightOverlay otomatis jika scene baru belum terhubung
        if (nightOverlay == null)
        {
            GameObject overlayObj = GameObject.Find("NightOverlay");
            if (overlayObj != null) nightOverlay = overlayObj.GetComponent<Image>();
        }

        timerDetik += Time.deltaTime;

        if (isSiang)
        {
            // Dari Siang (Terang/Alpha 0) menuju Malam (Gelap)
            float t = timerDetik / durasiSiang;
            SetAlpha(Mathf.Lerp(0f, kegelapanMaksimal, t));

            if (timerDetik >= durasiSiang)
            {
                isSiang = false;
                timerDetik = 0f;
                Debug.Log("🌙 Malam telah tiba!");
                
                // NYALAKAN LAMPU
                SetSemuaLampu(true);
            }
        }
        else
        {
            // Dari Malam (Gelap) kembali ke Siang (Terang/Alpha 0)
            float t = timerDetik / durasiMalam;
            SetAlpha(Mathf.Lerp(kegelapanMaksimal, 0f, t));

            if (timerDetik >= durasiMalam)
            {
                isSiang = true;
                timerDetik = 0f;
                hariSekarang++;
                Debug.Log("☀️ Hari baru: Hari ke-" + hariSekarang);
                UpdateTampilanUI();

                // MATIKAN LAMPU
                SetSemuaLampu(false);
            }
        }
    }

    void SetAlpha(float alphaValue)
    {
        if (nightOverlay != null)
        {
            Color c = nightOverlay.color;
            c.a = alphaValue;
            nightOverlay.color = c;
        }
    }

    // Fungsi untuk menyalakan/mematikan semua lampu di daftar
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

    // Fungsi pembantu jika ada script lain butuh tahu status siang/malam
    public bool IsSiangHari()
    {
        return isSiang;
    }
}