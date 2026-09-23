using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro; // Wajib jika memakai TextMeshPro untuk UI teks

public class NPCCrafting : MonoBehaviour
{
    [Header("Nama Alat Musik")]
    public string namaAlatMusik = "Sarone"; // Sesuaikan nama alat musiknya (contoh: Angklung, Sarone, GesoGeso)

    [Header("Daftar Bahan yang Dibutuhkan")]
    public List<string> bahanDibutuhkan = new List<string>();

    [Header("Hasil Crafting")]
    public GameObject prefabAlatMusikJadi; 
    public Transform spawnPoint;           

    [Header("Pengaturan Ruangan & Progress")]
    public GameObject kumpulanBahan; // Drag objek parent pembungkus semua bahan di scene

    [Header("UI Feedback")]
    public GameObject teksPetunjuk;               // Objek pembungkus teks (muncul saat player dekat)
    public TMP_Text komponenTeksTMP;             // Komponen TextMeshPro
    public UnityEngine.UI.Text komponenTeksBiasa; // Komponen UI Text biasa (Legacy)

    private bool playerDekat = false;
    private bool sudahDirakit = false;
    private PlayerInventory inventoryPlayer;

    void Start()
    {
        // Cek apakah ruangan/alat musik ini sudah pernah diselesaikan sebelumnya
        if (GameManager.Instance != null && GameManager.Instance.CekSudahSelesai(namaAlatMusik))
        {
            sudahDirakit = true;
            if (kumpulanBahan != null) kumpulanBahan.SetActive(false); // Hilangkan bahan
            TampilkanPesan($"{namaAlatMusik} sudah selesai dibuat!");
            return;
        }

        // Pesan awal
        TampilkanPesan($"Klik Kiri untuk membuat {namaAlatMusik}");
    }

    void Update()
    {
        if (playerDekat && !sudahDirakit && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            CobaCrafting();
        }
    }

    void CobaCrafting()
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
            sudahDirakit = true;

            // Buka badge otomatis ke memori game
            PlayerPrefs.SetInt("Badge_" + namaAlatMusik, 1);
            PlayerPrefs.Save();

            // 1. Simpan status selesai ke GameManager
            if (GameManager.Instance != null)
            {
                GameManager.Instance.TandaiSelesai(namaAlatMusik);
            }

            // 2. Hentikan timer ruangan jika ada
            RoomTimer timer = Object.FindFirstObjectByType<RoomTimer>();
            if (timer != null)
            {
                timer.BerhentiTimer();
            }

            Debug.Log($"🎉 Selamat! {namaAlatMusik} berhasil dirakit!");
            TampilkanPesan($"🎉 {namaAlatMusik} berhasil dirakit!");

            // Hapus bahan dari inventory player
            foreach (string bahan in bahanDibutuhkan)
            {
                inventoryPlayer.daftarItem.Remove(bahan);
            }

            // Munculkan alat musik di scene
            Vector3 titikMuncul = spawnPoint != null ? spawnPoint.position : transform.position + new Vector3(0, 1.2f, 0);
            if (prefabAlatMusikJadi != null)
            {
                Instantiate(prefabAlatMusikJadi, titikMuncul, Quaternion.identity);
            }

            if (teksPetunjuk != null) teksPetunjuk.SetActive(false);
        }
        else
        {
            Debug.Log($"❌ Bahan untuk membuat {namaAlatMusik} belum lengkap!");
            TampilkanPesan($"❌ Bahan pembuatan {namaAlatMusik} belum lengkap!");
        }
    }

    // Fungsi pembantu untuk mengubah isi teks secara dinamis
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
            if (teksPetunjuk != null) teksPetunjuk.SetActive(false);
        }
    }
}