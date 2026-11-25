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

    [Header("Referensi Objek di Scene Museum (dicari otomatis kalau kosong)")]
    public GameObject objekKabutDanMusuh;    // Objek musuh & quest di Scene Museum
    public GameObject panelQuizUI;           // Panel UI kuis
    public GameObject objekNPCKarnavalRamai; // Kerumunan karnaval

    [Header("Nama Scene & Objek (untuk dicari otomatis)")]
    [Tooltip("Harus persis sama dengan nama scene museum di Build Settings (dan namaSceneMuseumAwal di GameProgress).")]
    public string namaSceneMuseum = "05_MuseumDalam";
    public string namaObjekQuest = "Quest";
    public string namaPanelQuiz = "PanelQuiz";          // sesuaikan dengan nama di Hierarchy
    public string namaNPCKarnaval = "NPCKarnavalRamai"; // sesuaikan dengan nama di Hierarchy

    private float jadwalCek = 0f;

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
        if (SceneManager.GetActiveScene().name == namaSceneMuseum)
            CekKondisiMuseum();
    }

    void Update()
    {
        // Hanya di scene museum, dan cukup dicek tiap 0.25 detik (tidak perlu tiap frame)
        if (SceneManager.GetActiveScene().name != namaSceneMuseum) return;
        if (Time.unscaledTime < jadwalCek) return;

        jadwalCek = Time.unscaledTime + 0.25f;
        CekKondisiMuseum();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Referensi lama milik scene sebelumnya sudah hancur -> kosongkan, cari ulang
        objekKabutDanMusuh = null;
        panelQuizUI = null;
        objekNPCKarnavalRamai = null;

        if (scene.name == namaSceneMuseum)
            CekKondisiMuseum();
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
        if (namaAlatMusik == "Sarone") sudahCraftingSarone = true;
        if (namaAlatMusik == "GesoGeso") sudahCraftingGesoGeso = true;
    }

    void CekKondisiMuseum()
    {
        // Cari ulang objek scene kalau referensinya kosong (termasuk yang sedang nonaktif)
        if (objekKabutDanMusuh == null)    objekKabutDanMusuh    = CariObjek(namaObjekQuest);
        if (panelQuizUI == null)           panelQuizUI           = CariObjek(namaPanelQuiz);
        if (objekNPCKarnavalRamai == null) objekNPCKarnavalRamai = CariObjek(namaNPCKarnaval);

        // Karnaval sudah menang: jangan ditimpa lagi oleh logika di bawah
        if (faseAktif == FaseMuseum.KarnavalRamai)
        {
            SetAktif(objekKabutDanMusuh, false);
            SetAktif(panelQuizUI, false);
            SetAktif(objekNPCKarnavalRamai, true);
            return;
        }

        // SYARAT MUTLAK: KETIGANYA HARUS TRUE (LENGKAP) BARU MUNCUL
        bool semuaCraftingSelesai = sudahCraftingAngklung && sudahCraftingGesoGeso && sudahCraftingSarone;

        SetAktif(objekKabutDanMusuh, semuaCraftingSelesai);
        SetAktif(panelQuizUI, semuaCraftingSelesai);
        SetAktif(objekNPCKarnavalRamai, false);
    }

    public void MenangKarnaval()
    {
        faseAktif = FaseMuseum.KarnavalRamai;

        if (objekKabutDanMusuh == null)    objekKabutDanMusuh    = CariObjek(namaObjekQuest);
        if (panelQuizUI == null)           panelQuizUI           = CariObjek(namaPanelQuiz);
        if (objekNPCKarnavalRamai == null) objekNPCKarnavalRamai = CariObjek(namaNPCKarnaval);

        SetAktif(objekKabutDanMusuh, false);
        SetAktif(panelQuizUI, false);
        SetAktif(objekNPCKarnavalRamai, true);
    }

    // Bisa menemukan objek yang nonaktif (GameObject.Find tidak bisa)
    GameObject CariObjek(string nama)
    {
        if (string.IsNullOrEmpty(nama)) return null;

        Transform[] semua = FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Transform t in semua)
        {
            if (t.gameObject.name == nama && t.gameObject.scene.IsValid())
                return t.gameObject;
        }
        return null;
    }

    void SetAktif(GameObject go, bool aktif)
    {
        if (go != null && go.activeSelf != aktif)
            go.SetActive(aktif);
    }
}