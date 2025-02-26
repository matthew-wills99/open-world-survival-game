using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static Utils;

/*
Work very similar to biome generator
- create a dictionary exactly like biomeMap and the blocks will be either 'River' or 'NotRiver'
- fill the riverMap with 'NotRiver'
- use a noise function, generate a noise map and set an 'Obstacle' threshold to determine which blocks would be considered a closed node to an A* algorithm
- pick 'n' pairs of start and end points, each end point must be placed a certain distance '(riverAverageLength) * random change' from it's start point, and 
all points must be 'minimum distance' away from all other start and end points, no points may be placed in centreChunks, all end points must not be 'Obstacles' (obviously)
- run a* algorithm through the obstacle map from each start point to their end points
- set those points and their paths to 'River' in the riverMap based on a width 'riverWidth', this could be random too with '(riverAverageWidth) + random change'
- mapGenerator will be changed to set all river tiles to water in the CellularAutomata() method 

We should use the same noise map for all generation that uses one
*/

[CreateAssetMenu(fileName = "RiverGenerator", menuName = "River Generator")]
public class RiverGenerator : ScriptableObject
{
    // TODO: implement the random change to river average length?
    public int numberOfRivers;
    public int riverAverageLength;
    public int pointMinDistance;
    public int riverWidth;
    [Range(0, 1)] public float obstacleThreshold;

    private int chunkSize;
    Dictionary<long, ERiver[,]> riverMap;
    Dictionary<long, EObstacle[,]> obstacleMap;
    Dictionary<long, float[,]> noiseMap;
    List<(Vector2Int start, Vector2Int end)> pointPairs;
    List<Vector2Int> availablePoints;

    public (Dictionary<long, ERiver[,]> riverMap, Dictionary<long, EObstacle[,]> obstacleMap) GenerateRiverMap(System.Random random)
    {
        chunkSize = GameManager.Instance.chunkSize;

        riverMap = new Dictionary<long, ERiver[,]>();
        obstacleMap = new Dictionary<long, EObstacle[,]>();
        noiseMap = NoiseMap.Generate(random); // noise map to determine what is an obstacle or not
        pointPairs = new List<(Vector2Int, Vector2Int)>();
        availablePoints = new List<Vector2Int>();

        InitializeMaps();
        GenerateObstacleMap();
        InitializeAvailableChunks();

        for(int n = 0; n < numberOfRivers; n++)
        {
            CreatePointPair(random);
        }

        foreach (var pair in pointPairs)
        {
            Debug.Log($"Start: {pair.start}, End: {pair.end}");
        }

        RunAStarForRivers();

        // Run Astar algo from each start to their end
        
        return (riverMap, obstacleMap);
    }

    private void InitializeAvailableChunks()
    {
        List<Vector2Int> centreChunks = new List<Vector2Int>{
            new Vector2Int(-1, -1),
            new Vector2Int(-1, 0),
            new Vector2Int(0, -1),
            new Vector2Int(0, 0)
        };

        int halfMap = GameManager.Instance.mapGenerator.halfMap;

        for(int cx = -halfMap; cx < halfMap; cx++)
        {
            for(int cy = -halfMap; cy < halfMap; cy++)
            {
                if(!centreChunks.Contains(new Vector2Int(cx, cy)))
                {
                    for(int tx = 0; tx < chunkSize; tx++)
                    {
                        for(int ty = 0; ty < chunkSize; ty++)
                        {
                            if(obstacleMap.ContainsKey(GetChunkKey(cx, cy)) 
                            && obstacleMap[GetChunkKey(cx, cy)][tx, ty] != EObstacle.Obstacle)
                            {
                                // initialize to all tiles except for those in centre chunks and obstacles
                                availablePoints.Add(ChunkToWorldPos(cx, cy, tx, ty, chunkSize));
                            }
                        }
                    }
                }
            }
        }
    }

    private void InitializeMaps()
    {
        int halfMap = GameManager.Instance.mapGenerator.halfMap;

        for(int cx = -halfMap; cx < halfMap; cx++)
        {
            for(int cy = -halfMap; cy < halfMap; cy++)
            {
                riverMap.Add(GetChunkKey(cx, cy), InitializeRiverArray(chunkSize));
                obstacleMap.Add(GetChunkKey(cx, cy), InitializeObstacleArray(chunkSize));
            }
        }   
    }

    private ERiver[,] InitializeRiverArray(int chunkSize)
    {
        ERiver[,] riverArray = new ERiver[chunkSize, chunkSize];
        for(int x = 0; x < chunkSize; x++)
        {
            for(int y = 0; y < chunkSize; y++)
            {
                riverArray[x, y] = ERiver.NotRiver;
            }
        }
        return riverArray;
    }

