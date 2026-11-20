using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public enum TipeInput { Keyboard, KlikKiri }

[RequireComponent(typeof(Collider2D))]
public class ProximityPrompt : MonoBehaviour
{
    [Header("Teks (opsional, kosongin kalau pakai ikon)")]
    public string objectText = "";
    public string actionText = "";

    [Header("Input")]
    public TipeInput tipeInput = TipeInput.KlikKiri;
    public Key interactKey = Key.E;

    [Header("Ikon")]
    public Sprite ikonKlik;       // ikon mouse klik kiri
    public Sprite ikonKeyboard;   // keycap (mis. tombol E)
    public Sprite ikonAksi;       // ikon "ambil" (tangan, dll)

    [Header("Referensi UI (anak dari PromptCanvas)")]
    public GameObject panel;
    public Image keyIcon;
    public Image actionIcon;
    public TMP_Text keyLabel;
    public TMP_Text objectLabel;
    public TMP_Text actionLabel;

    [Header("Event")]
    public UnityEvent onTriggered;
    public UnityEvent onPlayerKeluar;

    [Header("Debug")]
    public bool tampilkanLog = true;

    public Transform Player { get; private set; }

    private static readonly List<ProximityPrompt> aktif = new List<ProximityPrompt>();

    private bool sedangTampil = false;
    private bool terdekatSebelumnya = false;
    private PopUpAnimasi popup;   // komponen animasi di Panel (opsional)

    void Awake()
    {
        AutoAssign();
        RefreshUI();

        if (panel == null)
        {
            Debug.LogWarning("[Prompt] Panel KOSONG di objek: " + gameObject.name, this);
        }
        else
        {
            popup = panel.GetComponent<PopUpAnimasi>();
            panel.SetActive(false);   // langsung sembunyi tanpa animasi
        }

        sedangTampil = false;
    }

    void OnDisable()
    {
        aktif.Remove(this);
        Player = null;
    }

    void Update()
    {
        if (Player == null) return;

        bool terdekat = IsTerdekat();

        if (tampilkanLog && terdekat != terdekatSebelumnya)
        {
            Debug.Log("[Prompt] " + gameObject.name + " terdekat = " + terdekat, this);
            terdekatSebelumnya = terdekat;
        }

        SetVisible(terdekat);

        if (terdekat && InputDitekan())
            onTriggered?.Invoke();
    }

    bool IsTerdekat()
    {
        float jarakSaya = ((Vector2)Player.position - (Vector2)transform.position).sqrMagnitude;

        foreach (var p in aktif)
        {
            if (p == this || p.Player == null) continue;
            if (p.panel == null) continue;

            float jarakLain = ((Vector2)p.Player.position - (Vector2)p.transform.position).sqrMagnitude;
            if (jarakLain < jarakSaya) return false;
        }
        return true;
    }

    bool InputDitekan()
    {
        if (tipeInput == TipeInput.KlikKiri)
            return Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;

        return Keyboard.current != null && Keyboard.current[interactKey].wasPressedThisFrame;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        Player = other.transform;
        if (!aktif.Contains(this)) aktif.Add(this);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        aktif.Remove(this);
        Player = null;
        terdekatSebelumnya = false;
        SetVisible(false);
        onPlayerKeluar?.Invoke();
    }

    public void SetActionText(string teks) { actionText = teks; RefreshUI(); }
    public void SetObjectText(string teks) { objectText = teks; RefreshUI(); }
    public void SetActionIcon(Sprite s)    { ikonAksi = s; RefreshUI(); }

    void AutoAssign()
    {
        Transform canvas = transform.Find("PromptCanvas");
        if (canvas == null) return;

        if (panel == null)
        {
            Transform p = canvas.Find("Panel");
            if (p != null) panel = p.gameObject;
        }
        if (panel == null) return;

        if (keyIcon == null)    keyIcon    = CariGambar(panel.transform, "Key");
        if (actionIcon == null) actionIcon = CariGambar(panel.transform, "Action");

        if (keyLabel == null)    keyLabel    = CariTeks(panel.transform, "Key");
        if (objectLabel == null) objectLabel = CariTeks(panel.transform, "Object");
        if (actionLabel == null) actionLabel = CariTeks(panel.transform, "Action");
    }

    Image CariGambar(Transform induk, string nama)
    {
        Transform t = induk.Find(nama);
        return t != null ? t.GetComponent<Image>() : null;
    }

    TMP_Text CariTeks(Transform induk, string nama)
    {
        Transform t = induk.Find(nama);
        return t != null ? t.GetComponentInChildren<TMP_Text>(true) : null;
    }

    void RefreshUI()
    {
        // Ikon tombol (ganti sesuai tipe input)
        Sprite ikonTombol = (tipeInput == TipeInput.KlikKiri) ? ikonKlik : ikonKeyboard;
        SetIkon(keyIcon, ikonTombol);
        SetIkon(actionIcon, ikonAksi);

        // Teks (hanya kalau ada label dan teksnya diisi)
        if (keyLabel != null)
        {
            keyLabel.text = NamaTombol();
            keyLabel.gameObject.SetActive(ikonTombol == null);
        }
        if (objectLabel != null) objectLabel.text = objectText;
        if (actionLabel != null) actionLabel.text = actionText;
    }

    void SetIkon(Image img, Sprite s)
    {
        if (img == null) return;
        img.sprite = s;
        img.enabled = s != null;
        img.preserveAspect = true;
    }

    string NamaTombol()
    {
        if (tipeInput == TipeInput.KlikKiri) return "Klik";
        return interactKey.ToString();
    }

    void SetVisible(bool visible)
    {
        if (sedangTampil == visible) return;
        sedangTampil = visible;
        if (panel == null) return;

        if (visible)
        {
            if (popup != null) popup.Buka();
            else panel.SetActive(true);
        }
        else
        {
            if (popup != null && panel.activeInHierarchy) popup.Tutup();
            else panel.SetActive(false);
        }
    }
}