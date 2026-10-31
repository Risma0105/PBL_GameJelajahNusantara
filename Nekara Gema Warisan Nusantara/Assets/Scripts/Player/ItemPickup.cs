using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [Header("Info Bahan Mentah")]
    public string itemName = "BambuApus"; // Nama bahan (contoh: BahanA, BahanB, TaliRotan, dll)

    private bool playerDiDekat = false;
    private PlayerInventory playerInventory;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerDiDekat = true;
            playerInventory = collision.GetComponent<PlayerInventory>();
            Debug.Log("💡 Berada dekat dengan " + itemName + ". Klik untuk mengambil bahan!");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerDiDekat = false;
            playerInventory = null;
        }
    }

    private void OnMouseDown()
    {
        if (playerDiDekat && playerInventory != null)
        {
            Debug.Log("✨ Berhasil mengambil bahan: " + itemName);
            
            // Masukkan nama bahan mentah ke tas inventaris
            playerInventory.AddItem(itemName);
            
            // Simpan inventaris agar aman
            playerInventory.SimpanInventaris();
            playerInventory.RefreshInventoryUI();

            // Hapus objek bahan dari map/scene
            Destroy(gameObject); 
        }
        else
        {
            Debug.Log("⚠️ Terlalu jauh atau player belum mendekat!");
        }
    }
}