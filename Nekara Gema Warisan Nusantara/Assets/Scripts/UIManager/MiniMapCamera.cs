using UnityEngine;

public class MiniMapCamera : MonoBehaviour
{
    public Transform player;
    public bool batasiKeArea = true;
    public Vector2 batasMin = new Vector2(-15f, -15f);
    public Vector2 batasMax = new Vector2(15f, 15f);

    void LateUpdate()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
            else return;
        }

        Vector3 pos = player.position;

        if (batasiKeArea)
        {
            pos.x = Mathf.Clamp(pos.x, batasMin.x, batasMax.x);
            pos.y = Mathf.Clamp(pos.y, batasMin.y, batasMax.y);
        }

        transform.position = new Vector3(pos.x, pos.y, -10f);
    }
}