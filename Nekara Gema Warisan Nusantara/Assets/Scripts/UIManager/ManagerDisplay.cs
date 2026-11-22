using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class ManagerDisplay : MonoBehaviour
{
    [Header("Pengaturan Display Alat Musik")]
    [Tooltip("Isi: Angklung, GesoGeso, atau Sarone")]
    public string namaAlatMusik = "Angklung";

    [Header("Visual & Titik Taruh di Meja")]
    public GameObject objekVisualAlatMusikDiMuseum;
    public Transform posisiTaruhDiMeja;

    [Header("UI Pesan")]
    public GameObject panelPesan;
    public TMP_Text teksPesan;

    [Header("Pengaturan Interaksi")]
    public float jarakInteraksi = 3.0f;

    [Header("Urutan Penutup Level")]
    [Tooltip("Lama alat musik berubah dari siluet abu-abu ke warna asli (plus efek membesar sebentar).")]
    public float durasiMuncul = 0.6f;
    [Tooltip("Jeda setelah alat terpajang, sebelum progress bar mulai naik.")]
    public float jedaSebelumBar = 0.5f;
    [Tooltip("Jeda setelah progress bar sampai di checkpoint, sebelum UI badge muncul.")]
    public float jedaSebelumBadge = 0.4f;
    [Tooltip("Suara saat alat musik berhasil dipajang (opsional).")]
    public AudioSource sfxBerhasil;
    [Tooltip("Dipanggil setelah UI badge muncul = level ini resmi selesai. Hubungkan ke aksi berikutnya kalau perlu.")]
    public UnityEvent onLevelSelesai;

    [Header("Debug")]
    [Tooltip("Cetak alasan kenapa input tidak berhasil di Console")]
    public bool tampilkanLogDebug = true;

    private static readonly Color warnaSiluet = new Color(0.4f, 0.4f, 0.4f, 1f);

    private Transform playerTransform;
    private PlayerInventory playerInventory;
    private bool sudahDipajang = false;
    private bool sedangUrutan = false;

    private string KeyDisplay
    {
        get { return "MuseumDisplay_" + namaAlatMusik; }
    }

    void Start()
    {
        CariPlayer();

        sudahDipajang = PlayerPrefs.GetInt(KeyDisplay, 0) == 1;

        if (sudahDipajang)
        {
            TampilkanAlatMusikDiMeja();
        }
        else if (objekVisualAlatMusikDiMuseum != null)
        {
            // Belum dipajang: tampil sebagai siluet abu-abu di meja
            if (posisiTaruhDiMeja != null)
                objekVisualAlatMusikDiMuseum.transform.position = posisiTaruhDiMeja.position;

            objekVisualAlatMusikDiMuseum.SetActive(true);
            AturWarnaVisual(warnaSiluet);
        }

        if (panelPesan != null)
            panelPesan.SetActive(false);

        Log("ManagerDisplay aktif. Klik kiri mouse untuk menaruh alat musik.");
    }

    void CariPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
            playerInventory = playerObj.GetComponent<PlayerInventory>();
        }
    }

    void Log(string pesan)
    {
        if (tampilkanLogDebug)
            Debug.Log($"[ManagerDisplay:{namaAlatMusik}] {pesan}");
    }

    // Ubah warna SEMUA SpriteRenderer dan UI Image di objek visual (termasuk anak-anaknya)
    void AturWarnaVisual(Color warna)
    {
        if (objekVisualAlatMusikDiMuseum == null)
            return;

        foreach (SpriteRenderer sr in objekVisualAlatMusikDiMuseum.GetComponentsInChildren<SpriteRenderer>(true))
            sr.color = warna;

        foreach (Image img in objekVisualAlatMusikDiMuseum.GetComponentsInChildren<Image>(true))
            img.color = warna;
    }

    // True kalau kursor ada di atas UI INTERAKTIF (Button, Toggle, Slider, dll.).
    // Overlay/frame dekoratif yang Raycast Target-nya nyala tidak dihitung.
    bool KlikKenaUIInteraktif()
    {
        if (EventSystem.current == null || Mouse.current == null)
            return false;

        PointerEventData data = new PointerEventData(EventSystem.current)
        {
            position = Mouse.current.position.ReadValue()
        };

        List<RaycastResult> hasil = new List<RaycastResult>();
        EventSystem.current.RaycastAll(data, hasil);

        foreach (RaycastResult h in hasil)
        {
            if (h.gameObject.GetComponentInParent<Selectable>() != null)
                return true;
        }

        return false;
    }

    void Update()
    {
        // Input: HANYA klik kiri mouse
        bool klikKiri = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;

        if (!klikKiri)
            return;

        // Urutan penutup sedang berjalan: input diabaikan
        if (sedangUrutan)
            return;

        if (sudahDipajang)
        {
            Log("Sudah dipajang (tersimpan di PlayerPrefs), input diabaikan. " +
                "Klik kanan komponen > 'Reset Display Ini' untuk mengulang.");
            return;
        }

        if (playerTransform == null || playerInventory == null)
            CariPlayer();

        if (playerTransform == null)
        {
            Debug.LogWarning($"[ManagerDisplay:{namaAlatMusik}] Objek ber-tag 'Player' tidak ditemukan!");
            return;
        }

        if (playerInventory == null)
        {
            Debug.LogWarning($"[ManagerDisplay:{namaAlatMusik}] Komponen PlayerInventory tidak ada di objek Player!");
            return;
        }

        // Klik yang kena tombol UI asli tidak dianggap klik ke display
        if (KlikKenaUIInteraktif())
        {
            Log("Klik kena tombol UI, diabaikan.");
            return;
        }

        // Jarak dihitung dari objek alat musik di meja (kalau ada)
        Vector3 titikAcuan = objekVisualAlatMusikDiMuseum != null
            ? objekVisualAlatMusikDiMuseum.transform.position
            : transform.position;

        float jarak = Vector2.Distance(titikAcuan, playerTransform.position);
        bool punya = playerInventory.PunyaItem(namaAlatMusik);
        Log($"Klik: jarak={jarak:F2} batas={jarakInteraksi} punya={punya}");

        if (jarak > jarakInteraksi)
        {
            Log("Player terlalu jauh. Dekati meja atau naikkan jarakInteraksi.");
            return;
        }

        CobaTaruhAlatMusikDariInventaris();
    }

    void CobaTaruhAlatMusikDariInventaris()
    {
        if (playerInventory == null)
        {
            Debug.LogWarning("PlayerInventory tidak ditemukan!");
            return;
        }

        if (!playerInventory.PunyaItem(namaAlatMusik))
        {
            TampilkanPesan("❌ Kamu belum memiliki " + namaAlatMusik + " di dalam tas inventaris!");
            return;
        }

        // Cek urutan sekat DULU, supaya alat tidak terlanjur hilang dari inventory kalau ditolak
        if (GameProgress.Instance != null && !GameProgress.Instance.BisaTaruhDisplay(namaAlatMusik))
        {
            TampilkanPesan("⏳ " + namaAlatMusik + " belum bisa dipajang sekarang.");
            return;
        }

        StartCoroutine(UrutanPenutupLevel());
    }

    // =====================================
    // URUTAN PENUTUP LEVEL
    //   1. alat diambil dari inventory & muncul di meja
    //   2. jeda -> progress bar naik halus ke checkpoint
    //   3. jeda -> UI badge muncul
    //   4. level selesai
    // =====================================

    IEnumerator UrutanPenutupLevel()
    {
        sedangUrutan = true;

        // 1. Simpan status pajangan & kosongkan alat dari inventory
        PlayerPrefs.SetInt(KeyDisplay, 1);
        PlayerPrefs.Save();

        playerInventory.daftarItem.RemoveAll(
            item => item.Trim().ToLower() == namaAlatMusik.Trim().ToLower());
        playerInventory.SimpanInventaris();
        playerInventory.RefreshInventoryUI();

        if (GameProgress.Instance != null)
            GameProgress.Instance.CollectInstrument(namaAlatMusik);

        // 2. Alat muncul di meja (siluet -> warna asli)
        sudahDipajang = true;

        if (sfxBerhasil != null)
            sfxBerhasil.Play();

        yield return StartCoroutine(AnimasiMunculDiMeja());

        TampilkanPesan("✨ Berhasil! " + namaAlatMusik + " berhasil dipajang di museum!");

        // 3. Progress bar naik halus ke checkpoint
        yield return new WaitForSeconds(jedaSebelumBar);

        bool barSelesai = false;
        if (GameProgress.Instance != null)
        {
            GameProgress.Instance.BerhasilTaruhDisplay(namaAlatMusik, () => barSelesai = true);
        }
        else
        {
            Debug.LogWarning("GameProgress.Instance null, progress tidak naik dan pintu berikutnya tidak terbuka!");
            barSelesai = true;
        }

        while (!barSelesai)
            yield return null;

        // 4. UI badge muncul
        yield return new WaitForSeconds(jedaSebelumBadge);

        BadgeManager managerBadge = FindFirstObjectByType<BadgeManager>();
        if (managerBadge != null)
            managerBadge.BukaBadge(namaAlatMusik);
        else
            Debug.LogWarning("BadgeManager tidak ditemukan di scene, UI badge tidak muncul!");

        Debug.Log("✨ " + namaAlatMusik + " dipajang di museum, inventory dikosongkan, badge terbuka.");

        // 5. Level selesai
        if (onLevelSelesai != null)
            onLevelSelesai.Invoke();

        sedangUrutan = false;
    }

    // Siluet abu-abu -> warna asli, dengan efek membesar sebentar lalu kembali
    IEnumerator AnimasiMunculDiMeja()
    {
        if (objekVisualAlatMusikDiMuseum == null)
        {
            Debug.LogWarning("Objek visual " + namaAlatMusik + " belum diisi di Inspector!");
            yield break;
        }

        Transform tr = objekVisualAlatMusikDiMuseum.transform;

        if (posisiTaruhDiMeja != null)
            tr.position = posisiTaruhDiMeja.position;

        objekVisualAlatMusikDiMuseum.SetActive(true);

        Vector3 skalaAsli = tr.localScale;
        float durasi = Mathf.Max(0.01f, durasiMuncul);
        float t = 0f;

        while (t < durasi)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / durasi);

            AturWarnaVisual(Color.Lerp(warnaSiluet, Color.white, k));
            tr.localScale = skalaAsli * (1f + 0.15f * Mathf.Sin(k * Mathf.PI));

            yield return null;
        }

        tr.localScale = skalaAsli;
        AturWarnaVisual(Color.white);
    }

    // Dipakai saat scene dimuat dan alat sudah pernah dipajang
    void TampilkanAlatMusikDiMeja()
    {
        sudahDipajang = true;

        if (objekVisualAlatMusikDiMuseum == null)
        {
            Debug.LogWarning("Objek visual " + namaAlatMusik + " belum diisi di Inspector!");
            return;
        }

        if (posisiTaruhDiMeja != null)
            objekVisualAlatMusikDiMuseum.transform.position = posisiTaruhDiMeja.position;

        objekVisualAlatMusikDiMuseum.SetActive(true);
        AturWarnaVisual(Color.white);
    }

    void TampilkanPesan(string pesan)
    {
        if (teksPesan != null)
            teksPesan.text = pesan;

        if (panelPesan != null)
            panelPesan.SetActive(true);
    }

    // =====================================
    // MENU TES (klik titik tiga di header komponen, saat Play Mode)
    // =====================================

    // Kasih alat ke inventory tanpa harus menyelesaikan quest
    [ContextMenu("TEST: Kasih Alat ke Inventory")]
    void TesKasihAlat()
    {
        CariPlayer();

        if (playerInventory == null)
        {
            Debug.LogWarning("PlayerInventory tidak ketemu. Pastikan sedang Play Mode dan Player ber-tag 'Player'.");
            return;
        }

        if (!playerInventory.PunyaItem(namaAlatMusik))
        {
            playerInventory.daftarItem.Add(namaAlatMusik);
            playerInventory.SimpanInventaris();
            playerInventory.RefreshInventoryUI();
        }

        Debug.Log($"[ManagerDisplay:{namaAlatMusik}] Alat ditambahkan ke inventory (mode tes).");
    }

    // Untuk testing ulang: reset status pajangan alat ini
    [ContextMenu("Reset Display Ini")]
    void ResetDisplay()
    {
        PlayerPrefs.DeleteKey(KeyDisplay);
        PlayerPrefs.Save();
        sudahDipajang = false;
        sedangUrutan = false;

        if (objekVisualAlatMusikDiMuseum != null)
            AturWarnaVisual(warnaSiluet);

        Debug.Log($"[ManagerDisplay:{namaAlatMusik}] Status pajangan direset.");
    }
}