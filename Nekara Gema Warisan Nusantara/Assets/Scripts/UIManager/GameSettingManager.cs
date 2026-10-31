
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameSettingsManager : MonoBehaviour
{
    [Header("Panel Settings Utama")]
    public GameObject panelSettings;

    [Header("Tombol Pause / Resume")]
    public TMP_Text teksTombolPause;

    private bool isGamePaused = false;

    void Start()
    {
        // Pastikan panel settings tertutup
        if (panelSettings != null)
            panelSettings.SetActive(false);

        // Pastikan game berjalan saat awal
        Time.timeScale = 1f;
        isGamePaused = false;

        if (teksTombolPause != null)
            teksTombolPause.text = "Pause";
    }

    // 1. Membuka Panel Settings
    public void TogglePanelSettings()
{
    if (panelSettings != null)
        panelSettings.SetActive(!panelSettings.activeSelf);
}

    // 2. Pause dan Resume Game
    public void PauseGameTotal()
    {
        isGamePaused = !isGamePaused;

        if (isGamePaused)
        {
            Time.timeScale = 0f;

            if (teksTombolPause != null)
                teksTombolPause.text = "Resume";
        }
        else
        {
            Time.timeScale = 1f;

            if (teksTombolPause != null)
                teksTombolPause.text = "Pause";
        }
    }

    // 3. Kembali ke Home
    public void KembaliKePlayerHub(string namaSceneHub)
{
    Time.timeScale = 1f;
    isGamePaused = false;

    SceneManager.LoadScene(namaSceneHub);
}

    // 4. Mengatur Volume Musik
    public void AturVolumeBGM(float volume)
    {
        AudioListener.volume = volume;
    }
}
