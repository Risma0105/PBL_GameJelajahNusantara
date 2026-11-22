using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class NPCCrafting : MonoBehaviour
{
    [Header("Nama Alat Musik")]
    public string namaAlatMusik = "Angklung";

    [Header("Daftar Bahan yang Dibutuhkan")]
    public List<string> bahanDibutuhkan = new List<string>();

    [Header("Hasil Crafting")]
    public GameObject prefabAlatMusikJadi;
    public Transform spawnPoint;

    [Header("Alat Musik yang Dibawa NPC (visual saja)")]
    [Tooltip("SpriteRenderer child dari SpriteNPC, diposisikan di depan badan NPC. Tanpa collider. Kalau Sprite-nya kosong, otomatis diambil dari prefab hasil.")]
    public SpriteRenderer spriteAlatDibawa;

    [Header("Efek Kilau di Alat yang Dibawa")]
    public bool kilauAktif = true;
    [Tooltip("Jumlah bintang kilau yang berkedip di sekitar alat musik.")]
    public int jumlahKilau = 6;
    [Tooltip("Pengali ukuran bintang. 1 = bintang selebar 7 pixel game.")]
    public float ukuranKilau = 1f;
    [Tooltip("Kecepatan kedip. Makin besar makin cepat.")]
    public float kecepatanKilau = 1.4f;
    public Color warnaKilau = new Color(1f, 0.95f, 0.6f, 1f);
    [Tooltip("Seberapa jauh kilau menyebar dari tepi sprite alat (1 = tepat di area sprite, 1.5 = agak keluar).")]
    public float sebaranKilau = 1.3f;

    [Header("Pengaturan Ruangan")]
    public GameObject kumpulanBahan;

    [Header("Efek Visual Crafting")]
    public ParticleSystem efekPoof;       // awan kecil di pintu (opsional)

    [Header("NPC (Fade & Arah Hadap)")]
    public SpriteRenderer spriteNPC;      // sprite NPC (harus child object)
    public Sprite spriteDepan;
    public Sprite spriteBelakang;
    public Transform titikPintu;          // tujuan NPC sebelum menghilang
    public Transform titikKeluar;         // posisi NPC saat muncul lagi
    public float kecepatanJalan = 2f;
    public float durasiFade = 0.8f;

    [Header("Animasi Rumah Saat Crafting")]
    [Tooltip("Drag SEMUA bagian rumah yang terpisah (saungbambu, saungbambu (1), dst). Semuanya digetarkan dengan offset yang sama.")]
    public Transform[] bagianRumah;
    [Tooltip("Seberapa tinggi rumah naik-turun (unit Unity).")]
    public float tinggiMelayang = 0.08f;
    [Tooltip("Kecepatan naik-turun. Makin besar makin cepat.")]
    public float kecepatanMelayang = 3f;
    public GameObject barLoading;         // root bar loading di atas rumah
    public Image isiBarLoading;           // Image type Filled
    public AudioSource sfxCrafting;       // suara ketukan (loop)
    public TMP_Text teksLoading;          // tulisan "Sedang merakit..."

    [Header("Dialog (dijalankan oleh NPCDialog)")]
    [Tooltip("Tarik komponen NPCDialog di NPC ini. Di NPCDialog, centang 'Dikendalikan Script Lain' dan isi referensi bubble-nya di sana.")]
    public NPCDialog npcDialog;

    [Header("Dialog Bahan Lengkap (sebelum crafting)")]
    public NPCDialog.DialogData[] daftarPercakapan;

    [Header("Dialog Bahan TIDAK Lengkap")]
    public NPCDialog.DialogData[] daftarPercakapanBahanKurang;

    [Header("Dialog Setelah Crafting")]
    public NPCDialog.DialogData[] daftarPercakapanSetelah;

    [Header("Setelah Alat Musik Diterima")]
    [Tooltip("Centang = NPC tetap bisa diajak bicara (dialog pendek di bawah). Kosong = NPC tidak bisa diajak bicara lagi dan prompt-nya disembunyikan.")]
    public bool bisaDiajakBicaraSetelahSelesai = true;
    [Tooltip("Dialog singkat kalau player bicara lagi setelah alat musik diterima. Isi dengan terima kasih + petunjuk ke museum.")]
    public NPCDialog.DialogData[] daftarPercakapanSelesai;
    [Tooltip("Objek ProximityPrompt/ikon 'E' milik NPC ini (misal PromptCanvas). Disembunyikan kalau 'Bisa Diajak Bicara Setelah Selesai' dimatikan.")]
    public GameObject promptNPC;

    [Header("Panel Badge / UI Tambahan")]
    public GameObject panelBadgeUnlock;

    [Header("Durasi Crafting")]
    public float durasiCrafting = 5f;

    // ---------- state ----------
    private bool playerDekat = false;
    private bool sudahDirakit = false;
    private bool sedangCrafting = false;
    private bool hasilSudahMuncul = false;
    private bool panelBadgeTerbuka = false;
    private bool craftingSelesaiMenungguDialog = false;

    private PlayerInventory inventoryPlayer;
    private PlayerInventory inventoryDipakai;

    // ---------- state kilau ----------
    private Coroutine kilauCoroutine;
    private List<GameObject> kilauObjek = new List<GameObject>();
    private List<SpriteRenderer> kilauRenderer = new List<SpriteRenderer>();
    private float[] kilauFase;
    private float[] kilauKecepatan;
    private Vector2[] kilauOffset;
    private Sprite spriteBintang;

    // =====================================
    // START
    // =====================================

    void Start()
    {
        if (efekPoof != null)
            efekPoof.Stop();

        if (npcDialog != null)
            npcDialog.TutupDialog();
        else
            Debug.LogWarning("NPCCrafting: slot 'Npc Dialog' belum diisi, dialog tidak akan muncul!");

        if (panelBadgeUnlock != null)
            panelBadgeUnlock.SetActive(false);

        if (barLoading != null)
            barLoading.SetActive(false);

        if (isiBarLoading != null)
            isiBarLoading.fillAmount = 0f;

        if (spriteNPC != null && spriteDepan != null)
            spriteNPC.sprite = spriteDepan;

        SiapkanAlatDibawa();

        // Cek apakah alat musik sudah selesai sebelumnya
        if (GameManager.Instance != null &&
            GameManager.Instance.CekSudahSelesai(namaAlatMusik))
        {
            sudahDirakit = true;

            if (kumpulanBahan != null)
                kumpulanBahan.SetActive(false);

            TerapkanPromptSetelahSelesai();

            Debug.Log(namaAlatMusik + " sudah selesai sebelumnya.");
        }
    }

    // Kalau NPC tidak boleh diajak bicara lagi setelah selesai, sembunyikan ikon "E"-nya
    void TerapkanPromptSetelahSelesai()
    {
        if (promptNPC != null && !bisaDiajakBicaraSetelahSelesai)
            promptNPC.SetActive(false);
    }

    void OnDestroy()
    {
        foreach (GameObject g in kilauObjek)
        {
            if (g != null) Destroy(g);
        }
        kilauObjek.Clear();
        kilauRenderer.Clear();
    }

    // Sembunyikan alat yang dibawa NPC; ambil sprite dari prefab kalau belum diisi
    void SiapkanAlatDibawa()
    {
        if (spriteAlatDibawa == null)
            return;

        if (spriteAlatDibawa.sprite == null && prefabAlatMusikJadi != null)
        {
            SpriteRenderer srPrefab = prefabAlatMusikJadi.GetComponentInChildren<SpriteRenderer>();
            if (srPrefab != null)
                spriteAlatDibawa.sprite = srPrefab.sprite;
        }

        spriteAlatDibawa.gameObject.SetActive(false);
    }

    // =====================================
    // UPDATE
    // =====================================

    void Update()
    {
        if (!playerDekat || sedangCrafting || panelBadgeTerbuka)
            return;

        if (!Input.GetKeyDown(KeyCode.E))
            return;

        // Alat musik sudah diterima: NPC cuma ngobrol singkat
        if (sudahDirakit)
        {
            if (!bisaDiajakBicaraSetelahSelesai || npcDialog == null)
                return;

            if (npcDialog.SedangBerdialog)
                npcDialog.TekanLanjut();
            else
                MulaiDialog(daftarPercakapanSelesai, () => { }); // callback kosong supaya NPCDialog tidak menjalankan aksi bawaannya

            return;
        }

        // Alat musik sudah muncul tapi belum diambil: tunggu player mengambilnya
        if (hasilSudahMuncul)
            return;

        if (npcDialog == null)
        {
            Debug.LogError("NPCCrafting: slot 'Npc Dialog' belum diisi!");
            return;
        }

        if (npcDialog.SedangBerdialog)
        {
            // Skip ketikan, atau lanjut ke baris berikutnya
            npcDialog.TekanLanjut();
        }
        else if (craftingSelesaiMenungguDialog)
        {
            MulaiDialog(daftarPercakapanSetelah, SelesaiDialogSetelah);
        }
        else
        {
            // Cek bahan DULU, baru pilih dialog yang sesuai
            if (CekBahanLengkap())
                MulaiDialog(daftarPercakapan, SelesaiDialogAwal);
            else
                MulaiDialog(daftarPercakapanBahanKurang, SelesaiDialogBahanKurang);
        }
    }

    // =====================================
    // SISTEM DIALOG (didelegasikan ke NPCDialog)
    // =====================================

    void MulaiDialog(NPCDialog.DialogData[] data, System.Action selesai)
    {
        // NPCDialog sendiri yang menangani data kosong (langsung panggil 'selesai')
        npcDialog.MulaiDialog(data, selesai);
    }

    // Dipanggil setelah dialog SEBELUM crafting selesai
    void SelesaiDialogAwal()
    {
        // Bahan sudah dicek sebelum dialog, jadi langsung mulai crafting.
        // Guard: player bisa saja sudah menjauh saat dialog berlangsung.
        if (inventoryPlayer == null)
            return;

        StartCoroutine(ProsesCraftingCoroutine());
    }

    // Dipanggil setelah dialog BAHAN TIDAK LENGKAP selesai
    void SelesaiDialogBahanKurang()
    {
        string pesanBelumLengkap = "Bahan untuk membuat " + namaAlatMusik + " belum lengkap!";
        Debug.Log(pesanBelumLengkap);

        if (UIManager.Instance != null)
        {
            UIManager.Instance.TampilkanPesan(pesanBelumLengkap);
        }
    }

    // Dipanggil setelah dialog SETELAH crafting selesai
    void SelesaiDialogSetelah()
    {
        craftingSelesaiMenungguDialog = false;

        // NPC "menyerahkan" alat: sprite yang dibawa & kilaunya hilang,
        // lalu alat musik yang bisa diklik muncul lewat SpawnAlatMusik()
        HentikanKilau();
        if (spriteAlatDibawa != null)
            spriteAlatDibawa.gameObject.SetActive(false);

        SpawnAlatMusik();

        // Tampilkan panel badge setelah NPC memberi alat musik
        if (panelBadgeUnlock != null)
        {
            panelBadgeTerbuka = true;
            panelBadgeUnlock.SetActive(true);
        }

        string pesanSelesaiCrafting = "Crafting selesai! " + namaAlatMusik + " muncul. Tutup panel badge untuk mengambil alat musik.";
        Debug.Log(pesanSelesaiCrafting);

        if (UIManager.Instance != null)
        {
            UIManager.Instance.TampilkanPesan(pesanSelesaiCrafting);
        }
    }

    // =====================================
    // CEK BAHAN INVENTORY
    // =====================================

    bool CekBahanLengkap()
    {
        if (inventoryPlayer == null)
            return false;

        foreach (string bahan in bahanDibutuhkan)
        {
            if (!inventoryPlayer.PunyaItem(bahan))
                return false;
        }

        return true;
    }

    // =====================================
    // PROSES CRAFTING (SEQUENCE)
    // =====================================

    IEnumerator ProsesCraftingCoroutine()
    {
        sedangCrafting = true;

        // Simpan referensi inventory, karena inventoryPlayer jadi null
        // kalau player menjauh saat crafting berlangsung
        inventoryDipakai = inventoryPlayer;

        string pesanMulai = "Crafting " + namaAlatMusik + " dimulai!";
        Debug.Log(pesanMulai);

        if (UIManager.Instance != null)
        {
            UIManager.Instance.TampilkanPesan(pesanMulai);
        }

        // 1. NPC menghadap belakang
        if (spriteNPC != null && spriteBelakang != null)
            spriteNPC.sprite = spriteBelakang;

        yield return new WaitForSeconds(0.4f);

        // 2. NPC jalan ke pintu rumah
        if (spriteNPC != null && titikPintu != null)
        {
            while (Vector2.Distance(spriteNPC.transform.position, titikPintu.position) > 0.05f)
            {
                spriteNPC.transform.position = Vector2.MoveTowards(
                    spriteNPC.transform.position,
                    titikPintu.position,
                    kecepatanJalan * Time.deltaTime
                );

                yield return null;
            }
        }

        // 3. NPC fade out (masuk rumah) + awan di pintu
        if (efekPoof != null)
        {
            if (titikPintu != null)
                efekPoof.transform.position = titikPintu.position;

            efekPoof.Play();
        }

        if (spriteNPC != null)
            yield return StartCoroutine(FadeSprite(spriteNPC, 1f, 0f, durasiFade));

        // 4. Loading crafting di rumah
        if (sfxCrafting != null)
            sfxCrafting.Play();

        if (barLoading != null)
            barLoading.SetActive(true);

        if (isiBarLoading != null)
            isiBarLoading.fillAmount = 0f;

        // Progress otomatis crafting jika masih digunakan
        if (GameProgress.Instance != null)
        {
            GameProgress.Instance.MulaiProgressOtomatis(durasiCrafting);
        }

        // Simpan posisi asli SEMUA bagian rumah
        int jumlahBagian = bagianRumah != null ? bagianRumah.Length : 0;
        Vector3[] posisiAsli = new Vector3[jumlahBagian];

        for (int i = 0; i < jumlahBagian; i++)
        {
            if (bagianRumah[i] != null)
                posisiAsli[i] = bagianRumah[i].position;
        }

        float waktu = 0f;

        while (waktu < durasiCrafting)
        {
            waktu += Time.deltaTime;

            // bar loading
            if (isiBarLoading != null)
                isiBarLoading.fillAmount = Mathf.Clamp01(waktu / durasiCrafting);

            // Tulisan "Sedang merakit..." dengan titik yang berganti 1-3
            if (teksLoading != null)
            {
                int titik = Mathf.FloorToInt(waktu * 2f) % 3 + 1;
                teksLoading.text = "Sedang merakit" + new string('.', titik);
            }

            // Gerak naik-turun halus (sinus). Envelope bikin gerakannya
            // pelan di awal, penuh di tengah, dan pelan lagi di akhir.
            float progres = Mathf.Clamp01(waktu / durasiCrafting);
            float envelope = Mathf.Sin(progres * Mathf.PI);
            float gelombang = Mathf.Sin(waktu * kecepatanMelayang);

            // SATU offset untuk semua bagian -> gerak selaras
            Vector3 offset = Vector3.up * (gelombang * tinggiMelayang * envelope);

            for (int i = 0; i < jumlahBagian; i++)
            {
                if (bagianRumah[i] != null)
                    bagianRumah[i].position = posisiAsli[i] + offset;
            }

            yield return null;
        }

        // Reset semua bagian rumah ke posisi asli
        for (int i = 0; i < jumlahBagian; i++)
        {
            if (bagianRumah[i] != null)
                bagianRumah[i].position = posisiAsli[i];
        }

        if (barLoading != null)
            barLoading.SetActive(false);

        if (sfxCrafting != null)
            sfxCrafting.Stop();

        // 5. Hapus bahan mentah dari inventory
        if (inventoryDipakai != null)
        {
            foreach (string bahan in bahanDibutuhkan)
            {
                inventoryDipakai.daftarItem.Remove(bahan);
            }

            inventoryDipakai.SimpanInventaris();
            inventoryDipakai.RefreshInventoryUI();
        }

        // 6. NPC muncul lagi di depan rumah, menghadap depan, sambil membawa alat musik
        if (spriteNPC != null)
        {
            if (titikKeluar != null)
                spriteNPC.transform.position = titikKeluar.position;

            if (spriteDepan != null)
                spriteNPC.sprite = spriteDepan;

            // Alat yang dibawa: mulai transparan, lalu fade in bareng NPC
            if (spriteAlatDibawa != null)
            {
                Color ca = spriteAlatDibawa.color;
                ca.a = 0f;
                spriteAlatDibawa.color = ca;
                spriteAlatDibawa.gameObject.SetActive(true);

                MulaiKilau();
            }

            if (efekPoof != null)
            {
                if (titikKeluar != null)
                    efekPoof.transform.position = titikKeluar.position;

                efekPoof.Play();
            }

            yield return StartCoroutine(FadeSprite(spriteNPC, 0f, 1f, durasiFade, spriteAlatDibawa));
        }

        // 7. Otomatis buka info tahap crafting di Culture Book
        if (!string.IsNullOrEmpty(namaAlatMusik))
        {
            CultureBookManager.BukaProgresBuku(namaAlatMusik, "Craft");
            Debug.Log("📖 Informasi tahap crafting " + namaAlatMusik + " terbuka di Culture Book!");
        }

        // 8. Crafting selesai -> langsung lanjut ke dialog penutup (tanpa tekan E)
        sedangCrafting = false;

        // Tetap true sampai dialog penutup tuntas (di-reset di SelesaiDialogSetelah).
        // Jadi kalau dialog terputus (player menjauh), E akan mengulang dialog penutup.
        craftingSelesaiMenungguDialog = true;

        // Hanya auto-mulai kalau player masih di dekat NPC.
        // Kalau player sedang menjauh, dialog menunggu sampai dia kembali dan tekan E.
        if (playerDekat && npcDialog != null)
        {
            MulaiDialog(daftarPercakapanSetelah, SelesaiDialogSetelah);
        }
    }

    // Fade satu sprite, plus sprite kedua (opsional) secara bersamaan
    IEnumerator FadeSprite(SpriteRenderer sr, float dari, float ke, float durasi, SpriteRenderer sr2 = null)
    {
        float t = 0f;
        Color c = sr.color;
        Color c2 = sr2 != null ? sr2.color : Color.white;

        while (t < durasi)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(dari, ke, t / durasi);

            c.a = a;
            sr.color = c;

            if (sr2 != null)
            {
                c2.a = a;
                sr2.color = c2;
            }

            yield return null;
        }

        c.a = ke;
        sr.color = c;

        if (sr2 != null)
        {
            c2.a = ke;
            sr2.color = c2;
        }
    }

    // =====================================
    // EFEK KILAU (bintang pixel berkedip di sekitar alat musik)
    // =====================================

    void MulaiKilau()
    {
        if (!kilauAktif || spriteAlatDibawa == null || jumlahKilau <= 0)
            return;

        BuatObjekKilau();

        if (kilauCoroutine != null)
            StopCoroutine(kilauCoroutine);

        kilauCoroutine = StartCoroutine(AnimasiKilau());
    }

    void HentikanKilau()
    {
        if (kilauCoroutine != null)
        {
            StopCoroutine(kilauCoroutine);
            kilauCoroutine = null;
        }

        foreach (GameObject g in kilauObjek)
        {
            if (g != null) g.SetActive(false);
        }
    }

    void BuatObjekKilau()
    {
        // Sudah pernah dibuat: cukup nyalakan lagi
        if (kilauObjek.Count == jumlahKilau)
        {
            foreach (GameObject g in kilauObjek)
            {
                if (g != null) g.SetActive(true);
            }
            AcakKilau();
            return;
        }

        // Bersihkan sisa lama kalau jumlahnya berubah
        foreach (GameObject g in kilauObjek)
        {
            if (g != null) Destroy(g);
        }
        kilauObjek.Clear();
        kilauRenderer.Clear();

        if (spriteBintang == null)
        {
            float ppu = (spriteAlatDibawa.sprite != null) ? spriteAlatDibawa.sprite.pixelsPerUnit : 16f;
            spriteBintang = BuatSpriteBintang(ppu);
        }

        for (int i = 0; i < jumlahKilau; i++)
        {
            GameObject g = new GameObject("Kilau_" + i);
            SpriteRenderer sr = g.AddComponent<SpriteRenderer>();
            sr.sprite = spriteBintang;
            sr.sortingLayerID = spriteAlatDibawa.sortingLayerID;
            sr.sortingOrder = spriteAlatDibawa.sortingOrder + 1;
            g.transform.localScale = Vector3.one * ukuranKilau;

            kilauObjek.Add(g);
            kilauRenderer.Add(sr);
        }

        kilauFase = new float[jumlahKilau];
        kilauKecepatan = new float[jumlahKilau];
        kilauOffset = new Vector2[jumlahKilau];
        AcakKilau();
    }

    void AcakKilau()
    {
        for (int i = 0; i < jumlahKilau; i++)
        {
            kilauFase[i] = Random.value;                                   // fase awal acak
            kilauKecepatan[i] = kecepatanKilau * Random.Range(0.7f, 1.3f); // kecepatan tiap bintang beda
            kilauOffset[i] = new Vector2(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f));
        }
    }

    IEnumerator AnimasiKilau()
    {
        while (true)
        {
            if (spriteAlatDibawa == null || !spriteAlatDibawa.gameObject.activeInHierarchy)
            {
                yield return null;
                continue;
            }

            Bounds b = spriteAlatDibawa.bounds;
            float alphaInduk = spriteAlatDibawa.color.a; // kilau ikut fade in bareng alat

            for (int i = 0; i < kilauObjek.Count; i++)
            {
                kilauFase[i] += Time.deltaTime * kilauKecepatan[i];

                // Satu siklus kedip selesai -> pindah ke posisi acak baru
                if (kilauFase[i] >= 1f)
                {
                    kilauFase[i] -= 1f;
                    kilauOffset[i] = new Vector2(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f));
                }

                float t = Mathf.Sin(kilauFase[i] * Mathf.PI); // 0 -> 1 -> 0

                Vector3 posisi = b.center + new Vector3(
                    kilauOffset[i].x * b.size.x * sebaranKilau,
                    kilauOffset[i].y * b.size.y * sebaranKilau,
                    0f);

                kilauObjek[i].transform.position = posisi;

                Color c = warnaKilau;
                c.a = warnaKilau.a * t * alphaInduk;
                kilauRenderer[i].color = c;
            }

            yield return null;
        }
    }

    // Bintang pixel 7x7 (tanda plus + diagonal tipis), dibuat lewat kode
    Sprite BuatSpriteBintang(float ppu)
    {
        int n = 7;
        Texture2D tex = new Texture2D(n, n, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        tex.wrapMode = TextureWrapMode.Clamp;

        Color[] px = new Color[n * n];
        for (int i = 0; i < px.Length; i++) px[i] = new Color(1f, 1f, 1f, 0f);

        // Garis tengah (plus)
        for (int i = 1; i <= 5; i++)
        {
            px[3 * n + i] = Color.white; // horizontal
            px[i * n + 3] = Color.white; // vertikal
        }

        // Diagonal tipis di sekitar pusat
        Color redup = new Color(1f, 1f, 1f, 0.6f);
        px[2 * n + 2] = redup;
        px[4 * n + 4] = redup;
        px[2 * n + 4] = redup;
        px[4 * n + 2] = redup;

        tex.SetPixels(px);
        tex.Apply();

        return Sprite.Create(tex, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), ppu);
    }

    // =====================================
    // SPAWN ALAT MUSIK
    // =====================================

    void SpawnAlatMusik()
    {
        // Tentukan posisi spawn alat musik
        Vector3 titikMuncul = spawnPoint != null
            ? spawnPoint.position
            : transform.position + new Vector3(0f, 1.2f, 0f);

        if (prefabAlatMusikJadi == null)
        {
            Debug.LogError("Prefab alat musik belum diisi!");
            return;
        }

        GameObject objekHasil = Instantiate(
            prefabAlatMusikJadi,
            titikMuncul,
            Quaternion.identity
        );

        // Hubungkan CollectibleItem dengan NPC
        CollectibleItem collectible =
            objekHasil.GetComponent<CollectibleItem>();

        if (collectible != null)
        {
            collectible.namaItem = namaAlatMusik;
            collectible.npcCrafting = this;
        }
        else
        {
            Debug.LogError(
                "Prefab " + prefabAlatMusikJadi.name +
                " belum memiliki CollectibleItem!"
            );
        }

        hasilSudahMuncul = true;
    }

    // =====================================
    // TOMBOL CLOSE PANEL BADGE
    // =====================================

    public void TutupPanelBadge()
    {
        if (panelBadgeUnlock != null)
            panelBadgeUnlock.SetActive(false);

        panelBadgeTerbuka = false;

        string pesanTutupBadge = "Panel badge ditutup. Silakan ambil alat musik!";
        Debug.Log(pesanTutupBadge);

        if (UIManager.Instance != null)
        {
            UIManager.Instance.TampilkanDefault();
        }
    }

    // =====================================
    // KONFIRMASI SETELAH ALAT DIAMBIL
    // =====================================

    public void KonfirmasiAlatDiambil()
    {
        if (sudahDirakit)
            return;

        sudahDirakit = true;

        // Update progress checkpoint
        if (GameProgress.Instance != null)
        {
            GameProgress.Instance.CollectInstrument(namaAlatMusik);
        }

        // Simpan badge
        PlayerPrefs.SetInt("Badge_" + namaAlatMusik, 1);
        PlayerPrefs.Save();

        // Tandai selesai di GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.TandaiSelesai(namaAlatMusik);
        }

        // Tandai selesai di MuseumManager
        if (MuseumManager.Instance != null)
        {
            MuseumManager.Instance.SelesaiCrafting(namaAlatMusik);
        }

        // Sembunyikan kumpulan bahan
        if (kumpulanBahan != null)
            kumpulanBahan.SetActive(false);

        TerapkanPromptSetelahSelesai();

        string pesanAmbil = "Alat musik " + namaAlatMusik + " berhasil diambil! Checkpoint selesai.";
        Debug.Log(pesanAmbil);

        if (UIManager.Instance != null)
        {
            UIManager.Instance.TampilkanDefault();
        }
    }

    // =====================================
    // DETEKSI PLAYER
    // =====================================

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && (!sudahDirakit || bisaDiajakBicaraSetelahSelesai))
        {
            playerDekat = true;
            inventoryPlayer = other.GetComponent<PlayerInventory>();

            // Jangan timpa pesan saat sequence crafting sedang berjalan
            if (sedangCrafting)
                return;

            string pesanPanduan = (sudahDirakit || craftingSelesaiMenungguDialog)
                ? "Tekan 'E' untuk bicara dengan NPC"
                : "Tekan 'E' untuk merakit " + namaAlatMusik;

            Debug.Log(pesanPanduan);

            // Tampilkan teks panduan lewat UIManager
            if (UIManager.Instance != null)
            {
                UIManager.Instance.TampilkanPesan(pesanPanduan);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerDekat = false;
            inventoryPlayer = null;

            // Tutup dialog (bubble + coroutine ketik) lewat NPCDialog
            if (npcDialog != null)
                npcDialog.TutupDialog();

            // Kembalikan ke teks statis default saat pemain menjauh
            if (UIManager.Instance != null)
            {
                UIManager.Instance.TampilkanDefault();
            }
        }
    }
}