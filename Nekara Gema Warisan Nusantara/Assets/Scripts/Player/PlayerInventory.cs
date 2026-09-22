using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [Header("Daftar Item di Tas")]
    public List<string> daftarItem = new List<string>();

    public void AddItem(string itemName)
    {
        if (!daftarItem.Contains(itemName))
        {
            daftarItem.Add(itemName);
            Debug.Log($"🎒 [INVENTORI]: {itemName} berhasil disimpan ke tas!");
        }
    }

    public bool PunyaItem(string itemName)
    {
        return daftarItem.Contains(itemName);
    }
}