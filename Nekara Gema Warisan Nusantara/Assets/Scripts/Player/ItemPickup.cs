using UnityEngine;

[RequireComponent(typeof(ProximityPrompt))]
public class ItemPickup : MonoBehaviour
{
    [Header("Info Item")]
    public string itemName = "BambuApus";

    [Header("Pengaturan Progres Culture Book (Opsional)")]
    [Tooltip("Nama alat musik yang terkait dengan bahan ini, contoh: Angklung")]
    public string namaAlatTerkait = "Angklung";

    [Tooltip("Berapa total item bahan yang harus dikumpulkan di scene ini untuk membuka info buku?")]
    public int targetBahanDiScene = 3;

    private bool sudahDiambil = false;
    private ProximityPrompt prompt;

    void Awake()
    {
        prompt = GetComponent<ProximityPrompt>();
    }

    void OnEnable()
    {
        prompt.onTriggered.AddListener(Ambil);
        prompt.onPlayerKeluar.AddListener(ResetGuide);
    }

    void OnDisable()
    {
        prompt.onTriggered.RemoveListener(Ambil);
        prompt.onPlayerKeluar.RemoveListener(ResetGuide);
    }

    void ResetGuide()
    {
        if (UIManager.Instance != null)
            UIManager.Instance.TampilkanDefault();
    }

    void Ambil()
    {
        if (sudahDiambil) return;

        PlayerInventory playerInventory = prompt.Player != null
            ? prompt.Player.GetComponent<PlayerInventory>()
            : null;

        if (playerInventory == null) return;

        if (playerInventory.PunyaItem(itemName))
        {
            string pesanAda = itemName + " sudah ada di inventory!";
            Debug.Log(pesanAda);

            if (UIManager.Instance != null)
                UIManager.Instance.TampilkanPesan(pesanAda);
            return;
        }

        sudahDiambil = true;

        // Masukkan item ke inventory
        playerInventory.AddItem(itemName);
        playerInventory.RefreshInventoryUI();

        // Cek total bahan untuk membuka info Culture Book
        if (!string.IsNullOrEmpty(namaAlatTerkait) && playerInventory.daftarItem != null)
        {
            int jumlahTerkumpul = playerInventory.daftarItem.Count;

            if (jumlahTerkumpul >= targetBahanDiScene)
            {
                CultureBookManager.BukaProgresBuku(namaAlatTerkait, "Bahan");
                Debug.Log("📖 Target bahan " + namaAlatTerkait + " (" + jumlahTerkumpul + "/" + targetBahanDiScene + ") tercapai! Info bahan di Culture Book terbuka.");
            }
        }

        if (itemName.Trim().ToLower() == "angklung")
        {
            if (GameProgress.Instance != null)
                GameProgress.Instance.CollectInstrument("Angklung");
        }

        string pesanBerhasil = "Berhasil mengambil " + itemName;
        Debug.Log(pesanBerhasil);

        if (UIManager.Instance != null)
            UIManager.Instance.TampilkanPesan(pesanBerhasil);

        Destroy(gameObject);
    }
}