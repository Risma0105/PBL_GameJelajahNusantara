using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [Header("Item Quest Parahyangan")]
    public bool hasBambuApus = false;

    // Fungsi untuk menambah item
    public void AddItem(string itemName)
    {
        if (itemName == "BambuApus")
        {
            hasBambuApus = true;
            Debug.Log("🎒 [INVENTORI]: Bambu Apus berhasil disimpan ke tas!");
        }
    }
}