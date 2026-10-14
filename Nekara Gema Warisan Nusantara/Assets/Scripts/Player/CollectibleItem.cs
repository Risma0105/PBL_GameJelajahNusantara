using UnityEngine;
using UnityEngine.InputSystem;

public class CollectibleItem : MonoBehaviour
{
    [Header("Nama Item yang Dimasukkan ke Inventori")]
    public string namaItem = "Angklung"; // Sesuaikan nama item (misal: Angklung, Sarone)

    private bool playerDekat = false;
    private PlayerInventory inventoryPlayer;

    void Update()
    {
        // Jika player dekat dan menekan Klik Kiri Mouse, ambil itemnya
        if (playerDekat && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            AmbilItem();
        }
    }

    void AmbilItem()
    {
        if (inventoryPlayer != null)
        {
            // Memanggil fungsi AddItem dari PlayerInventory-mu
            inventoryPlayer.AddItem(namaItem);

            // Hilangkan (destroy) objek alat musik dari scene setelah masuk tas
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerDekat = true;
            inventoryPlayer = other.GetComponent<PlayerInventory>();
            Debug.Log($"Tekan Klik Kiri untuk mengambil {namaItem}");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerDekat = false;
            inventoryPlayer = null;
        }
    }
}