    private EObstacle[,] InitializeObstacleArray(int chunkSize)
    {
        EObstacle[,] obstacleArray = new EObstacle[chunkSize, chunkSize];
        for(int x = 0; x < chunkSize; x++)
        {
            for(int y = 0; y < chunkSize; y++)
            {
                obstacleArray[x, y] = EObstacle.NotObstacle;
            }
        }
        return obstacleArray;
    }

    private void GenerateObstacleMap()
    {
        int halfMap = GameManager.Instance.mapGenerator.halfMap;

        for(int cx = -halfMap; cx < halfMap; cx++)
        {
            for(int cy = -halfMap; cy < halfMap; cy++)
            {
                for(int tx = 0; tx < chunkSize; tx++)
                {
                    for(int ty = 0; ty < chunkSize; ty++)
                    {
                        if(noiseMap[GetChunkKey(cx, cy)][tx, ty] < obstacleThreshold)
                        {
                            obstacleMap[GetChunkKey(cx, cy)][tx, ty] = EObstacle.Obstacle;
                        }
                    }
                }
            }
        }
    }

    private bool CreatePointPair(System.Random random)
    {
        // check available points (they will be blocks not chunks)
        // place point randomly out of available points
        // remove from available points all points within minimum distance of point placed
        // copy over to temporary list, remove all points within minimum end point distance of start point
        Vector2Int startPoint;
        Vector2Int endPoint;

        List<Vector2Int> availableEndPoints = new List<Vector2Int>();

        if(availablePoints.Count == 0)
        {
            return false;
        }

        // random start point
        startPoint = availablePoints[random.Next(availablePoints.Count)];
        availablePoints.Remove(startPoint);
        RemovePointsInRange(startPoint, 16); // remove surrounding 2 chunks worth of blocks

        if(availablePoints.Count == 0)
        {
            return false;
        }

        // availableEndPoints will be init here
        SetupEndPointsInRange(availablePoints, availableEndPoints, startPoint, riverAverageLength + random.Next(0, 3));
        if(availableEndPoints.Count == 0)
        {
            return false;
        }


        endPoint = availableEndPoints[random.Next(availableEndPoints.Count)];
        RemovePointsInRange(endPoint, 16);

        pointPairs.Add((startPoint, endPoint));

        return true;
    }

    private void SetupEndPointsInRange(List<Vector2Int> availablePoints, List<Vector2Int> availableEndPoints, Vector2Int startPoint, int range)
    {
        foreach(Vector2Int p in availablePoints)
        {
            if(Vector2Int.Distance(startPoint, p) < range) continue;

            availableEndPoints.Add(p);
        }
    }

    private void RemovePointsInRange(Vector2Int point, int range)
    {
        availablePoints.RemoveAll(p => Vector2Int.Distance(point, p) < range);
    }

    private void RunAStarForRivers()
    {
        foreach (var pair in pointPairs)
        {
            var path = AStar(pair.start, pair.end);
            if (path != null)
            {
                var controlPoints = GetControlPoints(path, 20);
                var bezierPath = GenerateBezierCurve(controlPoints, 10);
                
                // Fill in the river tiles
                for (int i = 0; i < bezierPath.Count - 1; i++)
                {
                    Vector2Int start = bezierPath[i];
                    Vector2Int end = bezierPath[i + 1];

                    // Interpolate between points to ensure no gaps
                    foreach (var point in InterpolateLine(start, end))
                    {
                        FillRiverTiles(point, 2); // Adjust radius as needed
                    }
                }
            }
        }
    }

