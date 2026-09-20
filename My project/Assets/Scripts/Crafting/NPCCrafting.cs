using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro; // Wajib jika memakai TextMeshPro untuk UI teks

public class NPCCrafting : MonoBehaviour
{
    [Header("Nama Alat Musik")]
    public string namaAlatMusik = "Sarone"; // Sesuaikan nama alat musiknya

    [Header("Daftar Bahan yang Dibutuhkan")]
    public List<string> bahanDibutuhkan = new List<string>();

    [Header("Hasil Crafting")]
    public GameObject prefabAlatMusikJadi; 
    public Transform spawnPoint;           

    [Header("UI Feedback")]
    public GameObject teksPetunjuk;       // Objek pembungkus teks (muncul saat player dekat)
    public TMP_Text komponenTeksTMP;      // Komponen TextMeshPro (jika pakai TMP)
    public UnityEngine.UI.Text komponenTeksBiasa; // Komponen UI Text biasa (jika pakai Legacy Text)

    private bool playerDekat = false;
    private bool sudahDirakit = false;
    private PlayerInventory inventoryPlayer;

    void Start()
    {
        // Set pesan awal interaksi saat game mulai
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

            // Tampilkan kembali pesan siap berinteraksi
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