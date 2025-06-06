using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class ClickablePrefabUI : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private GameObject prefab;
    public Action<GameObject> OnSelectUIObject, OnDragUIObject, OnDropUIObject;

    public void OnPointerClick(PointerEventData eventData)
    {
        OnSelectUIObject?.Invoke(prefab);
    }

    public void OnDrag()
    {
        OnDragUIObject?.Invoke(prefab);
    }
}
