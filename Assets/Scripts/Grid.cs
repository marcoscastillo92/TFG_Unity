using System;
using System.Collections.Generic;
using System.Linq;

public class Point
{
    public int x;
    public int y;
    public Point(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public override bool Equals(object obj)
    {
        if (obj is not Point) 
        {
            return false;
        }
        Point other = obj as Point;
        return other != null && x == other.x && y == other.y;
    }

    public override int GetHashCode()
    {
        unchecked 
        {
            int hash = 17;
            hash = hash * 23 + x.GetHashCode();
            hash = hash * 23 + y.GetHashCode();
            return hash;
        }
    }
}

public enum CellType {
    Empty,
    Road,
    Structure,
    None
}

public class Grid
{
    private CellType[,] grid;
    private int width;
    private int height;
    private List<Point> roadList = new();
    private List<Point> structureList = new();
    public Point invalidPoint = new(-1, -1);

    public Grid(int width, int height)
    {
        this.width = width;
        this.height = height;
        grid = new CellType[width, height];
    }

    public CellType this[int x, int y]
    {
        get
        {
            return grid[x, y];
        }
        set
        {
            if (value == CellType.Road)
            {
                structureList.Remove(new Point(x, y));
                roadList.Add(new Point(x, y));
            }
            else if (value == CellType.Structure)
            {
                roadList.Remove(new Point(x, y));
                structureList.Add(new Point(x, y));
            }
            grid[x, y] = value;
        }
    }

    public List<Point> GetAdjecentCells(int x, int y)
    {
        List<Point> adjecentCells = new();
        if (x > 0)
        {
            adjecentCells.Add(new Point(x - 1, y));
        }
        else
        {
            adjecentCells.Add(invalidPoint);
        }
        if (y < height - 1)
        {
            adjecentCells.Add(new Point(x, y + 1));
        }
        else
        {
            adjecentCells.Add(invalidPoint);
        }
        if (x < width - 1)
        {
            adjecentCells.Add(new Point(x + 1, y));
        }
        else
        {
            adjecentCells.Add(invalidPoint);
        }
        if (y > 0)
        {
            adjecentCells.Add(new Point(x, y - 1));
        }
        else
        {
            adjecentCells.Add(invalidPoint);
        }
        return adjecentCells;
    }

    public List<Point> GetValidPoints(List<Point> points)
    {
        return points.Where(point => point != invalidPoint).ToList();
    }

    public List<Point> GetAdjecentCellsByType(int x, int y, CellType type)
    {
        List<Point> adjecentCells = GetAdjecentCells(x, y);
        List<Point> adjecentCellsByType = new();
        foreach (Point point in adjecentCells)
        {
            if (point == invalidPoint)
            {
                continue;
            }
            if (grid[point.x, point.y] == type)
            {
                adjecentCellsByType.Add(point);
            }
        }
        return adjecentCellsByType;
    }

    public List<CellType> GetAdjecentCellTypes(int x, int y)
    {
        List<Point> adjecentCells = GetAdjecentCells(x, y);
        List<CellType> adjecentCellTypes = new();
        foreach (Point point in adjecentCells) 
        {
            if (point == invalidPoint)
            {
                adjecentCellTypes.Add(CellType.None);
            }
            else
            {
                adjecentCellTypes.Add(grid[point.x, point.y]);
            }
        }
        return adjecentCellTypes;
    }

    public void Clear()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                grid[i, j] = CellType.Empty;
            }
        }
        roadList.Clear();
        structureList.Clear();
    }

}
