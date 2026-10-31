
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class NPCCrafting : MonoBehaviour
{
    [Header("Nama Alat Musik")]
    public string namaAlatMusik = "Angklung";

    [Header("Daftar Bahan yang Dibutuhkan")]
    public List<string> bahanDibutuhkan = new List<string>();

    [Header("Hasil Crafting")]
    public GameObject prefabAlatMusikJadi;
    public Transform spawnPoint;

    [Header("Pengaturan Ruangan")]
    public GameObject kumpulanBahan;

    [Header("Efek Visual Crafting")]
    public ParticleSystem efekAsap;

    [Header("Referensi Bubble Dialog")]
    public GameObject bubblePlayer;
    public GameObject bubbleNPC;
    public TMP_Text teksPlayer;
    public TMP_Text teksNPC;

    [System.Serializable]
    public struct DialogData
    {
        public bool apakahPlayerYangNgomong;

        [TextArea(2, 4)]
        public string isiPesan;
    }

    [Header("Daftar Percakapan")]
    public DialogData[] daftarPercakapan;

    private int indeksDialog = 0;
    private bool sedangBerdialog = false;

    [Header("Panel Badge / UI Tambahan")]
    public GameObject panelBadgeUnlock;

    [Header("Durasi Crafting")]
    public float durasiCrafting = 5f;

    private bool playerDekat = false;
    private bool sudahDirakit = false;
    private bool sedangCrafting = false;
    private bool hasilSudahMuncul = false;
    private bool panelBadgeTerbuka = false;

    private PlayerInventory inventoryPlayer;

    // =====================================
    // START
    // =====================================

    void Start()
    {
        if (efekAsap != null)
            efekAsap.Stop();

        TutupSemuaBubble();

        if (panelBadgeUnlock != null)
            panelBadgeUnlock.SetActive(false);

        // Cek apakah alat musik sudah selesai sebelumnya
        if (GameManager.Instance != null &&
            GameManager.Instance.CekSudahSelesai(namaAlatMusik))
        {
            sudahDirakit = true;

            if (kumpulanBahan != null)
                kumpulanBahan.SetActive(false);

            Debug.Log(namaAlatMusik + " sudah selesai sebelumnya.");
        }
    }

    // =====================================
    // UPDATE
    // =====================================

    void Update()
    {
        if (playerDekat &&
            !sudahDirakit &&
            !sedangCrafting &&
            !hasilSudahMuncul &&
            !panelBadgeTerbuka)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                if (!sedangBerdialog)
                {
                    MulaiDialog();
                }
                else
                {
                    LanjutDialog();
                }
            }
        }
    }

    // =====================================
    // SISTEM DIALOG
    // =====================================

    void MulaiDialog()
    {
        if (daftarPercakapan == null ||
            daftarPercakapan.Length == 0)
        {
            SelesaiDialogDanCrafting();
            return;
        }

        sedangBerdialog = true;
        indeksDialog = 0;

        TampilkanPesanSaatIni();
    }

    void LanjutDialog()
    {
        indeksDialog++;

        if (indeksDialog < daftarPercakapan.Length)
        {
            TampilkanPesanSaatIni();
        }
        else
        {
            SelesaiDialogDanCrafting();
        }
    }

    void TampilkanPesanSaatIni()
    {
        TutupSemuaBubble();

        if (indeksDialog >= daftarPercakapan.Length)
            return;

        DialogData dialogAktif = daftarPercakapan[indeksDialog];

        if (dialogAktif.apakahPlayerYangNgomong)
        {
            if (bubblePlayer != null)
                bubblePlayer.SetActive(true);

            if (teksPlayer != null)
                teksPlayer.text = dialogAktif.isiPesan;
        }
        else
        {
            if (bubbleNPC != null)
                bubbleNPC.SetActive(true);

            if (teksNPC != null)
                teksNPC.text = dialogAktif.isiPesan;
        }
    }

    void TutupSemuaBubble()
    {
        if (bubblePlayer != null)
            bubblePlayer.SetActive(false);

        if (bubbleNPC != null)
            bubbleNPC.SetActive(false);
    }

    void SelesaiDialogDanCrafting()
    {
        sedangBerdialog = false;
        TutupSemuaBubble();

        CekDanMulaiCrafting();
    }

    // =====================================
    // CEK BAHAN INVENTORY
    // =====================================

    void CekDanMulaiCrafting()
    {
        if (inventoryPlayer == null)
            return;

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
            Debug.Log(
                "Bahan untuk membuat " +
                namaAlatMusik +
                " belum lengkap!"
            );
        }
    }

    // =====================================
    // PROSES CRAFTING
    // =====================================

    IEnumerator ProsesCraftingCoroutine()
    {
        sedangCrafting = true;

        if (efekAsap != null)
            efekAsap.Play();

        Debug.Log("Crafting " + namaAlatMusik + " dimulai!");

        // Progress otomatis crafting jika masih digunakan
        if (GameProgress.Instance != null)
        {
            GameProgress.Instance.MulaiProgressOtomatis(durasiCrafting);
        }

        yield return new WaitForSeconds(durasiCrafting);

        if (efekAsap != null)
            efekAsap.Stop();

        // Hapus bahan mentah dari inventory
        if (inventoryPlayer != null)
        {
            foreach (string bahan in bahanDibutuhkan)
            {
                inventoryPlayer.daftarItem.Remove(bahan);
            }

            inventoryPlayer.SimpanInventaris();
            inventoryPlayer.RefreshInventoryUI();
        }

        // Tentukan posisi spawn alat musik
        Vector3 titikMuncul = spawnPoint != null
            ? spawnPoint.position
            : transform.position + new Vector3(0f, 1.2f, 0f);

        // Spawn prefab alat musik
        if (prefabAlatMusikJadi != null)
        {
            GameObject objekHasil = Instantiate(
                prefabAlatMusikJadi,
                titikMuncul,
                Quaternion.identity
            );

            // Hubungkan CollectibleItem dengan NPC
            CollectibleItem collectible =
                objekHasil.GetComponent<CollectibleItem>();

            if (collectible != null)
            {
                collectible.namaItem = namaAlatMusik;
                collectible.npcCrafting = this;
            }
            else
            {
                Debug.LogError(
                    "Prefab " + prefabAlatMusikJadi.name +
                    " belum memiliki CollectibleItem!"
                );
            }

            hasilSudahMuncul = true;
        }
        else
        {
            Debug.LogError("Prefab alat musik belum diisi!");
        }

        // Tampilkan panel badge setelah crafting selesai
        if (panelBadgeUnlock != null)
        {
            panelBadgeTerbuka = true;
            panelBadgeUnlock.SetActive(true);
        }

        sedangCrafting = false;

        Debug.Log(
            "Crafting selesai! " +
            namaAlatMusik +
            " muncul. Tutup panel badge untuk mengambil alat musik."
        );
    }

    // =====================================
    // TOMBOL CLOSE PANEL BADGE
    // =====================================

    public void TutupPanelBadge()
    {
        if (panelBadgeUnlock != null)
            panelBadgeUnlock.SetActive(false);

        panelBadgeTerbuka = false;

        Debug.Log("Panel badge ditutup. Silakan ambil alat musik!");
    }

    // =====================================
    // KONFIRMASI SETELAH ALAT DIAMBIL
    // =====================================

    public void KonfirmasiAlatDiambil()
    {
        if (sudahDirakit)
            return;

        sudahDirakit = true;

        // Update progress checkpoint
        if (GameProgress.Instance != null)
        {
            GameProgress.Instance.CollectInstrument(namaAlatMusik);
        }

        // Simpan badge
        PlayerPrefs.SetInt("Badge_" + namaAlatMusik, 1);
        PlayerPrefs.Save();

        // Tandai selesai di GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.TandaiSelesai(namaAlatMusik);
        }

        // Tandai selesai di MuseumManager
        if (MuseumManager.Instance != null)
        {
            MuseumManager.Instance.SelesaiCrafting(namaAlatMusik);
        }

        // Sembunyikan kumpulan bahan
        if (kumpulanBahan != null)
            kumpulanBahan.SetActive(false);

        Debug.Log(
            "Alat musik " + namaAlatMusik +
            " berhasil diambil! Checkpoint selesai."
        );
    }

    // =====================================
    // DETEKSI PLAYER
    // =====================================

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !sudahDirakit)
        {
            playerDekat = true;
            inventoryPlayer = other.GetComponent<PlayerInventory>();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerDekat = false;
            inventoryPlayer = null;

            sedangBerdialog = false;
            TutupSemuaBubble();
        }
    }
}
