using UnityEngine;

public class ObjectClickSFX : MonoBehaviour
{
    public string namaSFX = "";

    private void OnMouseDown()
    {
        if (AudioManager.Instance != null && !string.IsNullOrEmpty(namaSFX))
        {
            AudioManager.Instance.PlaySFX(namaSFX);
        }
    }
}