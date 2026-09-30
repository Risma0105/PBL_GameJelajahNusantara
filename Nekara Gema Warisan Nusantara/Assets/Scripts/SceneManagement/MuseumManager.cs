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

    public void SelesaiCrafting(string namaAlatMusik)
    {
        if (namaAlatMusik == "Angklung") sudahCraftingAngklung = true;
        if (namaAlatMusik == "GesoGeso") sudahCraftingGesoGeso = true;
        if (namaAlatMusik == "Sarone") sudahCraftingSarone = true;

        SceneManager.LoadScene("Scene_Museum");
    }

    void CekKondisiMuseum()
    {
        // Jaga-jaga jika slot Inspector kosong, cari otomatis objek bernama "Quest" di Scene
        if (objekKabutDanMusuh == null)
        {
            GameObject foundQuest = GameObject.Find("Quest");
            if (foundQuest != null) objekKabutDanMusuh = foundQuest;
        }

        bool semuaCraftingSelesai = sudahCraftingAngklung && sudahCraftingGesoGeso && sudahCraftingSarone;

        if (semuaCraftingSelesai)
        {
            // Jika 3 alat musik sudah selesai, nyalakan musuh dan kuis
            if (objekKabutDanMusuh) objekKabutDanMusuh.SetActive(true);
            if (panelQuizUI) panelQuizUI.SetActive(true);
            if (objekNPCKarnavalRamai) objekNPCKarnavalRamai.SetActive(false);
        }
        else
        {
            // Jika belum lengkap, PAKSA MATIKAN objek Quest di awal!
            if (objekKabutDanMusuh) objekKabutDanMusuh.SetActive(false);
            if (panelQuizUI) panelQuizUI.SetActive(false);
            if (objekNPCKarnavalRamai) objekNPCKarnavalRamai.SetActive(false);
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