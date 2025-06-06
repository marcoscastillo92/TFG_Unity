using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SVImageController : MonoBehaviour, IDragHandler, IPointerClickHandler
{
    [SerializeField] private Image pickerImage;
    private RawImage SVImage;
    private ColorPickerController colorPickerController;
    private RectTransform rectTransform, pickerTransform;

    private void Awake()
    {
        SVImage = GetComponent<RawImage>();
        colorPickerController = FindFirstObjectByType<ColorPickerController>();
        rectTransform = GetComponent<RectTransform>();
        pickerTransform = pickerImage.GetComponent<RectTransform>();
        pickerTransform.position = new Vector2(-(rectTransform.sizeDelta.x * 0.5f), -(rectTransform.sizeDelta.y * 0.5f));
    }

    void UpdateColor(PointerEventData eventData)
    {
        Vector2 pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out pos);
        float deltaX = rectTransform.sizeDelta.x * 0.5f;
        float deltaY = rectTransform.sizeDelta.y * 0.5f;

        pos.x = Mathf.Clamp(pos.x, -deltaX, deltaX);
        pos.y = Mathf.Clamp(pos.y, -deltaY, deltaY);
        float x = pos.x + deltaX;
        float y = pos.y + deltaY;
        float xNorm = x / rectTransform.sizeDelta.x;
        float yNorm = y / rectTransform.sizeDelta.y;

        pickerTransform.localPosition = pos;
        pickerImage.color = Color.HSVToRGB(0, 0, 1 - yNorm);
        colorPickerController.SetSV(xNorm, yNorm);
    }

    public void OnDrag(PointerEventData eventData)
    {
        UpdateColor(eventData);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        UpdateColor(eventData);
    }

}
