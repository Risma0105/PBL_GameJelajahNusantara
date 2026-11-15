using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
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
    public float batasWaktuJawab = 45f; 
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

    [Header("Sprite Tombol (Opsional, menggantikan warna)")]
    public Sprite spriteNormal;
    public Sprite spriteBenar;
    public Sprite spriteSalah;

    [Header("Animasi Soal & Tombol")]
    public float kecepatanKetik = 40f;      // huruf per detik
    public float jedaTandaBaca = 0.15f;     // jeda tambahan di . , ! ?
    public float jedaAntarKotak = 0.12f;    // jeda muncul antar kotak jawaban
    public float durasiPop = 0.25f;         // lama animasi pop tiap kotak

    [Header("Kotak Soal Dinamis")]
    [Tooltip("Objek Kotak_Soal (yang punya Image kotaknya). Teks Soal harus jadi ANAK-nya.")]
    public RectTransform kotakSoal;
    public Vector2 paddingSoal = new Vector2(16f, 12f);
    public float lebarMinSoal = 80f;
    public float lebarMaksSoal = 240f;      // lebar maksimal TEKS (kotak = ini + padding)
    public float tinggiMinSoal = 24f;

    [Header("Cutscene Hasil Akhir")]
    [Tooltip("Image layar penuh (anak paling bawah di Quest). Kosongkan kalau tidak mau cutscene.")]
    public Image layarCutscene;
    [Tooltip("Teks di tengah layar, harus ANAK dari layarCutscene.")]
    public TextMeshProUGUI teksCutscene;
    [TextArea(2, 3)] public string teksKalah = "Kabut Memori telah menguasai...";
    [TextArea(2, 3)] public string teksMenang = "Kabut Memori sirna.\nIngatan Nusantara kembali bersinar.";
    public bool tampilkanSkor = true;
    public Color warnaLayarKalah = Color.black;
    public Color warnaTeksKalah = Color.white;
    public Color warnaLayarMenang = new Color(1f, 0.96f, 0.85f);
    public Color warnaTeksMenang = new Color(0.25f, 0.15f, 0.05f);
    public float durasiFade = 1f;               // lama layar memudar masuk
    public float kecepatanKetikCutscene = 15f;  // huruf per detik (lebih pelan dari soal)
    public float jedaTitikCutscene = 0.35f;     // jeda tiap tanda titik, bikin "..." terasa menggantung
    public float durasiTahanCutscene = 1.5f;    // lama teks ditahan setelah selesai diketik

    [Header("Hasil Akhir Kuis")]
    [Tooltip("Nama scene PERSIS seperti di Build Profiles, contoh: 06_Pagelaran...")]
    public string namaSceneMenang = "";     // dituju kalau BENAR lebih banyak
    [Tooltip("Nama scene PERSIS seperti di Build Profiles, contoh: 05_MuseumDalam")]
    public string namaSceneKalah = "";      // dituju kalau SALAH lebih banyak
    [Tooltip("Hanya dipakai kalau cutscene tidak diisi")]
    public float jedaSebelumPindahScene = 2f;
    [Tooltip("Matikan kalau MuseumManager.MenangKarnaval() sudah memindahkan scene sendiri")]
    public bool pindahSceneMenang = true;
    public bool resetProgresSaatKalah = true;

    private int jumlahBenar = 0;
    private int jumlahSalah = 0;

    private Coroutine alurSoal;
    private bool sedangKetik = false;
    private bool lewatiKetik = false;

    // Ukuran asli (scale) tiap objek, dicatat di awal supaya animasi pop tidak mengubah ukuran
    private Vector3[] skalaAsliTombol;
    private Vector3 skalaAsliKotakSoal = Vector3.one;

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
        jumlahBenar = 0;
        jumlahSalah = 0;

        // Layar cutscene disembunyikan sampai kuis selesai
        if (layarCutscene != null) layarCutscene.gameObject.SetActive(false);

        // Catat ukuran asli tombol & kotak soal
        skalaAsliTombol = new Vector3[tombolPilihan.Length];
        for (int i = 0; i < tombolPilihan.Length; i++)
            skalaAsliTombol[i] = tombolPilihan[i].transform.localScale;
        if (kotakSoal != null) skalaAsliKotakSoal = kotakSoal.localScale;

        // Warna diatur script, jadi efek warna bawaan Button dimatikan
        for (int i = 0; i < tombolPilihan.Length; i++)
        {
            int indexTombol = i;
            tombolPilihan[i].transition = Selectable.Transition.None;
            tombolPilihan[i].onClick.AddListener(() => CekJawaban(indexTombol));
        }
        
        UpdateUIStatus();
        TampilkanSoal();
        AturKarakterDanGerakan(false, false);

        if (imageMusuh != null && spriteMusuhHappy != null)
        {
            imageMusuh.sprite = spriteMusuhHappy;
        }

        // Sembunyikan ikon alat musik di awal
        if (imageIkonAlatMusik != null) imageIkonAlatMusik.gameObject.SetActive(false);
    }

    void Update()
    {
        // Spasi / klik saat teks masih diketik = langsung tampil penuh
        if (sedangKetik && (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)))
        {
            lewatiKetik = true;
        }

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
        sedangMenjawab = false; // timer baru jalan setelah semua kotak jawaban muncul

        if (daftarPertanyaan.Length == 0) return;
        if (indexSoalAktif >= daftarPertanyaan.Length) indexSoalAktif = 0;

        if (alurSoal != null) StopCoroutine(alurSoal);
        alurSoal = StartCoroutine(AlurTampilSoal(daftarPertanyaan[indexSoalAktif]));
    }

    IEnumerator AlurTampilSoal(Pertanyaan p)
    {
        // 1. Sembunyikan semua kotak jawaban dulu
        for (int i = 0; i < tombolPilihan.Length; i++)
        {
            tombolPilihan[i].transform.localScale = skalaAsliTombol[i];
            tombolPilihan[i].gameObject.SetActive(false);
        }

        if (teksTimer != null) teksTimer.text = $"Waktu: {batasWaktuJawab:F1}s";

        // 2. Siapkan soal: kotak menyesuaikan isi, lalu muncul dengan efek pop
        teksSoal.text = p.soal.Replace('\u00A0', ' ');
        teksSoal.maxVisibleCharacters = 0;

        if (kotakSoal != null)
        {
            AturUkuranKotakSoal();                                        // kotak pas dengan isi soal
            StartCoroutine(PopTombol(kotakSoal, skalaAsliKotakSoal));     // efek pop yang sama dengan tombol
            yield return new WaitForSeconds(durasiPop * 0.6f);
        }

        teksSoal.ForceMeshUpdate();

        // 3. Ketik soal huruf per huruf
        TMP_TextInfo info = teksSoal.textInfo;
        int total = info.characterCount;
        float jedaHuruf = 1f / Mathf.Max(1f, kecepatanKetik);
        float sisaJeda = 0f;
        int tampil = 0;

        sedangKetik = true;
        lewatiKetik = false;
        while (tampil < total && !lewatiKetik)
        {
            sisaJeda -= Time.deltaTime;
            while (sisaJeda <= 0f && tampil < total)
            {
                tampil++;
                char huruf = info.characterInfo[tampil - 1].character;
                sisaJeda += jedaHuruf;
                if (huruf == '.' || huruf == ',' || huruf == '!' || huruf == '?') sisaJeda += jedaTandaBaca;
            }
            teksSoal.maxVisibleCharacters = tampil;
            yield return null;
        }
        teksSoal.maxVisibleCharacters = 99999;
        sedangKetik = false;

        yield return new WaitForSeconds(0.2f);

        // 4. Kotak jawaban muncul satu per satu
        for (int i = 0; i < tombolPilihan.Length; i++)
        {
            if (i >= p.pilihanJawaban.Length) continue;

            TerapkanVisualTombol(i, spriteNormal, warnaNormal);
            tombolPilihan[i].interactable = false;

            TextMeshProUGUI teksTombol = tombolPilihan[i].GetComponentInChildren<TextMeshProUGUI>();
            if (teksTombol != null) teksTombol.text = p.pilihanJawaban[i];

            tombolPilihan[i].gameObject.SetActive(true);
            StartCoroutine(PopTombol(tombolPilihan[i].transform, skalaAsliTombol[i]));

            yield return new WaitForSeconds(jedaAntarKotak);
        }

        yield return new WaitForSeconds(durasiPop);

        // 5. Semua siap: tombol bisa diklik, timer mulai
        for (int i = 0; i < tombolPilihan.Length; i++)
        {
            if (tombolPilihan[i].gameObject.activeSelf) tombolPilihan[i].interactable = true;
        }

        timerSoal = 0f;
        sedangMenjawab = true;
    }

    // Ukur soal dari hasil render asli, lalu sesuaikan kotak Soal
        // Ukur soal (dengan semua huruf dihitung), lalu sesuaikan kotak Soal
