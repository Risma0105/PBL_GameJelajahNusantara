using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public struct DialogLine
{
    [Header("Nama & Isi Percakapan")]
    public string namaKarakter;
    [TextArea(2, 4)]
    public string isiDialog;

    [Header("Pengaturan Karakter (Sprite Samping)")]
    public Sprite spriteKiri;  // Karakter di sisi kiri layar
    public Sprite spriteKanan; // Karakter di sisi kanan layar
}

public class PrologueDialogSystem : MonoBehaviour
{
    [Header("UI Komponen")]
    public TextMeshProUGUI textNama;
    public TextMeshProUGUI textIsiDialog;
    public Image imageKiri;   // UI Image untuk karakter kiri
    public Image imageKanan;  // UI Image untuk karakter kanan

    [Header("Daftar Dialog Prolog (Atur di Inspector)")]
    public List<DialogLine> barisDialogList = new List<DialogLine>();

    [Header("Pengaturan Ketik & Scene Tujuan")]
    public float kecepatanKetik = 0.03f;
    public string namaSceneTujuan = "SceneMuseum_Dalam";

    private int indexAktif = 0;
    private bool sedangMengetik = false;
    private string kalimatFull = "";

    void Start()
    {
        if (barisDialogList.Count > 0)
        {
            TampilkanDialog(indexAktif);
        }
    }

    void Update()
    {
        // Klik kiri atau tekan Spasi untuk lanjut / skip ketik
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            if (sedangMengetik)
            {
                StopAllCoroutines();
                textIsiDialog.text = kalimatFull;
                sedangMengetik = false;
            }
            else
            {
                LanjutKeBerikutnya();
            }
        }
    }

    void TampilkanDialog(int index)
    {
        DialogLine line = barisDialogList[index];

        textNama.text = line.namaKarakter;
        kalimatFull = line.isiDialog;

        // Atur Sprite Karakter Kiri
        if (line.spriteKiri != null)
        {
            imageKiri.gameObject.SetActive(true);
            imageKiri.sprite = line.spriteKiri;
        }
        else
        {
            imageKiri.gameObject.SetActive(false); // Sembunyikan jika tidak ada
        }

        // Atur Sprite Karakter Kanan
        if (line.spriteKanan != null)
        {
            imageKanan.gameObject.SetActive(true);
            imageKanan.sprite = line.spriteKanan;
        }
        else
        {
            imageKanan.gameObject.SetActive(false);
        }

        StartCoroutine(KetikEfek(kalimatFull));
    }

    IEnumerator KetikEfek(string teks)
    {
        textIsiDialog.text = "";
        sedangMengetik = true;

        foreach (char huruf in teks.ToCharArray())
        {
            textIsiDialog.text += huruf;
            yield return new WaitForSeconds(kecepatanKetik);
        }

        sedangMengetik = false;
    }

    void LanjutKeBerikutnya()
    {
        indexAktif++;
        if (indexAktif < barisDialogList.Count)
        {
            TampilkanDialog(indexAktif);
        }
    }
}