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
    public TextMeshProUGUI textNamaKiri;   // Tarik teks nama bagian kiri ke sini
    public TextMeshProUGUI textNamaKanan;  // Tarik teks nama bagian kanan ke sini
    public TextMeshProUGUI textIsiDialog;  // Tarik teks isi dialog ke sini
    public Image imageKiri;                // Tarik UI Image karakter kiri
    public Image imageKanan;               // Tarik UI Image karakter kanan

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
                // Klik 1: Hentikan animasi ketik, tampilkan teks secara penuh
                StopAllCoroutines();
                textIsiDialog.text = kalimatSaatIni;
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

        // Atur Posisi Nama, Tampilan, dan Alignment Teks Berdasarkan Siapa yang Ngobrol
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

            // Teks dialog ikut rata kiri saat karakter kiri berbicara
            if (textIsiDialog != null)
            {
                textIsiDialog.alignment = TextAlignmentOptions.Left;
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

            // Teks dialog ikut rata kanan saat karakter kanan berbicara (bisa diubah ke Left jika ingin tetap rata kiri)
            if (textIsiDialog != null)
            {
                textIsiDialog.alignment = TextAlignmentOptions.Right;
            }
        }

        // Atur Gambar Sisi Kiri
        if (imageKiri != null && item.spriteKiri != null)
        {
            imageKiri.gameObject.SetActive(true);
            imageKiri.sprite = item.spriteKiri;
        }

        // Atur Gambar Sisi Kanan
        if (imageKanan != null && item.spriteKanan != null)
        {
            imageKanan.gameObject.SetActive(true);
            imageKanan.sprite = item.spriteKanan;
        }

        StartCoroutine(KetikTeks(kalimatSaatIni));
    }

    IEnumerator KetikTeks(string kalimat)
    {
        textIsiDialog.text = "";
        sedangMengetik = true;
        bolehLanjut = false;

        foreach (char huruf in kalimat.ToCharArray())
        {
            textIsiDialog.text += huruf;
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