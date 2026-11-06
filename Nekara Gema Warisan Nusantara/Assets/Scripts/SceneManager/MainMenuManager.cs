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

    [Header("Sprite Tombol (UI Image)")]
    public Image tombolPlayImage;      // Tarik komponen Image dari tombol Btn_Play ke sini
    public Sprite spriteMulai;         // Masukkan sprite gambar tombol "Start"
    public Sprite spriteResume;        // Masukkan sprite gambar tombol "Resume" kamu

    // ゲーム起動中、シーン遷移をまたいで記憶される静的フラグ
    private static bool sudahPernahLoadingAwal = false;

    void Start()
    {
        // Cek dan ganti gambar tombol (Start / Resume)
        PerbaruiSpriteTombol();

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

    // Fungsi untuk mengecek dan mengubah sprite tombol
    void PerbaruiSpriteTombol()
    {
        if (tombolPlayImage != null)
        {
            // Cek apakah pemain sudah pernah memulai game / punya progres
            bool sudahPernahMain = PlayerPrefs.GetInt("GameSudahDimulai", 0) == 1;

            if (sudahPernahMain && spriteResume != null)
            {
                tombolPlayImage.sprite = spriteResume; // Berubah jadi gambar Resume
            }
            else if (!sudahPernahMain && spriteMulai != null)
            {
                tombolPlayImage.sprite = spriteMulai;  // Tetap gambar Start
            }
        }
    }

    // Playボタン用
    public void TombolPlay()
    {
        // Tandai bahwa game sudah pernah dimulai/ditekan
        PlayerPrefs.SetInt("GameSudahDimulai", 1);
        PlayerPrefs.Save();

        // Pindah ke scene prolog atau scene permainan
        SceneManager.LoadScene("08_Prolog");
    }
}