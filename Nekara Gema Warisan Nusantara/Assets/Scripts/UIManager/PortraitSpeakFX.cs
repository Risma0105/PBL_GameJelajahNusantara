using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Pasang di objek portrait kiri dan kanan (yang dipakai imageKiri / imageKanan).
/// Set Pivot RectTransform ke (0.5, 0) biar pas membesar tumbuh ke atas.
/// </summary>
public class PortraitSpeakFX : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] RectTransform target;      // kosongin = objek ini sendiri
    [SerializeField] Graphic[] tintGraphics;    // frame + gambar karakter

    [Header("Speak (membesar)")]
    [SerializeField] float speakScale = 1.25f;
    [SerializeField] float duration = 0.25f;

    [Header("Talking (naik-turun pas teks diketik)")]
    [SerializeField] float bobAmount = 6f;
    [SerializeField] float bobSpeed = 10f;

    [Header("Warna")]
    [SerializeField] Color dimColor = new Color(0.6f, 0.6f, 0.6f, 1f);

    Vector2 basePos;
    Vector3 baseScale;
    bool inited;
    bool talking;
    Coroutine anim;

    void Awake() => EnsureInit();

    void EnsureInit()
    {
        if (inited) return;
        inited = true;
        if (!target) target = (RectTransform)transform;
        basePos = target.anchoredPosition;
        baseScale = target.localScale;
        ApplyColor(dimColor);
    }

    /// <summary>Dipanggil tiap ganti baris dialog. true = lagi bicara.</summary>
    public void SetSpeaking(bool value)
    {
        EnsureInit();
        if (!value) talking = false;

        if (anim != null) { StopCoroutine(anim); anim = null; }

        // objek lagi nonaktif -> coroutine nggak bisa jalan, langsung set aja
        if (!isActiveAndEnabled)
        {
            target.localScale = value ? baseScale * speakScale : baseScale;
            ApplyColor(value ? Color.white : dimColor);
            return;
        }

        anim = StartCoroutine(Animate(value));
    }

    /// <summary>true selama teks diketik, false pas udah selesai / di-skip.</summary>
    public void SetTalking(bool value) => talking = value;

    IEnumerator Animate(bool toSpeak)
    {
        Vector3 fromScale = target.localScale;
        Vector3 toScale = toSpeak ? baseScale * speakScale : baseScale;
        Color fromCol = tintGraphics.Length > 0 && tintGraphics[0] ? tintGraphics[0].color : Color.white;
        Color toCol = toSpeak ? Color.white : dimColor;

        for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime / duration)
        {
            float e = toSpeak ? EaseOutBack(t) : t * t * (3f - 2f * t);
            target.localScale = Vector3.LerpUnclamped(fromScale, toScale, e);
            ApplyColor(Color.Lerp(fromCol, toCol, t));
            yield return null;
        }

        target.localScale = toScale;
        ApplyColor(toCol);
        anim = null;
    }

    void Update()
    {
        Vector2 goal = talking
            ? basePos + Vector2.up * (Mathf.Abs(Mathf.Sin(Time.unscaledTime * bobSpeed)) * bobAmount)
            : basePos;

        target.anchoredPosition = Vector2.Lerp(
            target.anchoredPosition, goal, Time.unscaledDeltaTime * 12f);
    }

    void ApplyColor(Color c)
    {
        foreach (var g in tintGraphics)
            if (g) g.color = c;
    }

    static float EaseOutBack(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        t -= 1f;
        return 1f + c3 * t * t * t + c1 * t * t;
    }
}