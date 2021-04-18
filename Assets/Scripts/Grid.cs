using UnityEngine;
using System.Collections.Generic;

public class Grid : MonoBehaviour
{

    public Dictionary<int, List<int>> cells = new Dictionary<int, List<int>>();
    public Dictionary<int, int> boids = new Dictionary<int, int>();
    public int cellSize = 50;
    public int gridSize = 10000;

    public static int PositionToCell3D(Vector3 pos, int cellSize, int gridSize)
    {
        return ((int)(pos.x / cellSize))
            + ((int)(pos.z / cellSize)) * gridSize
            + ((int)(pos.y / cellSize)) * gridSize * gridSize;
    }

    public void UpdatePosition(int i, Vector3 pos)
    {
        int newCell = PositionToCell3D(pos, cellSize, gridSize);
        int currentCell = boids[i];
        if (newCell != currentCell)
        {
            cells[currentCell].Remove(i);
            AddToCells(newCell, i);
            boids[i] = newCell;
        }
    }

    public void Populate(int i, Vector3 pos)
    {
        int cell = PositionToCell3D(pos, cellSize, gridSize);
        AddToCells(cell, i);
        boids.Add(i, cell);
    }

    public void AddToCells(int cell, int i)
    {
        if (cells.ContainsKey(cell))
        {
            cells[cell].Add(i);
        }
        else
        {
            cells.Add(cell, new List<int>() { i });
        }
    }

    public void Clear()
    {
        cells.Clear();
        boids.Clear();
    }

    public List<int> GetNeighbours(int i)
    {
        return cells[boids[i]];
    }
}