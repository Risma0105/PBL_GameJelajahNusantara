using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInventory : MonoBehaviour
{
    [Header("Daftar Item di Tas (Data)")]
    public List<string> daftarItem = new List<string>();

    [Header("Pengaturan UI Slot Inventory (4 Slot)")]
    public Image[] slotIcons;          // Tarik Slot_1 sampai Slot_4 dari Panel_Inventory
    public GameObject[] itemPrefabs;   // Tarik prefab item dari Kumpulan Bahan ke sini

    public void AddItem(string itemName)
    {
        // Cek agar item yang sama tidak ditambahkan berkali-kali secara dobel
        if (!daftarItem.Contains(itemName))
        {
            daftarItem.Add(itemName);
            Debug.Log($"🎒 [INVENTORI]: {itemName} berhasil disimpan ke tas!");
            UpdateInventoryUI();
        }
        else
        {
            Debug.Log($"⚠️ [INVENTORI]: {itemName} sudah ada di dalam tas!");
        }
    }

    public void RefreshInventoryUI()
    {
        UpdateInventoryUI();
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
                    
                    // Munculkan gambar item dengan warna penuh (Alpha = 1)
                    Color warnaSlot = slotIcons[i].color;
                    warnaSlot.a = 1f; 
                    slotIcons[i].color = warnaSlot;
                }
            }
            else
            {
                // Kosongkan slot jika belum ada item
                slotIcons[i].sprite = null;
                
                // Bikin transparan (Alpha = 0) supaya background/desain slot aslinya kelihatan!
                Color warnaSlot = slotIcons[i].color;
                warnaSlot.a = 0f; 
                slotIcons[i].color = warnaSlot;
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