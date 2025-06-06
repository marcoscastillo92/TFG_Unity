using UnityEngine;

public class ToggleActive : MonoBehaviour
{
    public void ToggleActiveAction()
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }
}
