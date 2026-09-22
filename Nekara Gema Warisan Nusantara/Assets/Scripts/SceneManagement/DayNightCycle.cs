using UnityEngine;
using UnityEngine.Rendering.Universal; // Wajib untuk Light2D

public class DayNightCycle : MonoBehaviour
{
    [Header("Komponen Cahaya")]
    public Light2D globalLight;

    [Header("Durasi Siklus")]
    [Tooltip("Total waktu satu siklus penuh (siang ke malam lalu siang lagi)")]
    public float durasiSatuHari = 80f; // 1,33 menit = 80 detik

    [Header("Warna Cahaya")]
    public Color warnaSiang = Color.white;
    public Color warnaMalam = new Color(0.12f, 0.15f, 0.3f, 1f); // Biru gelap pekat

    [Header("Intensitas Cahaya")]
    public float intensitasSiang = 1.0f;
    public float intensitasMalam = 0.2f;

    private float timer = 0f;

    void Start()
    {
        // Otomatis mencari Global Light 2D jika slot di Inspector lupa diisi
        if (globalLight == null)
        {
            globalLight = GetComponent<Light2D>();
        }
    }

    void Update()
    {
        if (globalLight == null) return;

        // Tambahkan waktu setiap frame
        timer += Time.deltaTime;

        // Hitung gelombang siklus (0 = malam pekat, 1 = siang penuh)
        // Dimulai dari siang saat game baru jalan
        float progress = (Mathf.Sin((timer / durasiSatuHari) * Mathf.PI * 2 + (Mathf.PI / 2)) + 1f) / 2f;

        // Transisi warna dan intensitas secara perlahan
        globalLight.color = Color.Lerp(warnaMalam, warnaSiang, progress);
        globalLight.intensity = Mathf.Lerp(intensitasMalam, intensitasSiang, progress);
    }
}