
using UnityEngine;
using TMPro;

public class MuseumDisplay : MonoBehaviour
{
    [Header("Pengaturan Display Alat Musik")]
    [Tooltip("Isi: Angklung, GesoGeso, atau Sarone")]
    public string namaAlatMusik = "Angklung";

    [Header("Visual & Titik Taruh di Meja")]
    public GameObject objekVisualAlatMusikDiMuseum;
    public Transform posisiTaruhDiMeja;

    [Header("UI Pesan")]
    public GameObject panelPesan;
    public TMP_Text teksPesan;

    [Header("Pengaturan Interaksi")]
    public float jarakInteraksi = 2.0f;

    private Transform playerTransform;
    private PlayerInventory playerInventory;

    private bool sudahDipajang = false;

    // Key penyimpanan khusus untuk setiap alat musik
    private string KeyDisplay
    {
        get
        {
            return "MuseumDisplay_" + namaAlatMusik;
        }
    }

    void Start()
    {
        CariPlayer();

        // Cek apakah alat musik sudah pernah dipajang
        sudahDipajang = PlayerPrefs.GetInt(KeyDisplay, 0) == 1;

        if (sudahDipajang)
        {
            TampilkanAlatMusikDiMeja();
        }
        else
        {
            if (objekVisualAlatMusikDiMuseum != null)
            {
                objekVisualAlatMusikDiMuseum.SetActive(false);
            }
        }

        if (panelPesan != null)
        {
            panelPesan.SetActive(false);
        }
    }

    void CariPlayer()
    {
        GameObject playerObj =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
            playerInventory =
                playerObj.GetComponent<PlayerInventory>();
        }
    }

    void Update()
    {
        if (sudahDipajang)
            return;

        // Cari ulang jika referensi Player belum ditemukan
        if (playerTransform == null || playerInventory == null)
        {
            CariPlayer();
        }

        if (playerTransform == null || playerInventory == null)
            return;

        float jarak = Vector2.Distance(
            transform.position,
            playerTransform.position
        );

        if (jarak <= jarakInteraksi)
        {
            if (Input.GetKeyDown(KeyCode.E) ||
                Input.GetKeyDown(KeyCode.F))
            {
                CobaTaruhAlatMusikDariInventaris();
            }
        }
    }

    // =====================================
    // CEK INVENTORY DAN PAJANG ALAT MUSIK
    // =====================================

    void CobaTaruhAlatMusikDariInventaris()
    {
        if (playerInventory == null)
        {
            Debug.LogWarning(
                "PlayerInventory tidak ditemukan!"
            );
            return;
        }

        // Pastikan alat musik benar-benar ada di inventory
        if (playerInventory.PunyaItem(namaAlatMusik))
        {
            // Simpan status pajangan
            PlayerPrefs.SetInt(KeyDisplay, 1);
            PlayerPrefs.Save();

            // Tampilkan alat musik di meja museum
            TampilkanAlatMusikDiMeja();

            // Tampilkan pesan berhasil
            TampilkanPesan(
                "✨ Berhasil! " + namaAlatMusik +
                " berhasil dipajang di museum!"
            );

            Debug.Log(
                "✨ " + namaAlatMusik +
                " berhasil dipajang di museum!"
            );

            Debug.Log(
                "🎒 Inventory tetap menyimpan " +
                namaAlatMusik +
                ". Jumlah item: " +
                playerInventory.daftarItem.Count
            );

            // PENTING:
            // Tidak ada Remove(), HapusItem(),
            // atau penghapusan data inventory di sini.
        }
        else
        {
            TampilkanPesan(
                "❌ Kamu belum memiliki " +
                namaAlatMusik +
                " di dalam tas inventaris!"
            );

            Debug.LogWarning(
                namaAlatMusik +
                " tidak ditemukan di inventory."
            );
        }
    }

    // =====================================
    // TAMPILKAN ALAT MUSIK DI MEJA
    // =====================================

    void TampilkanAlatMusikDiMeja()
    {
        sudahDipajang = true;

        if (objekVisualAlatMusikDiMuseum != null)
        {
            // Atur posisi objek pada meja
            if (posisiTaruhDiMeja != null)
            {
                objekVisualAlatMusikDiMuseum.transform.position =
                    posisiTaruhDiMeja.position;
            }

            // Aktifkan visual alat musik
            objekVisualAlatMusikDiMuseum.SetActive(true);

            // Pastikan sprite terlihat
            SpriteRenderer spriteRenderer =
                objekVisualAlatMusikDiMuseum
                .GetComponentInChildren<SpriteRenderer>(true);

            if (spriteRenderer != null)
            {
                Color warna = spriteRenderer.color;
                warna.a = 1f;
                spriteRenderer.color = warna;
            }
        }
        else
        {
            Debug.LogWarning(
                "Objek visual " + namaAlatMusik +
                " belum diisi di Inspector!"
            );
        }
    }

    // =====================================
    // PESAN UI
    // =====================================

    void TampilkanPesan(string pesan)
    {
        if (teksPesan != null)
        {
            teksPesan.text = pesan;
        }

        if (panelPesan != null)
        {
            panelPesan.SetActive(true);
        }
    }
}
