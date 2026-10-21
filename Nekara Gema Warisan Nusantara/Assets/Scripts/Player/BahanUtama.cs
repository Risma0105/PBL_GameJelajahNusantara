using UnityEngine;
using UnityEngine.InputSystem;

public class BahanUtama : MonoBehaviour
{
    [Header("Pengaturan Tebang Pohon")]
    public int jumlahTebasanDibutuhkan = 3;
    private int tebasanSaatIni = 0;
    private bool sudahDitebang = false;

    [Header("Prefab Hasil Panen (Item Bahan Pengganti)")]
    public GameObject prefabItemBahan; // Tarik prefab item bahan ke sini di Inspector

    void OnMouseDown()
    {
        // Jika pohon belum ditebang habis, klik untuk menebang
        if (!sudahDitebang)
        {
            TebasPohon();
        }
    }

    void TebasPohon()
    {
        tebasanSaatIni++;
        Debug.Log("🪓 Pohon ditebas! Progress: " + tebasanSaatIni + "/" + jumlahTebasanDibutuhkan);

        if (tebasanSaatIni >= jumlahTebasanDibutuhkan)
        {
            SelesaikanPenebangan();
        }
    }

    void SelesaikanPenebangan()
    {
        sudahDitebang = true;
        Debug.Log("✨ Pohon ditebang habis dan berubah menjadi bahan!");

        // 1. Munculkan prefab item bahan tepat di posisi pohon ini
        if (prefabItemBahan != null)
        {
            Instantiate(prefabItemBahan, transform.position, Quaternion.identity);
        }
        else
        {
            Debug.LogError("⚠️ PERINGATAN: Prefab Item Bahan belum dimasukkan di Inspector pada objek " + gameObject.name);
        }

        // 2. Hancurkan / hilangkan objek pohon asalnya dari map
        Destroy(gameObject);
    }
}