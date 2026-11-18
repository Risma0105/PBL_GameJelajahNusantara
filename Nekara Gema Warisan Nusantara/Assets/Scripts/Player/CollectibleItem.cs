using UnityEngine;
using UnityEngine.InputSystem;

public class CollectibleItem : MonoBehaviour
{
    [Header("Nama Item yang Dimasukkan ke Inventori")]
    public string namaItem = "Angklung";

    [Header("Referensi NPC Crafting (kosong untuk bahan mentah)")]
    public NPCCrafting npcCrafting;

    private bool playerDekat = false;
    private bool sudahDiambil = false;

    private PlayerInventory inventoryPlayer;

    // Kunci unik per scene + nama objek (nama objek bahan harus unik di scene)
    private string KunciDiambil
    {
        get { return "Bahan_" + gameObject.scene.name + "_" + gameObject.name; }
    }

    // Bahan mentah = tidak terhubung ke NPC crafting
    private bool BahanMentah
    {
        get { return npcCrafting == null; }
    }

    void Start()
    {
        // Bahan yang sudah pernah diambil tidak muncul lagi
        if (BahanMentah && PlayerPrefs.GetInt(KunciDiambil, 0) == 1)
        {
            gameObject.SetActive(false);
        }
    }

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

        inventoryPlayer.AddItem(namaItem);
        inventoryPlayer.SimpanInventaris();
        inventoryPlayer.RefreshInventoryUI();

        sudahDiambil = true;

        if (npcCrafting != null)
        {
            npcCrafting.KonfirmasiAlatDiambil();
        }
        else
        {
            // Catat bahan ini sudah diambil supaya tidak muncul lagi setelah Resume
            PlayerPrefs.SetInt(KunciDiambil, 1);
            PlayerPrefs.Save();
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