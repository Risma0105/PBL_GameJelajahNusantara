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

    [Header("Dialog Sebelum Crafting")]
    public NPCDialog.DialogData[] daftarPercakapan;

    [Header("Dialog Setelah Crafting")]
    public NPCDialog.DialogData[] daftarPercakapanSetelah;

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

        // Cek apakah alat musik sudah selesai sebelumnya
        if (GameManager.Instance != null &&
            GameManager.Instance.CekSudahSelesai(namaAlatMusik))
        {
            sudahDirakit = true;

            if (kumpulanBahan != null)
                kumpulanBahan.SetActive(false);

            Debug.Log(namaAlatMusik + " sudah selesai sebelumnya.");
        }
    }

    // =====================================
    // UPDATE
    // =====================================

    void Update()
    {
        if (!playerDekat ||
            sudahDirakit ||
            sedangCrafting ||
            hasilSudahMuncul ||
            panelBadgeTerbuka)
            return;

        if (!Input.GetKeyDown(KeyCode.E))
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
            MulaiDialog(daftarPercakapan, SelesaiDialogAwal);
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
        CekDanMulaiCrafting();
    }

    // Dipanggil setelah dialog SETELAH crafting selesai
    void SelesaiDialogSetelah()
    {
        craftingSelesaiMenungguDialog = false;

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

    void CekDanMulaiCrafting()
    {
        if (inventoryPlayer == null)
            return;

        bool semuaLengkap = true;

        foreach (string bahan in bahanDibutuhkan)
        {
            if (!inventoryPlayer.PunyaItem(bahan))
            {
                semuaLengkap = false;
                break;
            }
        }

        if (semuaLengkap)
        {
            StartCoroutine(ProsesCraftingCoroutine());
        }
        else
        {
            string pesanBelumLengkap = "Bahan untuk membuat " + namaAlatMusik + " belum lengkap!";
            Debug.Log(pesanBelumLengkap);

            if (UIManager.Instance != null)
            {
                UIManager.Instance.TampilkanPesan(pesanBelumLengkap);
            }
        }
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

        // 6. NPC muncul lagi di depan rumah, menghadap depan
        if (spriteNPC != null)
        {
            if (titikKeluar != null)
                spriteNPC.transform.position = titikKeluar.position;

            if (spriteDepan != null)
                spriteNPC.sprite = spriteDepan;

            if (efekPoof != null)
            {
                if (titikKeluar != null)
                    efekPoof.transform.position = titikKeluar.position;

                efekPoof.Play();
            }

            yield return StartCoroutine(FadeSprite(spriteNPC, 0f, 1f, durasiFade));
        }

        // 7. Otomatis buka info tahap crafting di Culture Book
        if (!string.IsNullOrEmpty(namaAlatMusik))
        {
            CultureBookManager.BukaProgresBuku(namaAlatMusik, "Craft");
            Debug.Log("📖 Informasi tahap crafting " + namaAlatMusik + " terbuka di Culture Book!");
        }

        // 8. Tunggu player tekan E untuk dialog penutup
        sedangCrafting = false;
        craftingSelesaiMenungguDialog = true;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.TampilkanPesan("Tekan 'E' untuk bicara dengan NPC");
        }
    }

    IEnumerator FadeSprite(SpriteRenderer sr, float dari, float ke, float durasi)
    {
        float t = 0f;
        Color c = sr.color;

        while (t < durasi)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(dari, ke, t / durasi);
            sr.color = c;
            yield return null;
        }

        c.a = ke;
        sr.color = c;
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
        if (other.CompareTag("Player") && !sudahDirakit)
        {
            playerDekat = true;
            inventoryPlayer = other.GetComponent<PlayerInventory>();

            // Jangan timpa pesan saat sequence crafting sedang berjalan
            if (sedangCrafting)
                return;

            string pesanPanduan = craftingSelesaiMenungguDialog
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