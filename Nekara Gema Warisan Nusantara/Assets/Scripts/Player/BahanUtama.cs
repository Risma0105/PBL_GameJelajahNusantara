using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

public class BahanUtama : MonoBehaviour
{
    [Header("Pengaturan Tebang Pohon")]
    public int jumlahTebasanDibutuhkan = 3;
    private int tebasanSaatIni = 0;
    private bool sudahDitebang = false;

    [Header("Prefab Hasil Panen (Item Bahan di Samping Pohon)")]
    public GameObject prefabItemBahan; // Tarik prefab item bahan (misal: Item_TaliRotan) ke sini
    public Vector3 offsetSpawn = new Vector3(1.0f, 0f, 0f); // Posisi munculnya item di samping pohon

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
        Debug.Log("✨ Pohon selesai ditebang! Memunculkan hasil bahan di sampingnya.");

        // 1. Munculkan Prefab Item Bahan di samping pohon
        if (prefabItemBahan != null)
        {
            Vector3 posisiMuncul = transform.position + offsetSpawn;
            Instantiate(prefabItemBahan, posisiMuncul, Quaternion.identity);
        }
        else
        {
            Debug.LogError("⚠️ PERINGATAN: Prefab Item Bahan belum dimasukkan di Inspector pada objek " + gameObject.name);
        }

        // 2. Nonaktifkan collider pohon supaya tidak bisa ditebang lagi
        var collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = false;
        }

        // 3. Opsional: Ubah warna pohon sedikit jadi agak gelap sebagai tanda sudah dipanen
        var spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = new Color(0.7f, 0.7f, 0.7f, 1f);
        }
    }
}