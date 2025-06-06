using UnityEngine;
using UnityEngine.UI;

public class GizmosImagesController : MonoBehaviour
{
    [SerializeField] private GameObject[] images;
    private int currentImageIndex = 0;

    public void ToggleImage()
    {
        images[currentImageIndex].SetActive(false);
        currentImageIndex = (currentImageIndex + 1) % images.Length;
        images[currentImageIndex].SetActive(true);
    }

    public void ResetIndex()
    {
        images[currentImageIndex].SetActive(false);
        currentImageIndex = 0;
        images[currentImageIndex].SetActive(true);
    }
}
