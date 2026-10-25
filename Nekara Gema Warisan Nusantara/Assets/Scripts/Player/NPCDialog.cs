using UnityEngine;
using TMPro;

public class NPCDialog : MonoBehaviour
{
    [Header("Referensi Bubble Dialog")]
    public GameObject bubblePlayer;     // Tarik objek TandaBicara milik Player di sini
    public GameObject bubbleNPC;        // Tarik objek TandaBicara milik NPC di sini

    public TMP_Text teksPlayer;         // Komponen TextMeshPro di bubble Player
    public TMP_Text teksNPC;            // Komponen TextMeshPro di bubble NPC

    [System.Serializable]
    public struct DialogData
    {
        public bool apakahPlayerYangNgomong; // Centang jika Player yang ngomong, kosongkan jika NPC
        [TextArea(2, 4)]
        public string isiPesan;
    }

    public DialogData[] daftarPercakapan; 
    private int indeksDialog = 0;

    [Header("Referensi UI Crafting / Progress")]
    public GameObject panelCraftingAtauBadge; // Panel yang muncul setelah obrolan selesai

    private bool playerDekat = false;
    private bool sedangBerdialog = false;

    void Start()
    {
        TutupSemuaBubble();
        if (panelCraftingAtauBadge != null) panelCraftingAtauBadge.SetActive(false);
    }

    void Update()
    {
        if (playerDekat && Input.GetKeyDown(KeyCode.E))
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
                // Munculkan bubble di atas kepala Player
                if (bubblePlayer != null) bubblePlayer.SetActive(true);
                if (teksPlayer != null) teksPlayer.text = dialogAktif.isiPesan;
            }
            else
            {
                // Munculkan bubble di atas kepala NPC
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

        // Buka panel crafting/badge setelah obrolan selesai
        if (panelCraftingAtauBadge != null)
        {
            panelCraftingAtauBadge.SetActive(true);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerDekat = true;
            Debug.Log("Tekan 'E' untuk berbicara.");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerDekat = false;
            sedangBerdialog = false;
            TutupSemuaBubble();
        }
    }
}