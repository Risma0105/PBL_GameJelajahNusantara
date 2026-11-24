using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalKeluar : MonoBehaviour
{
    [Tooltip("Opsional. Kalau kosong, dicari otomatis lewat tag Player.")]
    public Transform player;

    [Tooltip("ID titik muncul. Kosongkan kalau scene ini hanya punya satu PortalKeluar. " +
             "Kalau ada beberapa, beri ID berbeda dan samakan dengan 'Id Spawn Di Tujuan' di pintu asal.")]
    public string idPortal = "";

    public string namaSFXKeluar;

    private Vector3 skalaAsli;
    private Rigidbody2D rb;
    private readonly List<MonoBehaviour> skripDikunci = new List<MonoBehaviour>();
    private bool sudahMuncul;

    private void OnEnable()  => SceneTransitionManager.OnLayarTerbuka += Muncul;
    private void OnDisable() => SceneTransitionManager.OnLayarTerbuka -= Muncul;

    private void Start()
    {
        // Hanya PortalKeluar dengan ID yang cocok yang bekerja
        if (idPortal != SceneTransitionManager.SpawnIdTerakhir)
        {
            enabled = false;
            return;
        }

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        if (player == null)
        {
            Debug.LogWarning("PortalKeluar: objek ber-tag Player tidak ditemukan.");
            return;
        }

        skalaAsli = player.localScale;

        // Matikan fisika dulu, baru kecilkan (collider berskala 0 saat fisika aktif bikin glitch)
        rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }

        player.position = transform.position;
        player.localScale = Vector3.zero;

        // Kunci skrip Player yang sedang aktif
        foreach (MonoBehaviour skrip in player.GetComponents<MonoBehaviour>())
        {
            if (skrip.enabled)
            {
                skrip.enabled = false;
                skripDikunci.Add(skrip);
            }
        }

        // Kalau scene dijalankan langsung (tidak lewat transisi), langsung munculkan
        if (SceneTransitionManager.Instance == null || !SceneTransitionManager.Instance.SedangTransisi)
            Muncul();
    }

    private void Muncul()
    {
        if (sudahMuncul || player == null) return;
        sudahMuncul = true;
        StartCoroutine(RutinKeluar());
    }

    private IEnumerator RutinKeluar()
    {
        if (AudioManager.Instance != null && !string.IsNullOrEmpty(namaSFXKeluar))
            AudioManager.Instance.PlaySFX(namaSFXKeluar);

        float t = 0f, dur = 0.5f;
        while (t < dur)
        {
            t += Time.deltaTime;
            player.localScale = Vector3.Lerp(Vector3.zero, skalaAsli, t / dur);
            yield return null;
        }
        player.localScale = skalaAsli;

        // Nyalakan lagi fisika dan skrip
        if (rb != null) rb.simulated = true;
        foreach (MonoBehaviour skrip in skripDikunci)
            if (skrip != null) skrip.enabled = true;
    }
}