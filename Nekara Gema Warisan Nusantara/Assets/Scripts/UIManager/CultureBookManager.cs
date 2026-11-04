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

    [Header("Komponen UI Buku & Konten")]
    public Image imgAlatMusik;          
    public TextMeshProUGUI txtJudul;    
    public TextMeshProUGUI txtDeskripsi;

    [Header("Animasi & Navigasi")]
    public Animator AnimatorBuku;       
    public Button BtnNext;
    public Button BtnPrev;

    void Start()
    {
        if (BtnNext != null) BtnNext.onClick.AddListener(HalamanBerikutnya);
        if (BtnPrev != null) BtnPrev.onClick.AddListener(HalamanSebelumnya);
        
        PerbaruiTampilan();
    }

    public void HalamanBerikutnya()
    {
        if (indexSekarang < daftarBudaya.Count - 1)
        {
            indexSekarang++;
            if (AnimatorBuku != null)
            {
                AnimatorBuku.SetTrigger("BukaBuku");
            }
            
            // 1. Kosongkan layar dulu supaya pas kertas jalan, gambarnya ga kelihatan
            KosongkanTampilanSementara();
            
            // 2. Munculkan gambar & teks BARU pas di ujung akhir animasi (misal 0.35 detik / pas lembar buku sudah kebuka penuh)
            CancelInvoke("PerbaruiTampilan");
            Invoke("PerbaruiTampilan", 0.35f); 
        }
    }

    public void HalamanSebelumnya()
    {
        if (indexSekarang > 0)
        {
            indexSekarang--;
            if (AnimatorBuku != null)
            {
                AnimatorBuku.SetTrigger("TutupBuku");
            }
            
            // 1. Kosongkan layar dulu
            KosongkanTampilanSementara();
            
            // 2. Munculkan pas lembar buku selesai menutup/terbuka
            CancelInvoke("PerbaruiTampilan");
            Invoke("PerbaruiTampilan", 0.35f);
        }
    }

    void KosongkanTampilanSementara()
    {
        if (imgAlatMusik != null) imgAlatMusik.sprite = null;
        if (txtJudul != null) txtJudul.text = "";
        if (txtDeskripsi != null) txtDeskripsi.text = "";
    }

    public void PerbaruiTampilan()
    {
        if (daftarBudaya.Count == 0) return;

        CultureItem data = daftarBudaya[indexSekarang];

        if (imgAlatMusik != null) imgAlatMusik.sprite = data.gambarAlatMusik;
        if (txtJudul != null) txtJudul.text = data.judul;
        if (txtDeskripsi != null) txtDeskripsi.text = data.deskripsi;

        if (BtnPrev != null) BtnPrev.interactable = (indexSekarang > 0);
        if (BtnNext != null) BtnNext.interactable = (indexSekarang < daftarBudaya.Count - 1);
    }
}