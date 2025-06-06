using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class ZoomSlider : MonoBehaviour
{
    private float incrementStep = 1.0f;
    private Slider slider;

    private void Start()
    {
        slider = GetComponent<Slider>();
    }

    public void IncrementValue()
    {
        float newValue = Mathf.Clamp(slider.value + incrementStep, slider.minValue, slider.maxValue);
        slider.value = newValue;
    }

    public void DecrementValue()
    {
        float newValue = Mathf.Clamp(slider.value - incrementStep, slider.minValue, slider.maxValue);
        slider.value = newValue;
    }
}
