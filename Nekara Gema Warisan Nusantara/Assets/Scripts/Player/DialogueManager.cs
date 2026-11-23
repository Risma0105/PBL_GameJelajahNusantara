using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

[System.Serializable]
public struct DialogItem
{
    public enum PosisiSpeaker { Kiri, Kanan }
    
    [Header("Siapa yang Berbicara?")]
    public PosisiSpeaker siapaYangNgobrol;

    [Header("Identitas & Percakapan")]
    public string namaKarakter;
    [TextArea(3, 10)]
    public string isiDialog;

    [Header("Sprite Karakter (Opsional)")]
    public Sprite spriteKiri;
    public Sprite spriteKanan;
}

public class DialogueManager : MonoBehaviour
{
    [Header("UI Komponen Nama & Dialog")]
    public TextMeshProUGUI textNamaKiri;   
    public TextMeshProUGUI textNamaKanan;  
    public TextMeshProUGUI textIsiDialog;  
    public Image imageKiri;                
    public Image imageKanan;               

    [Header("Efek Portrait (PortraitSpeakFX)")]
    [Tooltip("Portrait yang posisinya di KIRI layar")]
    public PortraitSpeakFX fxKiri;
    [Tooltip("Portrait yang posisinya di KANAN layar")]
    public PortraitSpeakFX fxKanan;

    [Header("Daftar Percakapan (Atur di Inspector)")]
    public List<DialogItem> daftarPercakapan = new List<DialogItem>();

    [Header("Pengaturan Ketik")]
    public float kecepatanKetik = 0.03f;

    [Header("Event Ketika Dialog Selesai")]
    public UnityEvent onDialogSelesai;

    private int indexDialog = 0;
    private string kalimatSaatIni = "";
    private bool sedangMengetik = false;
    private bool bolehLanjut = false;
    private PortraitSpeakFX pembicaraAktif;

    void Awake()
    {
        // kalau fx belum di-drag, coba ambil dari imageKiri / imageKanan
        if (!fxKiri && imageKiri) fxKiri = imageKiri.GetComponent<PortraitSpeakFX>();
        if (!fxKanan && imageKanan) fxKanan = imageKanan.GetComponent<PortraitSpeakFX>();
    }

    void Start()
    {
        if (daftarPercakapan.Count > 0)
        {
            MulaiDialogBaru();
        }
    }

    void Update()
    {
        // Mendeteksi klik mouse / tap layar / tombol Spasi
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            if (sedangMengetik)
            {
                // Klik 1: Hentikan animasi ketik, tampilkan seluruh teks secara penuh seketika
                StopAllCoroutines();
                textIsiDialog.maxVisibleCharacters = int.MaxValue;

                // Pastikan alignment tetap Top-Left atau Top-Right saat di-skip
                DialogItem item = daftarPercakapan[indexDialog];
                if (textIsiDialog != null)
                {
                    if (item.siapaYangNgobrol == DialogItem.PosisiSpeaker.Kiri)
                        textIsiDialog.alignment = TextAlignmentOptions.TopLeft;
                    else
                        textIsiDialog.alignment = TextAlignmentOptions.TopRight;
                }

                sedangMengetik = false;
                bolehLanjut = true;
                if (pembicaraAktif) pembicaraAktif.SetTalking(false);
            }
            else if (bolehLanjut)
            {
                // Klik 2: Lanjut ke dialog berikutnya
                bolehLanjut = false;
                LanjutDialog();
            }
        }
    }

    void MulaiDialogBaru()
    {
        DialogItem item = daftarPercakapan[indexDialog];
        kalimatSaatIni = item.isiDialog;

        bool kiriBicara = item.siapaYangNgobrol == DialogItem.PosisiSpeaker.Kiri;

        // Atur Posisi Nama & Alignment Teks (Top-Left untuk Kiri, Top-Right untuk Kanan)
        if (kiriBicara)
        {
            if (textNamaKiri != null) 
            { 
                textNamaKiri.gameObject.SetActive(true); 
                textNamaKiri.text = item.namaKarakter; 
            }
            if (textNamaKanan != null) 
            { 
                textNamaKanan.gameObject.SetActive(false); 
            }

            if (textIsiDialog != null)
            {
                textIsiDialog.alignment = TextAlignmentOptions.TopLeft;
            }
        }
        else
        {
            if (textNamaKanan != null) 
            { 
                textNamaKanan.gameObject.SetActive(true); 
                textNamaKanan.text = item.namaKarakter; 
            }
            if (textNamaKiri != null) 
            { 
                textNamaKiri.gameObject.SetActive(false); 
            }

            if (textIsiDialog != null)
            {
                textIsiDialog.alignment = TextAlignmentOptions.TopRight;
            }
        }

        // Atur Gambar Sisi Kiri
        if (imageKiri != null && item.spriteKiri != null)
        {
            imageKiri.gameObject.SetActive(true);
            imageKiri.sprite = item.spriteKiri;
        }
        else if (imageKiri != null)
        {
            imageKiri.gameObject.SetActive(false);
        }

        // Atur Gambar Sisi Kanan
        if (imageKanan != null && item.spriteKanan != null)
        {
            imageKanan.gameObject.SetActive(true);
            imageKanan.sprite = item.spriteKanan;
        }
        else if (imageKanan != null)
        {
            imageKanan.gameObject.SetActive(false);
        }

        // Sorot portrait yang lagi bicara (yang diem jadi redup)
        if (fxKiri) fxKiri.SetSpeaking(kiriBicara);
        if (fxKanan) fxKanan.SetSpeaking(!kiriBicara);
        pembicaraAktif = kiriBicara ? fxKiri : fxKanan;

        // Masukkan teks penuh ke TextMeshPro, lalu jalankan animasi ketik
        if (textIsiDialog != null)
        {
            textIsiDialog.text = kalimatSaatIni;
            StartCoroutine(KetikTeks());
        }
    }

    IEnumerator KetikTeks()
    {
        sedangMengetik = true;
        bolehLanjut = false;
        if (pembicaraAktif) pembicaraAktif.SetTalking(true);

        // hitung karakter yang benar-benar tampil (tag rich text nggak ikut dihitung)
        textIsiDialog.maxVisibleCharacters = 0; 
        textIsiDialog.ForceMeshUpdate();
        int totalKarakter = textIsiDialog.textInfo.characterCount;

        for (int i = 0; i <= totalKarakter; i++)
        {
            textIsiDialog.maxVisibleCharacters = i;
            yield return new WaitForSeconds(kecepatanKetik);
        }

        sedangMengetik = false;
        bolehLanjut = true;
        if (pembicaraAktif) pembicaraAktif.SetTalking(false);
    }

    public void LanjutDialog()
    {
        indexDialog++;

        if (indexDialog < daftarPercakapan.Count)
        {
            MulaiDialogBaru();
        }
        else
        {
            Debug.Log("Dialog Selesai!");
            if (fxKiri) fxKiri.SetSpeaking(false);
            if (fxKanan) fxKanan.SetSpeaking(false);
            onDialogSelesai.Invoke();
        }
    }
}