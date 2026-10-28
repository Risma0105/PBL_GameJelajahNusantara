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
    private bool sudahDipajang = false;

    void Start()
    {
        // Cari posisi player secara otomatis di scene
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }

        // Cek apakah alat musik ini sudah pernah diselesaikan dari GameManager
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
                    CobaTaruhAlatMusik();
                }
            }
        }
    }

    void CobaTaruhAlatMusik()
    {
        if (GameManager.Instance != null)
        {
            bool sudahCrafting = GameManager.Instance.CekSudahSelesai(namaAlatMusik);

            if (sudahCrafting)
            {
                TampilkanAlatMusikDiMeja();
                TampilkanPesan($"✨ Berhasil! {namaAlatMusik} telah dipajang di museum.");
                Debug.Log($"✨ {namaAlatMusik} berhasil dipajang di museum!");
            }
            else
            {
                TampilkanPesan($"❌ Kamu belum menyelesaikan crafting untuk {namaAlatMusik}!");
                Debug.Log($"❌ Kamu belum memiliki hasil crafting {namaAlatMusik}.");
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