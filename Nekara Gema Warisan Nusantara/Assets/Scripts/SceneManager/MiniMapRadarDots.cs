using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MiniMapRadarDots : MonoBehaviour
{
    [Header("Referensi UI")]
    public RectTransform containerMiniMap; 
    public GameObject prefabTitik;        

    [Header("Target di Game")]
    public Transform player;              
    public List<Transform> daftarBahan;   
    public Transform tempatCrafting;      

    [Header("Referensi Batas Jalan Map")]
    public Transform batasKiri;
    public Transform batasKanan;
    public Transform batasAtas;
    public Transform batasBawah;

    private GameObject titikPlayer;
    private List<GameObject> titikBahanList = new List<GameObject>();
    private GameObject titikCrafting;

    void Start()
    {
        if (containerMiniMap == null || prefabTitik == null) return;

        titikPlayer = Instantiate(prefabTitik, containerMiniMap);
        AturWarna(titikPlayer, Color.green);

        foreach (var bahan in daftarBahan)
        {
            if (bahan != null)
            {
                GameObject tBahan = Instantiate(prefabTitik, containerMiniMap);
                AturWarna(tBahan, Color.yellow);
                titikBahanList.Add(tBahan);
            }
        }

        if (tempatCrafting != null)
        {
            titikCrafting = Instantiate(prefabTitik, containerMiniMap);
            AturWarna(titikCrafting, Color.red);
        }
    }

    void Update()
    {
        if (containerMiniMap == null || batasKiri == null || batasKanan == null || batasAtas == null || batasBawah == null) return;

        float radiusMax = (containerMiniMap.rect.width / 2f) - 5f;

        if (player != null)
            PetakanKeRadar(titikPlayer, player.position, radiusMax);

        for (int i = 0; i < daftarBahan.Count; i++)
        {
            if (daftarBahan[i] != null && i < titikBahanList.Count)
            {
                PetakanKeRadar(titikBahanList[i], daftarBahan[i].position, radiusMax);
            }
        }

        if (tempatCrafting != null && titikCrafting != null)
        {
            PetakanKeRadar(titikCrafting, tempatCrafting.position, radiusMax);
        }
    }

    void PetakanKeRadar(GameObject objTitik, Vector3 posisiDunia, float radiusMax)
    {
        RectTransform rect = objTitik.GetComponent<RectTransform>();

        // Mengambil batas min dan max secara langsung dari posisi objek batas jalan yang sudah ada
        float minX = batasKiri.position.x;
        float maxX = batasKanan.position.x;
        float minY = batasBawah.position.y;
        float maxY = batasAtas.position.y;

        float normalX = Mathf.InverseLerp(minX, maxX, posisiDunia.x);
        float normalY = Mathf.InverseLerp(minY, maxY, posisiDunia.y);

        float posX = (normalX - 0.5f) * (radiusMax * 2f);
        float posY = (normalY - 0.5f) * (radiusMax * 2f);

        posX = Mathf.Clamp(posX, -radiusMax, radiusMax);
        posY = Mathf.Clamp(posY, -radiusMax, radiusMax);

        rect.anchoredPosition = new Vector2(posX, posY);
    }

    void AturWarna(GameObject obj, Color warna)
    {
        Image img = obj.GetComponent<Image>();
        if (img != null) img.color = warna;
    }
}