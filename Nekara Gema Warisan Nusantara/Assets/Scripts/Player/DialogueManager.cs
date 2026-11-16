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
                textIsiDialog.maxVisibleCharacters = kalimatSaatIni.Length;

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

        // Atur Posisi Nama & Alignment Teks (Top-Left untuk Kiri, Top-Right untuk Kanan)
        if (item.siapaYangNgobrol == DialogItem.PosisiSpeaker.Kiri)
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

        int totalKarakter = kalimatSaatIni.Length;
        textIsiDialog.maxVisibleCharacters = 0; 

        for (int i = 0; i <= totalKarakter; i++)
        {
            textIsiDialog.maxVisibleCharacters = i;
            yield return new WaitForSeconds(kecepatanKetik);
        }

        sedangMengetik = false;
        bolehLanjut = true;
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
            onDialogSelesai.Invoke();
        }
    }
}