using UnityEngine;

public class TombolClosePanel : MonoBehaviour
{
    [Header("Target Panel yang Ingin Ditutup")]
    public GameObject panelTarget; // Tarik objek Panel_BadgeUnlock ke sini di Inspector

    // Fungsi ini dipanggil oleh tombol X
    public void TutupPanel()
    {
        if (panelTarget != null)
        {
            panelTarget.SetActive(false);
        }
    }
}