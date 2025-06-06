using UnityEngine;

public class StructureModel : MonoBehaviour
{
    public float yHeight = 0f;
    public string type;
    Vector3 currentPosition { get; set; }
    Quaternion currentRotation { get; set; }
    public GameObject currentModel { get; set; }

    public void CreateModel(GameObject model)
    {
        model.layer = LayerMask.NameToLayer("Selectable");
        GameObject newModel = Instantiate(model, transform);
        currentModel = newModel;
        yHeight = newModel.transform.position.y;
        currentPosition = newModel.transform.localPosition;
        currentRotation = newModel.transform.localRotation;
    }

    public void SwapModel(GameObject model, Quaternion rotation)
    {
        model.layer = LayerMask.NameToLayer("Selectable");
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        GameObject newModel = Instantiate(model, transform);
        newModel.transform.localPosition = new Vector3(0, yHeight, 0);
        newModel.transform.localRotation = rotation;
        currentPosition = newModel.transform.localPosition;
        currentRotation = newModel.transform.localRotation;
        currentModel = newModel;
    }

    public void ToggleModel(GameObject model)
    {
        model.layer = LayerMask.NameToLayer("Selectable");
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        GameObject newModel = Instantiate(model, transform);
        newModel.transform.localPosition = currentPosition;
        newModel.transform.localRotation = currentRotation;
        currentModel = newModel;
    }

    public void setType(string type)
    {
        this.type = type;
    }
}
