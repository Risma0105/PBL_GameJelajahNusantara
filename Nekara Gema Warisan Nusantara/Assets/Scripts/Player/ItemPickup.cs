using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [Header("Info Item")]
    public string itemName = "BambuApus";

    [Header("Sistem Badge (Buka jika ini item alat musik)")]
    public bool bukaBadgeSaatDiambil = false;
    public string namaAlatMusik = "Angklung"; 

    private bool playerDiDekat = false;
    private PlayerInventory playerInventory;

    // Deteksi jika pemain mendekati item menggunakan Trigger 2D
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerDiDekat = true;
            playerInventory = collision.GetComponent<PlayerInventory>();
            Debug.Log("💡 Berada dekat dengan " + itemName + ". Klik item untuk mengambil!");
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

    // Fungsi bawaan Unity: Hanya aktif jika GameObject item ini DIKLIK langsung dengan mouse!
    private void OnMouseDown()
    {
        if (playerDiDekat && playerInventory != null)
        {
            Debug.Log("✨ Berhasil mengambil via klik: " + itemName);
            playerInventory.AddItem(itemName);

            if (bukaBadgeSaatDiambil)
            {
                PlayerPrefs.SetInt("Badge_" + namaAlatMusik, 1);
                PlayerPrefs.Save();
            }

            Destroy(gameObject); // Hapus item dari map
        }
        else
        {
            Debug.Log("⚠️ Terlalu jauh atau player belum mendekat!");
        }
    }
}