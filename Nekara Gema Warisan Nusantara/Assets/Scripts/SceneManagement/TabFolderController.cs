using UnityEngine;
using UnityEngine.UI;

public class TabFolderController : MonoBehaviour
{
    [System.Serializable]
    public class TabItem
    {
        public Button button;
        public GameObject contentPanel;
        public RectTransform buttonRect; // RectTransform dari tombol
    }

    [Header("Daftar Tab")]
    public TabItem tabBadge;
    public TabItem tabProfile;

    [Header("Efek Tab Aktif")]
    public Vector3 scaleAktif = new Vector3(1.2f, 1.2f, 1f); // Membesar 20%
    public Vector3 scaleNormal = Vector3.one;                 // Ukuran normal (1, 1, 1)
    public float offsetTinggiAktif = 12f;                    // Terangkat naik sedikit
    public Color warnaAktif = Color.white;
    public Color warnaTidakAktif = new Color(0.7f, 0.7f, 0.7f, 1f);

    private float posYAwalBadge;
    private float posYAwalProfile;

    void Start()
    {
        // Catat posisi Y awal agar bisa dikembalikan saat tidak aktif
        if (tabBadge.buttonRect != null) posYAwalBadge = tabBadge.buttonRect.anchoredPosition.y;
        if (tabProfile.buttonRect != null) posYAwalProfile = tabProfile.buttonRect.anchoredPosition.y;

        tabBadge.button.onClick.AddListener(BukaTabBadge);
        tabProfile.button.onClick.AddListener(BukaTabProfile);

        BukaTabBadge();
    }

    public void BukaTabBadge()
    {
        AturStatusTab(tabBadge, posYAwalBadge, true);
        AturStatusTab(tabProfile, posYAwalProfile, false);
    }

    public void BukaTabProfile()
    {
        AturStatusTab(tabBadge, posYAwalBadge, false);
        AturStatusTab(tabProfile, posYAwalProfile, true);
    }

    private void AturStatusTab(TabItem tab, float posYAwal, bool isActive)
    {
        // Buka / tutup panel konten
        if (tab.contentPanel != null) tab.contentPanel.SetActive(isActive);

        if (tab.buttonRect != null)
        {
            // Ubah skala (membesar jika aktif)
            tab.buttonRect.localScale = isActive ? scaleAktif : scaleNormal;

            // Angkat posisinya ke atas sedikit
            Vector2 pos = tab.buttonRect.anchoredPosition;
            pos.y = isActive ? posYAwal + offsetTinggiAktif : posYAwal;
            tab.buttonRect.anchoredPosition = pos;

            // Buat tab aktif berada di lapisan paling depan
            if (isActive) tab.buttonRect.SetAsLastSibling();
        }

        // Ubah warna
        Image img = tab.button.GetComponent<Image>();
        if (img != null)
        {
            img.color = isActive ? warnaAktif : warnaTidakAktif;
        }
    }
}