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
            // === JIKA BELUM DIPAJANG: TAMPILKAN SEBAGAI SILUET ABU-ABU DI MEJA ===
            if (objekVisualAlatMusikDiMuseum != null)
            {
                // Atur posisi objek pada meja terlebih dahulu agar terlihat sebagai siluet
                if (posisiTaruhDiMeja != null)
                {
                    objekVisualAlatMusikDiMuseum.transform.position = posisiTaruhDiMeja.position;
                }

                objekVisualAlatMusikDiMuseum.SetActive(true);

                // Ubah warnanya menjadi abu-abu gelap (siluet)
                SpriteRenderer spriteRenderer = objekVisualAlatMusikDiMuseum.GetComponentInChildren<SpriteRenderer>(true);
                if (spriteRenderer != null)
                {
                    Color warnaAbu = new Color(0.4f, 0.4f, 0.4f, 1f); // Abu-abu gelap
                    warnaAbu.a = 1f; // Pastikan tetap terlihat (tidak transparan)
                    spriteRenderer.color = warnaAbu;
                }
            }
        }

        if (panelPesan != null)
        {
            panelPesan.SetActive(false);
        }
    }

    void CariPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
            playerInventory = playerObj.GetComponent<PlayerInventory>();
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
            if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.F))
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
            Debug.LogWarning("PlayerInventory tidak ditemukan!");
            return;
        }

        // Pastikan alat musik benar-benar ada di inventory
        if (playerInventory.PunyaItem(namaAlatMusik))
        {
            // Simpan status pajangan
            PlayerPrefs.SetInt(KeyDisplay, 1);
            PlayerPrefs.Save();

            // OTOMATIS BUKA BADGE SAAT BERHASIL DIPAJANG DI MUSEUM
            BadgeManager managerBadge = FindFirstObjectByType<BadgeManager>();
            if (managerBadge != null)
            {
                managerBadge.BukaBadge(namaAlatMusik);
            }

            // Tampilkan alat musik di meja museum
            TampilkanAlatMusikDiMeja();

            // PENTING: Matikan timer & kunci progress bar via GameProgress
            if (GameProgress.Instance != null)
            {
                GameProgress.Instance.CollectInstrument(namaAlatMusik);
            }

            // Tampilkan pesan berhasil
            TampilkanPesan(
                "✨ Berhasil! " + namaAlatMusik +
                " berhasil dipajang di museum!"
            );

            Debug.Log(
                "✨ " + namaAlatMusik +
                " berhasil dipajang di museum dan badge terbuka!"
            );
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

            // Kembalikan warna sprite menjadi normal/terang
            SpriteRenderer spriteRenderer =
                objekVisualAlatMusikDiMuseum
                .GetComponentInChildren<SpriteRenderer>(true);

            if (spriteRenderer != null)
            {
                Color warnaNormal = Color.white; // Warna asli / terang
                warnaNormal.a = 1f;
                spriteRenderer.color = warnaNormal;
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