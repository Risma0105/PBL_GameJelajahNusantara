using UnityEngine;

// Pasang di tiap objek pajangan (Angklung, GesoGeso, Sarone).
// Objek harus punya SpriteRenderer dan Collider2D (misal BoxCollider2D).
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public class MuseumDisplaySlot : MonoBehaviour
{
    [Tooltip("Harus sama dengan nama item yang dipakai saat AddItem(...) di crafting. Contoh: angklung, gesogeso, sarone")]
    public string itemId;

    [Header("Tampilan")]
    public Color silhouetteColor = new Color(0.2f, 0.2f, 0.2f, 1f);
    public Color displayedColor = Color.white;

    SpriteRenderer sr;
    PlayerInventory inventory;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        inventory = FindFirstObjectByType<PlayerInventory>();
    }

    void Start()
    {
        Refresh();
    }

    public void Refresh()
    {
        bool displayed = MuseumData.IsDisplayed(itemId);
        sr.color = displayed ? displayedColor : silhouetteColor;
    }

    // Dipanggil otomatis saat klik kiri pada collider
    void OnMouseDown()
    {
        if (MuseumData.IsDisplayed(itemId)) return;

        if (inventory == null)
        {
            inventory = FindFirstObjectByType<PlayerInventory>();
            if (inventory == null)
            {
                Debug.LogWarning("PlayerInventory tidak ditemukan di scene museum.");
                return;
            }
        }

        if (inventory.PunyaItem(itemId))
        {
            inventory.RemoveItem(itemId);
            MuseumData.SetDisplayed(itemId, true);
            Refresh();
            Debug.Log($"🏛️ [MUSEUM]: {itemId} berhasil dipajang!");
        }
        else
        {
            Debug.Log($"🏛️ [MUSEUM]: Kamu belum punya {itemId}.");
            // TODO: tampilkan pesan di UI
        }
    }
}