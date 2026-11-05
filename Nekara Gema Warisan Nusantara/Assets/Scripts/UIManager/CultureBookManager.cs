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
        public string judul;
        [TextArea(3, 5)] public string deskripsi;
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

    [Header("Tombol Navigasi (Opsional untuk referensi interaktif)")]
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
        while (waktu < durasiBalik) // wait, keep standard variable name
        {
            waktu += Time.deltaTime;
            float rotYProgress = Mathf.Lerp(90f * arah, 0f, waktu / durasiBalik);
            if (lembarTarget != null) lembarTarget.localEulerAngles = new Vector3(0, rotYProgress, 0);
            yield return null;
        }

        if (lembarTarget != null) lembarTarget.localEulerAngles = Vector3.zero;
        
        sedangAnimasi = false;
        UpdateTampilanLangsung(); // Refresh tombol setelah animasi benar-benar selesai
    }

    void UpdateTampilanLangsung()
    {
        if (daftarBudaya.Count == 0) return;

        CultureItem data = daftarBudaya[indexSekarang];

        if (txtJudulKiri != null) txtJudulKiri.text = data.judul;
        if (txtDeskripsiKiri != null) txtDeskripsiKiri.text = data.deskripsi;
        if (imgKanan != null) imgKanan.sprite = data.gambarAlatMusik;

        if (btnPrev != null) btnPrev.interactable = (indexSekarang > 0 && !sedangAnimasi);
        if (btnNext != null) btnNext.interactable = (indexSekarang < daftarBudaya.Count - 1 && !sedangAnimasi);
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