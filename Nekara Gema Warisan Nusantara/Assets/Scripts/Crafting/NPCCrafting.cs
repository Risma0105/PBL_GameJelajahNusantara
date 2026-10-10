using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections; // Wajib untuk Coroutine
using TMPro;

public class NPCCrafting : MonoBehaviour
{
    [Header("Nama Alat Musik")]
    public string namaAlatMusik = "Sarone"; // Sesuaikan nama alat musiknya

    [Header("Daftar Bahan yang Dibutuhkan")]
    public List<string> bahanDibutuhkan = new List<string>();

    [Header("Hasil Crafting")]
    public GameObject prefabAlatMusikJadi; 
    public Transform spawnPoint;          

    [Header("Pengaturan Ruangan & Progress")]
    public GameObject kumpulanBahan; // Drag objek parent pembungkus semua bahan di scene

    [Header("Efek Visual Crafting")]
    public ParticleSystem efekAsap; // Tarik objek Particle System ke sini

    [Header("UI Feedback")]
    public GameObject teksPetunjuk;              
    public TMP_Text komponenTeksTMP;             
    public UnityEngine.UI.Text komponenTeksBiasa; 

    private bool playerDekat = false;
    private bool sudahDirakit = false;
    private bool sedangCrafting = false; // Status untuk mencegah klik berulang saat proses berjalan
    private PlayerInventory inventoryPlayer;

    void Start()
    {
        if (efekAsap != null) efekAsap.Stop();

        if (GameManager.Instance != null && GameManager.Instance.CekSudahSelesai(namaAlatMusik))
        {
            sudahDirakit = true;
            if (kumpulanBahan != null) kumpulanBahan.SetActive(false);
            TampilkanPesan($"{namaAlatMusik} sudah selesai dibuat!");
            return;
        }
    }

    void Update()
    {
        // Klik kiri untuk mulai crafting jika player dekat, belum dirakit, dan tidak sedang dalam proses hitung mundur
        if (playerDekat && !sudahDirakit && !sedangCrafting && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            CekDanMulaiCrafting();
        }
    }

    void CekDanMulaiCrafting()
    {
        if (inventoryPlayer == null) return;

        // Cek kelengkapan bahan
        bool semuaLengkap = true;
        foreach (string bahan in bahanDibutuhkan)
        {
            if (!inventoryPlayer.PunyaItem(bahan))
            {
                semuaLengkap = false;
                break;
            }
        }

        if (semuaLengkap)
        {
            // Mulai proses perakitan berdurasi 5 detik
            StartCoroutine(ProsesCraftingCoroutine());
        }
        else
        {
            Debug.Log($"❌ Bahan untuk membuat {namaAlatMusik} belum lengkap!");
            TampilkanPesan($"❌ Bahan pembuatan {namaAlatMusik} belum lengkap!");
        }
    }

    // Coroutine untuk mengatur jeda waktu 5 detik saat NPC merakit
    IEnumerator ProsesCraftingCoroutine()
    {
        sedangCrafting = true;
        sudahDirakit = true;

        // 1. Nyalakan efek asap tanda mulai merakit
        if (efekAsap != null)
        {
            efekAsap.Play();
        }

        TampilkanPesan($"⏳ Sedang merakit {namaAlatMusik}...");
        if (teksPetunjuk != null) teksPetunjuk.SetActive(false);

        // 2. Tunggu selama 5 detik
        yield return new WaitForSeconds(5f);

        // 3. Setelah 5 detik, matikan efek asap
        if (efekAsap != null)
        {
            efekAsap.Stop();
        }

        // Simpan progres game, badge, dan status selesai
        if (GameProgress.Instance != null)
        {
            GameProgress.Instance.CollectInstrument(namaAlatMusik);
        }

        PlayerPrefs.SetInt("Badge_" + namaAlatMusik, 1);
        PlayerPrefs.Save();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.TandaiSelesai(namaAlatMusik);
        }

        if (MuseumManager.Instance != null)
        {
            MuseumManager.Instance.SelesaiCrafting(namaAlatMusik);
        }

        RoomTimer timer = Object.FindFirstObjectByType<RoomTimer>();
        if (timer != null)
        {
            timer.BerhentiTimer();
        }

        Debug.Log($"🎉 Selamat! {namaAlatMusik} berhasil dirakit!");
        TampilkanPesan($"🎉 {namaAlatMusik} berhasil dirakit!");

        // Hapus bahan dari inventori pemain
        foreach (string bahan in bahanDibutuhkan)
        {
            inventoryPlayer.daftarItem.Remove(bahan);
        }

        // 4. Munculkan alat musik jadinya secara otomatis di scene
        Vector3 titikMuncul = spawnPoint != null ? spawnPoint.position : transform.position + new Vector3(0, 1.2f, 0);
        if (prefabAlatMusikJadi != null)
        {
            Instantiate(prefabAlatMusikJadi, titikMuncul, Quaternion.identity);
        }

        sedangCrafting = false;
    }

    void TampilkanPesan(string pesan)
    {
        if (komponenTeksTMP != null)
        {
            komponenTeksTMP.text = pesan;
        }
        else if (komponenTeksBiasa != null)
        {
            komponenTeksBiasa.text = pesan;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !sudahDirakit)
        {
            playerDekat = true;
            inventoryPlayer = other.GetComponent<PlayerInventory>();

            TampilkanPesan($"Klik Kiri untuk membuat {namaAlatMusik}");
            if (teksPetunjuk != null) teksPetunjuk.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerDekat = false;
            inventoryPlayer = null;

            if (teksPetunjuk != null && !sudahDirakit) teksPetunjuk.SetActive(false);
        }
    }
}