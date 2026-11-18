using UnityEngine;

[RequireComponent(typeof(ProximityPrompt))]
public class CollectibleItem : MonoBehaviour
{
    [Header("Nama Item yang Dimasukkan ke Inventori")]
    public string namaItem = "Angklung";

    [Header("Referensi NPC Crafting")]
    public NPCCrafting npcCrafting;

    private bool sudahDiambil = false;
    private ProximityPrompt prompt;

    void Awake()
    {
        prompt = GetComponent<ProximityPrompt>();
    }

    void OnEnable()  { prompt.onTriggered.AddListener(AmbilItem); }
    void OnDisable() { prompt.onTriggered.RemoveListener(AmbilItem); }

    void AmbilItem()
    {
        if (sudahDiambil) return;

        PlayerInventory inventoryPlayer = prompt.Player != null
            ? prompt.Player.GetComponent<PlayerInventory>()
            : null;

        if (inventoryPlayer == null) return;

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
}