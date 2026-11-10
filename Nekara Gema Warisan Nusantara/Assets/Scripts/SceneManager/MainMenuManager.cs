using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject panelMainMenu;
    public GameObject panelLoading;
    public GameObject uiGameObj;

    [Header("Loading Component")]
    public Slider sliderLoading;
    public float durasiLoading = 2.5f; 

    [Header("Sprite Tombol (UI Image)")]
    public Image tombolPlayImage;      
    public Sprite spriteMulai;         
    public Sprite spriteResume;        

    private static bool sudahPernahLoadingAwal = false;

    void Start()
    {
        PerbaruiSpriteTombol();

        if (!sudahPernahLoadingAwal)
        {
            sudahPernahLoadingAwal = true; 

            if (panelLoading != null) panelLoading.SetActive(true);
            if (panelMainMenu != null) panelMainMenu.SetActive(false);
            if (uiGameObj != null) uiGameObj.SetActive(false); // Sembunyikan UI game saat loading awal

            StartCoroutine(JalankanLoadingAwal());
        }
        else
        {
            if (panelLoading != null) panelLoading.SetActive(false);
            if (panelMainMenu != null) panelMainMenu.SetActive(true);
            if (uiGameObj != null) uiGameObj.SetActive(true); // Munculkan UI game jika sudah melewati loading
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
        if (uiGameObj != null) uiGameObj.SetActive(true); // Aktifkan UI game setelah bar loading selesai
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

    // Fungsi saat tombol Start / Resume diklik di Main Menu
    public void TombolPlay()
    {
        bool sudahPernahMain = PlayerPrefs.GetInt("GameSudahDimulai", 0) == 1;

        if (sudahPernahMain)
        {
            // --- JIKA RESUME: Ambil nama scene terakhir yang terekam ---
            string sceneTerakhir = PlayerPrefs.GetString("SceneTerakhir", "08_Prolog");
            
            // Validasi agar tidak meload scene Main Menu lagi
            if (sceneTerakhir == "00_MainMenu" || string.IsNullOrEmpty(sceneTerakhir))
            {
                sceneTerakhir = "08_Prolog";
            }

            Debug.Log("▶️ Melanjutkan permainan ke scene: " + sceneTerakhir);
            SceneManager.LoadScene(sceneTerakhir);
        }
        else
        {
            // --- JIKA START BARU ---
            PlayerPrefs.SetInt("GameSudahDimulai", 1);
            PlayerPrefs.SetString("SceneTerakhir", "08_Prolog");
            PlayerPrefs.Save();

            SceneManager.LoadScene("08_Prolog");
        }
    }
}