using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BadgeManager : MonoBehaviour
{
    [System.Serializable]
    public class BadgeData
    {
        public string namaAlat;     // Contoh: "Angklung", "Sarone", "GesoGeso"
        public GameObject badgeObjek; // Objek GameObject badge-nya (untuk digeser kanan-kiri)
        public Image iconImage;     // Komponen Image icon badge
    }

    [Header("Daftar Badge")]
    public List<BadgeData> listBadge = new List<BadgeData>();

    [Header("Warna")]
    public Color warnaTerkunci = new Color(0.25f, 0.25f, 0.25f, 0.7f); // Abu-abu saat terkunci
    public Color warnaTerbuka = Color.white;                            // Warna asli saat terbuka

    private int currentIndex = 0;

    void Start()
    {
        PerbaruiStatusBadge();
        TampilkanBadgeIndex(currentIndex);
    }

    void OnEnable()
    {
        PerbaruiStatusBadge();
        TampilkanBadgeIndex(currentIndex);
    }

    // Cek status terkunci / terbuka via PlayerPrefs
    public void PerbaruiStatusBadge()
    {
        foreach (var badge in listBadge)
        {
            if (badge.iconImage == null) continue;

            bool sudahDidapat = PlayerPrefs.GetInt("Badge_" + badge.namaAlat, 0) == 1;
            
            // DIPERBAIKI DI SINI: Jika sudah didapat pakai warnaTerbuka, jika belum pakai warnaTerkunci
            badge.iconImage.color = sudahDidapat ? warnaTerbuka : warnaTerkunci;
        }
    }

    // Fungsi untuk Tombol Kanan (>)
    public void NextBadge()
    {
        currentIndex++;
        if (currentIndex >= listBadge.Count)
        {
            currentIndex = 0; // Balik ke awal kalau sudah mentok kanan
        }
        TampilkanBadgeIndex(currentIndex);
    }

    // Fungsi untuk Tombol Kiri (<)
    public void PrevBadge()
    {
        currentIndex--;
        if (currentIndex < 0)
        {
            currentIndex = listBadge.Count - 1; // Ke ujung akhir kalau dari awal
        }
        TampilkanBadgeIndex(currentIndex);
    }

    // Menampilkan hanya 1 badge sesuai index yang aktif, yang lain disembunyikan
    void TampilkanBadgeIndex(int index)
    {
        for (int i = 0; i < listBadge.Count; i++)
        {
            if (listBadge[i].badgeObjek != null)
            {
                listBadge[i].badgeObjek.SetActive(i == index);
            }
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