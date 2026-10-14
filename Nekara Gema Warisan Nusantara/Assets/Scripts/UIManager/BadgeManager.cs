using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BadgeManager : MonoBehaviour
{
    [System.Serializable]
    public class BadgeData
    {
        public string namaAlat;     // Contoh: "Angklung", "Sarone", "GesoGeso"
        public Image iconImage;     // Komponen Image icon badge
    }

    [Header("Daftar Badge")]
    public List<BadgeData> listBadge = new List<BadgeData>();

    [Header("Warna")]
    public Color warnaTerkunci = new Color(0.25f, 0.25f, 0.25f, 0.7f); // Abu-abu saat terkunci
    public Color warnaTerbuka = Color.white;                            // Warna asli saat terbuka

    void Start()
    {
        PerbaruiStatusBadge();
    }

    // Dipanggil otomatis setiap kali Panel UI Badge diaktifkan/dibuka
    void OnEnable()
    {
        PerbaruiStatusBadge();
    }

    public void PerbaruiStatusBadge()
    {
        foreach (var badge in listBadge)
        {
            if (badge.iconImage == null) continue;

            // PlayerPrefs format: Badge_Angklung (1 = didapat, 0 = belum)
            bool sudahDidapat = PlayerPrefs.GetInt("Badge_" + badge.namaAlat, 0) == 1;

            badge.iconImage.color = sudahDidapat ? warnaTerbuka : warnaTerkunci;
        }
    }

    public void BukaBadge(string namaAlat)
    {
        PlayerPrefs.SetInt("Badge_" + namaAlat, 1);
        PlayerPrefs.Save();
        PerbaruiStatusBadge();
    }

    [ContextMenu("Reset Semua Badge")]
    public void ResetBadge()
    {
        foreach (var badge in listBadge)
        {
            PlayerPrefs.DeleteKey("Badge_" + badge.namaAlat);
        }
        PlayerPrefs.Save();
        PerbaruiStatusBadge();
    }
}