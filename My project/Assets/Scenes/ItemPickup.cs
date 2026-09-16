using UnityEngine;
using UnityEngine.InputSystem;

public class ItemPickup : MonoBehaviour
{
    [Header("Info Item")]
    public string itemName = "BambuApus";
    public float interactionDistance = 2.5f; // Jarak maksimal player bisa ambil barang

    private Transform playerTransform;
    private PlayerInventory playerInventory;

    void Start()
    {
        // Mencari objek Player di scene secara otomatis
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            playerInventory = player.GetComponent<PlayerInventory>();
        }
    }

    // Fungsi bawaan Unity saat collider objek ini diklik oleh mouse
    void Update()
{
    // Cukup klik kiri di mana saja
    if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
    {
        if (playerTransform == null) return;

        // Cek jarak antara player dan barang
        float distance = Vector2.Distance(transform.position, playerTransform.position);

        if (distance <= interactionDistance)
        {
            AmbilBarang();
        }
    }
}

void AmbilBarang()
{
    Debug.Log("✨ Berhasil mengambil: " + itemName);
    if (playerInventory != null)
    {
        playerInventory.AddItem(itemName);
    }
    Destroy(gameObject);
}
}