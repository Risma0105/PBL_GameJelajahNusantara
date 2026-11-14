using System.Collections;
using UnityEngine;
using TMPro;

public class NPCDialog : MonoBehaviour
{
    [Header("Referensi Bubble Dialog")]
    public GameObject bubblePlayer;     // Tarik Textbox_MC
    public GameObject bubbleNPC;        // Tarik Textbox_NPC

    public TMP_Text teksPlayer;         // Tarik Text_Player
    public TMP_Text teksNPC;            // Tarik Text_NPC

    [Header("Ukuran Bubble Dinamis")]
    public Vector2 padding = new Vector2(24f, 20f); // ruang kosong kiri-kanan & atas-bawah
    public float lebarMin = 40f;
    public float lebarMaks = 300f;
    public float tinggiMin = 20f;

    [Header("Efek Ketik & Animasi")]
    [Tooltip("Centang = bubble bertambah tinggi tiap baris baru. Kosong = bubble langsung tinggi penuh.")]
    public bool bubbleIkutNgetik = true;
    public float kecepatanKetik = 40f;      // huruf per detik
    public float jedaTandaBaca = 0.15f;     // jeda tambahan di . , ! ?
    [Tooltip("Makin besar makin cepat bubble membesar. 15 = santai, 30 = cepat, 60 = hampir instan")]
    public float kecepatanTumbuh = 20f;
    [Tooltip("Bubble muncul dari ukuran ini (0.6 = 60%) lalu membesar ke 100%. Isi 1 untuk mematikan pop-in.")]
    [Range(0.2f, 1f)] public float skalaAwal = 0.6f;
    [Tooltip("Berapa detik SEBELUM baris baru mulai, kotak sudah mulai naik")]
    public float waktuAntisipasi = 0.25f;

    [System.Serializable]
    public struct DialogData
    {
        public bool apakahPlayerYangNgomong; // Centang jika Player yang ngomong
        [TextArea(2, 4)]
        public string isiPesan;
    }

    public DialogData[] daftarPercakapan;
    private int indeksDialog = 0;

    [Header("Referensi UI Crafting / Progress")]
    public GameObject panelCraftingAtauBadge;

    [Header("Pengaturan Buku Budaya (Culture Book)")]
    [Tooltip("Nama alat musik yang terkait dengan NPC ini, contoh: Angklung")]
    public string namaAlatMusikTerkait = "Angklung";

    private bool playerDekat = false;
    private bool sedangBerdialog = false;

    // State bubble yang sedang aktif
    private Coroutine ketikCoroutine;
    private bool sedangMengetik = false;
    private TMP_Text teksAktif;
    private RectTransform rtBubbleAktif;
    private float lebarAkhirAktif;
    private float tinggiAkhirAktif;

    void Start()
    {
        TutupSemuaBubble();
        if (panelCraftingAtauBadge != null) panelCraftingAtauBadge.SetActive(false);
    }

    void Update()
    {
        if (playerDekat && Input.GetKeyDown(KeyCode.E))
        {
            if (!sedangBerdialog)
            {
                MulaiDialog();
            }
            else if (sedangMengetik)
            {
                // Tekan E saat masih mengetik = langsung tampilkan semua teks
                SelesaikanKetik();
            }
            else
            {
                LanjutDialog();
            }
        }
    }

    void MulaiDialog()
    {
        sedangBerdialog = true;
        indeksDialog = 0;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.TampilkanPesan("");
        }

