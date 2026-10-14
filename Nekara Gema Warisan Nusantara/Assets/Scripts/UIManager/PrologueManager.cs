using UnityEngine;

public class PrologueManager : MonoBehaviour
{
    [Header("Nama Scene Museum Awal")]
    public string namaSceneMuseum = "Scene_MuseumDalam"; 

    // Fungsi ini dipanggil pas dialog prolog terakhir selesai
    public void LanjutKeMuseum()
    {
        // Memanggil efek fade transisi global
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.PindahScene(namaSceneMuseum);
            Debug.Log("Prolog selesai, memulai transisi ke " + namaSceneMuseum);
        }
        else
        {
            // Cadangan kalau manajer transisi belum terpasang di scene
            UnityEngine.SceneManagement.SceneManager.LoadScene(namaSceneMuseum);
        }
    }
}