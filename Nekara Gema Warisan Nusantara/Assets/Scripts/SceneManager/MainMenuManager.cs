using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject panelMainMenu;
    public GameObject panelLoading;

    [Header("Loading Component")]
    public Slider sliderLoading;
    public float durasiLoading = 2.5f; 

    [Header("Sprite Tombol (UI Image)")]
    public Image tombolPlayImage;      
    public Sprite spriteMulai;         
    public Sprite spriteResume;        

    private static bool sudahPernahLoadingAwal = false;

    // Supaya objek MainMenuManager ini tidak hancur saat pindah scene dan bisa terus mencatat
    void Awake()
    {
        // Daftarkan fungsi pemantau scene
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        // Hapus pendaftaran saat objek dihancurkan
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Fungsi otomatis yang berjalan setiap kali pindah scene
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Jangan simpan kalau sedang berada di scene MainMenu (00_MainMenu)
        if (scene.name != "00_MainMenu")
        {
            PlayerPrefs.SetString("SceneTerakhir", scene.name);
            PlayerPrefs.Save();
            Debug.Log("📌 Scene terakhir berhasil dicatat: " + scene.name);
        }
    }

    void Start()
    {
        PerbaruiSpriteTombol();

        if (!sudahPernahLoadingAwal)
        {
            sudahPernahLoadingAwal = true; 

            if (panelLoading != null) panelLoading.SetActive(true);
            if (panelMainMenu != null) panelMainMenu.SetActive(false);

            StartCoroutine(JalankanLoadingAwal());
        }
        else
        {
            if (panelLoading != null) panelLoading.SetActive(false);
            if (panelMainMenu != null) panelMainMenu.SetActive(true);
        }
    }

    IEnumerator JalankanLoadingAwal()
    {
        float timer = 0f;

        while (timer < durasiLoading)
        {
            timer += Time.deltaTime;
            if (sliderLoading != null)
            {
                sliderLoading.value = timer / durasiLoading; 
            }
            yield return null;
        }

        if (panelLoading != null) panelLoading.SetActive(false);
        if (panelMainMenu != null) panelMainMenu.SetActive(true);
    }

    void PerbaruiSpriteTombol()
    {
        if (tombolPlayImage != null)
        {
            bool sudahPernahMain = PlayerPrefs.GetInt("GameSudahDimulai", 0) == 1;

            if (sudahPernahMain && spriteResume != null)
            {
                tombolPlayImage.sprite = spriteResume; 
            }
            else if (!sudahPernahMain && spriteMulai != null)
            {
                tombolPlayImage.sprite = spriteMulai;  
            }
        }
    }

    // Fungsi saat tombol Start / Resume diklik
    public void TombolPlay()
    {
        bool sudahPernahMain = PlayerPrefs.GetInt("GameSudahDimulai", 0) == 1;

        if (sudahPernahMain)
        {
            // --- JIKA RESUME: Langsung buka scene terakhir yang tersimpan ---
            string sceneTerakhir = PlayerPrefs.GetString("SceneTerakhir", "08_Prolog");
            SceneManager.LoadScene(sceneTerakhir);
        }
        else
        {
            // --- JIKA START BARU: Mulai dari prolog ---
            PlayerPrefs.SetInt("GameSudahDimulai", 1);
            PlayerPrefs.SetString("SceneTerakhir", "08_Prolog");
            PlayerPrefs.Save();

            SceneManager.LoadScene("08_Prolog");
        }
    }
}