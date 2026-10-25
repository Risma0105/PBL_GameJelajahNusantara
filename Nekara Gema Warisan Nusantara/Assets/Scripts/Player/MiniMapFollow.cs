using UnityEngine;

public class MiniMapFollow : MonoBehaviour
{
    public Transform player; // Tarik objek Player ke sini di Inspector
    public float tinggiKamera = 10f;

    void LateUpdate()
    {
        if (player != null)
        {
            // Posisi kamera mengikuti X dan Y player, sedangkan Z tetap di atas
            Vector3 posisiBaru = player.position;
            posisiBaru.z = -10f; 
            transform.position = posisiBaru;
        }
    }
}