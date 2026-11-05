using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [Header("Info Item")]
    public string itemName = "BambuApus";

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

            // Tampilkan ke UIManager saat player mendekat
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

            // Kembali ke teks statis default saat player menjauh
            if (UIManager.Instance != null)
            {
                UIManager.Instance.TampilkanDefault();
            }
        }
    }

    private void OnMouseDown()
    {
        // Pastikan belum diambil
        if (sudahDiambil) return;

        // Pastikan pemain berada dekat objek
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

        // Cegah item yang sama masuk dua kali
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

        // Jika item adalah Angklung, update GameProgress
        if (itemName.Trim().ToLower() == "angklung")
        {
            if (GameProgress.Instance != null)
            {
                GameProgress.Instance.CollectInstrument("Angklung");
            }
        }

        string pesanBerhasil = "Berhasil mengambil " + itemName;
        Debug.Log(pesanBerhasil);

        // Tampilkan pesan sukses ke UIManager
        if (UIManager.Instance != null)
        {
            UIManager.Instance.TampilkanPesan(pesanBerhasil);
        }

        // Hapus objek dari scene
        Destroy(gameObject);
    }
}