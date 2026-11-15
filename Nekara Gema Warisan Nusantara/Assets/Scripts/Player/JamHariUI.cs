using UnityEngine;
using TMPro;

public class JamHariUI : MonoBehaviour
{
    public TMP_Text teksJam;    // "9:03 am"
    public TMP_Text teksHari;   // "Day 1"

    [Header("Format")]
    public bool format24Jam = false;
    [Tooltip("Jam naik per kelipatan menit ini. 1 = mulus, 10 = seperti Stardew.")]
    public int kelipatanMenit = 1;
    public string awalanHari = "Day ";
    public bool tampilkanMaksHari = false;   // "Day 3/10"

    void Update()
    {
        if (GameManager.Instance == null) return;

        int totalMenit = Mathf.FloorToInt(GameManager.Instance.JamSekarang * 60f);
        int jam = (totalMenit / 60) % 24;
        int menit = totalMenit % 60;
        menit -= menit % Mathf.Max(1, kelipatanMenit);

        if (teksJam != null)
        {
            if (format24Jam)
            {
                teksJam.text = $"{jam:00}:{menit:00}";
            }
            else
            {
                int jam12 = jam % 12 == 0 ? 12 : jam % 12;
                string akhiran = jam < 12 ? "am" : "pm";
                teksJam.text = $"{jam12}:{menit:00} {akhiran}";
            }
        }

        if (teksHari != null)
        {
            string hari = awalanHari + GameManager.Instance.hariSekarang;
            if (tampilkanMaksHari) hari += "/" + GameManager.Instance.totalMaksimalHari;
            teksHari.text = hari;
        }
    }
}