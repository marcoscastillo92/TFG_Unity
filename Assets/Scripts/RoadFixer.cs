using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoadFixer : MonoBehaviour
{
    public GameObject deadEnd, roadStraight, roadStraightCrossWalk, corner, cornerCrossWalk, threeWay, threeWayCrossWalk, fourWay, fourWayCrossWalk;

    public void FixRoad(PlacementManager placementManager, Vector3Int position)
    {
        var adjacent = placementManager.GetAdjacentCellTypes(position);
        int count = adjacent.Where(x => x == CellType.Road).Count();
        if (count == 0 || count == 1)
        {
            FixDeadEnd(placementManager, position, adjacent);
        }
        else if (count == 2)
        {
            if (adjacent[1] == CellType.Road && adjacent[3] == CellType.Road)
            {
                GameObject gameObjectToPlace = placementManager.IsCrossWalk(position) ? roadStraightCrossWalk : roadStraight;
                placementManager.SwapStructureModel(position, gameObjectToPlace, Quaternion.identity);
            } else if (adjacent[0] == CellType.Road && adjacent[2] == CellType.Road) 
            {
                GameObject gameObjectToPlace = placementManager.IsCrossWalk(position) ? roadStraightCrossWalk : roadStraight;
                placementManager.SwapStructureModel(position, gameObjectToPlace, Quaternion.Euler(0, 90, 0));
            }
            else
            {
                FixCorner(placementManager, position, adjacent);
            }
        }
        else if (count == 3)
        {
            FixThreeWay(placementManager, position, adjacent);
        }
        else if (count == 4)
        {
            GameObject gameObjectToPlace = placementManager.IsCrossWalk(position) ? fourWayCrossWalk : fourWay;
            placementManager.SwapStructureModel(position, gameObjectToPlace, Quaternion.identity);
        }
    }

    // [left, up, right, down]
    private void FixThreeWay(PlacementManager placementManager, Vector3Int position, List<CellType> adjacent)
    {
        bool isCrossWalk = placementManager.IsCrossWalk(position);
        Quaternion rotation = GetRotationForThreeWay(0, isCrossWalk);; // left, up, right
        if (adjacent[0] == CellType.Road && adjacent[1] == CellType.Road && adjacent[2] == CellType.Road) // left, up, down
        {
            rotation = GetRotationForThreeWay(90, isCrossWalk);
        } 
        else if (adjacent[1] == CellType.Road && adjacent[2] == CellType.Road && adjacent[3] == CellType.Road) // up, right, down
        {
            rotation = GetRotationForThreeWay(180, isCrossWalk);
        }
        else if (adjacent[0] == CellType.Road && adjacent[2] == CellType.Road && adjacent[3] == CellType.Road) // left, right, down
        {
            rotation = GetRotationForThreeWay(270, isCrossWalk);
        }
        GameObject gameObjectToPlace = isCrossWalk ? threeWayCrossWalk : threeWay;
        placementManager.SwapStructureModel(position, gameObjectToPlace, rotation);
    }

    private Quaternion GetRotationForThreeWay(int yRot, bool isCrossWalk)
    {
        int angles = isCrossWalk ? yRot - 90 : yRot;
        return Quaternion.Euler(0, angles, 0);
    }

    private void FixCorner(PlacementManager placementManager, Vector3Int position, List<CellType> adjacent)
    {
        Quaternion rotation = Quaternion.identity;
        if (adjacent[1] == CellType.Road && adjacent[2] == CellType.Road)
        {
            rotation = Quaternion.Euler(0, 90, 0);
        }
        else if (adjacent[2] == CellType.Road && adjacent[3] == CellType.Road)
        {
            rotation = Quaternion.Euler(0, 180, 0);
        }
        else if (adjacent[3] == CellType.Road && adjacent[0] == CellType.Road)
        {
            rotation = Quaternion.Euler(0, 270, 0);
        }
        GameObject gameObjectToPlace = placementManager.IsCrossWalk(position) ? cornerCrossWalk : corner;
        placementManager.SwapStructureModel(position, gameObjectToPlace, rotation);
    }

    private void FixDeadEnd(PlacementManager placementManager, Vector3Int position, List<CellType> adjacent)
    {
        Quaternion rotation = Quaternion.identity;
        if (adjacent[0] == CellType.Road)
        {
            rotation = Quaternion.Euler(0, 90, 0);
        }
        else if (adjacent[1] == CellType.Road)
        {
            rotation = Quaternion.Euler(0, 180, 0);
        }
        else if (adjacent[2] == CellType.Road)
        {
            rotation = Quaternion.Euler(0, 270, 0);
        }
        placementManager.SwapStructureModel(position, deadEnd, rotation);
    }
}
