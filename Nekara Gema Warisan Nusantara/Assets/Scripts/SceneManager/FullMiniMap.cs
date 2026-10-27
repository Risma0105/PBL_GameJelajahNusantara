using UnityEngine;

public class FullMiniMap : MonoBehaviour
{
    [Header("Referensi UI")]
    public RectTransform containerMiniMap; // Panel tempat area minimap
    public GameObject prefabTitikPlayer;
    public GameObject prefabTitikBahan;
    public GameObject prefabTitikCrafting;

    [Header("Target di Game")]
    public Transform player;
    public Transform[] daftarBahan;
    public Transform[] tempatCrafting;

    [Header("Referensi Batas Jalan Map (World Space)")]
    public Transform batasKiri;
    public Transform batasKanan;
    public Transform batasAtas;
    public Transform batasBawah;

    private RectTransform uiPlayerInstance;
    private RectTransform[] uiBahanInstances;
    private RectTransform[] uiCraftingInstances;

    void Start()
    {
        // 1. Buat instansiasi titik player
        if (prefabTitikPlayer && containerMiniMap && player)
        {
            GameObject obj = Instantiate(prefabTitikPlayer, containerMiniMap);
            uiPlayerInstance = obj.GetComponent<RectTransform>();
        }

        // 2. Buat instansiasi titik bahan (Kuning)
        if (prefabTitikBahan && containerMiniMap && daftarBahan.Length > 0)
        {
            uiBahanInstances = new RectTransform[daftarBahan.Length];
            for (int i = 0; i < daftarBahan.Length; i++)
            {
                if (daftarBahan[i] != null)
                {
                    GameObject obj = Instantiate(prefabTitikBahan, containerMiniMap);
                    uiBahanInstances[i] = obj.GetComponent<RectTransform>();
                }
            }
        }

        // 3. Buat instansiasi titik crafting (Merah)
        if (prefabTitikCrafting && containerMiniMap && tempatCrafting.Length > 0)
        {
            uiCraftingInstances = new RectTransform[tempatCrafting.Length];
            for (int i = 0; i < tempatCrafting.Length; i++)
            {
                if (tempatCrafting[i] != null)
                {
                    GameObject obj = Instantiate(prefabTitikCrafting, containerMiniMap);
                    uiCraftingInstances[i] = obj.GetComponent<RectTransform>();
                }
            }
        }
    }

    void Update()
    {
        if (batasKiri == null || batasKanan == null || batasAtas == null || batasBawah == null || containerMiniMap == null)
            return;

        // Hitung ukuran batas dunia nyata
        float worldMinX = batasKiri.position.x;
        float worldMaxX = batasKanan.position.x;
        float worldMinY = batasBawah.position.y;
        float worldMaxY = batasAtas.position.y;

        // Ukuran rect UI minimap
        float uiWidth = containerMiniMap.rect.width;
        float uiHeight = containerMiniMap.rect.height;

        // Fungsi lambda untuk konversi posisi World ke posisi lokal UI Minimap
        System.Func<Vector3, Vector2> WorldToMiniMap = (worldPos) =>
        {
            float normalizedX = Mathf.InverseLerp(worldMinX, worldMaxX, worldPos.x);
            float normalizedY = Mathf.InverseLerp(worldMinY, worldMaxY, worldPos.y);

            float localX = (normalizedX - 0.5f) * uiWidth;
            float localY = (normalizedY - 0.5f) * uiHeight;

            return new Vector2(localX, localY);
        };

        // Update posisi Player (Hijau) - Bergerak dinamis
        if (player != null && uiPlayerInstance != null)
        {
            uiPlayerInstance.anchoredPosition = WorldToMiniMap(player.position);
        }

        // Update posisi Bahan (Kuning) - Statis/Tetap di tempatnya
        if (daftarBahan != null && uiBahanInstances != null)
        {
            for (int i = 0; i < daftarBahan.Length; i++)
            {
                if (daftarBahan[i] != null && uiBahanInstances[i] != null)
                {
                    uiBahanInstances[i].anchoredPosition = WorldToMiniMap(daftarBahan[i].position);
                }
            }
        }

        // Update posisi Crafting (Merah) - Statis/Tetap di tempatnya
        if (tempatCrafting != null && uiCraftingInstances != null)
        {
            for (int i = 0; i < tempatCrafting.Length; i++)
            {
                if (tempatCrafting[i] != null && uiCraftingInstances[i] != null)
                {
                    uiCraftingInstances[i].anchoredPosition = WorldToMiniMap(tempatCrafting[i].position);
                }
            }
        }
    }
}