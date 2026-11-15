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
    public float jarakInteraksi = 2.0f;

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

            SpriteRenderer sr = objekVisualAlatMusikDiMuseum.GetComponentInChildren<SpriteRenderer>(true);
            if (sr != null)
                sr.color = new Color(0.4f, 0.4f, 0.4f, 1f);
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

    void Update()
    {
        if (sudahDipajang)
            return;

        if (playerTransform == null || playerInventory == null)
            CariPlayer();

        if (playerTransform == null || playerInventory == null)
            return;

        bool klik = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        if (!klik)
            return;

        // Abaikan klik yang mengenai tombol/panel UI (Pause, Culture Book, dll)
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        float jarak = Vector2.Distance(transform.position, playerTransform.position);
        Debug.Log($"[{namaAlatMusik}] klik: jarak={jarak} batas={jarakInteraksi} punya={playerInventory.PunyaItem(namaAlatMusik)}");

        if (jarak <= jarakInteraksi)
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

        // 4. Tampilkan alat di meja
        TampilkanAlatMusikDiMeja();

        // 5. Kunci progress bar & lanjut ke sekat berikutnya
        if (GameProgress.Instance != null)
        {
            GameProgress.Instance.CollectInstrument(namaAlatMusik);
            GameProgress.Instance.BerhasilTaruhDisplay(namaAlatMusik);
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

        SpriteRenderer sr = objekVisualAlatMusikDiMuseum.GetComponentInChildren<SpriteRenderer>(true);
        if (sr != null)
            sr.color = Color.white;
    }

    void TampilkanPesan(string pesan)
    {
        if (teksPesan != null)
            teksPesan.text = pesan;

        if (panelPesan != null)
            panelPesan.SetActive(true);
    }
}