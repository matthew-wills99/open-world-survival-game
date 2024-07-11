using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WaveFunction : MonoBehaviour
{
    public MapController mapController;
    public OldTileIndex tileIndex;
    public StructureGenerator structureGenerator;
    System.Random random;
    int offset = 4;

    Cell[,] grid;
    int gridSize;

    public void Awake()
    {
        random = new System.Random(mapController.GetWorldSeed());
    }

    // generate a structure at the given coordinates, and up to the given radius away
    public void GenerateStructure(MapController.Coords coords, int radius)
    {
        int x = coords.xCoord;
        int y = coords.yCoord;
        gridSize = radius * 2 + 1;
        grid = new Cell[gridSize, gridSize];
        //Debug.Log($"grid {grid}");

        InitializeGrid(radius);
        WFC();
        PlaceTiles(x, y, radius);
    }

    void InitializeGrid(int radius)
    {
        for(int x = 0; x < gridSize; x++)
        {
            for(int y = 0; y < gridSize; y++)
            {
                Cell temp = new Cell();
                temp.X = x;
                temp.Y = y;
                temp.PossibleStates = new List<int>(Enumerable.Range(0, 25));
                grid[x, y] = temp;
            }
        }

        Cell t = new Cell();
        t.X = gridSize / 2;
        t.Y = gridSize / 2;
        t.PossibleStates = new List<int>{16};
        grid[gridSize / 2, gridSize / 2] = t;
        //Debug.Log($"PLACING CENTER HERE: {gridSize / 2}, {gridSize / 2}");
        PropagateConstaints(t.X, t.Y);
    }

    void WFC()
    {
        while(true)
        {
            var minEntropyCell = GetMinEntropyCell();
            if(minEntropyCell == null || minEntropyCell.PossibleStates.Count == 0)
            {
                break; // :(
            }

            int state = minEntropyCell.PossibleStates[random.Next(minEntropyCell.PossibleStates.Count)]; // pick a random new possible neighbour
            minEntropyCell.PossibleStates = new List<int> { state };

            PropagateConstaints(minEntropyCell.X, minEntropyCell.Y);
        }
    }

    Cell GetMinEntropyCell()
    {
        Cell minEntropyCell = null;
        int minEntropy = int.MaxValue;

        foreach(Cell cell in grid)
        {
            if(cell.PossibleStates.Count > 1 && cell.PossibleStates.Count < minEntropy)
            {
                minEntropy = cell.PossibleStates.Count;
                minEntropyCell = cell;
            }
        }

        return minEntropyCell;
    }

    // mayhaps we redo this
    void PropagateConstaints(int x, int y)
    {
        int chosenState = grid[x, y].PossibleStates.First();

        var n = structureGenerator.GetNeighbours().First(t => t.Key == chosenState).Value;

        foreach(var i in n)
        {
            int nx = x, ny = y;

            switch(i.Key)
            {
                case StructureGenerator.Direction.Up:
                    ny += 1;
                    break;
                case StructureGenerator.Direction.Right:
                    nx += 1;
                    break;
                case StructureGenerator.Direction.Down:
                    ny -= 1;
                    break;
                case StructureGenerator.Direction.Left:
                    nx -= 1;
                    break;
            }

            if(nx >= 0 && nx < gridSize && ny >= 0 && ny < gridSize)
            {
                grid[nx, ny]?.PossibleStates.RemoveAll(state => !i.Value.Contains(state));
            }
        }
    }

    void PlaceTiles(int X, int Y, int radius)
    {
        Debug.Log("PLACING");
        for(int x = 0; x < gridSize; x++)
        {
            for(int y = 0; y < gridSize; y++)
            {
                if(grid[x, y].PossibleStates.Count > 0)
                {
                    var test = grid[x, y];
                    int tileID = grid[x, y].PossibleStates.First() + offset; // important add offset

                    mapController.tempTilemap.SetTile(new Vector3Int(X + x, Y + y, 0), tileIndex.GetTileIndex()[tileID]);

                    //Vector3Int coords = new Vector3Int(X + x - radius, Y + y - radius, 0);
                    //mapController.PlaceTile(coords, tileID);
                }
            }
        }
    }

    class Cell
    {
        public List<int> PossibleStates {get; set;}
        public int SelectedState {get; set;} = -1;
        public int X {get; set;}
        public int Y {get; set;}

    }
}