        TampilkanPesanSaatIni();
    }

    void LanjutDialog()
    {
        indeksDialog++;
        if (indeksDialog < daftarPercakapan.Length) TampilkanPesanSaatIni();
        else SelesaiDialog();
    }

    void TampilkanPesanSaatIni()
    {
        TutupSemuaBubble();

        if (indeksDialog >= daftarPercakapan.Length) return;

        DialogData dialogAktif = daftarPercakapan[indeksDialog];

        GameObject bubble = dialogAktif.apakahPlayerYangNgomong ? bubblePlayer : bubbleNPC;
        TMP_Text teks = dialogAktif.apakahPlayerYangNgomong ? teksPlayer : teksNPC;

        if (bubble == null || teks == null) return;

        bubble.SetActive(true);
        teksAktif = teks;
        rtBubbleAktif = bubble.GetComponent<RectTransform>();
        ketikCoroutine = StartCoroutine(KetikTeks(teks, dialogAktif.isiPesan));
    }

    IEnumerator KetikTeks(TMP_Text teks, string isi)
    {
        sedangMengetik = true;
        RectTransform rtTeks = teks.rectTransform;

        // 1. Isi teks penuh, tapi sembunyikan dulu
        teks.text = isi;
        teks.maxVisibleCharacters = 0;

        // 2. Ukur teks (sementara pakai titik-anchor supaya sizeDelta = ukuran asli)
        PasangAnchorUkur(rtTeks);
        HitungUkuranAkhir(teks, out lebarAkhirAktif, out tinggiAkhirAktif);

        // 3. Teks mengisi bubble dikurangi padding (Unity yang menata posisinya)
        PasangAnchorIsi(rtTeks);

        TMP_TextInfo info = teks.textInfo;
        int total = info.characterCount;
        int totalBaris = Mathf.Max(1, info.lineCount);

        // Keadaan awal animasi
        float tinggiSekarang = bubbleIkutNgetik ? HitungTinggiUntukBaris(1, totalBaris) : tinggiAkhirAktif;
        float skalaSekarang = skalaAwal;
        AturBubble(tinggiSekarang, skalaSekarang);

        float jedaHuruf = 1f / Mathf.Max(1f, kecepatanKetik);
        int antisipasi = Mathf.CeilToInt(kecepatanKetik * waktuAntisipasi);
        float sisaJeda = 0f;
        int tampil = 0;

        // 4. Loop per frame: huruf muncul berdasarkan waktu, bubble bergerak halus
        while (tampil < total)
        {
            sisaJeda -= Time.unscaledDeltaTime;
            while (sisaJeda <= 0f && tampil < total)
            {
                tampil++;
                char huruf = info.characterInfo[tampil - 1].character;
                sisaJeda += jedaHuruf;
                if (huruf == '.' || huruf == ',' || huruf == '!' || huruf == '?') sisaJeda += jedaTandaBaca;
            }
            teks.maxVisibleCharacters = tampil;

            // Target tinggi: lihat sedikit ke depan supaya kotak naik SEBELUM baris baru muncul
            float targetTinggi = tinggiAkhirAktif;
            if (bubbleIkutNgetik)
            {
                int idx = Mathf.Clamp(tampil + antisipasi, 1, total);
                int baris = Mathf.Clamp(info.characterInfo[idx - 1].lineNumber + 1, 1, totalBaris);
                targetTinggi = HitungTinggiUntukBaris(baris, totalBaris);
            }

            float k = 1f - Mathf.Exp(-kecepatanTumbuh * Time.unscaledDeltaTime);
            tinggiSekarang = Mathf.Lerp(tinggiSekarang, targetTinggi, k);
            skalaSekarang = Mathf.Lerp(skalaSekarang, 1f, k);
            AturBubble(tinggiSekarang, skalaSekarang);

            yield return null;
        }

        // 5. Huruf sudah habis, selesaikan gerakan bubble sampai mulus
        while (Mathf.Abs(tinggiSekarang - tinggiAkhirAktif) > 0.3f || skalaSekarang < 0.995f)
        {
            float k = 1f - Mathf.Exp(-kecepatanTumbuh * Time.unscaledDeltaTime);
            tinggiSekarang = Mathf.Lerp(tinggiSekarang, tinggiAkhirAktif, k);
            skalaSekarang = Mathf.Lerp(skalaSekarang, 1f, k);
            AturBubble(tinggiSekarang, skalaSekarang);
            yield return null;
        }

        SelesaikanKetik();
    }

    // Tinggi bubble (tanpa padding) saat 'baris' baris sudah dibutuhkan
    float HitungTinggiUntukBaris(int baris, int totalBaris)
    {
        return Mathf.Max(tinggiMin, tinggiAkhirAktif * baris / totalBaris);
    }

    void SelesaikanKetik()
    {
        if (ketikCoroutine != null) StopCoroutine(ketikCoroutine);
        ketikCoroutine = null;
        sedangMengetik = false;

        if (teksAktif != null)
        {
            teksAktif.maxVisibleCharacters = 99999;
            AturBubble(tinggiAkhirAktif, 1f);
        }
    }

    // Ukur teks yang BENERAN ke-render
    void HitungUkuranAkhir(TMP_Text teks, out float lebar, out float tinggi)
    {
        RectTransform rt = teks.rectTransform;

        teks.textWrappingMode = TextWrappingModes.Normal;
        teks.overflowMode = TextOverflowModes.Overflow;
        teks.alignment = bubbleIkutNgetik ? TextAlignmentOptions.TopLeft : TextAlignmentOptions.Center;

        // Pass 1: render selebar maksimal, cari baris terpanjang
        rt.sizeDelta = new Vector2(lebarMaks, 2000f);
        teks.ForceMeshUpdate();

        TMP_TextInfo info = teks.textInfo;
        int barisTarget = info.lineCount;
        float terlebar = 0f;
        for (int i = 0; i < info.lineCount; i++)
            terlebar = Mathf.Max(terlebar, info.lineInfo[i].width);

        lebar = Mathf.Clamp(terlebar + 4f, lebarMin, lebarMaks);

        // Pass 2: pastikan di lebar final tidak ada baris tambahan
        for (int n = 0; n < 30; n++)
        {
            rt.sizeDelta = new Vector2(lebar, 2000f);
            teks.ForceMeshUpdate();

            if (teks.textInfo.lineCount <= barisTarget || lebar >= lebarMaks) break;
            lebar = Mathf.Min(lebar + 4f, lebarMaks);
        }

        info = teks.textInfo;
        float tinggiBaris = info.lineCount > 0 ? info.lineInfo[0].lineHeight : teks.fontSize;
        float dariBaris = info.lineCount * tinggiBaris;
        float dariPreferred = teks.GetPreferredValues(teks.text, lebar, 0f).y;
        tinggi = Mathf.Max(Mathf.Max(dariBaris, dariPreferred), tinggiMin);
    }

    // Titik-anchor di tengah: dipakai hanya saat mengukur
    void PasangAnchorUkur(RectTransform rt)
    {
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
    }

    // Teks mengisi seluruh bubble, dengan jarak tepi sebesar padding
    void PasangAnchorIsi(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.offsetMin = new Vector2(padding.x * 0.5f, padding.y * 0.5f);
        rt.offsetMax = new Vector2(-padding.x * 0.5f, -padding.y * 0.5f);
    }

    // Lebar bubble selalu final; tinggi dan skala dianimasikan
    void AturBubble(float tinggiTeks, float skala)
    {
        if (rtBubbleAktif == null) return;
        rtBubbleAktif.sizeDelta = new Vector2(lebarAkhirAktif + padding.x, tinggiTeks + padding.y);
        rtBubbleAktif.localScale = new Vector3(skala, skala, 1f);
    }

    void TutupSemuaBubble()
    {
        if (ketikCoroutine != null) StopCoroutine(ketikCoroutine);
        ketikCoroutine = null;
        sedangMengetik = false;

        if (bubblePlayer != null)
        {
            bubblePlayer.transform.localScale = Vector3.one;
            bubblePlayer.SetActive(false);
        }
        if (bubbleNPC != null)
        {
            bubbleNPC.transform.localScale = Vector3.one;
            bubbleNPC.SetActive(false);
        }
    }

    void SelesaiDialog()
    {
        sedangBerdialog = false;
        TutupSemuaBubble();

        if (!string.IsNullOrEmpty(namaAlatMusikTerkait))
        {
            CultureBookManager.BukaProgresBuku(namaAlatMusikTerkait, "Warlok");
            Debug.Log("📖 Informasi awal " + namaAlatMusikTerkait + " terbuka di Culture Book!");
        }

        if (panelCraftingAtauBadge != null) panelCraftingAtauBadge.SetActive(true);

        if (UIManager.Instance != null) UIManager.Instance.TampilkanDefault();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerDekat = true;
            string pesanPanduan = "Tekan 'E' untuk berbicara dengan " + gameObject.name;
            Debug.Log(pesanPanduan);

            if (UIManager.Instance != null) UIManager.Instance.TampilkanPesan(pesanPanduan);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerDekat = false;
            sedangBerdialog = false;
            TutupSemuaBubble();

            if (UIManager.Instance != null) UIManager.Instance.TampilkanDefault();
        }
    }
}