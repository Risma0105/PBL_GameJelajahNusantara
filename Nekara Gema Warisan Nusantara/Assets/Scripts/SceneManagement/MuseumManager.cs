using UnityEngine;

public class MuseumManager : MonoBehaviour
{
    public static MuseumManager Instance;

    public enum FaseMuseum
    {
        AwalKosong,         // Fase 1: Masuk museum pertama kali (masih sepi)
        CraftingAlatMusik,  // Fase 2: Pemain membuat/merakit alat musik
        PertempuranKabut,   // Fase 3: Adu pertanyaan pakai alat musik lawan kabut
        KarnavalRamai       // Fase 4: Menang, museum jadi ramai
    }

    [Header("Status Museum Saat Ini")]
    public FaseMuseum faseAktif = FaseMuseum.AwalKosong;

    [Header("Referensi Objek di Scene")]
    public GameObject objekAreaCrafting;   // Meja/alat untuk merakit
    public GameObject objekKabutDanMusuh;  // Efek kabut memori
    public GameObject objekNPCKarnavalRamai; // Kerumunan NPC karnaval
    public GameObject panelQuizUI;         // Panel UI kuis tanya-jawab

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        TerapkanFaseMuseum();
    }

    public void UbahFase(FaseMuseum faseBaru)
    {
        faseAktif = faseBaru;
        TerapkanFaseMuseum();
    }

    void TerapkanFaseMuseum()
    {
        // Atur objek apa saja yang aktif/mati berdasarkan fase cerita
        switch (faseAktif)
        {
            case FaseMuseum.AwalKosong:
                if (objekAreaCrafting) objekAreaCrafting.SetActive(true);
                if (objekKabutDanMusuh) objekKabutDanMusuh.SetActive(false);
                if (objekNPCKarnavalRamai) objekNPCKarnavalRamai.SetActive(false);
                if (panelQuizUI) panelQuizUI.SetActive(false);
                break;

            case FaseMuseum.CraftingAlatMusik:
                // Pemain fokus merakit alat musik di sini
                if (objekAreaCrafting) objekAreaCrafting.SetActive(true);
                if (objekKabutDanMusuh) objekKabutDanMusuh.SetActive(false);
                if (objekNPCKarnavalRamai) objekNPCKarnavalRamai.SetActive(false);
                if (panelQuizUI) panelQuizUI.SetActive(false);
                break;

            case FaseMuseum.PertempuranKabut:
                if (objekAreaCrafting) objekAreaCrafting.SetActive(false);
                if (objekKabutDanMusuh) objekKabutDanMusuh.SetActive(true);
                if (objekNPCKarnavalRamai) objekNPCKarnavalRamai.SetActive(false);
                if (panelQuizUI) panelQuizUI.SetActive(true);
                break;

            case FaseMuseum.KarnavalRamai:
                if (objekAreaCrafting) objekAreaCrafting.SetActive(false);
                if (objekKabutDanMusuh) objekKabutDanMusuh.SetActive(false);
                if (objekNPCKarnavalRamai) objekNPCKarnavalRamai.SetActive(true);
                if (panelQuizUI) panelQuizUI.SetActive(false);
                break;
        }
    }
}