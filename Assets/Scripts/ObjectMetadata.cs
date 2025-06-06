using UnityEngine;
using Newtonsoft.Json;

public class ObjectMetadataSerializable
{
    public float positionX;
    public float positionY;
    public float positionZ;
    public float rotationX;
    public float rotationY;
    public float rotationZ;
    public float rotationW;
    public string prefabName;
    public float colorR;
    public float colorG;
    public float colorB;
    public float colorA;
    public bool variant;
}

[System.Serializable]
public class ObjectMetadata
{
    public Vector3 position { get; set; }
    public Quaternion rotation { get; set; }
    public string prefabName { get; set; }
    public Color color { get; set; }
    public bool variant { get; set; }

    public override string ToString()
    {
        ObjectMetadataSerializable serializable = new ObjectMetadataSerializable
        {
            positionX = position.x,
            positionY = position.y,
            positionZ = position.z,
            rotationX = rotation.x,
            rotationY = rotation.y,
            rotationZ = rotation.z,
            rotationW = rotation.w,
            prefabName = prefabName,
            colorR = color.r,
            colorG = color.g,
            colorB = color.b,
            colorA = color.a,
            variant = variant
        };
        return JsonConvert.SerializeObject(serializable);
    }

    public ObjectMetadata fromString(string jsonString)
    {
        var json = JsonConvert.DeserializeObject<ObjectMetadataSerializable>(jsonString.ToString());
        position = new Vector3(json.positionX, json.positionY, json.positionZ);
        rotation = new Quaternion(json.rotationX, json.rotationY, json.rotationZ, json.rotationW);
        prefabName = json.prefabName;
        color = new Color(json.colorR, json.colorG, json.colorB, json.colorA);
        variant = json.variant;
        return this;
    }
}