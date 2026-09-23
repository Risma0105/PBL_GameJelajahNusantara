using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BadgeManager : MonoBehaviour
{
    [System.Serializable]
    public class BadgeData
    {
        public string namaAlat;     // 識別名（例: "Angklung", "Sarone", "GesoGeso"）
        public Image iconImage;     // 対象バッジのImageコンポーネント
    }

    [Header("Daftar Badge")]
    public List<BadgeData> listBadge = new List<BadgeData>();

    [Header("Warna")]
    public Color warnaTerkunci = new Color(0.25f, 0.25f, 0.25f, 0.7f); // 未獲得（グレー）
    public Color warnaTerbuka = Color.white;                            // 獲得済み（元の色）

    void Start()
    {
        PerbaruiStatusBadge();
    }

    // 各バッジの状態を更新
    public void PerbaruiStatusBadge()
    {
        foreach (var badge in listBadge)
        {
            if (badge.iconImage == null) continue;

            // PlayerPrefs で獲得状態を判定 (1 = 獲得済み, 0 = 未獲得)
            bool sudahDidapat = PlayerPrefs.GetInt("Badge_" + badge.namaAlat, 0) == 1;

            // 色を反映
            badge.iconImage.color = sudahDidapat ? warnaTerbuka : warnaTerkunci;
        }
    }

    // ミニゲームクリア時などに呼び出してバッジを解放する関数
    public void BukaBadge(string namaAlat)
    {
        PlayerPrefs.SetInt("Badge_" + namaAlat, 1);
        PlayerPrefs.Save();
        PerbaruiStatusBadge();
    }

    // テスト用：全バッジをリセット
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