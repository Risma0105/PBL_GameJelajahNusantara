
using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [Header("Info Item")]
    public string itemName = "BambuApus";

    private bool playerDiDekat = false;
    private bool sudahDiambil = false;

    private PlayerInventory playerInventory;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerDiDekat = true;
            playerInventory = collision.GetComponent<PlayerInventory>();

            Debug.Log("Dekat dengan " + itemName + ". Klik untuk mengambil!");
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
        // Pastikan belum diambil
        if (sudahDiambil) return;

        // Pastikan pemain berada dekat objek
        if (!playerDiDekat || playerInventory == null)
        {
            Debug.Log("Pemain terlalu jauh dari " + itemName);
            return;
        }

        // Cegah item yang sama masuk dua kali
        if (playerInventory.PunyaItem(itemName))
        {
            Debug.Log(itemName + " sudah ada di inventory!");
            return;
        }

        sudahDiambil = true;

        // Masukkan item ke inventory
        playerInventory.AddItem(itemName);
        playerInventory.RefreshInventoryUI();

        // Jika item adalah Angklung, update GameProgress
        if (itemName.Trim().ToLower() == "angklung")
        {
            if (GameProgress.Instance != null)
            {
                GameProgress.Instance.CollectInstrument("Angklung");
            }
        }

        Debug.Log("Berhasil mengambil " + itemName);

        // Hapus objek dari scene
        Destroy(gameObject);
    }
}
