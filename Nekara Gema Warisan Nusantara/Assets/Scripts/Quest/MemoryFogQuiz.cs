using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class MemoryFogQuiz : MonoBehaviour
{
    [System.Serializable]
    public struct Pertanyaan
    {
        [TextArea(2, 3)]
        public string soal;
        public string[] pilihanJawaban; // 4 pilihan (A, B, C, D)
        public int indexJawabanBenar;   // Indeks jawaban benar (0 sampai 3)
    }

    [Header("Data Pertanyaan")]
    public Pertanyaan[] daftarPertanyaan;
    private int indexSoalAktif = 0;

    [Header("UI Referensi")]
    public TextMeshProUGUI teksSoal;
    public Button[] tombolPilihan; // Masukkan 4 tombol A, B, C, D
    public TextMeshProUGUI teksStatusKabut;

    [Header("Referensi Musuh & Ekspresi UI Image")]
    public GameObject objekKabut;       // Tarik objek "Musuh" dari Hierarchy ke sini
    public Image imageMusuh;            // Tarik komponen "Image" milik objek Musuh ke sini
    public Sprite spriteMusuhHappy;     // Tarik asset gambar Musuh_Happy
    public Sprite spriteMusuhSakit;     // Tarik asset gambar Musuh_Sakit

    [Header("Pengaturan Warna Tombol (UI Image)")]
    public Color warnaNormal = Color.white;
    public Color warnaBenar = Color.green;
    public Color warnaSalah = Color.red;
    public float durasiFeedback = 1f;   

    [Header("Pengaturan Pertempuran")]
    public int totalKabutHP = 5; 
    private int sisaKabutHP;
    private bool sedangMenunggu = false; 

    void Start()
    {
        // PENGAMAN: Cek apakah ketiga alat musik sudah selesai dirakit atau belum
        if (MuseumManager.Instance != null)
        {
            bool semuaSelesai = MuseumManager.Instance.sudahCraftingAngklung && 
                                MuseumManager.Instance.sudahCraftingGesoGeso && 
                                MuseumManager.Instance.sudahCraftingSarone;

            // Jika belum lengkap, matikan langsung objek Quest ini di awal game!
            if (!semuaSelesai)
            {
                gameObject.SetActive(false);
                return; 
            }
        }

        // Kalau sudah lengkap, jalankan kuis seperti biasa
        sisaKabutHP = totalKabutHP;
        UpdateUIStatus();
        TampilkanSoal();

        // Set ekspresi awal musuh ke Happy
        if (imageMusuh != null && spriteMusuhHappy != null)
        {
            imageMusuh.sprite = spriteMusuhHappy;
        }

        for (int i = 0; i < tombolPilihan.Length; i++)
        {
            int indexTombol = i;
            tombolPilihan[i].onClick.AddListener(() => CekJawaban(indexTombol));
        }
    }

    void TampilkanSoal()
    {
        sedangMenunggu = false;
        if (indexSoalAktif < daftarPertanyaan.Length)
        {
            Pertanyaan p = daftarPertanyaan[indexSoalAktif];
            teksSoal.text = p.soal;

            for (int i = 0; i < tombolPilihan.Length; i++)
            {
                Image img = tombolPilihan[i].GetComponent<Image>();
                if (img != null) img.color = warnaNormal;

                tombolPilihan[i].interactable = true;

                if (i < p.pilihanJawaban.Length)
                {
                    tombolPilihan[i].gameObject.SetActive(true);
                    TextMeshProUGUI teksTombol = tombolPilihan[i].GetComponentInChildren<TextMeshProUGUI>();
                    if (teksTombol != null)
                    {
                        teksTombol.text = p.pilihanJawaban[i];
                    }
                }
                else
                {
                    tombolPilihan[i].gameObject.SetActive(false);
                }
            }
        }
        else
        {
            indexSoalAktif = 0;
            TampilkanSoal();
        }
    }

    public void CekJawaban(int pilihanPemain)
    {
        if (sedangMenunggu) return;
        sedangMenunggu = true;

        Pertanyaan p = daftarPertanyaan[indexSoalAktif];
        
        foreach (Button btn in tombolPilihan)
        {
            btn.interactable = false;
        }

        Image imgPemain = tombolPilihan[pilihanPemain].GetComponent<Image>();
        Image imgBenar = tombolPilihan[p.indexJawabanBenar].GetComponent<Image>();

        if (pilihanPemain == p.indexJawabanBenar)
        {
            // JAWABAN BENAR: Tombol Hijau, Musuh Sakit/Marah
            if (imgPemain != null) imgPemain.color = warnaBenar;
            sisaKabutHP--;

            if (imageMusuh != null && spriteMusuhSakit != null)
            {
                imageMusuh.sprite = spriteMusuhSakit;
            }

            Debug.Log("✨ Benar! Kabut kesakitan (-100 HP)");
        }
        else
        {
            // JAWABAN SALAH: Tombol Merah, Musuh Tetap/Happy
            if (imgPemain != null) imgPemain.color = warnaSalah;
            if (imgBenar != null) imgBenar.color = warnaBenar; 

            if (imageMusuh != null && spriteMusuhHappy != null)
            {
                imageMusuh.sprite = spriteMusuhHappy;
            }

            Debug.Log("❌ Salah! Kabut memori menguat.");
        }

        UpdateUIStatus();

        if (sisaKabutHP <= 0)
        {
            StartCoroutine(SelesaiMenangCoroutine());
            return;
        }

        StartCoroutine(ResetEkspresiDanLanjutSoal());
    }

    IEnumerator ResetEkspresiDanLanjutSoal()
    {
        yield return new WaitForSeconds(durasiFeedback);
        
        // Kembalikan ekspresi musuh ke Happy setelah jeda
        if (imageMusuh != null && spriteMusuhHappy != null)
        {
            imageMusuh.sprite = spriteMusuhHappy;
        }

        indexSoalAktif++;
        TampilkanSoal();
    }

    IEnumerator SelesaiMenangCoroutine()
    {
        yield return new WaitForSeconds(durasiFeedback);
        Debug.Log("🎉 Kabut Memori Musnah! Museum Menjadi Ramai.");
        
        // Matikan objek kabut musuh
        if (objekKabut != null) objekKabut.SetActive(false);
        gameObject.SetActive(false);

        // Panggil MuseumManager untuk mengubah fase ke Karnaval Ramai
        if (MuseumManager.Instance != null)
        {
            MuseumManager.Instance.MenangKarnaval();
        }
    }

    void UpdateUIStatus()
    {
        if (teksStatusKabut != null)
        {
            teksStatusKabut.text = "Sisa Kabut: " + sisaKabutHP + " / " + totalKabutHP;
        }
    }
}