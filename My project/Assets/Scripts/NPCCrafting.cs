using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class NPCCrafting : MonoBehaviour
{
    [Header("Nama Alat Musik")]
    public string namaAlatMusik = "Angklung";

    [Header("Daftar Bahan yang Dibutuhkan")]
    public List<string> bahanDibutuhkan = new List<string>();

    [Header("Hasil Crafting")]
    public GameObject prefabAlatMusikJadi; // Objek angklung yang akan muncul
    public Transform spawnPoint;           // Posisi munculnya angklung

    [Header("UI Feedback")]
    public GameObject teksPetunjuk;

    private bool playerDekat = false;
    private bool sudahDirakit = false;
    private PlayerInventory inventoryPlayer;

    void Update()
    {
        if (playerDekat && !sudahDirakit && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
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

            // Hapus bahan dari inventory player
            foreach (string bahan in bahanDibutuhkan)
            {
                inventoryPlayer.daftarItem.Remove(bahan);
            }

            // Munculkan alat musik angklung di scene
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
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !sudahDirakit)
        {
            playerDekat = true;
            inventoryPlayer = other.GetComponent<PlayerInventory>();
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