using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class EvenBetterWaveFunction : MonoBehaviour
{
    public StructureGenerator structureGenerator;
    public MapController mapController;
    public OldTileIndex tileIndex;
    int offset;

    System.Random random;

    //List<string> debugLines;
    //string fileName = "Debug Log.txt";

    // origin of structure
    int ox;
    int oy;

    Cell[,] grid;
    int gridSize;

    void Awake()
    {
        //debugLines = new List<string>();
        random = new System.Random(mapController.GetWorldSeed());
        offset = mapController.GetStructureOffset();
    }

    public void Generate(int x, int y, int size)
    {
        ox = x;
        oy = y;
        gridSize = size;

        InitializeGrid();
        GenerateBuilding();

        /*var sr = File.CreateText(fileName);
        foreach(var line in debugLines)
        {
            sr.WriteLine(line);
        }
        sr.Close();*/
    }

    void InitializeGrid()
    {
        grid = new Cell[gridSize, gridSize];

        for(int x = 0; x < gridSize; x++)
        {
            for(int y = 0; y < gridSize; y++)
            {
                // initialize the grid such that it can only be structure tiles
                grid[x, y] = new Cell {
                    X = x,
                    Y = y,
                    PossibleStates = new List<int> (Enumerable.Range(0, tileIndex.GetStructCount()))
                };
            }
        }

        /*****************************************
         all tiles have 0 - 12 as selections
        *****************************************/

        // set the tile at the centre to be the 4 tile (16 (20))
        grid[gridSize / 2, gridSize / 2].PossibleStates = new List<int> {4};
        grid[gridSize / 2, gridSize / 2].Collapsed = true;

        int sx = ox - gridSize + grid[gridSize / 2, gridSize / 2].X;
        int sy = oy - gridSize + grid[gridSize / 2, gridSize / 2].Y;

        mapController.tempTilemap.SetTile(new Vector3Int(sx, sy, 0), tileIndex.GetTileIndex()[2/*startCell.PossibleStates.First() + 4*/]);
        PropagateConstaints(gridSize / 2, gridSize / 2);

        /*debugLines.Add("Initialized grid");

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

        debugLines.Add("Done");*/
    }

    // generate the building part of the structure (1st)
    void GenerateBuilding()
    {
        //int i = 0;
        //debugLines.Add("\n**********\nBEGIN BUILDING:\n**********\n");
        while(true)
        {
            Cell minEntropyCell = GetMinEntropyCell();

            if(minEntropyCell == null || minEntropyCell.PossibleStates.Count == 0)
            {
                break;
            }

            int state = minEntropyCell.PossibleStates[random.Next(minEntropyCell.PossibleStates.Count)];
            grid[minEntropyCell.X, minEntropyCell.Y].PossibleStates = new List<int>{state};
            grid[minEntropyCell.X, minEntropyCell.Y].Collapsed = true;

            int x = ox - gridSize + minEntropyCell.X;
            int y = oy - gridSize + minEntropyCell.Y;

            mapController.tempTilemap.SetTile(new Vector3Int(x, y, 0), tileIndex.GetTileIndex()[tileIndex.ConvertStructToIndex(state) + offset]);

            PropagateConstaints(minEntropyCell.X, minEntropyCell.Y);

            /*debugLines.Add($"\nITERATION: {i}");
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
            i++;*/
        }
        //debugLines.Add("\n**********\nEND BUILDING:\n**********\n");
    }

    // generate the path part of the structure (2nd)
    void GeneratePath()
    {

    }

    // generate the grass part of the structure (3rd)
    void GenerateGrass()
    {

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
                List<int> temp = new List<int>();
                foreach(int num in i.Value)
                {
                    if(tileIndex.ConvertStructToIndex(num) != -1)
                    {
                        temp.Add(tileIndex.ConvertStructToIndex(num));
                    }
                }
                grid[nx, ny].PossibleStates.RemoveAll(item => !temp.Contains(item));

                if(grid[nx, ny].PossibleStates.Count == 1)
                {
                    grid[nx, ny].Collapsed = true;
                    int state = grid[nx, ny].PossibleStates.First();
                    mapController.tempTilemap.SetTile(new Vector3Int(ox - gridSize + nx, ox - gridSize + ny, 0), tileIndex.GetTileIndex()[tileIndex.ConvertStructToIndex(state) + offset]);
                    PropagateConstaints(nx, ny);
                }
            }
        }
    }

    Cell GetMinEntropyCell()
    {
        int minEntropy = int.MaxValue;
        Cell minEntropyCell = null;

        // find minimum entropy
        for(int x = 0; x < gridSize; x++)
        {
            for(int y = 0; y < gridSize; y++)
            {
                if(grid[x, y].PossibleStates.Count > 1 && grid[x, y].PossibleStates.Count < minEntropy && !grid[x, y].Collapsed)
                {
                    //debugLines.Add($"Found new min entropy: ({x},{y}) with new entropy of {grid[x, y].PossibleStates.Count}");
                    minEntropy = grid[x, y].PossibleStates.Count;
                    minEntropyCell = grid[x, y];
                }
            }
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
