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

    // Kunci unik per scene + nama objek (nama pohon harus unik di scene)
    private string KunciTebang
    {
        get { return "Tebang_" + gameObject.scene.name + "_" + gameObject.name; }
    }

    void Start()
    {
        // Pohon yang sudah pernah ditebang tidak muncul lagi
        if (PlayerPrefs.GetInt(KunciTebang, 0) == 1)
        {
            sudahDitebang = true;
            gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !sudahDitebang)
        {
            playerDiDekat = true;

            if (UIManager.Instance != null)
            {
                UIManager.Instance.TampilkanPesan(pesanPetunjuk);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerDiDekat = false;
            if (!sudahDitebang)
            {
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.TampilkanDefault();
                }
            }
        }
    }

    void OnMouseDown()
    {
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

        // Catat pohon ini sudah ditebang supaya tidak muncul lagi setelah Resume
        PlayerPrefs.SetInt(KunciTebang, 1);
        PlayerPrefs.Save();

        string pesanSelesai = "Pohon ditebang habis dan berubah menjadi bahan!";
        Debug.Log(pesanSelesai);

        if (UIManager.Instance != null)
        {
            UIManager.Instance.TampilkanPesan(pesanSelesai);
        }

        if (prefabItemBahan != null)
        {
            Vector3 posisiSpawn = transform.position + new Vector3(0f, -1.2f, 0f);

            GameObject itemBaru = Instantiate(prefabItemBahan, posisiSpawn, Quaternion.identity);

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

        if (UIManager.Instance != null)
        {
            UIManager.Instance.TampilkanDefault();
        }

        Destroy(gameObject);
    }
}