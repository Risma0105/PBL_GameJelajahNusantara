using UnityEngine;
using TMPro;

public class MuseumDisplay : MonoBehaviour
{
    [Header("Pengaturan Display Alat Musik")]
    [Tooltip("Isi dengan nama alat musik yang sesuai: Angklung, GesoGeso, atau Sarone")]
    public string namaAlatMusik = "Angklung"; 
    
    [Header("Visual & Titik Taruh di Meja")]
    public GameObject objekVisualAlatMusikDiMuseum; // Sprite/objek alat musik di meja
    public Transform posisiTaruhDiMeja;             // Titik kosong (GameObject Empty) tepat di atas permukaan meja
    
    [Header("UI Pesan (Opsional)")]
    public GameObject panelPesan;                  
    public TMP_Text teksPesan;                     

    [Header("Pengaturan Interaksi")]
    public float jarakInteraksi = 2.0f;              
    private Transform playerTransform;
    private PlayerInventory playerInventory; // Referensi ke inventaris pemain
    private bool sudahDipajang = false;

    void Start()
    {
        // Cari posisi player dan komponen PlayerInventory secara otomatis di scene
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
            playerInventory = playerObj.GetComponent<PlayerInventory>();
        }

        // Cek apakah alat musik ini sudah pernah dipajang sebelumnya (melalui GameManager)
        if (GameManager.Instance != null && GameManager.Instance.CekSudahSelesai(namaAlatMusik))
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
    }

    void Update()
    {
        if (sudahDipajang) return;

        // Cek jarak antara player dan meja display
        if (playerTransform != null)
        {
            float jarak = Vector2.Distance(transform.position, playerTransform.position);

            if (jarak <= jarakInteraksi)
            {
                // Pemain mendekat dan menekan tombol interaksi (E atau F)
                if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.F))
                {
                    CobaTaruhAlatMusikDariInventaris();
                }
            }
        }
    }

    void CobaTaruhAlatMusikDariInventaris()
    {
        if (playerInventory != null && GameManager.Instance != null)
        {
            // Cek apakah alat musiknya ada di dalam tas inventaris pemain
            bool adaDiTas = playerInventory.PunyaItem(namaAlatMusik);

            if (adaDiTas)
            {
                // Tandai selesai di GameManager pusat agar tersimpan permanen
                GameManager.Instance.TandaiSelesai(namaAlatMusik);

                // Tampilkan visual alat musik di meja museum
                TampilkanAlatMusikDiMeja();

                // Opsional: Jika ingin itemnya hilang dari tas setelah dipajang, bisa tambahkan fungsi hapus item di sini jika ada.
                
                TampilkanPesan($"✨ Berhasil! {namaAlatMusik} dari tas telah dipajang di museum.");
                Debug.Log($"✨ {namaAlatMusik} berhasil dipajang di museum dari inventaris!");
            }
            else
            {
                TampilkanPesan($"❌ Kamu belum memiliki {namaAlatMusik} di dalam tas inventaris!");
                Debug.Log($"❌ {namaAlatMusik} tidak ditemukan di tas pemain.");
            }
        }
    }

    void TampilkanAlatMusikDiMeja()
    {
        sudahDipajang = true;

        if (objekVisualAlatMusikDiMuseum != null)
        {
            // Jika titik taruh di meja diatur, sesuaikan posisinya
            if (posisiTaruhDiMeja != null)
            {
                objekVisualAlatMusikDiMuseum.transform.position = posisiTaruhDiMeja.position;
            }
            
            objekVisualAlatMusikDiMuseum.SetActive(true);
        }
    }

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