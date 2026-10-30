using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using TMPro;

public class NPCCrafting : MonoBehaviour
{
    [Header("Nama Alat Musik")]
    public string namaAlatMusik = "Sarone";

    [Header("Daftar Bahan yang Dibutuhkan")]
    public List<string> bahanDibutuhkan = new List<string>();

    [Header("Hasil Crafting")]
    public GameObject prefabAlatMusikJadi;
    public Transform spawnPoint;          

    [Header("Pengaturan Ruangan & Progress")]
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

    private bool playerDekat = false;
    private bool sudahDirakit = false;
    private bool sedangCrafting = false; 
    private PlayerInventory inventoryPlayer;

    void Start()
    {
        if (efekAsap != null) efekAsap.Stop();
        TutupSemuaBubble();

        if (panelBadgeUnlock != null) panelBadgeUnlock.SetActive(false);

        if (GameManager.Instance != null && GameManager.Instance.CekSudahSelesai(namaAlatMusik))
        {
            sudahDirakit = true;
            if (kumpulanBahan != null) kumpulanBahan.SetActive(false);
            return;
        }
    }

    void Update()
    {
        // Tekan 'E' untuk mulai atau lanjut dialog saat dekat NPC
        if (playerDekat && !sudahDirakit && !sedangCrafting)
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

    void MulaiDialog()
    {
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

        if (indeksDialog < daftarPercakapan.Length)
        {
            DialogData dialogAktif = daftarPercakapan[indeksDialog];

            if (dialogAktif.apakahPlayerYangNgomong)
            {
                if (bubblePlayer != null) bubblePlayer.SetActive(true);
                if (teksPlayer != null) teksPlayer.text = dialogAktif.isiPesan;
            }
            else
            {
                if (bubbleNPC != null) bubbleNPC.SetActive(true);
                if (teksNPC != null) teksNPC.text = dialogAktif.isiPesan;
            }
        }
    }

    void TutupSemuaBubble()
    {
        if (bubblePlayer != null) bubblePlayer.SetActive(false);
        if (bubbleNPC != null) bubbleNPC.SetActive(false);
    }

    void SelesaiDialogDanCrafting()
    {
        sedangBerdialog = false;
        TutupSemuaBubble();

        CekDanMulaiCrafting();
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
        }
    }

    IEnumerator ProsesCraftingCoroutine()
    {
        sedangCrafting = true;
        sudahDirakit = true;

        if (efekAsap != null) efekAsap.Play();

        // Menjalankan Progress Bar otomatis selama 5 detik
        float durasiCrafting = 5f;
        if (GameProgress.Instance != null)
        {
            GameProgress.Instance.MulaiProgressOtomatis(durasiCrafting);
        }

        yield return new WaitForSeconds(durasiCrafting);

        if (efekAsap != null) efekAsap.Stop();

        if (GameProgress.Instance != null)
        {
            GameProgress.Instance.CollectInstrument(namaAlatMusik);
        }

        PlayerPrefs.SetInt("Badge_" + namaAlatMusik, 1);
        PlayerPrefs.Save();

        if (GameManager.Instance != null) GameManager.Instance.TandaiSelesai(namaAlatMusik);
        if (MuseumManager.Instance != null) MuseumManager.Instance.SelesaiCrafting(namaAlatMusik);

        // Hapus bahan dari inventaris pemain
        foreach (string bahan in bahanDibutuhkan)
        {
            inventoryPlayer.daftarItem.Remove(bahan);
        }

        // Masukkan hasil alat musik jadi ke inventaris pemain & simpan secara permanen
        if (inventoryPlayer != null)
        {
            if (!inventoryPlayer.daftarItem.Contains(namaAlatMusik))
            {
                inventoryPlayer.daftarItem.Add(namaAlatMusik);
            }
            
            // Simpan perubahan ke PlayerPrefs agar aman saat pindah scene
            inventoryPlayer.SimpanInventaris();
            inventoryPlayer.RefreshInventoryUI();
        }

        Vector3 titikMuncul = spawnPoint != null ? spawnPoint.position : transform.position + new Vector3(0, 1.2f, 0);
        if (prefabAlatMusikJadi != null)
        {
            Instantiate(prefabAlatMusikJadi, titikMuncul, Quaternion.identity);
        }

        if (panelBadgeUnlock != null) panelBadgeUnlock.SetActive(true);

        Debug.Log($"🎉 Selamat! {namaAlatMusik} berhasil dirakit!");
        sedangCrafting = false;
    }

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