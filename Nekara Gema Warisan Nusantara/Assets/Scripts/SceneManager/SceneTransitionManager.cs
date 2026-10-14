using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance; // Singleton global

    public Image imageFade; // Tarik PanelFade ke sini di Inspector
    public float kecepatanFade = 1f;

    void Awake()
    {
        // Memastikan cuma ada 1 SceneTransitionManager dan tidak hancur saat pindah scene
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

    public void PindahScene(string namaScene)
    {
        StartCoroutine(ProsesFade(namaScene));
    }

    IEnumerator ProsesFade(string namaScene)
    {
        float timer = 0f;
        Color warnaPanel = imageFade.color;

        // Pastikan panel aktif selama fade
        imageFade.gameObject.SetActive(true);

        // Efek Fade to Black (dari transparan 0 ke hitam penuh 1)
        while (timer < 1f)
        {
            timer += Time.deltaTime * kecepatanFade;
            warnaPanel.a = Mathf.Clamp01(timer);
            imageFade.color = warnaPanel;
            yield return null;
        }

        // Setelah layar gelap, pindah scene
        SceneManager.LoadScene(namaScene);
    }
}