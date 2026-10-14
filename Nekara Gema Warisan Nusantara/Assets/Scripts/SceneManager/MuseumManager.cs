using UnityEngine;
using UnityEngine.SceneManagement;

public class MuseumManager : MonoBehaviour
{
    public static MuseumManager Instance;

    public enum FaseMuseum
    {
        AwalKosong,         
        CraftingAlatMusik,  
        PertempuranKabut,   
        KarnavalRamai       
    }
    public FaseMuseum faseAktif = FaseMuseum.AwalKosong;

    [Header("Status Crafting 3 Alat Musik")]
    public bool sudahCraftingAngklung = false;
    public bool sudahCraftingGesoGeso = false;
    public bool sudahCraftingSarone = false;

    [Header("Referensi Objek di Scene Museum Awal")]
    public GameObject objekKabutDanMusuh;    // Objek musuh & quest di Scene Museum
    public GameObject panelQuizUI;           // Panel UI kuis
    public GameObject objekNPCKarnavalRamai; // Kerumunan karnaval

    void Awake()
    {
        if (Instance == null) 
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); 
        }
        else 
        {
            Destroy(gameObject);
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        CekKondisiMuseum();
    }

    void Update()
    {
        // Setiap frame dicek, khusus saat berada di Scene Museum
        if (SceneManager.GetActiveScene().name == "Scene_Museum")
        {
            CekKondisiMuseum();
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Scene_Museum")
        {
            CekKondisiMuseum();
        }
    }

    public void UbahFase(FaseMuseum faseBaru)
    {
        faseAktif = faseBaru;
        if (faseBaru == FaseMuseum.KarnavalRamai)
        {
            MenangKarnaval();
        }
    }

    // Dipanggil dari NPC Crafting saat alat musik selesai dirakit (TIDAK ADA SceneManager.LoadScene lagi!)
    public void SelesaiCrafting(string namaAlatMusik)
    {
        if (namaAlatMusik == "Angklung") sudahCraftingAngklung = true;
        if (namaAlatMusik == "GesoGeso") sudahCraftingGesoGeso = true;
        if (namaAlatMusik == "Sarone") sudahCraftingSarone = true;
    }

    void CekKondisiMuseum()
    {
        // Cari otomatis objek "Quest" jika slot Inspector kosong
        if (objekKabutDanMusuh == null)
        {
            GameObject foundQuest = GameObject.Find("Quest");
            if (foundQuest != null) objekKabutDanMusuh = foundQuest;
        }

        // SYARAT MUTLAK: KETIGANYA HARUS TRUE (LENGKAP) BARU MUNCUL!
        bool semuaCraftingSelesai = sudahCraftingAngklung && sudahCraftingGesoGeso && sudahCraftingSarone;

        if (semuaCraftingSelesai)
        {
            // Jika 3 alat musik sudah lengkap, nyalakan musuh dan kuis
            if (objekKabutDanMusuh && !objekKabutDanMusuh.activeSelf) objekKabutDanMusuh.SetActive(true);
            if (panelQuizUI && !panelQuizUI.activeSelf) panelQuizUI.SetActive(true);
            if (objekNPCKarnavalRamai && objekNPCKarnavalRamai.activeSelf) objekNPCKarnavalRamai.SetActive(false);
        }
        else
        {
            // Jika belum lengkap (baru 1 atau 2), PAKSA MATIKAN/SEMBUNYIKAN!
            if (objekKabutDanMusuh && objekKabutDanMusuh.activeSelf) objekKabutDanMusuh.SetActive(false);
            if (panelQuizUI && panelQuizUI.activeSelf) panelQuizUI.SetActive(false);
            if (objekNPCKarnavalRamai && objekNPCKarnavalRamai.activeSelf) objekNPCKarnavalRamai.SetActive(false);
        }
    }

    public void MenangKarnaval()
    {
        faseAktif = FaseMuseum.KarnavalRamai;
        if (objekKabutDanMusuh) objekKabutDanMusuh.SetActive(false);
        if (panelQuizUI) panelQuizUI.SetActive(false);
        if (objekNPCKarnavalRamai) objekNPCKarnavalRamai.SetActive(true);
    }
}