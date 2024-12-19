using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

using static EUtils;

public class RegenRocks : MonoBehaviour
{
    public MapManager mapManager;
    public int rockClusterCap = 100;
    public Transform player;

    // ------------- time -------------------
    // 2 minutes between destroying the cluster and adding to rockClustersToPlace
    public float clusterDestroyDelay = 120f; // in seconds
    // regenerate clusters every 15 minutes
    public float regenClustersDelay = 900f;
    private float timeSinceClusterRegen = 0f;
    // ------------- time -------------------

    // ----------- distance -----------------
    public int distanceBetweenClusterAndPlayer = 20; // 1 unit is 1 tile
    public int distanceBetweenOtherClusters = 15;
    public int distanceFromPlayerPlacedTiles = 4;
    // ----------- distance -----------------

    // this variable needs to be saved and loaded with the world
    private int rockClustersToPlace = 0;

    private Queue<float> timeOfDestruction = new Queue<float>();

    public void RockClusterDestroyed()
    {
        timeOfDestruction.Enqueue(Time.time); // current time
        //Debug.Log($"enqueued a rock cluster: {timeOfDestruction.Count}");
    }

    void CheckClusterRegenTime()
    {
        if(timeOfDestruction.Count > 0)
        {
            //Debug.Log($"There are {timeOfDestruction.Count} clusters in queue");
            if(Time.time - timeOfDestruction.Peek() > clusterDestroyDelay)
            {
                rockClustersToPlace++;
                timeOfDestruction.Dequeue();
                //Debug.Log($"Rock cluster ready for placement: {rockClustersToPlace}");
            }
        }  
    }
    void Update()
    {
        CheckClusterRegenTime();
        if(Time.time - timeSinceClusterRegen > regenClustersDelay)
        {
            TryRegenRocks();
            timeSinceClusterRegen = Time.time;
        }
    }


    /* 
    -create a rock cluster cap for the world
    -create a list of times for when a rock cluster is destroyed
    -create an int to count how many rock clusters have been destroyed

    -every time a rock cluster is completely destroyed, 
        add the current time to the list
    
    -whenever one of the times in the list become more than x minutes old
        add 1 to the int
    
    -every x minutes, attempt to spawn as many rock clusters as possible until there are no rock clusters left to spawn
        or there are no suitable locations for a rock cluster to spawn
    */

    void TryRegenRocks()
    {
        List<(int x, int y)> currentCoords = new List<(int x, int y)>();
        int timeout = Math.Max(20, rockClustersToPlace * 4);
        int counter = 0;
        (int cx, int cy, int tx, int ty) c;

        //Debug.Log($"Rock Clusters To Place: {rockClustersToPlace}");

        while(rockClustersToPlace > 0 && counter < timeout)
        {
            c = mapManager.GetRandomPoint();
            Vector3Int worldPos = ChunkToWorldPos(c.cx, c.cy, c.tx, c.ty, mapManager.chunkSize);

            //Debug.Log($"{c.cx} {c.cy} {c.tx} {c.ty}");
            //Debug.Log($"{mapManager.TileHasObject(c.cx, c.cy, c.tx, c.ty)}, {mapManager.TileHasWater(c.cx, c.cy, c.tx, c.ty)}, {GetDistance(worldPos.x, worldPos.y, (int)player.position.x, (int)player.position.y) > distanceBetweenClusterAndPlayer}");

            // no trees, rocks, cactus
            // no water
            // far from player
            // far from other clusters
            // far from player placed tiles
            if(!mapManager.TileHasObject(c.cx, c.cy, c.tx, c.ty) &&
                !mapManager.TileHasWater(c.cx, c.cy, c.tx, c.ty) &&
                GetDistance(worldPos.x, worldPos.y, (int)player.position.x, (int)player.position.y) > distanceBetweenClusterAndPlayer)
            {
                if(FarFromOtherClusters(worldPos.x, worldPos.y, currentCoords))
                {
                    if(FarFromPlayerPlacedTiles(worldPos.x, worldPos.y))
                    {
                        mapManager.PlaceRockCluster(c.cx, c.cy, c.tx, c.ty);
                        //Debug.Log($"Placed a new cluster C:({c.cx}, {c.cy}) T:({c.tx}, {c.ty})");
                        rockClustersToPlace--;
                    }
                }
            }
            counter++;
        }

        if(rockClustersToPlace <= 0)
        {
            //Debug.Log($"Placed all rock clusters : {rockClustersToPlace}");
        }
        if(counter >= timeout)
        {
            //Debug.Log("Timed out");
        }
    }

    bool FarFromOtherClusters(int ox, int oy, List<(int x, int y)> currentCoords)
    {
        if(currentCoords.Count !> 0)
        {
            return true;
        }
        foreach((int x, int y) c in currentCoords)
        {
            if(GetDistance(ox, oy, c.x, c.y) > distanceBetweenOtherClusters)
            {
                return false;
            }
        }
        return true;
    }

    bool FarFromPlayerPlacedTiles(int ox, int oy)
    {
        for(int x = ox - distanceFromPlayerPlacedTiles; x < ox + distanceFromPlayerPlacedTiles; x++)
        {
            for(int y = oy - distanceFromPlayerPlacedTiles; y < oy + distanceFromPlayerPlacedTiles; y++)
            {
                if(mapManager.TileHasPlacedTile(x, y))
                {
                    return false;
                }
            }
        }
        return true;
    }
}
