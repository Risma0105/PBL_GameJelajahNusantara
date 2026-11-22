using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CollectibleItem : MonoBehaviour
{
    [Header("Nama Item yang Dimasukkan ke Inventori")]
    public string namaItem = "Angklung";

    [Header("Referensi NPC Crafting")]
    public NPCCrafting npcCrafting;

    [Header("Klik Mouse")]
    [Tooltip("Klik kiri pada alat musik untuk mengambilnya.")]
    public bool bisaDiklik = true;
    [Tooltip("Jarak maksimal player ke alat musik supaya bisa diambil. 0 = tanpa batas.")]
    public float jarakAmbilMaks = 4f;
    [Tooltip("Alat musik sedikit membesar saat kursor di atasnya.")]
    public bool efekHover = true;
    public float skalaHover = 1.1f;
    [Tooltip("Aktif = klik hanya diblok kalau kursor di atas TOMBOL/UI interaktif. Mati = UI apa pun (termasuk gambar transparan) memblok klik.")]
    public bool hanyaBlokirTombolUI = true;

    [Header("Debug")]
    [Tooltip("Cetak info ke Console setiap kali klik, untuk melacak kenapa klik tidak jalan.")]
    public bool debugKlik = true;

    private bool sudahDiambil = false;
    private ProximityPrompt prompt;   // opsional: kalau masih ada di prefab, tombol E tetap berfungsi
    private PlayerInventory inventoryPlayer;
    private Camera kamera;
    private SpriteRenderer spriteRenderer;
    private Vector3 skalaAsli;
    private bool sedangHover = false;
    private readonly List<RaycastResult> hasilUI = new List<RaycastResult>();

    void Awake()
    {
        prompt = GetComponent<ProximityPrompt>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        skalaAsli = transform.localScale;

        // Klik butuh Collider2D. Kalau prefab belum punya, tambahkan otomatis sesuai ukuran sprite.
        if (GetComponentInChildren<Collider2D>() == null)
        {
            BoxCollider2D box = gameObject.AddComponent<BoxCollider2D>();
            box.isTrigger = true;

            if (spriteRenderer != null && spriteRenderer.sprite != null)
            {
                Bounds b = spriteRenderer.sprite.bounds;
                box.size = b.size;
                box.offset = b.center;
            }

            Debug.Log("[Collectible] Collider2D otomatis ditambahkan ke " + gameObject.name);
        }

        if (debugKlik)
            Debug.Log("[Collectible] " + namaItem + " muncul di " + transform.position);
    }

    void OnEnable()
    {
        if (prompt != null) prompt.onTriggered.AddListener(AmbilViaPrompt);
    }

    void OnDisable()
    {
        if (prompt != null) prompt.onTriggered.RemoveListener(AmbilViaPrompt);
    }

    void Update()
    {
        if (sudahDiambil || !bisaDiklik) return;

        bool klik = Input.GetMouseButtonDown(0);

        // Selama panel badge terbuka, alat musik belum bisa diambil
        bool panelTerbuka = npcCrafting != null &&
                            npcCrafting.panelBadgeUnlock != null &&
                            npcCrafting.panelBadgeUnlock.activeInHierarchy;
        if (panelTerbuka)
        {
            if (klik && debugKlik) Debug.Log("[Collectible] klik diabaikan: panel badge masih terbuka.");
            return;
        }

        string penghalang;
        bool uiMenghalangi = KursorDiAtasUI(out penghalang);
        bool diAtasItem = !uiMenghalangi && KursorDiAtasItem();

        // Efek hover
        if (efekHover && diAtasItem != sedangHover)
        {
            sedangHover = diAtasItem;
            transform.localScale = skalaAsli * (sedangHover ? skalaHover : 1f);
        }

        if (!klik) return;

        if (debugKlik)
        {
            Debug.Log("[Collectible] klik | item di " + transform.position +
                      " | kursor (dunia) di " + PosisiKursorDunia() +
                      " | UI menghalangi: " + (uiMenghalangi ? penghalang : "tidak") +
                      " | kursor di atas item: " + diAtasItem);
        }

        if (diAtasItem)
            CobaAmbilLewatKlik();
    }

    Vector2 PosisiKursorDunia()
    {
        if (kamera == null) kamera = Camera.main;
        if (kamera == null) return Vector2.zero;
        return kamera.ScreenToWorldPoint(Input.mousePosition);
    }

    // Apakah kursor ada di atas UI yang memblok klik?
    bool KursorDiAtasUI(out string namaPenghalang)
    {
        namaPenghalang = null;
        if (EventSystem.current == null) return false;

        PointerEventData data = new PointerEventData(EventSystem.current);
        data.position = Input.mousePosition;

        hasilUI.Clear();
        EventSystem.current.RaycastAll(data, hasilUI);
        if (hasilUI.Count == 0) return false;

        if (!hanyaBlokirTombolUI)
        {
            namaPenghalang = hasilUI[0].gameObject.name;
            return true;
        }

        // Hanya blok kalau ada tombol/elemen interaktif di bawah kursor
        foreach (RaycastResult h in hasilUI)
        {
            if (h.gameObject.GetComponentInParent<Selectable>() != null)
            {
                namaPenghalang = h.gameObject.name;
                return true;
            }
        }

        return false;
    }

    // Apakah kursor ada di atas alat musik ini?
    bool KursorDiAtasItem()
    {
        if (kamera == null) kamera = Camera.main;
        if (kamera == null) return false;

        Vector2 posisiDunia = kamera.ScreenToWorldPoint(Input.mousePosition);

        // Cara 1: collider (OverlapPointAll supaya tidak terhalang collider lain yang menumpuk)
        Collider2D[] kena = Physics2D.OverlapPointAll(posisiDunia);
        foreach (Collider2D c in kena)
        {
            if (c.gameObject == gameObject || c.transform.IsChildOf(transform))
                return true;
        }

        // Cara 2 (cadangan): area sprite, tidak bergantung pada collider
        if (spriteRenderer != null)
        {
            Bounds b = spriteRenderer.bounds;
            if (b.Contains(new Vector3(posisiDunia.x, posisiDunia.y, b.center.z)))
                return true;
        }

        return false;
    }

    void CobaAmbilLewatKlik()
    {
        if (inventoryPlayer == null)
            inventoryPlayer = FindFirstObjectByType<PlayerInventory>();

        if (inventoryPlayer == null)
        {
            Debug.LogWarning("[Collectible] PlayerInventory tidak ditemukan di scene!");
            return;
        }

        // Cek jarak player ke alat musik
        if (jarakAmbilMaks > 0f)
        {
            float jarak = Vector2.Distance(inventoryPlayer.transform.position, transform.position);
            if (jarak > jarakAmbilMaks)
            {
                if (debugKlik)
                    Debug.Log("[Collectible] terlalu jauh: " + jarak.ToString("F1") + " > " + jarakAmbilMaks);

                if (UIManager.Instance != null)
                    UIManager.Instance.TampilkanPesan("Dekati " + namaItem + " untuk mengambilnya.");
                return;
            }
        }

        AmbilItem(inventoryPlayer);
    }

    // Jalur lama: tombol E lewat ProximityPrompt (hanya kalau prompt masih terpasang)
    void AmbilViaPrompt()
    {
        PlayerInventory inv = (prompt != null && prompt.Player != null)
            ? prompt.Player.GetComponent<PlayerInventory>()
            : null;

        if (inv == null) return;

        AmbilItem(inv);
    }

    void AmbilItem(PlayerInventory inventory)
    {
        if (sudahDiambil) return;

        // Masukkan alat musik ke inventory
        inventory.AddItem(namaItem);

        // Simpan inventory
        inventory.SimpanInventaris();
        inventory.RefreshInventoryUI();

        sudahDiambil = true;

        // Tandai alat musik selesai setelah benar-benar diambil
        if (npcCrafting != null)
        {
            npcCrafting.KonfirmasiAlatDiambil();
        }
        else if (GameProgress.Instance != null)
        {
            GameProgress.Instance.CollectInstrument(namaItem);
        }

        Debug.Log(namaItem + " berhasil diambil dan masuk inventory!");

        Destroy(gameObject);
    }
}