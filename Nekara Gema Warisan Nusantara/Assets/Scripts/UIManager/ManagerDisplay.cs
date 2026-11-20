using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using TMPro;

public class MuseumDisplay : MonoBehaviour
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

    [Header("Debug")]
    [Tooltip("Cetak alasan kenapa input tidak berhasil di Console")]
    public bool tampilkanLogDebug = true;

    private static readonly Color warnaSiluet = new Color(0.4f, 0.4f, 0.4f, 1f);

    private Transform playerTransform;
    private PlayerInventory playerInventory;
    private bool sudahDipajang = false;

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
            Debug.Log($"[MuseumDisplay:{namaAlatMusik}] {pesan}");
    }

    // Ubah warna SEMUA sprite di objek visual (termasuk anak-anaknya)
    void AturWarnaVisual(Color warna)
    {
        if (objekVisualAlatMusikDiMuseum == null)
            return;

        SpriteRenderer[] semuaSprite =
            objekVisualAlatMusikDiMuseum.GetComponentsInChildren<SpriteRenderer>(true);

        foreach (SpriteRenderer sr in semuaSprite)
            sr.color = warna;
    }

    void Update()
    {
        // Input: klik kiri, klik kanan, atau tombol E
        bool klikKiri = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        bool klikKanan = Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame;
        bool tekanE = Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;

        if (!klikKiri && !klikKanan && !tekanE)
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
            Debug.LogWarning($"[MuseumDisplay:{namaAlatMusik}] Objek ber-tag 'Player' tidak ditemukan!");
            return;
        }

        if (playerInventory == null)
        {
            Debug.LogWarning($"[MuseumDisplay:{namaAlatMusik}] Komponen PlayerInventory tidak ada di objek Player!");
            return;
        }

        // UI hanya menahan klik mouse, tombol E tidak
        bool klikMouse = klikKiri || klikKanan;
        if (klikMouse && EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            Log("Klik tertahan UI (cek Raycast Target pada panel/Image). Coba tekan E.");
            return;
        }

        // Jarak dihitung dari objek alat musik di meja (kalau ada)
        Vector3 titikAcuan = objekVisualAlatMusikDiMuseum != null
            ? objekVisualAlatMusikDiMuseum.transform.position
            : transform.position;

        float jarak = Vector2.Distance(titikAcuan, playerTransform.position);
        bool punya = playerInventory.PunyaItem(namaAlatMusik);
        Log($"Input: jarak={jarak:F2} batas={jarakInteraksi} punya={punya}");

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

        // 1. Simpan status pajangan
        PlayerPrefs.SetInt(KeyDisplay, 1);
        PlayerPrefs.Save();

        // 2. Buka badge
        BadgeManager managerBadge = FindFirstObjectByType<BadgeManager>();
        if (managerBadge != null)
            managerBadge.BukaBadge(namaAlatMusik);

        // 3. Kosongkan alat dari inventory
        playerInventory.daftarItem.RemoveAll(
            item => item.Trim().ToLower() == namaAlatMusik.Trim().ToLower());
        playerInventory.SimpanInventaris();
        playerInventory.RefreshInventoryUI();

        // 4. Tampilkan alat di meja (warna asli)
        TampilkanAlatMusikDiMeja();

        // 5. Kunci progress bar & lanjut ke sekat berikutnya
        if (GameProgress.Instance != null)
        {
            GameProgress.Instance.CollectInstrument(namaAlatMusik);
            GameProgress.Instance.BerhasilTaruhDisplay(namaAlatMusik);
        }
        else
        {
            Debug.LogWarning("GameProgress.Instance null, progress tidak naik dan pintu berikutnya tidak terbuka!");
        }

        TampilkanPesan("✨ Berhasil! " + namaAlatMusik + " berhasil dipajang di museum!");
        Debug.Log("✨ " + namaAlatMusik + " dipajang di museum, inventory dikosongkan, badge terbuka.");
    }

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

    // Untuk testing: klik kanan komponen MuseumDisplay di Inspector > "Reset Display Ini"
    [ContextMenu("Reset Display Ini")]
    void ResetDisplay()
    {
        PlayerPrefs.DeleteKey(KeyDisplay);
        PlayerPrefs.Save();
        sudahDipajang = false;

        if (objekVisualAlatMusikDiMuseum != null)
            AturWarnaVisual(warnaSiluet);

        Debug.Log($"[MuseumDisplay:{namaAlatMusik}] Status pajangan direset.");
    }
}