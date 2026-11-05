using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Referensi Text Guide Utama")]
    public TextMeshProUGUI teksGuideUtama;

    [Header("Teks Statis Saat Idle / Tidak Dekat Apa-apa")]
    [TextArea]
    public string teksDefault = "🧭 Jelajahi area sekitar dan berinteraksi dengan objek.";

    void Awake()
    {
        // Singleton pattern sederhana
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Tampilkan teks statis di awal game
        TampilkanDefault();
    }

    // Fungsi untuk menampilkan pesan khusus (saat dekat objek/pohon/NPC)
    public void TampilkanPesan(string pesan)
    {
        if (teksGuideUtama != null)
        {
            teksGuideUtama.text = pesan;
        }
    }

    // Fungsi untuk kembali ke teks statis awal
    public void TampilkanDefault()
    {
        if (teksGuideUtama != null)
        {
            teksGuideUtama.text = teksDefault;
        }
    }
}