using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class BlinkInstruksi : MonoBehaviour
{
    [Header("Kedip")]
    public float kecepatanKedip = 3f;
    [Range(0f, 1f)] public float alphaMin = 0.3f;
    [Range(0f, 1f)] public float alphaMax = 1f;

    [Header("Fade Muncul / Hilang")]
    public float durasiFadeIn = 0.25f;
    public float durasiFadeOut = 0.15f;

    CanvasGroup group;
    float fade;         // 0 = hilang, 1 = tampil penuh
    float fadeTujuan;

    void Awake()
    {
        AmbilGroup();
        group.alpha = fade;
    }

    void AmbilGroup()
    {
        if (group == null) group = GetComponent<CanvasGroup>();
        group.blocksRaycasts = false; // ikon cuma hiasan, jangan blokir klik
        group.interactable = false;
    }

    // Dipanggil dari PrologFokusUI
    public void SetTampil(bool tampil, bool langsung = false)
    {
        AmbilGroup();
        fadeTujuan = tampil ? 1f : 0f;
        if (langsung)
        {
            fade = fadeTujuan;
            group.alpha = fade;
        }
    }

    void Update()
    {
        // fade in / out
        float durasi = fadeTujuan > fade ? durasiFadeIn : durasiFadeOut;
        fade = Mathf.MoveTowards(fade, fadeTujuan, Time.unscaledDeltaTime / Mathf.Max(0.01f, durasi));

        if (fade <= 0f)
        {
            group.alpha = 0f;
            return;
        }

        // unscaledTime supaya tetap jalan walau Time.timeScale = 0
        float t = (Mathf.Sin(Time.unscaledTime * kecepatanKedip) + 1f) * 0.5f; // 0..1
        group.alpha = Mathf.Lerp(alphaMin, alphaMax, t) * fade;
    }
}