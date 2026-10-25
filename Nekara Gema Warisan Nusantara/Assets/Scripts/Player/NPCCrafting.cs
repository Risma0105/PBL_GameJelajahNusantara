using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
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

    [Header("Referensi Bubble Dialog Crafting")]
    public GameObject bubblePlayer;     // Tarik TandaBicara Player
    public GameObject bubbleNPC;        // Tarik TandaBicara NPC Crafting ini
    public TMP_Text teksPlayer;         // Komponen Text TMP di bubble Player
    public TMP_Text teksNPC;            // Komponen Text TMP di bubble NPC

    [System.Serializable]
    public struct DialogData
    {
        public bool apakahPlayerYangNgomong;
        [TextArea(2, 4)]
        public string isiPesan;
    }

    [Header("Alur Obrolan Sebelum Crafting")]
    public DialogData[] daftarPercakapan; 
    private int indeksDialog = 0;
    private bool sedangBerdialog = false;

    [Header("UI Feedback & Overlay")]
    public GameObject teksPetunjuk;              
    public TMP_Text komponenTeksTMP;             
    public UnityEngine.UI.Text komponenTeksBiasa; 
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
            TampilkanPesan($"{namaAlatMusik} sudah selesai dibuat!");
            return;
        }
    }

    void Update()
    {
        // Tekan 'E' untuk memulai atau melanjutkan dialog
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

            // Klik kiri untuk merakit setelah dialog selesai
            if (!sedangBerdialog && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                CekDanMulaiCrafting();
            }
        }
    }

    void MulaiDialog()
    {
        sedangBerdialog = true;
        indeksDialog = 0;
        if (teksPetunjuk != null) teksPetunjuk.SetActive(false);
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
            SelesaiDialog();
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

    void SelesaiDialog()
    {
        sedangBerdialog = false;
        TutupSemuaBubble();

        // Setelah ngobrol beres, kasih tahu player buat klik kiri merakit
        TampilkanPesan($"Klik Kiri untuk merakit {namaAlatMusik}");
        if (teksPetunjuk != null) teksPetunjuk.SetActive(true);
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

        if (efekAsap != null) efekAsap.Play();

        TampilkanPesan($"⏳ Sedang merakit {namaAlatMusik}...");
        if (teksPetunjuk != null) teksPetunjuk.SetActive(false);

        yield return new WaitForSeconds(5f);

        if (efekAsap != null) efekAsap.Stop();

        if (GameProgress.Instance != null)
        {
            GameProgress.Instance.CollectInstrument(namaAlatMusik);
        }

        PlayerPrefs.SetInt("Badge_" + namaAlatMusik, 1);
        PlayerPrefs.Save();

        if (GameManager.Instance != null) GameManager.Instance.TandaiSelesai(namaAlatMusik);
        if (MuseumManager.Instance != null) MuseumManager.Instance.SelesaiCrafting(namaAlatMusik);

        RoomTimer timer = Object.FindFirstObjectByType<RoomTimer>();
        if (timer != null) timer.BerhentiTimer();

        Debug.Log($"🎉 Selamat! {namaAlatMusik} berhasil dirakit!");
        TampilkanPesan($"🎉 {namaAlatMusik} berhasil dirakit!");

        foreach (string bahan in bahanDibutuhkan)
        {
            inventoryPlayer.daftarItem.Remove(bahan);
        }

        if (inventoryPlayer != null) inventoryPlayer.RefreshInventoryUI();

        Vector3 titikMuncul = spawnPoint != null ? spawnPoint.position : transform.position + new Vector3(0, 1.2f, 0);
        if (prefabAlatMusikJadi != null)
        {
            Instantiate(prefabAlatMusikJadi, titikMuncul, Quaternion.identity);
        }

        if (panelBadgeUnlock != null) panelBadgeUnlock.SetActive(true);

        sedangCrafting = false;
    }

    void TampilkanPesan(string pesan)
    {
        if (komponenTeksTMP != null) komponenTeksTMP.text = pesan;
        else if (komponenTeksBiasa != null) komponenTeksBiasa.text = pesan;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !sudahDirakit)
        {
            playerDekat = true;
            inventoryPlayer = other.GetComponent<PlayerInventory>();

            TampilkanPesan($"Tekan 'E' untuk berbicara dengan perajin");
            if (teksPetunjuk != null) teksPetunjuk.SetActive(true);
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

            if (teksPetunjuk != null && !sudahDirakit) teksPetunjuk.SetActive(false);
        }
    }
}