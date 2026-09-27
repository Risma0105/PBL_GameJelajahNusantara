using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameProgress : MonoBehaviour
{
    public static GameProgress Instance;

    [Header("Status Alat Musik (Tersimpan Otomatis)")]
    public static bool hasAngklung = false;
    public static bool hasGesoGeso = false;
    public static bool hasSarone = false;

    [Header("UI References")]
    public Slider progressBar;
    public TextMeshProUGUI progressText;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    void Start()
    {
        UpdateUI();
    }

    // Fungsi untuk menandai alat mana yang didapat
    public void CollectInstrument(string instrumentName)
    {
        if (instrumentName == "Angklung") hasAngklung = true;
        if (instrumentName == "GesoGeso") hasGesoGeso = true;
        if (instrumentName == "Sarone") hasSarone = true;

        UpdateUI();

        // Cek jika ketiga alat sudah terkumpul semuanya
        if (GetTotalCompleted() >= 3)
        {
            Debug.Log("Semua alat lengkap! Karnaval terbuka.");
            // SceneManager.LoadScene("Scene_Karnaval");
        }
    }

    // Menghitung berapa banyak alat yang sudah selesai
    public int GetTotalCompleted()
    {
        int total = 0;
        if (hasAngklung) total++;
        if (hasGesoGeso) total++;
        if (hasSarone) total++;
        return total;
    }

    public void UpdateUI()
    {
        int count = GetTotalCompleted();

        if (progressBar != null)
        {
            progressBar.value = count; // Nilai bar: 0, 1, 2, atau 3
        }

        if (progressText != null)
        {
            progressText.text = "Alat Terkumpul: " + count + " / 3";
        }
    }
}