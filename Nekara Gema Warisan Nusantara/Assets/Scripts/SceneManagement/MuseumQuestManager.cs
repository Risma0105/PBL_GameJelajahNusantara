using UnityEngine;
using TMPro;

public class MuseumQuestManager : MonoBehaviour
{
    public static MuseumQuestManager Instance;

    public enum StatusQuestMuseum
    {
        MumpungSepi_KumpulBahan, // Tahap 1: Eksplorasi & kumpul bahan/alat musik
        SedangCrafting,          // Tahap 2: Merakit alat musik
        LawanKabutMemori,        // Tahap 3: Adu pertanyaan kuis
        Selesai_KarnavalRamai    // Tahap 4: Menang dan museum jadi ramai
    }

    [Header("Status Quest Saat Ini")]
    public StatusQuestMuseum questAktif = StatusQuestMuseum.MumpungSepi_KumpulBahan;

    [Header("UI Panduan Misi (Opsional)")]
    public TextMeshProUGUI teksTujuanMisi;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        PerbaruiTampilanMisi();
    }

    // Fungsi untuk memindahkan tahap quest dari luar (misal dipanggil saat item selesai dibuat)
    public void LanjutKeTahap(StatusQuestMuseum tahapBaru)
    {
        questAktif = tahapBaru;
        PerbaruiTampilanMisi();

        // Otomatis sinkronkan dengan MuseumManager yang sudah kita buat sebelumnya
        SinkronkanDenganMuseumManager();
    }

    void SinkronkanDenganMuseumManager()
    {
        if (MuseumManager.Instance == null) return;

        switch (questAktif)
        {
            case StatusQuestMuseum.MumpungSepi_KumpulBahan:
            case StatusQuestMuseum.SedangCrafting:
                MuseumManager.Instance.UbahFase(MuseumManager.FaseMuseum.CraftingAlatMusik);
                break;

            case StatusQuestMuseum.LawanKabutMemori:
                MuseumManager.Instance.UbahFase(MuseumManager.FaseMuseum.PertempuranKabut);
                break;

            case StatusQuestMuseum.Selesai_KarnavalRamai:
                MuseumManager.Instance.UbahFase(MuseumManager.FaseMuseum.KarnavalRamai);
                break;
        }
    }

    void PerbaruiTampilanMisi()
    {
        if (teksTujuanMisi == null) return;

        switch (questAktif)
        {
            case StatusQuestMuseum.MumpungSepi_KumpulBahan:
                teksTujuanMisi.text = "Misi: Jelajahi museum yang sepi dan kumpulkan bahan alat musik.";
                break;
            case StatusQuestMuseum.SedangCrafting:
                teksTujuanMisi.text = "Misi: Gunakan meja kerja untuk merakit alat musik.";
                break;
            case StatusQuestMuseum.LawanKabutMemori:
                teksTujuanMisi.text = "Misi: Jawab pertanyaan dengan benar untuk melemahkan Kabut Memori!";
                break;
            case StatusQuestMuseum.Selesai_KarnavalRamai:
                teksTujuanMisi.text = "Misi Selesai: Nikmati suasana karnaval di museum yang meriah!";
                break;
        }
    }
}