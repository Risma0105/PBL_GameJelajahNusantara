using UnityEngine;
using UnityEngine.InputSystem;

public class ItemPickup : MonoBehaviour
{
    [Header("Info Item")]
    public string itemName = "BambuApus";

    [Header("Sistem Badge (Buka jika ini item alat musik)")]
    public bool bukaBadgeSaatDiambil = false;
    public string namaAlatMusik = "Angklung"; // Samakan dengan namaAlat di BadgeManager

    private bool isPlayerTouching = false;
    private PlayerInventory playerInventory;

    void Update()
    {
        if (isPlayerTouching && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            AmbilBarang();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerTouching = true;
            playerInventory = collision.GetComponent<PlayerInventory>();
            Debug.Log("💡 Menempel dengan " + itemName + ". Klik kiri untuk mengambil!");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
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

        // Buka badge alat musik jika item ini adalah alat musik jadi
        if (bukaBadgeSaatDiambil)
        {
            PlayerPrefs.SetInt("Badge_" + namaAlatMusik, 1);
            PlayerPrefs.Save();
            Debug.Log("🏅 Badge berhasil dibuka: Badge_" + namaAlatMusik);
        }

        Destroy(gameObject);
    }
}