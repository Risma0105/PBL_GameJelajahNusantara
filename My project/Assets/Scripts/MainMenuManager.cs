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
    public float durasiLoading = 2.5f; // Berapa detik pura-pura loading

    void Start()
    {
        // Pastikan loading aktif duluan saat game nyala
        if (panelLoading != null) panelLoading.SetActive(true);
        if (panelMainMenu != null) panelMainMenu.SetActive(false);

        StartCoroutine(JalankanLoadingAwal());
    }

    IEnumerator JalankanLoadingAwal()
    {
        float timer = 0f;

        while (timer < durasiLoading)
        {
            timer += Time.deltaTime;
            if (sliderLoading != null)
            {
                sliderLoading.value = timer / durasiLoading; // Mengisi slider 0 sampai 1
            }
            yield return null;
        }

        // Setelah selesai loading, buka menu utama
        if (panelLoading != null) panelLoading.SetActive(false);
        if (panelMainMenu != null) panelMainMenu.SetActive(true);
    }

    // Fungsi untuk tombol Play
    public void TombolPlay()
    {
        SceneManager.LoadScene("SampleScene"); // Pindah ke scene gameplay
    }
}