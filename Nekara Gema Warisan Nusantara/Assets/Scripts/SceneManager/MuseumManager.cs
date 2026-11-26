using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

    [Header("Transisi ke Scene Pertanyaan")]
    public string namaScenePertanyaan = "07_Pertanyaan";
    public float jedaSebelumTransisi = 1.0f;
    public float durasiFade = 1.0f;
    private bool sudahMulaiTransisi = false;

    [Header("Referensi Objek di Scene Museum (dicari otomatis kalau kosong)")]
    public GameObject objekKabutDanMusuh;
    public GameObject panelQuizUI;
    public GameObject objekNPCKarnavalRamai;

    [Header("Nama Scene & Objek (untuk dicari otomatis)")]
    [Tooltip("Harus persis sama dengan nama scene museum di Build Settings (dan namaSceneMuseumAwal di GameProgress).")]
    public string namaSceneMuseum = "05_MuseumDalam";
    public string namaObjekQuest = "Quest";
    public string namaPanelQuiz = "PanelQuiz";
    public string namaNPCKarnaval = "NPCKarnavalRamai";

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
        if (SceneManager.GetActiveScene().name != namaSceneMuseum) return;
        if (Time.unscaledTime < jadwalCek) return;

        jadwalCek = Time.unscaledTime + 0.25f;
        CekKondisiMuseum();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        objekKabutDanMusuh = null;
        panelQuizUI = null;
        objekNPCKarnavalRamai = null;

        if (scene.name == namaSceneMuseum)
        {
            sudahMulaiTransisi = false;
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
        if (namaAlatMusik == "Sarone") sudahCraftingSarone = true;
        if (namaAlatMusik == "GesoGeso") sudahCraftingGesoGeso = true;
    }

    void CekKondisiMuseum()
    {
        if (objekKabutDanMusuh == null)    objekKabutDanMusuh    = CariObjek(namaObjekQuest);
        if (panelQuizUI == null)           panelQuizUI           = CariObjek(namaPanelQuiz);
        if (objekNPCKarnavalRamai == null) objekNPCKarnavalRamai = CariObjek(namaNPCKarnaval);

        if (faseAktif == FaseMuseum.KarnavalRamai)
        {
            SetAktif(objekKabutDanMusuh, false);
            SetAktif(panelQuizUI, false);
            SetAktif(objekNPCKarnavalRamai, true);
            return;
        }

        bool semuaCraftingSelesai = sudahCraftingAngklung && sudahCraftingGesoGeso && sudahCraftingSarone;

        SetAktif(objekKabutDanMusuh, semuaCraftingSelesai);
        SetAktif(panelQuizUI, semuaCraftingSelesai);
        SetAktif(objekNPCKarnavalRamai, false);

        if (semuaCraftingSelesai && !sudahMulaiTransisi)
        {
            sudahMulaiTransisi = true;
            StartCoroutine(ProsesTransisiKePertanyaan());
        }
    }

    private IEnumerator ProsesTransisiKePertanyaan()
    {
        if (jedaSebelumTransisi > 0f)
            yield return new WaitForSeconds(jedaSebelumTransisi);

        // Otomatis ambil TransisiCanvas & LayarHitam yang sudah ada di scene
        GameObject transisiObj = CariObjek("TransisiCanvas");
        Image layarHitam = null;

        if (transisiObj != null)
        {
            transisiObj.SetActive(true);
            Transform tfLayar = transisiObj.transform.Find("LayarHitam");
            if (tfLayar != null)
            {
                tfLayar.gameObject.SetActive(true);
                layarHitam = tfLayar.GetComponent<Image>();
            }
        }

        // Gelapkan layar secara bertahap lewat LayarHitam
        if (layarHitam != null)
        {
            layarHitam.raycastTarget = true;
            Color c = layarHitam.color;
            c.a = 0f;
            layarHitam.color = c;

            float waktu = 0f;
            while (waktu < durasiFade)
            {
                waktu += Time.deltaTime;
                c.a = Mathf.Clamp01(waktu / durasiFade);
                layarHitam.color = c;
                yield return null;
            }

            c.a = 1f;
            layarHitam.color = c;
        }

        if (!string.IsNullOrEmpty(namaScenePertanyaan))
            SceneManager.LoadScene(namaScenePertanyaan);
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