using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

public enum PosisiPembicara
{
    Otomatis, // cocokin namaKarakter dengan namaKarakterKiri / namaKarakterKanan
    Kiri,
    Kanan,
    Narator   // nggak ada portrait yang disorot
}

[System.Serializable]
public struct DialogLine
{
    [Header("Nama & Isi Percakapan")]
    public string namaKarakter;
    [TextArea(2, 4)]
    public string isiDialog;

    [Header("Siapa yang bicara")]
    public PosisiPembicara pembicara;

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

    [Header("Efek Portrait (PortraitSpeakFX)")]
    [Tooltip("Kosongin = otomatis diambil dari imageKiri")]
    public PortraitSpeakFX fxKiri;
    [Tooltip("Kosongin = otomatis diambil dari imageKanan")]
    public PortraitSpeakFX fxKanan;

    [Header("Fokus UI (vignette + instruksi klik)")]
    public PrologFokusUI fokusUI;

    [Header("Deteksi Pembicara Otomatis")]
    public string namaKarakterKiri = "Aki";
    public string namaKarakterKanan = "Ujang";

    [Header("Daftar Dialog Prolog (Atur di Inspector)")]
    public List<DialogLine> barisDialogList = new List<DialogLine>();

    [Header("Pengaturan Ketik & Scene Tujuan")]
    public float kecepatanKetik = 0.03f;
    public string namaSceneTujuan = "SceneMuseum_Dalam"; // belum dipakai di sini, pindah scene lewat onDialogSelesai

    [Header("Setelah dialog terakhir")]
    [Tooltip("Drag PrologueManager ke sini, pilih LanjutKeMuseum()")]
    public UnityEvent onDialogSelesai;

    private int indexAktif = 0;
    private bool sedangMengetik = false;
    private bool dialogSelesai = false;
    private string kalimatFull = "";
    private PortraitSpeakFX pembicaraAktif;

    void Awake()
    {
        if (!fxKiri && imageKiri) fxKiri = imageKiri.GetComponent<PortraitSpeakFX>();
        if (!fxKanan && imageKanan) fxKanan = imageKanan.GetComponent<PortraitSpeakFX>();
    }

    void Start()
    {
        if (fokusUI) fokusUI.SetVignette(true);

        if (barisDialogList.Count > 0)
        {
            TampilkanDialog(indexAktif);
        }
    }

    void Update()
    {
        if (dialogSelesai) return;

        // Klik kiri atau tekan Spasi untuk lanjut / skip ketik
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            if (sedangMengetik)
            {
                StopAllCoroutines();
                textIsiDialog.maxVisibleCharacters = int.MaxValue;
                SelesaiMengetik();
            }
            else
            {
                LanjutKeBerikutnya();
            }
        }
    }

    void TampilkanDialog(int index)
    {
        if (fokusUI) fokusUI.SetInstruksi(false);

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

        // Sorot portrait yang lagi bicara
        PosisiPembicara pos = TentukanPembicara(line);
        bool kiriBicara = pos == PosisiPembicara.Kiri && line.spriteKiri != null;
        bool kananBicara = pos == PosisiPembicara.Kanan && line.spriteKanan != null;

        if (fxKiri) fxKiri.SetSpeaking(kiriBicara);
        if (fxKanan) fxKanan.SetSpeaking(kananBicara);
        pembicaraAktif = kiriBicara ? fxKiri : (kananBicara ? fxKanan : null);

        StartCoroutine(KetikEfek(kalimatFull));
    }

    PosisiPembicara TentukanPembicara(DialogLine line)
    {
        if (line.pembicara != PosisiPembicara.Otomatis) return line.pembicara;

        // cocokin nama
        if (!string.IsNullOrEmpty(line.namaKarakter))
        {
            if (line.namaKarakter.Equals(namaKarakterKiri, System.StringComparison.OrdinalIgnoreCase))
                return PosisiPembicara.Kiri;
            if (line.namaKarakter.Equals(namaKarakterKanan, System.StringComparison.OrdinalIgnoreCase))
                return PosisiPembicara.Kanan;
        }

        // cadangan: kalau cuma satu sisi yang tampil, dia yang bicara
        if (line.spriteKiri != null && line.spriteKanan == null) return PosisiPembicara.Kiri;
        if (line.spriteKanan != null && line.spriteKiri == null) return PosisiPembicara.Kanan;

        return PosisiPembicara.Narator;
    }

    IEnumerator KetikEfek(string teks)
    {
        // pakai maxVisibleCharacters biar tag rich text (<b>, <color>) nggak kebaca per huruf
        textIsiDialog.text = teks;
        textIsiDialog.maxVisibleCharacters = 0;
        textIsiDialog.ForceMeshUpdate();
        int total = textIsiDialog.textInfo.characterCount;

        sedangMengetik = true;
        if (pembicaraAktif) pembicaraAktif.SetTalking(true);

        for (int i = 1; i <= total; i++)
        {
            textIsiDialog.maxVisibleCharacters = i;
            yield return new WaitForSeconds(kecepatanKetik);
        }

        SelesaiMengetik();
    }

    void SelesaiMengetik()
    {
        sedangMengetik = false;
        if (pembicaraAktif) pembicaraAktif.SetTalking(false);
        if (fokusUI) fokusUI.SetInstruksi(true); // munculin sprite klik kiri yang berkedip
    }

    void LanjutKeBerikutnya()
    {
        indexAktif++;
        if (indexAktif < barisDialogList.Count)
        {
            TampilkanDialog(indexAktif);
        }
        else
        {
            dialogSelesai = true;
            if (fxKiri) fxKiri.SetSpeaking(false);
            if (fxKanan) fxKanan.SetSpeaking(false);
            if (fokusUI)
            {
                fokusUI.SetInstruksi(false);
                fokusUI.SetVignette(false);
            }
            onDialogSelesai?.Invoke();
        }
    }
}