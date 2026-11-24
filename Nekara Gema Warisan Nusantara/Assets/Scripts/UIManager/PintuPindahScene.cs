using System.Collections;
using UnityEngine;

public class PintuPindahScene : MonoBehaviour
{
    [Header("Tujuan Ruangan")]
    public string namaSceneTujuan;
    public Sprite bannerNamaMap;        // kosongkan kalau tidak perlu banner
    public Transform titikMasuk;        // opsional, kosong = posisi objek ini

    [Tooltip("Samakan dengan ID di PortalKeluar yang jadi titik muncul di scene tujuan. " +
             "Kosongkan kalau scene tujuan hanya punya satu PortalKeluar (ID kosong).")]
    public string idSpawnDiTujuan;

    [Header("Pengaturan SFX Transisi")]
    public string namaSFXPintu;

    private bool sudahDipakai;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (sudahDipakai || !other.CompareTag("Player")) return;

        // Abaikan pintu selama transisi sedang berjalan (mencegah player terkunci/glitch)
        if (SceneTransitionManager.Instance != null && SceneTransitionManager.Instance.SedangTransisi) return;

        if (string.IsNullOrEmpty(namaSceneTujuan))
        {
            Debug.LogWarning("Nama scene tujuan belum diisi di Inspector!");
            return;
        }

        sudahDipakai = true;
        StartCoroutine(RutinMasuk(other.gameObject));
    }

    private IEnumerator RutinMasuk(GameObject player)
    {
        // Kunci semua skrip di Player (gerak, dll)
        foreach (MonoBehaviour skrip in player.GetComponents<MonoBehaviour>())
            skrip.enabled = false;

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false; // supaya transform bisa digeser manual
        }

        if (AudioManager.Instance != null && !string.IsNullOrEmpty(namaSFXPintu))
            AudioManager.Instance.PlaySFX(namaSFXPintu);

        // Tarik ke tengah portal sambil mengecil
        Vector3 posAwal = player.transform.position;
        Vector3 skalaAwal = player.transform.localScale;
        Vector3 target = titikMasuk != null ? titikMasuk.position : transform.position;

        float t = 0f, dur = 0.5f;
        while (t < dur)
        {
            t += Time.deltaTime;
            float k = t / dur;
            player.transform.position = Vector3.Lerp(posAwal, target, k);
            player.transform.localScale = Vector3.Lerp(skalaAwal, Vector3.zero, k);
            yield return null;
        }

        if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.PindahScene(namaSceneTujuan, bannerNamaMap, idSpawnDiTujuan);
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene(namaSceneTujuan); // fallback
    }
}