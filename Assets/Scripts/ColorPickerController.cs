using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ColorPickerController : MonoBehaviour
{
    public float currentHue, currentSat, currentVal;
    [SerializeField] private RawImage hueImage, satValImage, outputImage;
    [SerializeField] private Slider hueSlider;
    [SerializeField] private TMP_InputField hexInputField;
    private Texture2D hueTexture, svTexture, outputTexture;
    public Action<Color> OnColorChange;

    private void Start()
    {
        CreateHueImage();
        CreateSVImage();
        CreateOutputImage();
        UpdateOutputImage();
    }

    private void CreateHueImage()
    {
        hueTexture = new Texture2D(1, 16);
        hueTexture.wrapMode = TextureWrapMode.Clamp;
        hueTexture.name = "HueTexture";
        for (int i = 0; i < hueTexture.height; i++)
        {
            hueTexture.SetPixel(0, i, Color.HSVToRGB((float) i / hueTexture.height, 1, 1));
        }
        hueTexture.Apply();
        currentHue = 0;
        hueImage.texture = hueTexture;
    }

    private void CreateSVImage()
    {
        svTexture = new Texture2D(16, 16);
        svTexture.wrapMode = TextureWrapMode.Clamp;
        svTexture.name = "SatValTexture";
        for (int i = 0; i < svTexture.height; i++)
        {
            for (int j = 0; j < svTexture.width; j++)
            {
                svTexture.SetPixel(j, i, Color.HSVToRGB(currentHue, (float) j / svTexture.width, (float) i / svTexture.height));
            }
        }
        svTexture.Apply();
        currentSat = 0;
        currentVal = 0;
        satValImage.texture = svTexture;
    }

    private void CreateOutputImage()
    {
        outputTexture = new Texture2D(1, 16);
        outputTexture.wrapMode = TextureWrapMode.Clamp;
        outputTexture.name = "OutputTexture";
        Color currentColor = Color.HSVToRGB(currentHue, currentSat, currentVal);
        for (int i = 0; i < outputTexture.height; i++)
        {
            outputTexture.SetPixel(0, i, currentColor);
        }
        outputTexture.Apply();
        outputImage.texture = outputTexture;
    }

    private void UpdateOutputImage()
    {
        Color currentColor = Color.HSVToRGB(currentHue, currentSat, currentVal);
        for (int i = 0; i < outputTexture.height; i++)
        {
            outputTexture.SetPixel(0, i, currentColor);
        }
        outputTexture.Apply();
        hexInputField.text = ColorUtility.ToHtmlStringRGB(currentColor);
        OnColorChange?.Invoke(currentColor);
    }

    public void SetSV(float S, float V)
    {
        currentSat = S;
        currentVal = V;
        UpdateOutputImage();
    }

    public void UpdateSVImage()
    {
        currentHue = hueSlider.value;
        for (int i = 0; i < svTexture.height; i++)
        {
            for (int j = 0; j < svTexture.width; j++)
            {
                svTexture.SetPixel(j, i, Color.HSVToRGB(currentHue, (float) j / svTexture.width, (float) i / svTexture.height));
            }
        }
        svTexture.Apply();
        UpdateOutputImage();
    }

    public void OnTextInput()
    {
        string hex = hexInputField.text;
        if (hex.Length != 6)
        {
            return;
        }
        if (ColorUtility.TryParseHtmlString("#" + hex, out Color color))
        {
            Color.RGBToHSV(color, out currentHue, out currentSat, out currentVal);
            hueSlider.value = currentHue;
            hexInputField.text = "";
            UpdateOutputImage();
        }
    }
}
