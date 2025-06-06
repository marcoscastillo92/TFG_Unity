using System.Collections.Generic;
using UnityEngine;

public class PrefabMap: MonoBehaviour
{
    [SerializeField] private List<GameObject> prefabs = new();
    private Dictionary<string, GameObject> prefabMap = new();

    private void Awake()
    {
        foreach (var prefab in prefabs)
        {
            prefabMap.Add(prefab.name, prefab);
        }
    }

    public GameObject GetPrefabByName(string name)
    {
        return prefabMap[name];
    }
}
