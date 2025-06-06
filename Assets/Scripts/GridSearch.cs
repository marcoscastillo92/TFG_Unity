using System;
using System.Collections.Generic;
using System.Linq;

public class GridSearch {
  public static List<Point> AStarSearch(Grid grid, Point start, Point goal)
  {
    List<Point> path = new();
    List<Point> positionsToCheck = new();
    Dictionary<Point, float> cost = new();
    Dictionary<Point, float> priority = new();
    Dictionary<Point, Point> parents = new();

    positionsToCheck.Add(start);
    priority.Add(start, 0);
    cost.Add(start, 0);
    parents.Add(start, null);

    while (positionsToCheck.Count > 0)
    {
      Point current = GetClosestVertex(positionsToCheck, priority);
      positionsToCheck.RemoveAt(0);

      if (current.Equals(goal))
      {
        while (current != null && parents.ContainsKey(current))
        {
          path.Add(current);
          current = parents[current];
        }
        break;
      }

      List<Point> adjacentCells = grid.GetAdjecentCells(current.x, current.y).Where(p => p != grid.invalidPoint).ToList();
      foreach (Point adjacent in adjacentCells)
      {
        float newCost = cost[current] + 1;
        if (!cost.ContainsKey(adjacent) || newCost < cost[adjacent])
        {
          cost[adjacent] = newCost;
          float priorityValue = newCost + Math.Abs(goal.x - adjacent.x) + Math.Abs(goal.x - adjacent.y);
          priority[adjacent] = priorityValue;
          parents[adjacent] = current;
          positionsToCheck.Add(adjacent);
          positionsToCheck.Sort((a, b) => priority[a].CompareTo(priority[b]));
        }
      }
    }

    return path;
  }

    private static Point GetClosestVertex(List<Point> positionsTocheck, Dictionary<Point, float> priority)
    {
      Point candidate = positionsTocheck.First();
      foreach (Point point in positionsTocheck)
      {
          if (priority[point] < priority[candidate])
          {
              candidate = point;
          }
      }
      return candidate;
    }
}