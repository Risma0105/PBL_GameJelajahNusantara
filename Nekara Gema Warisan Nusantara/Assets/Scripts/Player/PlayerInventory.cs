using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInventory : MonoBehaviour
{
    [Header("Daftar Item di Tas (Data)")]
    public List<string> daftarItem = new List<string>();

    [Header("Pengaturan UI Slot Inventory (4 Slot)")]
    public Image[] slotIcons;          // Tarik Slot_1 sampai Slot_4 dari Panel_Inventory
    public GameObject[] itemPrefabs;   // 👈 Tarik prefab item dari Kumpulan Bahan ke sini!

    public void AddItem(string itemName)
    {
        if (!daftarItem.Contains(itemName))
        {
            daftarItem.Add(itemName);
            Debug.Log($"🎒 [INVENTORI]: {itemName} berhasil disimpan ke tas!");

            // Perbarui tampilan UI Inventory
            UpdateInventoryUI();
        }
    }

    void UpdateInventoryUI()
    {
        for (int i = 0; i < slotIcons.Length; i++)
        {
            if (i < daftarItem.Count)
            {
                // Ambil Sprite dari GameObject item yang cocok
                Sprite itemSprite = GetSpriteFromPrefab(daftarItem[i]);

                if (itemSprite != null)
                {
                    slotIcons[i].sprite = itemSprite;
                    slotIcons[i].gameObject.SetActive(true); // Nyalakan gambar slot
                }
            }
            else
            {
                // Kosongkan slot jika belum ada item
                slotIcons[i].sprite = null;
                slotIcons[i].gameObject.SetActive(false); // Sembunyikan jika kosong
            }
        }
    }

    // Fungsi untuk mengambil komponen Sprite secara otomatis dari objek bahan
    Sprite GetSpriteFromPrefab(string itemName)
    {
        foreach (GameObject prefab in itemPrefabs)
        {
            if (prefab != null && prefab.name.Contains(itemName))
            {
                var spriteRenderer = prefab.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    return spriteRenderer.sprite;
                }
            }
        }
        return null;
    }

    public bool PunyaItem(string itemName)
    {
        return daftarItem.Contains(itemName);
    }
}