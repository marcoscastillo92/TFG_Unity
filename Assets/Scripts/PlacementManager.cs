using System;
using System.Collections.Generic;
using UnityEngine;

public class PlacementManager : MonoBehaviour
{
    [SerializeField] private int width, height;
    public Action<GameObject> CreateMetadata;
    private Grid placementGrid;
    public Dictionary<Vector3Int, bool> isCrossWalk = new();
    public Dictionary<Vector3Int, StructureModel> tempObjects = new();
    public Dictionary<Vector3Int, StructureModel> permanentObjects = new();
    private PrefabMap prefabMap;

    private void Start()
    {
        placementGrid = new Grid(width, height);
        prefabMap = FindFirstObjectByType<PrefabMap>();
    }

    public bool IsPositionInsideBounds(Vector3Int position)
    {
        return position.x >= 0 && position.x < width && position.z >= 0 && position.z < height;
    }

    internal bool IsFreePosition(Vector3Int position)
    {
        if (!IsPositionInsideBounds(position))
        {
            return false;
        }
        return IsSameType(position, CellType.Empty);
    }

    private bool IsSameType(Vector3Int position, CellType type)
    {
        return placementGrid[position.x, position.z] == type;
    }

    internal void RemoveStructure(Vector3Int position)
    {
        if (tempObjects.ContainsKey(position))
        {
            placementGrid[position.x, position.z] = CellType.Empty;
            Destroy(tempObjects[position].gameObject);
            tempObjects.Remove(position);
            isCrossWalk.Remove(position);
        }
        else if (permanentObjects.ContainsKey(position))
        {
            placementGrid[position.x, position.z] = CellType.Empty;
            Destroy(permanentObjects[position].gameObject);
            permanentObjects.Remove(position);
            isCrossWalk.Remove(position);
        }
    }

    internal void PlaceTempObject(Vector3Int position, GameObject prefab, CellType type)
    {
        placementGrid[position.x, position.z] = type;
        StructureModel structureModel = CreateNewStructureModel(position, prefab, type);
        tempObjects.Add(position, structureModel);
    }

    private StructureModel CreateNewStructureModel(Vector3Int position, GameObject prefab, CellType type)
    {
        GameObject newModel = new(type.ToString());
        newModel.transform.SetParent(transform);
        newModel.transform.localPosition = position;
        StructureModel structureModel = newModel.AddComponent<StructureModel>();
        structureModel.setType(prefab.tag);
        structureModel.CreateModel(prefab);
        return structureModel;
    }

    public void SwapStructureModel(Vector3Int position, GameObject prefab, Quaternion rotation)
    {
        if (tempObjects.ContainsKey(position))
        {
            tempObjects[position].SwapModel(prefab, rotation);
        }
        else if (permanentObjects.ContainsKey(position))
        {
            permanentObjects[position].SwapModel(prefab, rotation);
        }
    }

    internal List<CellType> GetAdjacentCellTypes(Vector3Int position)
    {
        return placementGrid.GetAdjecentCellTypes(position.x, position.z);
    }

    internal List<Vector3Int> GetAdjecentCellsByType(Vector3Int position, CellType type)
    {
        List<Vector3Int> adjecents = new();
        foreach (Point point in placementGrid.GetAdjecentCellsByType(position.x, position.z, type))
        {
            adjecents.Add(new Vector3Int(point.x, 0, point.y));
        }
        return adjecents;
    }

    internal void RemoveAllTempObjects()
    {
        foreach (StructureModel obj in tempObjects.Values)
        {
            var position = Vector3Int.RoundToInt(obj.transform.position);
            placementGrid[position.x, position.z] = CellType.Empty;
            Destroy(obj.gameObject);
        }
        tempObjects.Clear();
    }

    internal List<Vector3Int> GetPathBetween(Vector3Int startPosition, Vector3Int position)
    {
        var pointsPath = GridSearch.AStarSearch(placementGrid, new Point(startPosition.x, startPosition.z), new Point(position.x, position.z));
        List<Vector3Int> path = new();
        foreach (Point point in pointsPath)
        {
            path.Add(new Vector3Int(point.x, 0, point.y));
        }
        return path;
    }

    internal void persistStructures()
    {
        foreach (var tempObject in tempObjects)
        {
            if (!permanentObjects.ContainsKey(tempObject.Key))
            {
                permanentObjects.Add(tempObject.Key, tempObject.Value);
                CreateMetadata.Invoke(tempObject.Value.currentModel);
            }
        }
        tempObjects.Clear();
    }

    public void ToggleCrossWalk(Vector3Int position)
    {
        if (isCrossWalk.ContainsKey(position))
        {
            isCrossWalk[position] = !isCrossWalk[position];
        }
        else
        {
            isCrossWalk.Add(position, true);
        }
    }

    public bool IsCrossWalk(Vector3Int position)
    {
        if (isCrossWalk.ContainsKey(position))
        {
            return isCrossWalk[position];
        }
        return false;
    }

    public GameObject GetObjectByPosition(Vector3Int position)
    {
        if (permanentObjects.ContainsKey(position))
        {
            return permanentObjects[position].gameObject;
        }
        return null;
    }

    public void SetObjectByPosition(ObjectMetadata metadata)
    {
        Vector3Int position = Vector3Int.RoundToInt(metadata.position);
        GameObject prefab = prefabMap.GetPrefabByName(metadata.prefabName);
        if (permanentObjects.ContainsKey(position))
        {
            permanentObjects[position].SwapModel(prefab, metadata.rotation);
        }
        else
        {
            CellType cellType = prefab.CompareTag("Road") ? CellType.Road : CellType.Empty;
            StructureModel structureModel = CreateNewStructureModel(position, prefab, cellType);
            permanentObjects.Add(position, structureModel);
            CreateMetadata.Invoke(structureModel.currentModel);
        }
    }

    public void ClearScene()
    {
        RemoveAllTempObjects();
        foreach (var obj in permanentObjects)
        {
            Destroy(obj.Value.gameObject);
        }
        permanentObjects.Clear();
        placementGrid.Clear();
        isCrossWalk.Clear();
        tempObjects.Clear();
    }

}
