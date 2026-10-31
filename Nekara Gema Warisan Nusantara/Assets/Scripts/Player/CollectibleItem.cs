
using UnityEngine;
using UnityEngine.InputSystem;

public class CollectibleItem : MonoBehaviour
{
    [Header("Nama Item yang Dimasukkan ke Inventori")]
    public string namaItem = "Angklung";

    [Header("Referensi NPC Crafting")]
    public NPCCrafting npcCrafting;

    private bool playerDekat = false;
    private bool sudahDiambil = false;

    private PlayerInventory inventoryPlayer;

    void Update()
    {
        if (playerDekat &&
            !sudahDiambil &&
            Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame)
        {
            AmbilItem();
        }
    }

    void AmbilItem()
    {
        if (inventoryPlayer == null || sudahDiambil)
            return;

        // Masukkan alat musik ke inventory
        inventoryPlayer.AddItem(namaItem);

        // Simpan inventory
        inventoryPlayer.SimpanInventaris();
        inventoryPlayer.RefreshInventoryUI();

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

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerDekat = true;
            inventoryPlayer = other.GetComponent<PlayerInventory>();

            Debug.Log("Tekan klik kiri untuk mengambil " + namaItem);
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