    private List<Vector2Int> AStar(Vector2Int start, Vector2Int end)
    {
        HashSet<Vector2Int> closedSet = new HashSet<Vector2Int>();
        PriorityQueue<(Vector2Int point, float cost), float> openSet = new PriorityQueue<(Vector2Int point, float cost), float>();
        HashSet<Vector2Int> openSetTracker = new HashSet<Vector2Int>();
        Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        Dictionary<Vector2Int, float> gScore = new Dictionary<Vector2Int, float> { [start] = 0 };
        Dictionary<Vector2Int, float> fScore = new Dictionary<Vector2Int, float>();

        if (!fScore.ContainsKey(start))
        {
            fScore[start] = Heuristic(start, end);
        }

        openSet.Enqueue((start, fScore[start]), fScore[start]);
        openSetTracker.Add(start);

        while (openSet.Count > 0)
        {
            Vector2Int current = openSet.Dequeue().point;
            openSetTracker.Remove(current);

            if (current == end)
            {
                return ReconstructPath(cameFrom, current);
            }

            closedSet.Add(current);

            foreach (var neighbor in GetNeighbors(current))
            {
                if (closedSet.Contains(neighbor))
                    continue;

                (int cx, int cy, int tx, int ty) = WorldToChunkPos(neighbor.x, neighbor.y, chunkSize);

                if (obstacleMap.ContainsKey(GetChunkKey(cx, cy)) && obstacleMap[GetChunkKey(cx, cy)][tx, ty] == EObstacle.Obstacle)
                    continue;

                float tentativeGScore = gScore[current] + Vector2Int.Distance(current, neighbor);

                if (!gScore.ContainsKey(neighbor) || tentativeGScore < gScore[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeGScore;
                    fScore[neighbor] = tentativeGScore + Heuristic(neighbor, end);

                    if (!openSetTracker.Contains(neighbor))
                    {
                        openSet.Enqueue((neighbor, fScore[neighbor]), fScore[neighbor]);
                        openSetTracker.Add(neighbor);
                    }
                }
            }
        }

        return null; // No path found
    }

    private float Heuristic(Vector2Int a, Vector2Int b)
    {
        return Vector2Int.Distance(a, b);
    }

    private List<Vector2Int> GetNeighbors(Vector2Int point)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>
        {
            new Vector2Int(point.x + 1, point.y),
            new Vector2Int(point.x - 1, point.y),
            new Vector2Int(point.x, point.y + 1),
            new Vector2Int(point.x, point.y - 1)
        };

        return neighbors;
    }

    private List<Vector2Int> ReconstructPath(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int current)
    {
        List<Vector2Int> path = new List<Vector2Int> { current };
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            path.Add(current);
        }
        path.Reverse();
        return path;
    }

    private List<Vector2Int> GenerateBezierCurve(List<Vector2Int> controlPoints, int resolution = 10)
    {
        List<Vector2Int> bezierPoints = new List<Vector2Int>();
        for (int i = 0; i < controlPoints.Count - 3; i += 3)
        {
            Vector2Int p0 = controlPoints[i];
            Vector2Int p1 = controlPoints[i + 1];
            Vector2Int p2 = controlPoints[i + 2];
            Vector2Int p3 = controlPoints[i + 3];

            for (int j = 0; j <= resolution; j++)
            {
                float t = j / (float)resolution;
                float oneMinusT = 1 - t;

                int x = Mathf.RoundToInt(
                    oneMinusT * oneMinusT * oneMinusT * p0.x +
                    3 * oneMinusT * oneMinusT * t * p1.x +
                    3 * oneMinusT * t * t * p2.x +
                    t * t * t * p3.x
                );

                int y = Mathf.RoundToInt(
                    oneMinusT * oneMinusT * oneMinusT * p0.y +
                    3 * oneMinusT * oneMinusT * t * p1.y +
                    3 * oneMinusT * t * t * p2.y +
                    t * t * t * p3.y
                );

                bezierPoints.Add(new Vector2Int(x, y));
            }
        }

        return bezierPoints;
    }

    private List<Vector2Int> GetControlPoints(List<Vector2Int> path, int step = 20)
    {
        List<Vector2Int> controlPoints = new List<Vector2Int>();
        for (int i = 0; i < path.Count; i += step)
        {
            controlPoints.Add(path[i]);
        }

        // Ensure the last point is included
        if (!controlPoints.Contains(path[^1]))
        {
            controlPoints.Add(path[^1]);
        }

        return controlPoints;
    }

    // Helper function to interpolate between two points
    private List<Vector2Int> InterpolateLine(Vector2Int start, Vector2Int end)
    {
        List<Vector2Int> interpolatedPoints = new List<Vector2Int>();

        int dx = Mathf.Abs(end.x - start.x);
        int dy = Mathf.Abs(end.y - start.y);
        int steps = Mathf.Max(dx, dy);

        for (int i = 0; i <= steps; i++)
        {
            float t = i / (float)steps;
            int x = Mathf.RoundToInt(Mathf.Lerp(start.x, end.x, t));
            int y = Mathf.RoundToInt(Mathf.Lerp(start.y, end.y, t));
            interpolatedPoints.Add(new Vector2Int(x, y));
        }

        return interpolatedPoints;
    }

    // Helper function to fill tiles around a point
    private void FillRiverTiles(Vector2Int point, int radius)
    {
        for (int dx = -radius; dx <= radius; dx++)
        {
            for (int dy = -radius; dy <= radius; dy++)
            {
                Vector2Int neighbour = new Vector2Int(point.x + dx, point.y + dy);
                (int ncx, int ncy, int ntx, int nty) = WorldToChunkPos(neighbour.x, neighbour.y, chunkSize);

                if (riverMap.ContainsKey(GetChunkKey(ncx, ncy)))
                {
                    riverMap[GetChunkKey(ncx, ncy)][ntx, nty] = ERiver.River;
                }
            }
        }
    }

}