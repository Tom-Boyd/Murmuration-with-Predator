using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Grid : MonoBehaviour
{

    public Dictionary<int, List<int>> cells = new Dictionary<int, List<int>>();
    public Dictionary<int, List<int>> getCells { get { return cells; } }
    public Dictionary<int, int> boids = new Dictionary<int, int>();
    public Dictionary<int, int> getBoids { get { return boids; } }
    public int cellSize = 50;
    public int gridSize = 10000;

    public static int PositionToCell3D(Vector3 pos, int cellSize, int gridSize)
    {
        return ((int)(pos.x / cellSize))
            + ((int)(pos.z / cellSize)) * gridSize
            + ((int)(pos.y / cellSize)) * gridSize * gridSize;
    }

    public static List<int> getSurroundingCells(int cell, int cellSize, int gridSize)
    {
        List<int> neighbourCells = new List<int>() { cell };
        int row = cell / gridSize;
        int col = cell - (row * gridSize);

        neighbourCells.Add(gridSize * (row + 1));
        neighbourCells.Add(gridSize * (row - 1));
        neighbourCells.Add(col + 1 + (row * gridSize));
        neighbourCells.Add(col - 1 + (row * gridSize));
        neighbourCells.Add(col + 1 + ((row + 1) * gridSize));
        neighbourCells.Add(col - 1 + ((row - 1) * gridSize));
        neighbourCells.Add(col + 1 + ((row - 1) * gridSize));
        neighbourCells.Add(col - 1 + ((row + 1) * gridSize));

        return neighbourCells;
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
        List<int> neighbours = new List<int>();
        int thisCell = boids[i];
        List<int> neighbourCells = getSurroundingCells(thisCell, cellSize, gridSize);
        foreach (int cell in neighbourCells) {
            if (cells.ContainsKey(cell)) {
                neighbours.AddRange(cells[cell]);
            }
        }

        return neighbours;
    }
}