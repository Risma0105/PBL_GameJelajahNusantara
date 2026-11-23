using UnityEngine;

// Static class: tidak perlu ditempel ke GameObject.
// Menyimpan status alat musik sudah dipajang atau belum, agar tetap ada
// walau scene dipindah atau game ditutup.
public static class MuseumData
{
    const string Prefix = "museum_";

    public static bool IsDisplayed(string id)
    {
        return PlayerPrefs.GetInt(Prefix + id.Trim().ToLower(), 0) == 1;
    }

    public static void SetDisplayed(string id, bool value)
    {
        PlayerPrefs.SetInt(Prefix + id.Trim().ToLower(), value ? 1 : 0);
        PlayerPrefs.Save();
    }
}