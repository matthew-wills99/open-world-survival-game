using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using System.IO;
using System;
using ExtrasClipperLib;

public class BetterWaveFunction : MonoBehaviour
{
    public StructureGenerator structureGenerator;
    public MapController mapController;
    public TileIndex tileIndex;
    System.Random random;
    int offset;

    List<string> debugLines;
    string fileName = "Debug Log.txt";

    int cx;
    int cy;

    Cell[,] grid;
    int gridSize;

    public void Awake()
    {
        debugLines = new List<string>();
        random = new System.Random(mapController.GetWorldSeed());
        offset = mapController.GetStructureOffset();
    }

    // generate a structure in a size*size area at the centre of chunk (chunkX, chunkY)
    public void GenerateStructure(int chunkX, int chunkY, int size)
    {
        cx = chunkX;
        cy = chunkY;
        gridSize = size;
        grid = new Cell[gridSize, gridSize];

        debugLines.Add($"Generating structure in chunk: ({chunkX}, {chunkY}) of size: {size}");

        InitializeGrid();
        WFC();

        var sr = File.CreateText(fileName);
        foreach(var line in debugLines)
        {
            sr.WriteLine(line);
        }
        sr.Close();
    }

    void InitializeGrid()
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
        // every structure in the middle will be a 16 tile
        Cell startCell = grid[gridSize / 2, gridSize / 2];
        startCell.PossibleStates = new List<int>{16};
        int sx = cx - gridSize + startCell.X;
        int sy = cy - gridSize + startCell.Y;

        mapController.tempTilemap.SetTile(new Vector3Int(sx, sy, 0), tileIndex.GetTileIndex()[2/*startCell.PossibleStates.First() + 4*/]);
        startCell.Collapsed = true;
        PropagateConstaints(startCell.X, startCell.Y);

        debugLines.Add("Initialized grid");

        for(int y = gridSize; y > 0; y--)
        {
            string line = "";
            for(int x = 0; x < gridSize; x++)
            {
                //line += $"{grid[x, y - 1].PossibleStates.Count.ToString().PadLeft(2, ' ')} ";
                line += $"{grid[x, y - 1].PossibleStates.Count.ToString().PadLeft(2, ' ')}{(grid[x, y - 1].Collapsed ? 't' : 'f')} ";
            }
            debugLines.Add(line);
        }

