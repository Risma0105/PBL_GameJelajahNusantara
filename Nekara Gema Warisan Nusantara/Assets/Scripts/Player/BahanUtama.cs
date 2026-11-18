using UnityEngine;

[RequireComponent(typeof(ProximityPrompt))]
public class BahanUtama : MonoBehaviour
{
    [Header("Pengaturan Tebang Pohon")]
    public int jumlahTebasanDibutuhkan = 3;
    private int tebasanSaatIni = 0;
    private bool sudahDitebang = false;

    [Header("Prefab Hasil Panen (Item Bahan Pengganti)")]
    public GameObject prefabItemBahan;

    private ProximityPrompt prompt;

    void Awake()
    {
        prompt = GetComponent<ProximityPrompt>();
    }

    void OnEnable()  { prompt.onTriggered.AddListener(TebasPohon); }
    void OnDisable() { prompt.onTriggered.RemoveListener(TebasPohon); }

    void TebasPohon()
    {
        if (sudahDitebang) return;

        tebasanSaatIni++;
        Debug.Log("Memotong pohon... Progress: " + tebasanSaatIni + "/" + jumlahTebasanDibutuhkan);

        prompt.SetActionText("Tebang (" + tebasanSaatIni + "/" + jumlahTebasanDibutuhkan + ")");

        if (tebasanSaatIni >= jumlahTebasanDibutuhkan)
            SelesaikanPenebangan();
    }

    void SelesaikanPenebangan()
    {
        sudahDitebang = true;

        if (prefabItemBahan != null)
        {
            // Posisi muncul sedikit di bawah pusat pohon (akar). Ubah -1.2f kalau perlu.
            Vector3 posisiSpawn = transform.position + new Vector3(0f, -1.2f, 0f);
            GameObject itemBaru = Instantiate(prefabItemBahan, posisiSpawn, Quaternion.identity);

            GameObject kumpulanBahanObj = GameObject.Find("KumpulanBahan");
            if (kumpulanBahanObj != null)
                itemBaru.transform.SetParent(kumpulanBahanObj.transform);
        }
        else
        {
            Debug.LogError("PERINGATAN: Prefab Item Bahan belum dimasukkan di Inspector pada objek " + gameObject.name);
        }

        Destroy(gameObject); // prompt ikut hilang karena Canvas-nya anak objek ini
    }
}