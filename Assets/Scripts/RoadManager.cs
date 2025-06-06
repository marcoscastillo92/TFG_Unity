using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RoadManager : MonoBehaviour
{
    [SerializeField] private PlacementManager placementManager;
    [SerializeField] private List<Vector3Int> tempPlacementPositions = new();
    [SerializeField] private List<Vector3Int> positionsToRecheck = new();
    public Action<GameObject> CreateMetadata, RemoveMetadata;
    private RoadFixer roadFixer;
    private Vector3Int startPosition;
    private bool placementMode = false;

    private void Start()
    {
        roadFixer = GetComponent<RoadFixer>();
        placementManager.CreateMetadata += CreateMetadataHandler;
    }

    private void CreateMetadataHandler(GameObject prefab)
    {
        CreateMetadata.Invoke(prefab);
    }

    public void RemoveRoad(Vector3Int position)
    {
        if (placementManager.IsFreePosition(position))
        {
            return;
        }
        GameObject prefab = placementManager.GetObjectByPosition(position);
        RemoveMetadata.Invoke(prefab);
        placementManager.RemoveStructure(position);
        var adjacent = placementManager.GetAdjecentCellsByType(position, CellType.Road);
        foreach (Vector3Int adjecentPosition in adjacent)
        {
            roadFixer.FixRoad(placementManager, adjecentPosition);
        }
    }

    public void PlaceRoad(Vector3Int position, bool importing = false)
    {
        if (!placementManager.IsPositionInsideBounds(position))
        {
            placementMode = false;
            return;
        }
        if (!placementMode || importing)
        {
            tempPlacementPositions.Clear();
            positionsToRecheck.Clear();
            placementMode = true;
            startPosition = position;
            tempPlacementPositions.Add(position);
            if (placementManager.IsFreePosition(position))
            {
                placementManager.PlaceTempObject(position, roadFixer.deadEnd, CellType.Road);
            }
            else 
            {
                positionsToRecheck.Add(position);
            }
        } 
        else 
        {
            placementManager.RemoveAllTempObjects();
            tempPlacementPositions.Clear();
            foreach (var positionsToFix in positionsToRecheck)
            {
                roadFixer.FixRoad(placementManager, positionsToFix);
            }
            positionsToRecheck.Clear();
            tempPlacementPositions = placementManager.GetPathBetween(startPosition, position);

            foreach (var tempPosition in tempPlacementPositions)
            {
                if (!placementManager.IsFreePosition(tempPosition))
                {
                    positionsToRecheck.Add(tempPosition);
                    continue;
                }
                placementManager.PlaceTempObject(tempPosition, roadFixer.deadEnd, CellType.Road);
            }
        }
        FixRoadPrefabs();
    }

    private void FixRoadPrefabs()
    {
        foreach (Vector3Int position in tempPlacementPositions)
        {
            roadFixer.FixRoad(placementManager, position);
            var adjacent = placementManager.GetAdjecentCellsByType(position, CellType.Road);
            foreach (Vector3Int adjecentPosition in adjacent)
            {
                if (!positionsToRecheck.Contains(adjecentPosition))
                {
                    positionsToRecheck.Add(adjecentPosition);
                }
            }
        }

        foreach (Vector3Int position in positionsToRecheck)
        {
            roadFixer.FixRoad(placementManager, position);
        }
    }

    public void ToggleCrossWalk(Vector3Int position)
    {
        placementManager.ToggleCrossWalk(position);
        roadFixer.FixRoad(placementManager, position);
    }

    public void FinishPlacement()
    {
        if (placementMode)
        {
            placementMode = false;
            placementManager.persistStructures();
            tempPlacementPositions.Clear();
            startPosition = Vector3Int.zero;
        }
    }

    public bool IsCrossWalk(Vector3Int position)
    {
        return placementManager.IsCrossWalk(position);
    }

    public void ClearScene()
    {
        placementManager.ClearScene();
        tempPlacementPositions.Clear();
        positionsToRecheck.Clear();
    }

    public void SetInitialState()
    {
        placementManager.persistStructures();
        placementMode = false;
        startPosition = Vector3Int.zero;
    }

}
