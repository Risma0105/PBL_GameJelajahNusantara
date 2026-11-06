using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [Header("Info Item")]
    public string itemName = "BambuApus";

    [Header("Pengaturan Progres Culture Book (Opsional)")]
    [Tooltip("Nama alat musik yang terkait dengan bahan ini, contoh: Angklung")]
    public string namaAlatTerkait = "Angklung";
    
    [Tooltip("Berapa total item bahan yang harus dikumpulkan di scene ini untuk membuka info buku?")]
    public int targetBahanDiScene = 3; 

    private bool playerDiDekat = false;
    private bool sudahDiambil = false;

    private PlayerInventory playerInventory;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerDiDekat = true;
            playerInventory = collision.GetComponent<PlayerInventory>();

            string pesan = "Dekat dengan " + itemName + ". Klik untuk mengambil!";
            Debug.Log(pesan);

            if (UIManager.Instance != null)
            {
                UIManager.Instance.TampilkanPesan(pesan);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerDiDekat = false;
            playerInventory = null;

            if (UIManager.Instance != null)
            {
                UIManager.Instance.TampilkanDefault();
            }
        }
    }

    private void OnMouseDown()
    {
        if (sudahDiambil) return;

        if (!playerDiDekat || playerInventory == null)
        {
            string pesanJauh = "Pemain terlalu jauh dari " + itemName;
            Debug.Log(pesanJauh);

            if (UIManager.Instance != null)
            {
                UIManager.Instance.TampilkanPesan(pesanJauh);
            }
            return;
        }

        if (playerInventory.PunyaItem(itemName))
        {
            string pesanAda = itemName + " sudah ada di inventory!";
            Debug.Log(pesanAda);

            if (UIManager.Instance != null)
            {
                UIManager.Instance.TampilkanPesan(pesanAda);
            }
            return;
        }

        sudahDiambil = true;

        // Masukkan item ke inventory
        playerInventory.AddItem(itemName);
        playerInventory.RefreshInventoryUI();

        // =========================================================================
        // CEK TOTAL BAHAN DI INVENTORY UNTUK MEMBUKA INFO CULTURE BOOK
        // =========================================================================
        if (!string.IsNullOrEmpty(namaAlatTerkait) && playerInventory.daftarItem != null)
        {
            // Menghitung total seluruh item yang sudah terkumpul di inventaris
            int jumlahTerkumpul = playerInventory.daftarItem.Count;

            // Jika jumlah bahan yang dikumpulkan sudah mencapai atau melebihi target scene ini
            if (jumlahTerkumpul >= targetBahanDiScene)
            {
                CultureBookManager.BukaProgresBuku(namaAlatTerkait, "Bahan");
                Debug.Log("📖 Target bahan " + namaAlatTerkait + " (" + jumlahTerkumpul + "/" + targetBahanDiScene + ") tercapai! Info bahan di Culture Book terbuka.");
            }
        }
        // =========================================================================

        if (itemName.Trim().ToLower() == "angklung")
        {
            if (GameProgress.Instance != null)
            {
                GameProgress.Instance.CollectInstrument("Angklung");
            }
        }

        string pesanBerhasil = "Berhasil mengambil " + itemName;
        Debug.Log(pesanBerhasil);

        if (UIManager.Instance != null)
        {
            UIManager.Instance.TampilkanPesan(pesanBerhasil);
        }

        Destroy(gameObject);
    }
}