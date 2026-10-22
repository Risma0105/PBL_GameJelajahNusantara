using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using TMPro;

public class NPCCrafting : MonoBehaviour
{
    [Header("Nama Alat Musik")]
    public string namaAlatMusik = "Sarone"; // Sesuaikan nama alat musiknya

    [Header("Daftar Bahan yang Dibutuhkan")]
    public List<string> bahanDibutuhkan = new List<string>();

    [Header("Hasil Crafting")]
    public GameObject prefabAlatMusikJadi; // Prefab fisik alat musik yang akan muncul di map
    public Transform spawnPoint;          

    [Header("Pengaturan Ruangan & Progress")]
    public GameObject kumpulanBahan; // Drag objek parent pembungkus semua bahan di scene

    [Header("Efek Visual Crafting")]
    public ParticleSystem efekAsap; // Tarik objek Particle System ke sini

    [Header("UI Feedback & Overlay")]
    public GameObject teksPetunjuk;              
    public TMP_Text komponenTeksTMP;             
    public UnityEngine.UI.Text komponenTeksBiasa; 
    public GameObject panelBadgeUnlock; // TARIK Panel_BadgeUnlock full layar ke sini di Inspector!

    private bool playerDekat = false;
    private bool sudahDirakit = false;
    private bool sedangCrafting = false; 
    private PlayerInventory inventoryPlayer;

    void Start()
    {
        if (efekAsap != null) efekAsap.Stop();

        // Pastikan panel badge mati di awal game
        if (panelBadgeUnlock != null) panelBadgeUnlock.SetActive(false);

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
        if (playerDekat && !sudahDirakit && !sedangCrafting && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            CekDanMulaiCrafting();
        }
    }

    void CekDanMulaiCrafting()
    {
        if (inventoryPlayer == null) return;

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
            StartCoroutine(ProsesCraftingCoroutine());
        }
        else
        {
            Debug.Log($"❌ Bahan untuk membuat {namaAlatMusik} belum lengkap!");
            TampilkanPesan($"❌ Bahan pembuatan {namaAlatMusik} belum lengkap!");
        }
    }

    IEnumerator ProsesCraftingCoroutine()
    {
        sedangCrafting = true;
        sudahDirakit = true;

        if (efekAsap != null)
        {
            efekAsap.Play();
        }

        TampilkanPesan($"⏳ Sedang merakit {namaAlatMusik}...");
        if (teksPetunjuk != null) teksPetunjuk.SetActive(false);

        yield return new WaitForSeconds(5f);

        if (efekAsap != null)
        {
            efekAsap.Stop();
        }

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

        // 1. Hapus bahan dari inventori pemain
        foreach (string bahan in bahanDibutuhkan)
        {
            inventoryPlayer.daftarItem.Remove(bahan);
        }

        // 2. Segarkan tampilan UI inventory bahan agar kosong kembali
        if (inventoryPlayer != null)
        {
            inventoryPlayer.RefreshInventoryUI();
        }

        // 3. Munculkan fisik alat musik jadi di map (pemain harus mendekat dan mengkliknya untuk mengambil)
        Vector3 titikMuncul = spawnPoint != null ? spawnPoint.position : transform.position + new Vector3(0, 1.2f, 0);
        if (prefabAlatMusikJadi != null)
        {
            Instantiate(prefabAlatMusikJadi, titikMuncul, Quaternion.identity);
        }

        // 4. MUNCULKAN OVERLAY BADGE UNLOCK FULL LAYAR DI SINI!
        if (panelBadgeUnlock != null)
        {
            panelBadgeUnlock.SetActive(true);
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