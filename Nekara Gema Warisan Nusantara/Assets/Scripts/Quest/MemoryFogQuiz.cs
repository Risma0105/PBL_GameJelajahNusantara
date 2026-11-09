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
        public string[] pilihanJawaban; 
        public int indexJawabanBenar;   
    }

    [Header("Data Pertanyaan")]
    public Pertanyaan[] daftarPertanyaan;
    private int indexSoalAktif = 0;

    [Header("UI Referensi Teks & Tombol")]
    public TextMeshProUGUI teksSoal;
    public Button[] tombolPilihan; 
    public TextMeshProUGUI teksTimer; 

    [Header("UI Bar HP (Sesuai Desain)")]
    public Slider sliderHpBoss;      // Bilah HP merah di atas (Kabut Memori)
    public Slider sliderHpPlayer;    // Bilah HP hijau di bawah (Bramanta)
    public TextMeshProUGUI teksNamaPlayer; 

    [Header("Referensi Musuh & Ekspresi UI Image")]
    public GameObject objekKabut;       
    public Image imageMusuh;            
    public Sprite spriteMusuhHappy;     
    public Sprite spriteMusuhSakit;     
    
    [Header("Visual Efek Alat Musik (Otomatis Muncul)")]
    public Image imageIkonAlatMusik;    // Tarik UI Image tempat ikon alat musik muncul di kabut
    public Sprite[] spriteAlatMusik;    // Masukkan sprite (Angklung, Sarone, Gesok) di sini

    [Header("Pengaturan Waktu & HP")]
    public float batasWaktuJawab = 30f; 
    private float timerSoal = 0f;
    private bool sedangMenjawab = false;

    public int maxHpBoss = 5;
    private int currentHpBoss;
    public int maxHpPlayer = 100;
    private int currentHpPlayer;

    [Header("Pengaturan Warna Tombol")]
    public Color warnaNormal = Color.white;
    public Color warnaBenar = Color.green;
    public Color warnaSalah = Color.red;
    public float durasiFeedback = 1f;   

    void Start()
    {
        if (MuseumManager.Instance != null)
        {
            bool semuaSelesai = MuseumManager.Instance.sudahCraftingAngklung && 
                                MuseumManager.Instance.sudahCraftingGesoGeso && 
                                MuseumManager.Instance.sudahCraftingSarone;

            if (!semuaSelesai)
            {
                gameObject.SetActive(false);
                return; 
            }
        }

        currentHpBoss = maxHpBoss;
        currentHpPlayer = maxHpPlayer;
        
        UpdateUIStatus();
        TampilkanSoal();
        AturKarakterDanGerakan(false, false);

        if (imageMusuh != null && spriteMusuhHappy != null)
        {
            imageMusuh.sprite = spriteMusuhHappy;
        }

        // Sembunyikan ikon alat musik di awal
        if (imageIkonAlatMusik != null) imageIkonAlatMusik.gameObject.SetActive(false);

        for (int i = 0; i < tombolPilihan.Length; i++)
        {
            int indexTombol = i;
            tombolPilihan[i].onClick.AddListener(() => CekJawaban(indexTombol));
        }
    }

    void Update()
    {
        if (sedangMenjawab)
        {
            timerSoal += Time.deltaTime;

            if (teksTimer != null)
            {
                float sisaWaktuHitung = Mathf.Max(0f, batasWaktuJawab - timerSoal);
                teksTimer.text = $"Waktu: {sisaWaktuHitung:F1}s";
            }

            if (timerSoal >= batasWaktuJawab)
            {
                ProsesWaktuHabis();
            }
        }
    }

    void AturKarakterDanGerakan(bool statusJalan, bool statusTampil)
    {
        PlayerMovement playerMove = Object.FindFirstObjectByType<PlayerMovement>();
        if (playerMove != null) playerMove.enabled = statusJalan;

        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null) playerObj.SetActive(statusTampil);
    }

    void TampilkanSoal()
    {
        timerSoal = 0f;
        sedangMenjawab = true;

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
                    if (teksTombol != null) teksTombol.text = p.pilihanJawaban[i];
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
        if (!sedangMenjawab) return;
        sedangMenjawab = false; 

        float waktuPilihan = timerSoal; 
        Pertanyaan p = daftarPertanyaan[indexSoalAktif];
        
        foreach (Button btn in tombolPilihan) btn.interactable = false;

        Image imgPemain = tombolPilihan[pilihanPemain].GetComponent<Image>();
        Image imgBenar = tombolPilihan[p.indexJawabanBenar].GetComponent<Image>();

        bool apakahBenar = (pilihanPemain == p.indexJawabanBenar);

        if (apakahBenar)
        {
            HitungPenaltiAtauRewardWaktu(waktuPilihan);

            if (imgPemain != null) imgPemain.color = warnaBenar;
            currentHpBoss--; // Kurangi HP Boss di atas

            // MUNCULKAN ALAT MUSIK OTOMATIS SAAT JAWAB BENAR
            MunculkanEfekAlatMusik();

            if (imageMusuh != null && spriteMusuhSakit != null)
                imageMusuh.sprite = spriteMusuhSakit;

            Debug.Log($"✨ Benar dalam {waktuPilihan:F1} detik! Boss kesakitan.");
        }
        else
        {
            currentHpPlayer -= 25; // Penalti salah
            if (imgPemain != null) imgPemain.color = warnaSalah;
            if (imgBenar != null) imgBenar.color = warnaBenar; 

            if (imageMusuh != null && spriteMusuhHappy != null)
                imageMusuh.sprite = spriteMusuhHappy;

            Debug.Log("❌ Salah! HP Bramanta berkurang.");
        }

        UpdateUIStatus();

        if (currentHpBoss <= 0)
        {
            StartCoroutine(SelesaiMenangCoroutine());
            return;
        }

        if (currentHpPlayer <= 0)
        {
            Debug.Log("💀 Pemain Kalah!");
        }

        StartCoroutine(ResetEkspresiDanLanjutSoal());
    }

    void HitungPenaltiAtauRewardWaktu(float waktu)
    {
        if (waktu <= 5f)
        {
            currentHpPlayer -= 25;
            Debug.Log("🎵 Respon Cepat (<= 5s): [Gesok] | HP Player -25");
        }
        else if (waktu > 5f && waktu <= 10f)
        {
            currentHpPlayer -= 15;
            Debug.Log("🎵 Respon Sedang (5s - 10s): [Sarone] | HP Player -15");
        }
        else
        {
            currentHpPlayer -= 10;
            Debug.Log("🎵 Respon Lambat (> 10s): [Angklung] | HP Player -10");
        }
    }

    void MunculkanEfekAlatMusik()
    {
        if (imageIkonAlatMusik == null || spriteAlatMusik.Length == 0) return;

        int indexSprite = (maxHpBoss - currentHpBoss - 1);
        indexSprite = Mathf.Clamp(indexSprite, 0, spriteAlatMusik.Length - 1);

        if (spriteAlatMusik[indexSprite] != null)
        {
            imageIkonAlatMusik.sprite = spriteAlatMusik[indexSprite];
            imageIkonAlatMusik.gameObject.SetActive(true);
        }
    }

    void ProsesWaktuHabis()
    {
        sedangMenjawab = false;
        currentHpPlayer -= 30; 
        Debug.Log("⏳ Waktu Habis!");
        
        foreach (Button btn in tombolPilihan) btn.interactable = false;

        UpdateUIStatus();
        StartCoroutine(ResetEkspresiDanLanjutSoal());
    }

    IEnumerator ResetEkspresiDanLanjutSoal()
    {
        yield return new WaitForSeconds(durasiFeedback);
        
        // Sembunyikan lagi ikon alat musik untuk soal berikutnya
        if (imageIkonAlatMusik != null) imageIkonAlatMusik.gameObject.SetActive(false);

        if (imageMusuh != null && spriteMusuhHappy != null)
            imageMusuh.sprite = spriteMusuhHappy;

        indexSoalAktif++;
        TampilkanSoal();
    }

    IEnumerator SelesaiMenangCoroutine()
    {
        yield return new WaitForSeconds(durasiFeedback);
        Debug.Log("🎉 Kabut Memori Musnah!");
        
        AturKarakterDanGerakan(true, true);
        if (objekKabut != null) objekKabut.SetActive(false);
        gameObject.SetActive(false);

        if (MuseumManager.Instance != null)
        {
            MuseumManager.Instance.MenangKarnaval();
        }
    }

    void UpdateUIStatus()
    {
        if (sliderHpBoss != null)
        {
            sliderHpBoss.maxValue = maxHpBoss;
            sliderHpBoss.value = currentHpBoss;
        }

        if (sliderHpPlayer != null)
        {
            sliderHpPlayer.maxValue = maxHpPlayer;
            sliderHpPlayer.value = currentHpPlayer;
        }
    }
}