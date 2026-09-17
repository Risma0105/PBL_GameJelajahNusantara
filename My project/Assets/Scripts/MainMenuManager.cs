using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Referensi Panel UI")]
    public GameObject panelMainMenu;
    public GameObject panelLoading;
    public Slider loadingSlider; // Slot untuk slider loading

    [Header("Pengaturan Loading")]
    public float loadingDuration = 2.0f;
    public string targetSceneName = "SampleScene";

    void Start()
    {
        if (panelMainMenu != null) panelMainMenu.SetActive(true);
        if (panelLoading != null) panelLoading.SetActive(false);
    }

    public void StartGame()
    {
        StartCoroutine(LoadingRoutine());
    }

    IEnumerator LoadingRoutine()
    {
        if (panelMainMenu != null) panelMainMenu.SetActive(false);
        if (panelLoading != null) panelLoading.SetActive(true);

        float timer = 0f;

        // Mengisi progress bar secara bertahap
        while (timer < loadingDuration)
        {
            timer += Time.deltaTime;
            if (loadingSlider != null)
            {
                loadingSlider.value = timer / loadingDuration;
            }
            yield return null;
        }

        SceneManager.LoadScene(targetSceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}