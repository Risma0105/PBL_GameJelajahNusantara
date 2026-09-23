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
    public float durasiLoading = 2.5f; // ローディングの表示時間（秒）

    // ゲーム起動中、シーン遷移をまたいで記憶される静的フラグ
    private static bool sudahPernahLoadingAwal = false;

    void Start()
    {
        // 初回起動時かどうかのチェック
        if (!sudahPernahLoadingAwal)
        {
            // --- ゲームを初めて起動した時 ---
            sudahPernahLoadingAwal = true; // 初回ローディング完了フラグを立てる

            if (panelLoading != null) panelLoading.SetActive(true);
            if (panelMainMenu != null) panelMainMenu.SetActive(false);

            StartCoroutine(JalankanLoadingAwal());
        }
        else
        {
            // --- 博物館などの他シーンから戻ってきた時 ---
            // ローディングをスキップして直接メインメニューを開く
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
                sliderLoading.value = timer / durasiLoading; // 0から1へ進行
            }
            yield return null;
        }

        // ローディング完了後、メインメニューを表示
        if (panelLoading != null) panelLoading.SetActive(false);
        if (panelMainMenu != null) panelMainMenu.SetActive(true);
    }

    // Playボタン用
    public void TombolPlay()
    {
        SceneManager.LoadScene("Scene_Museum");
    }
}