        debugLines.Add("Done");
    }

    void WFC()
    {
        int i = 0;
        debugLines.Add("\n**********\nBEGIN WFC:\n**********\n");
        while(true)
        {
            Cell minEntropyCell = GetMinEntropyCell();

            if(minEntropyCell == null || minEntropyCell.PossibleStates.Count == 0)
            {
                break;
            }

            //Debug.Log($"Min Entropy Cell: ({minEntropyCell.X}, {minEntropyCell.Y})");

            int state = minEntropyCell.PossibleStates[random.Next(minEntropyCell.PossibleStates.Count)]; // pick a random new possible neighbour
            switch(state)
            {
                case 0:
                    state = random.Next(0, 3 + 1);
                    break;
                case 4:
                    state = random.Next(4, 5 + 1);
                    break;
                case 6:
                    state = random.Next(6, 7 + 1);
                    break;
                case 8:
                    state = random.Next(8, 9 + 1);
                    break;
                case 10:
                    state = random.Next(10, 11 + 1);
                    break;
                case 21:
                    state = random.Next(21, 24 + 1);
                    break;
            }

            grid[minEntropyCell.X, minEntropyCell.Y].PossibleStates = new List<int>{state};

            //Debug.Log($"Cell new possible cell count {minEntropyCell.PossibleStates.Count}");

            int x = cx - gridSize + minEntropyCell.X;
            int y = cy - gridSize + minEntropyCell.Y;

            mapController.tempTilemap.SetTile(new Vector3Int(x, y, 0), tileIndex.GetTileIndex()[state + 4]);

            grid[minEntropyCell.X, minEntropyCell.Y].Collapsed = true;
            //Debug.Log($"min cell at ({minEntropyCell.X}, {minEntropyCell.Y}) with count {minEntropyCell.PossibleStates.Count}");

            //Debug.Log($"Min entropy cell found @ ({minEntropyCell.X}, {minEntropyCell.Y}), selected id: {state}");

            PropagateConstaints(minEntropyCell.X, minEntropyCell.Y);

            debugLines.Add($"\nITERATION: {i}");
            for(int gy = gridSize; gy > 0; gy--)
            {
                string line = "";
                for(int gx = 0; gx < gridSize; gx++)
                {
                    //line += $"{grid[x, y - 1].PossibleStates.Count.ToString().PadLeft(2, ' ')} ";
                    line += $"{grid[gx, gy - 1].PossibleStates.Count,2}{(grid[gx, gy - 1].Collapsed ? 'c' : ' ')} ";
                    //Debug.Log($"iteration at ({gx}, {gy - 1}) with count {grid[gx, gy - 1].PossibleStates.Count}");
                }
            debugLines.Add(line);
            }
            debugLines.Add("\n");
            i++;
            //yield return new WaitForSeconds(1f);
        }
        debugLines.Add("\n**********\nEND WFC:\n**********\n");
    }

    int GetDistanceFromOrigin(int x, int y)
    {
        int ox = gridSize / 2;
        int oy = gridSize / 2;
        Debug.Log($"Distance between ({ox}, {oy}) and ({x}, {y}) is {(float)Math.Abs(Math.Sqrt((x - ox)*(x - ox)+ (y - oy)*(y - oy)))}");
        return (int)Math.Abs(Math.Sqrt((x - ox)*(x - ox) + (y - oy)*(y - oy)));
    }

    void PropagateConstaints(int x, int y)
    {

        // get the state of the cell at x, y
        // get neighbours that state in every direction
        //
        //Debug.Log($"Available states: {grid[x, y].PossibleStates.Count}");
        int chosenState = grid[x, y].PossibleStates.First();

        // what the fuck is this
        var neighbours = structureGenerator.GetNeighbours();
        var n = neighbours[chosenState].Value;

        foreach(var i in n)
        {
            int nx = x, ny = y;

            switch(i.Key)
            {
                case StructureGenerator.Direction.Up:
                    ny += 1;
                    //Debug.Log("Up");
                    break;
                case StructureGenerator.Direction.Right:
                    nx += 1;
                    //Debug.Log("Right");
                    break;
                case StructureGenerator.Direction.Down:
                    ny -= 1;
                    //Debug.Log("Down");
                    break;
                case StructureGenerator.Direction.Left:
                    nx -= 1;
                    //Debug.Log("Left");
                    break;
            }

            //i is a List<Direction, List<int>> Direction to List<int>

            if(nx >= 0 && nx < gridSize && ny >= 0 && ny < gridSize && grid[nx, ny] != null && !grid[nx, ny].Collapsed)
            {
                grid[nx, ny].PossibleStates.RemoveAll(item => !i.Value.Contains(item));
                if(grid[nx, ny].PossibleStates.Count == 1)
                {
                    grid[nx, ny].Collapsed = true;
                    int state = grid[nx, ny].PossibleStates.First();
                    switch(grid[nx, ny].PossibleStates.First())
                    {
                        case 0:
                            state = random.Next(0, 3 + 1);
                            break;
                        case 4:
                            state = random.Next(4, 5 + 1);
                            break;
                        case 6:
                            state = random.Next(6, 7 + 1);
                            break;
                        case 8:
                            state = random.Next(8, 9 + 1);
                            break;
                        case 10:
                            state = random.Next(10, 11 + 1);
                            break;
                        case 21:
                            state = random.Next(21, 24 + 1);
                            break;
                    }
                    mapController.tempTilemap.SetTile(new Vector3Int(cx - gridSize + nx, cx - gridSize + ny, 0), tileIndex.GetTileIndex()[state + 4]);
                    PropagateConstaints(nx, ny);
                }
            }
        }
    }

    Cell GetMinEntropyCell()
    {
        int minEntropy = int.MaxValue;
        float minDistance = int.MaxValue;
        Cell minEntropyCell = null;
        List<Cell> minEntropyCells = new List<Cell>();

        //debugLines.Add($"\nFinding min entropy:");

        // find minimum entropy
        for(int x = 0; x < gridSize; x++)
        {
            for(int y = 0; y < gridSize; y++)
            {
                if(grid[x, y].PossibleStates.Count > 1 && grid[x, y].PossibleStates.Count < minEntropy && !grid[x, y].Collapsed)
                {
                    //debugLines.Add($"Found new min entropy: ({x},{y}) with new entropy of {grid[x, y].PossibleStates.Count}");
                    minEntropy = grid[x, y].PossibleStates.Count;
                    //minEntropyCell = grid[x, y];
                }
            }
        }

        string line = "Min entropy cells: ";

        // make list of all cells that have the min entropy
        for(int x = 0; x < gridSize; x++)
        {
            for(int y = 0; y < gridSize; y++)
            {
                if(grid[x, y].PossibleStates.Count == minEntropy && !grid[x, y].Collapsed)
                {
                    minEntropyCells.Add(grid[x, y]);
                    line += $"({x}, {y}): {grid[x, y].PossibleStates.Count} ";
                }
            }
        }

        debugLines.Add(line);

        // find the cell closest to the origin
        foreach(Cell cell in minEntropyCells)
        {
            if(GetDistanceFromOrigin(cell.X, cell.Y) < minDistance)
            {
                minDistance = GetDistanceFromOrigin(cell.X, cell.Y);
                minEntropyCell = cell;
            }
        }

        // find the cell closest 

        /*
        if(minEntropyCell != null)
        {
            debugLines.Add($"\nFinal min entropy: ({minEntropyCell.X}, {minEntropyCell.Y}) with entropy of {minEntropyCell.PossibleStates.Count}\n");

        }
        */
 
        if(minEntropyCell != null)
        {
            //Debug.Log($"Closest cell to origin: ({minEntropyCell.X}, {minEntropyCell.Y})");
        }
        
        return minEntropyCell;
    }

    class Cell  
    {   
        public List<int> PossibleStates {get; set;}
        public int X {get; set;}
        public int Y {get; set;}
        public bool Collapsed {get; set;} = false;

    }
}

/*

when selecting a tile, grab all collapsed neighbours and pick from that list

*/

//MAKE A LIST OF ALL LOWEST ENTROPIES AND COLLAPSE THE ONE CLOSEST TO THE ORIGIN