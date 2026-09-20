using UnityEngine;
using UnityEngine.InputSystem;

public class ItemPickup : MonoBehaviour
{
    [Header("Info Item")]
    public string itemName = "BambuApus";

    private bool isPlayerTouching = false;
    private PlayerInventory playerInventory;

    void Update()
    {
        // Hanya bisa diambil jika karakter sedang benar-benar menempel DAN klik kiri ditekan
        if (isPlayerTouching && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            AmbilBarang();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Mengecek apakah objek yang menempel ber-tag Player
        if (collision.CompareTag("Player"))
        {
            isPlayerTouching = true;
            playerInventory = collision.GetComponent<PlayerInventory>();
            Debug.Log("💡 Menempel dengan " + itemName + ". Klik kiri untuk mengambil!");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // Jika karakter menjauh/tidak menempel lagi
        if (collision.CompareTag("Player"))
        {
            isPlayerTouching = false;
            playerInventory = null;
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