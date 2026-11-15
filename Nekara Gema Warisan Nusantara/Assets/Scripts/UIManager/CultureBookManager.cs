using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using TMPro;

public class CultureBookManager : MonoBehaviour
{
    [System.Serializable]
    public class CultureItem
    {
        public string namaAlat; // Contoh: "Angklung" (untuk mencocokkan kunci progres)
        public string judul;

        [Header("Lembar 1: Deskripsi Singkat (terbuka setelah ngobrol dengan Warlok)")]
        [FormerlySerializedAs("infoNPCWarlok")]
        [TextArea(2, 4)] public string deskripsiSingkat;

        [Header("Lembar 2: Daftar Bahan (terbuka setelah alat musik berhasil dibuat)")]
        [FormerlySerializedAs("infoBahan")]
        [TextArea(2, 6)] public string daftarBahan;

        [Tooltip("Opsional. Kalau diisi, tampil di bawah daftar bahan.")]
        [FormerlySerializedAs("infoCrafting")]
        [TextArea(2, 4)] public string catatanCrafting;

        public Sprite gambarAlatMusik;
    }

    [Header("Daftar Konten Buku")]
    public List<CultureItem> daftarBudaya = new List<CultureItem>();
    private int indexSekarang = 0;
    private int halamanSekarang = 0; // 0 = lembar deskripsi, 1 = lembar bahan

    [Header("Komponen UI")]
    public TextMeshProUGUI txtJudulKiri;      // objek Judul
    public TextMeshProUGUI txtDeskripsiKiri;  // objek Info (halaman kanan)
    public Image imgKanan;                    // objek AlatMusik

    [Header("Teks Tambahan")]
    public string judulLembarBahan = "Bahan yang Dibutuhkan";
    [TextArea(1, 3)] public string teksTerkunci = "Temui Warlok setempat untuk membuka informasi awal...";

    [Header("Transform Lembaran untuk Animasi Flip")]
    public RectTransform lembarKanan;
    public RectTransform lembarKiri;
    public float durasiBalik = 0.2f;

    [Header("Tombol Navigasi")]
    public Button btnNext;
    public Button btnPrev;

    private bool sedangAnimasi = false;

    // OnEnable, bukan Start: panel buku sering dimatikan, jadi harus refresh tiap dibuka
    void OnEnable()
    {
        sedangAnimasi = false; // jaga-jaga kalau buku ditutup di tengah animasi
        if (lembarKanan != null) lembarKanan.localEulerAngles = Vector3.zero;
        if (lembarKiri != null) lembarKiri.localEulerAngles = Vector3.zero;
        UpdateTampilanLangsung();
    }

    // ---------- Logika navigasi per lembar ----------

    bool SudahCraft(CultureItem item)
    {
        return PlayerPrefs.GetInt("Quest_Craft_" + item.namaAlat, 0) == 1;
    }

    bool BisaMaju()
    {
        if (daftarBudaya.Count == 0) return false;
        bool adaLembarBahan = halamanSekarang == 0 && SudahCraft(daftarBudaya[indexSekarang]);
        return adaLembarBahan || indexSekarang < daftarBudaya.Count - 1;
    }

    bool BisaMundur()
    {
        return halamanSekarang > 0 || indexSekarang > 0;
    }

    void Maju()
    {
        if (halamanSekarang == 0 && SudahCraft(daftarBudaya[indexSekarang]))
        {
            halamanSekarang = 1;           // buka lembar bahan alat yang sama
        }
        else
        {
            indexSekarang++;               // pindah ke alat berikutnya
            halamanSekarang = 0;
        }
    }

    void Mundur()
    {
        if (halamanSekarang == 1)
        {
            halamanSekarang = 0;
        }
        else
        {
            indexSekarang--;
            // mundur mendarat di lembar terakhir alat sebelumnya, seperti buku asli
            halamanSekarang = SudahCraft(daftarBudaya[indexSekarang]) ? 1 : 0;
        }
    }

    public void HalamanBerikutnya()
    {
        if (sedangAnimasi || !BisaMaju()) return;
        StartCoroutine(AnimasiBalikHalaman(true));
    }

    public void HalamanSebelumnya()
    {
        if (sedangAnimasi || !BisaMundur()) return;
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
            float rotY = Mathf.Lerp(0f, 90f * arah, waktu / durasiBalik);
            if (lembarTarget != null) lembarTarget.localEulerAngles = new Vector3(0, rotY, 0);
            yield return null;
        }

        if (keKanan) Maju();
        else Mundur();
        UpdateTampilanLangsung();

        waktu = 0f;
        while (waktu < durasiBalik)
        {
            waktu += Time.deltaTime;
            float rotY = Mathf.Lerp(90f * arah, 0f, waktu / durasiBalik);
            if (lembarTarget != null) lembarTarget.localEulerAngles = new Vector3(0, rotY, 0);
            yield return null;
        }

        if (lembarTarget != null) lembarTarget.localEulerAngles = Vector3.zero;

        sedangAnimasi = false;
        UpdateTampilanLangsung();
    }

    // ---------- Tampilan ----------

    void UpdateTampilanLangsung()
    {
        if (daftarBudaya.Count == 0) return;

        indexSekarang = Mathf.Clamp(indexSekarang, 0, daftarBudaya.Count - 1);
        CultureItem data = daftarBudaya[indexSekarang];

        bool progressNPC = PlayerPrefs.GetInt("Quest_Warlok_" + data.namaAlat, 0) == 1;
        bool progressBahan = PlayerPrefs.GetInt("Quest_Bahan_" + data.namaAlat, 0) == 1;
        bool progressCraft = PlayerPrefs.GetInt("Quest_Craft_" + data.namaAlat, 0) == 1;

        // Lembar bahan tidak ada kalau alat musiknya belum didapat
        if (!progressCraft) halamanSekarang = 0;

        if (txtJudulKiri != null) txtJudulKiri.text = data.judul;

        // Gambar: terang kalau semua progres lengkap, abu-abu gelap kalau belum
        if (imgKanan != null)
        {
            imgKanan.sprite = data.gambarAlatMusik;
            bool semuaTerbuka = progressNPC && progressBahan && progressCraft;
            imgKanan.color = semuaTerbuka ? Color.white : new Color(0.4f, 0.4f, 0.4f, 1f);
        }

        // Teks halaman kanan, tergantung lembar yang sedang dibuka
        if (txtDeskripsiKiri != null)
        {
            if (halamanSekarang == 0)
            {
                txtDeskripsiKiri.text = progressNPC ? data.deskripsiSingkat : teksTerkunci;
            }
            else
            {
                string teks = judulLembarBahan + "\n\n" + data.daftarBahan;
                if (!string.IsNullOrEmpty(data.catatanCrafting))
                    teks += "\n\n" + data.catatanCrafting;
                txtDeskripsiKiri.text = teks;
            }
        }

        if (btnPrev != null) btnPrev.interactable = BisaMundur() && !sedangAnimasi;
        if (btnNext != null) btnNext.interactable = BisaMaju() && !sedangAnimasi;
    }

    // Dipanggil dari skrip Quest / NPC / Crafting. jenisProgres: "Warlok", "Bahan", atau "Craft"
    public static void BukaProgresBuku(string namaAlat, string jenisProgres)
    {
        PlayerPrefs.SetInt("Quest_" + jenisProgres + "_" + namaAlat, 1);
        PlayerPrefs.Save();
    }

    public void TogglePanelBuku()
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }

    public void TutupBuku()
    {
        gameObject.SetActive(false);
    }
}