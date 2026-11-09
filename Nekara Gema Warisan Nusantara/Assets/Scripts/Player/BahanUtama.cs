using UnityEngine;

public class BahanUtama : MonoBehaviour
{
    [Header("Pengaturan Tebang Pohon")]
    public int jumlahTebasanDibutuhkan = 3;
    private int tebasanSaatIni = 0;
    private bool sudahDitebang = false;

    [Header("Prefab Hasil Panen (Item Bahan Pengganti)")]
    public GameObject prefabItemBahan;
    
    [Header("Pesan Petunjuk Awal")]
    public string pesanPetunjuk = "Klik untuk menebang pohon";

    private bool playerDiDekat = false;

    // 1. Deteksi saat Player masuk ke area lingkaran/collider pohon
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !sudahDitebang)
        {
            playerDiDekat = true;
            
            // Tampilkan pesan petunjuk lewat UIManager
            if (UIManager.Instance != null)
            {
                UIManager.Instance.TampilkanPesan(pesanPetunjuk);
            }
        }
    }

    // 2. Deteksi saat Player menjauh dari area pohon -> Kembali ke teks statis!
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerDiDekat = false;
            if (!sudahDitebang)
            {
                // Suruh UIManager kembali menampilkan teks statis saat keliling map
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.TampilkanDefault();
                }
            }
        }
    }

    void OnMouseDown()
    {
        // Menebang hanya bisa dilakukan jika Player sedang berada di dekat pohon
        if (!sudahDitebang && playerDiDekat)
        {
            TebasPohon();
        }
    }

    void TebasPohon()
    {
        tebasanSaatIni++;
        string pesanProgres = "Memotong pohon... Progress: " + tebasanSaatIni + "/" + jumlahTebasanDibutuhkan;
        
        Debug.Log(pesanProgres);

        // Tampilkan progres tebasan lewat UIManager
        if (UIManager.Instance != null)
        {
            UIManager.Instance.TampilkanPesan(pesanProgres);
        }

        if (tebasanSaatIni >= jumlahTebasanDibutuhkan)
        {
            SelesaikanPenebangan();
        }
    }

    void SelesaikanPenebangan()
    {
        sudahDitebang = true;
        string pesanSelesai = "Pohon ditebang habis dan berubah menjadi bahan!";
        
        Debug.Log(pesanSelesai);

        if (UIManager.Instance != null)
        {
            UIManager.Instance.TampilkanPesan(pesanSelesai);
        }

        if (prefabItemBahan != null)
        {
            // Atur posisi muncul sedikit di bawah pusat pohon (akar pohon)
            // Ubah angka -1.2f jika ingin posisinya disesuaikan lagi
            Vector3 posisiSpawn = transform.position + new Vector3(0f, -1.2f, 0f);
            
            // Buat item di posisi bawah pohon
            GameObject itemBaru = Instantiate(prefabItemBahan, posisiSpawn, Quaternion.identity);
            
            // Masukkan otomatis ke dalam wadah KumpulanBahan di Hierarchy agar rapi
            GameObject kumpulanBahanObj = GameObject.Find("KumpulanBahan");
            if (kumpulanBahanObj != null)
            {
                itemBaru.transform.SetParent(kumpulanBahanObj.transform);
            }
        }
        else
        {
            Debug.LogError("PERINGATAN: Prefab Item Bahan belum dimasukkan di Inspector pada objek " + gameObject.name);
        }

        // Karena objek hancur, kembalikan UI Guide ke teks statis default
        if (UIManager.Instance != null)
        {
            UIManager.Instance.TampilkanDefault();
        }

        Destroy(gameObject);
    }
}