string BungkusManual(string isi, float lebarMaks)
{
    teksSoal.textWrappingMode = TextWrappingModes.NoWrap;
    string[] kata = isi.Replace("\n", " ").Split(' ');
    string baris = "";
    string hasil = "";

    foreach (string k in kata)
    {
        string coba = baris.Length == 0 ? k : baris + " " + k;
        float w = teksSoal.GetPreferredValues(coba).x;
        if (w > lebarMaks && baris.Length > 0)
        {
            hasil += baris + "\n";
            baris = k;
        }
        else baris = coba;
    }
    return hasil + baris;
}

void AturUkuranKotakSoal()
{
    RectTransform rtTeks = teksSoal.rectTransform;
    if (teksSoal.transform.parent != kotakSoal) teksSoal.transform.SetParent(kotakSoal, false);
    rtTeks.localScale = Vector3.one;

    int visibleLama = teksSoal.maxVisibleCharacters;
    teksSoal.maxVisibleCharacters = 99999;
    teksSoal.overflowMode = TextOverflowModes.Overflow;
    teksSoal.alignment = TextAlignmentOptions.Center;
    teksSoal.enableAutoSizing = false;

    teksSoal.text = BungkusManual(teksSoal.text, lebarMaksSoal);

    Vector2 ukuran = teksSoal.GetPreferredValues(teksSoal.text);
    float lebar = Mathf.Max(Mathf.Ceil(ukuran.x) + 4f, lebarMinSoal);
    float tinggi = Mathf.Max(Mathf.Ceil(ukuran.y), tinggiMinSoal);

    kotakSoal.sizeDelta = new Vector2(lebar + paddingSoal.x, tinggi + paddingSoal.y);

    rtTeks.anchorMin = Vector2.zero;
    rtTeks.anchorMax = Vector2.one;
    rtTeks.pivot = new Vector2(0.5f, 0.5f);
    rtTeks.offsetMin = paddingSoal * 0.5f;
    rtTeks.offsetMax = -paddingSoal * 0.5f;

    Canvas.ForceUpdateCanvases();
    teksSoal.ForceMeshUpdate(true, true);

    Debug.Log($"[Kotak Soal] baris={teksSoal.textInfo.lineCount}, teks lebar={ukuran.x:F0}, " +
              $"kotak={kotakSoal.sizeDelta}, ada newline={teksSoal.text.Contains("\n")}", teksSoal);

    teksSoal.maxVisibleCharacters = visibleLama;
}
    
    // Pop dari 0 menuju ukuran asli objek (skalaAkhir), bukan selalu 1
    IEnumerator PopTombol(Transform t, Vector3 skalaAkhir)
    {
        t.localScale = Vector3.zero;
        float waktu = 0f;
        while (waktu < durasiPop)
        {
            waktu += Time.deltaTime;
            float n = Mathf.Clamp01(waktu / durasiPop);
            t.localScale = skalaAkhir * EaseOutBack(n);
            yield return null;
        }
        t.localScale = skalaAkhir;
    }

    // Membesar sedikit melebihi 1 lalu mengendap (kesan "pop")
    float EaseOutBack(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }

    // Ganti sprite kalau disediakan, kalau tidak ganti warna
    void TerapkanVisualTombol(int index, Sprite sprite, Color warna)
    {
        if (index < 0 || index >= tombolPilihan.Length) return;

        Image img = tombolPilihan[index].GetComponent<Image>();
        if (img == null) return;

        if (sprite != null)
        {
            img.sprite = sprite;
            img.color = Color.white;
        }
        else
        {
            if (spriteNormal != null) img.sprite = spriteNormal;
            img.color = warna;
        }
    }

    public void CekJawaban(int pilihanPemain)
    {
        if (!sedangMenjawab) return;
        sedangMenjawab = false; 

        float waktuPilihan = timerSoal; 
        Pertanyaan p = daftarPertanyaan[indexSoalAktif];
        
        foreach (Button btn in tombolPilihan) btn.interactable = false;

        bool apakahBenar = (pilihanPemain == p.indexJawabanBenar);

        if (apakahBenar)
        {
            jumlahBenar++;
            HitungPenaltiAtauRewardWaktu(waktuPilihan);

            TerapkanVisualTombol(pilihanPemain, spriteBenar, warnaBenar);
            currentHpBoss--; // Kurangi HP Boss di atas

            // MUNCULKAN ALAT MUSIK OTOMATIS SAAT JAWAB BENAR
            MunculkanEfekAlatMusik();

            if (imageMusuh != null && spriteMusuhSakit != null)
                imageMusuh.sprite = spriteMusuhSakit;

            Debug.Log($"✨ Benar dalam {waktuPilihan:F1} detik! Boss kesakitan.");
        }
        else
        {
            jumlahSalah++;
            currentHpPlayer -= 25; // Penalti salah
            TerapkanVisualTombol(pilihanPemain, spriteSalah, warnaSalah);
            TerapkanVisualTombol(p.indexJawabanBenar, spriteBenar, warnaBenar); 

            if (imageMusuh != null && spriteMusuhHappy != null)
                imageMusuh.sprite = spriteMusuhHappy;

            Debug.Log("❌ Salah! HP Bramanta berkurang.");
        }

        UpdateUIStatus();
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
        jumlahSalah++; // waktu habis dihitung salah
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

        if (indexSoalAktif >= daftarPertanyaan.Length)
        {
            // Semua soal sudah dijawab -> tentukan menang / kalah
            StartCoroutine(SelesaiKuis());
        }
        else
        {
            TampilkanSoal();
        }
    }

    IEnumerator SelesaiKuis()
    {
        sedangMenjawab = false;
        bool menang = jumlahBenar > jumlahSalah; // 5 soal = tidak mungkin seri
        int totalSoal = daftarPertanyaan.Length;

        Debug.Log(menang ? "🎉 Kabut Memori Musnah!" : "💀 Pemain Kalah!");

        if (layarCutscene != null && teksCutscene != null)
        {
            yield return JalankanCutscene(menang, totalSoal);
        }
        else
        {
            // Cadangan: pesan sederhana di kotak soal kalau cutscene belum disiapkan
            for (int i = 0; i < tombolPilihan.Length; i++) tombolPilihan[i].gameObject.SetActive(false);
            if (teksTimer != null) teksTimer.text = "";

            teksSoal.text = menang
                ? $"Kabut Memori sirna!\nBenar {jumlahBenar} dari {totalSoal}."
                : $"Ingatanmu masih kabur...\nBenar {jumlahBenar} dari {totalSoal}. Ulangi dari awal.";
            teksSoal.maxVisibleCharacters = 99999;
            if (kotakSoal != null) AturUkuranKotakSoal();

            yield return new WaitForSeconds(jedaSebelumPindahScene);
        }

        if (menang)
        {
            AturKarakterDanGerakan(true, true);
            if (objekKabut != null) objekKabut.SetActive(false);

            if (MuseumManager.Instance != null)
            {
                MuseumManager.Instance.MenangKarnaval();
            }

            if (pindahSceneMenang) PindahScene(namaSceneMenang);
            else gameObject.SetActive(false);
        }
        else
        {
            if (resetProgresSaatKalah) ResetProgresMuseum();
            PindahScene(namaSceneKalah);
        }
    }

    // Layar memudar masuk -> teks diketik -> ditahan sebentar
    IEnumerator JalankanCutscene(bool menang, int totalSoal)
    {
        Color warnaLayar = menang ? warnaLayarMenang : warnaLayarKalah;
        Color warnaTeks = menang ? warnaTeksMenang : warnaTeksKalah;

        string isi = menang ? teksMenang : teksKalah;
        if (tampilkanSkor)
            isi += $"\n\n<size=60%>Benar {jumlahBenar} dari {totalSoal}</size>";

        // Sembunyikan timer, siapkan layar dan teks
        if (teksTimer != null) teksTimer.text = "";

        teksCutscene.text = isi;
        teksCutscene.color = warnaTeks;
        teksCutscene.maxVisibleCharacters = 0;

        layarCutscene.color = new Color(warnaLayar.r, warnaLayar.g, warnaLayar.b, 0f);
        layarCutscene.gameObject.SetActive(true);

        // 1. Layar memudar masuk
        float waktu = 0f;
        while (waktu < durasiFade)
        {
            waktu += Time.deltaTime;
            float a = Mathf.Clamp01(waktu / Mathf.Max(0.01f, durasiFade));
            layarCutscene.color = new Color(warnaLayar.r, warnaLayar.g, warnaLayar.b, a);
            yield return null;
        }
        layarCutscene.color = new Color(warnaLayar.r, warnaLayar.g, warnaLayar.b, 1f);

        yield return new WaitForSeconds(0.5f);

        // 2. Ketik teks (spasi / klik = lewati)
        teksCutscene.ForceMeshUpdate();
        TMP_TextInfo info = teksCutscene.textInfo;
        int total = info.characterCount;
        float jedaHuruf = 1f / Mathf.Max(1f, kecepatanKetikCutscene);
        float sisaJeda = 0f;
        int tampil = 0;

        sedangKetik = true;
        lewatiKetik = false;
        while (tampil < total && !lewatiKetik)
        {
            sisaJeda -= Time.deltaTime;
            while (sisaJeda <= 0f && tampil < total)
            {
                tampil++;
                char huruf = info.characterInfo[tampil - 1].character;
                sisaJeda += jedaHuruf;
                if (huruf == '.') sisaJeda += jedaTitikCutscene; // titik-titik terasa menggantung
            }
            teksCutscene.maxVisibleCharacters = tampil;
            yield return null;
        }
        teksCutscene.maxVisibleCharacters = 99999;
        sedangKetik = false;

        // 3. Tahan sebentar sebelum pindah scene
        yield return new WaitForSeconds(durasiTahanCutscene);
    }

    // Reset progress crafting supaya pemain mengulang dari awal
    void ResetProgresMuseum()
    {
        if (MuseumManager.Instance == null) return;

        MuseumManager.Instance.sudahCraftingAngklung = false;
        MuseumManager.Instance.sudahCraftingGesoGeso = false;
        MuseumManager.Instance.sudahCraftingSarone = false;
    }

    void PindahScene(string namaScene)
    {
        if (string.IsNullOrEmpty(namaScene))
        {
            Debug.LogError("Nama scene tujuan belum diisi di Inspector (Memory Fog Quiz > Hasil Akhir Kuis).");
            return;
        }
        SceneManager.LoadScene(namaScene);
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