using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CultureBookManager : MonoBehaviour
{
    [System.Serializable]
    public class CultureItem
    {
        public string namaAlat; // Contoh: "Angklung" (untuk mencocokkan kunci progres)
        public string judul;
        
        [Header("Tahapan Deskripsi Berdasarkan Progres")]
        [TextArea(2, 4)] public string infoNPCWarlok;    // Terbuka setelah ngobrol sama NPC Warlok
        [TextArea(2, 4)] public string infoBahan;       // Terbuka setelah berhasil ambil 3 bahan
        [TextArea(2, 4)] public string infoCrafting;    // Terbuka setelah berhasil crafting alat musik
        
        public Sprite gambarAlatMusik;
    }

    [Header("Daftar Konten Buku")]
    public List<CultureItem> daftarBudaya = new List<CultureItem>();
    private int indexSekarang = 0;

    [Header("Komponen UI (Halaman Kiri: Teks | Halaman Kanan: Gambar)")]
    public TextMeshProUGUI txtJudulKiri;
    public TextMeshProUGUI txtDeskripsiKiri;
    public Image imgKanan;

    [Header("Transform Lembaran untuk Animasi Flip")]
    public RectTransform lembarKanan;
    public RectTransform lembarKiri;
    public float durasiBalik = 0.2f;

    [Header("Tombol Navigasi")]
    public Button btnNext;
    public Button btnPrev;

    private bool sedangAnimasi = false;

    void Start()
    {
        UpdateTampilanLangsung();
    }

    public void HalamanBerikutnya()
    {
        if (sedangAnimasi || indexSekarang >= daftarBudaya.Count - 1) return;
        StartCoroutine(AnimasiBalikHalaman(true));
    }

    public void HalamanSebelumnya()
    {
        if (sedangAnimasi || indexSekarang <= 0) return;
        StartCoroutine(AnimasiBalikHalaman(false));
    }

    IEnumerator AnimasiBalikHalaman(bool keKanan)
    {
        sedangAnimasi = true;

        RectTransform lembarTarget = keKanan ? lembarKanan : lembarKiri;
        float arah = keKanan ? 1f : -1f;

        float waktu = 0f;
        while (waktu < durasiBalik)
        {
            waktu += Time.deltaTime;
            float rotYProgress = Mathf.Lerp(0f, 90f * arah, waktu / durasiBalik);
            if (lembarTarget != null) lembarTarget.localEulerAngles = new Vector3(0, rotYProgress, 0);
            yield return null;
        }

        if (keKanan) indexSekarang++;
        else indexSekarang--;
        UpdateTampilanLangsung();

        waktu = 0f;
        while (waktu < durasiBalik)
        {
            waktu += Time.deltaTime;
            float rotYProgress = Mathf.Lerp(90f * arah, 0f, waktu / durasiBalik);
            if (lembarTarget != null) lembarTarget.localEulerAngles = new Vector3(0, rotYProgress, 0);
            yield return null;
        }

        if (lembarTarget != null) lembarTarget.localEulerAngles = Vector3.zero;
        
        sedangAnimasi = false;
        UpdateTampilanLangsung();
    }

    void UpdateTampilanLangsung()
    {
        if (daftarBudaya.Count == 0) return;

        CultureItem data = daftarBudaya[indexSekarang];

        if (txtJudulKiri != null) txtJudulKiri.text = data.judul;

        // --- PENGATURAN GAMBAR ALAT MUSIK & EFEK ABU-ABU (BADGE STYLE) ---
        if (imgKanan != null)
        {
            imgKanan.sprite = data.gambarAlatMusik;

            // Cek apakah ketiga progres sudah terbuka semua
            bool progressNPC = PlayerPrefs.GetInt("Quest_Warlok_" + data.namaAlat, 0) == 1;
            bool progressBahan = PlayerPrefs.GetInt("Quest_Bahan_" + data.namaAlat, 0) == 1;
            bool progressCraft = PlayerPrefs.GetInt("Quest_Craft_" + data.namaAlat, 0) == 1;

            bool semuaTerbuka = (progressNPC && progressBahan && progressCraft);

            if (semuaTerbuka)
            {
                // Jika sudah lengkap semua, gambar berwarna normal (terang)
                imgKanan.color = Color.white; 
            }
            else
            {
                // Jika belum lengkap, gambar berubah jadi abu-abu gelap (terkunci)
                imgKanan.color = new Color(0.4f, 0.4f, 0.4f, 1f); 
            }
        }
        // -----------------------------------------------------------------

        // --- GABUNGKAN DESKRIPSI BERDASARKAN PROGRES PLAYER ---
        if (txtDeskripsiKiri != null)
        {
            string teksGabungan = "";

            // Cek Progres 1: Ngobrol sama NPC Warlok
            bool progressNPC = PlayerPrefs.GetInt("Quest_Warlok_" + data.namaAlat, 0) == 1;
            // Cek Progres 2: Berhasil ambil 3 bahan
            bool progressBahan = PlayerPrefs.GetInt("Quest_Bahan_" + data.namaAlat, 0) == 1;
            // Cek Progres 3: Berhasil crafting
            bool progressCraft = PlayerPrefs.GetInt("Quest_Craft_" + data.namaAlat, 0) == 1;

            if (progressNPC)
            {
                teksGabungan += data.infoNPCWarlok + "\n\n";
            }
            else
            {
                teksGabungan += "🔒 Temui Warlok setempat untuk membuka informasi awal...\n\n";
            }

            if (progressBahan)
            {
                teksGabungan += data.infoBahan + "\n\n";
            }

            if (progressCraft)
            {
                teksGabungan += data.infoCrafting;
            }

            txtDeskripsiKiri.text = teksGabungan;
        }

        if (btnPrev != null) btnPrev.interactable = (indexSekarang > 0 && !sedangAnimasi);
        if (btnNext != null) btnNext.interactable = (indexSekarang < daftarBudaya.Count - 1 && !sedangAnimasi);
    }

    // Fungsi helper yang bisa dipanggil dari skrip Quest / NPC / Crafting manapun
    public static void BukaProgresBuku(string namaAlat, string jenisProgres) 
    // jenisProgres bisa diisi: "Warlok", "Bahan", atau "Craft"
    {
        PlayerPrefs.SetInt("Quest_" + jenisProgres + "_" + namaAlat, 1);
        PlayerPrefs.Save();
    }

    public void TogglePanelBuku()
    {
        gameObject.SetActive(!gameObject.activeSelf);
        if (gameObject.activeSelf) UpdateTampilanLangsung(); // Refresh saat buku dibuka
    }

    public void TutupBuku()
    {
        gameObject.SetActive(false);
    }
}