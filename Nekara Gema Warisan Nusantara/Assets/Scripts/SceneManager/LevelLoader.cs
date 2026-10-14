using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LevelLoader : MonoBehaviour
{
    public static LevelLoader Instance;

    [Header("UI Loading")]
    public GameObject loadingScreen;
    public Slider progressBar;
    public TextMeshProUGUI progressText;

    private void Awake()
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

    public void LoadLevel(string sceneName)
    {
        StartCoroutine(LoadAsync(sceneName));
    }

    private IEnumerator LoadAsync(string sceneName)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        if (loadingScreen != null) loadingScreen.SetActive(true);

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            if (progressBar != null) progressBar.value = progress;
            if (progressText != null) progressText.text = (progress * 100f).ToString("F0") + "%";

            yield return null;
        }

        if (loadingScreen != null) loadingScreen.SetActive(false);
    }
}