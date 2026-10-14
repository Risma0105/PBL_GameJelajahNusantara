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

    [Header("Daftar Konten")]
    public List<CultureItem> daftarBudaya = new List<CultureItem>();
    private int indexSekarang = 0;

    [Header("Komponen UI Kiri & Kanan")]
    public Image imgKiri;
    public TextMeshProUGUI txtJudulKanan;
    public TextMeshProUGUI txtDeskripsiKanan;

    [Header("Transform Lembaran untuk Animasi")]
    public RectTransform lembarKanan;
    public RectTransform lembarKiri;
    public float durasiBalik = 0.2f; // Kecepatan membalik kertas

    [Header("Tombol Navigasi")]
    public Button btnNext;
    public Button btnPrev;

    private bool sedangAnimasi = false;

    void Start()
    {
        btnNext.onClick.AddListener(HalamanBerikutnya);
        btnPrev.onClick.AddListener(HalamanSebelumnya);
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

        // Tentukan lembaran mana yang membalik
        RectTransform lembarTarget = keKanan ? lembarKanan : lembarKiri;
        float arah = keKanan ? 1f : -1f;

        // Tahap 1: Kertas terangkat ke tengah (0 ke 90 derajat)
        float waktu = 0f;
        while (waktu < durasiBalik)
        {
            waktu += Time.deltaTime;
            float rotY = Mathf.Lerp(0f, 90f * arah, waktu / durasiBalik);
            lembarTarget.localEulerAngles = new Vector3(0, rotY, 0);
            yield return null;
        }

        // Ganti data tepat saat kertas tegak lurus
        if (keKanan) indexSekarang++;
        else indexSekarang--;
        UpdateTampilanLangsung();

        // Tahap 2: Kertas membuka kembali (90 ke 0 derajat)
        waktu = 0f;
        while (waktu < durasiBalik)
        {
            waktu += Time.deltaTime;
            float rotY = Mathf.Lerp(90f * arah, 0f, waktu / durasiBalik);
            lembarTarget.localEulerAngles = new Vector3(0, rotY, 0);
            yield return null;
        }

        lembarTarget.localEulerAngles = Vector3.zero;
        sedangAnimasi = false;
    }

    void UpdateTampilanLangsung()
    {
        if (daftarBudaya.Count == 0) return;

        CultureItem data = daftarBudaya[indexSekarang];

        if (imgKiri != null) imgKiri.sprite = data.gambarAlatMusik;
        if (txtJudulKanan != null) txtJudulKanan.text = data.judul;
        if (txtDeskripsiKanan != null) txtDeskripsiKanan.text = data.deskripsi;

        btnPrev.interactable = (indexSekarang > 0);
        btnNext.interactable = (indexSekarang < daftarBudaya.Count - 1);
    }
}