using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Tilemaps;
using static Utils;

public class WaterController : MonoBehaviour
{
    public TileIndex tileIndex;
    bool gameLoop = false;
    List<string> waterCoords;
    List<Tuple<int, int>> waterEventTiles;
    Dictionary<string, Chunk> waterChunks;

    int mapSizeInChunks;

    const int waterTile = 34;
    const int deepWaterTile = 36;

    public AnimationClip bubbles1Clip;
    public AnimationClip bubbles2Clip;
    public AnimationClip ripplesClip;

    public AnimatorController bubbles1Controller;
    public AnimatorController bubbles2Controller;
    public AnimatorController ripplesController;

    public GameObject waterEventsParent;

    public float playbackSpeed = 0.5f;

    public void Setup(Tilemap waterTilemap, Dictionary<string, Chunk> waterChunks, int chunkSize, int mapSizeInChunks)
    {
        this.mapSizeInChunks = mapSizeInChunks;
        this.waterChunks = waterChunks;
        SetSeafoam(waterTilemap, chunkSize);

        SetDepth(waterTilemap, chunkSize, 2);

        // TODO: make this work it is horrible
        //SmoothDeepWater(waterTilemap, chunkSize);
        gameLoop = true;
    }

    /// <summary>
    /// Replaces water bordering terrain with the correct
    /// seafoam tile.
    /// </summary>
    /// <param name="waterTilemap">Tilemap that water is stored in</param>
    /// <param name="chunkSize">Length and width of a chunk in tiles</param>
    void SetSeafoam(Tilemap waterTilemap, int chunkSize)
    {
        //TODO: add corners, seafoam should not be placed around the border of the map.

        waterCoords = new List<string>();
        waterEventTiles = new List<Tuple<int, int>>();
        foreach(Chunk chunk in waterChunks.Values)
        {
            for(int tx = 0; tx < chunkSize; tx++)
            {
                for(int ty = 0; ty < chunkSize; ty++)
                {
                    if(chunk.ChunkTiles[tx, ty] != -1)
                    {
                        Vector3Int pos = ChunkToWorldPos(chunk.X, chunk.Y, tx, ty, chunkSize);
                        int sfIndex = GetSeafoamIndex(waterTilemap, pos);
                        if(sfIndex != -1)
                        {
                            waterTilemap.SetTile(pos, tileIndex.GetSeafoam(sfIndex));
                        }
                        else
                        {
                            waterTilemap.SetTile(pos, tileIndex.GetTile(waterTile));
                            waterCoords.Add(GetCoordinateKey(chunk.X, chunk.Y, tx, ty));
                            waterEventTiles.Add(new Tuple<int, int>(pos.x, pos.y));
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Replaces any water x tiles from terrain in all directions with deep water.
    /// </summary>
    /// <param name="waterTilemap">Tilemap that water is stored in</param>
    /// <param name="waterChunks">List of chunks containing water</param>
    /// <param name="chunkSize">Length and width of a chunk in tiles</param>
    /// <param name="distanceFromShore">Distance in tiles from shore required for deep water</param>
    void SetDepth(Tilemap waterTilemap, int chunkSize, int distanceFromShore=3)
    {
        foreach(Chunk chunk in waterChunks.Values)
        {
            for(int tx = 0; tx < chunkSize; tx++)
            {
                for(int ty = 0; ty < chunkSize; ty++)
                {
                    if(chunk.ChunkTiles[tx, ty] != -1)
                    {
                        if(IsDeepWater(chunk.X, chunk.Y, tx, ty, chunkSize, distanceFromShore))
                        {
                            waterTilemap.SetTile(ChunkToWorldPos(chunk.X, chunk.Y, tx, ty, chunkSize), tileIndex.GetTile(deepWaterTile));
                        }
                    }
                }
            }
        }
    }

    bool IsDeepWater(int cx, int cy, int tx, int ty, int chunkSize, int distanceFromShore)
    {
        int ncx;
        int ncy;
        int ntx;
        int nty;

        for(int x = tx - distanceFromShore; x <= tx + distanceFromShore; x++)
        {
            for(int y = ty - distanceFromShore; y <= ty + distanceFromShore; y++)
            {
                // ignore the original tile
                if(x != tx || y != ty)
                {
                    ncx = cx;
                    ncy = cy;
                    ntx = x;
                    nty = y;
                    // check if the neighbouring tile is in a different chunk
                    if(x < 0 || x >= chunkSize || y < 0 || y >= chunkSize)
                    {
                        if(x < 0)
                        {
                            ncx--;
                            ntx = chunkSize + x;
                        }
                        else if(x >= chunkSize)
                        {
                            ncx++;
                            ntx = x - chunkSize;
                        }
                        if(y < 0)
                        {
                            ncy--;
                            nty = chunkSize + y;
                        }
                        else if(y >= chunkSize)
                        {
                            ncy++;
                            nty = y - chunkSize;
                        }
                    }
                    if(waterChunks.ContainsKey(GetChunkKey(ncx, ncy)))
                    {
                        if(waterChunks[GetChunkKey(ncx, ncy)].ChunkTiles[ntx, nty] != waterTile || !waterCoords.Contains(GetCoordinateKey(ncx, ncy, ntx, nty)))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }
        return true;
    }

    void SmoothDeepWater(Tilemap waterTilemap, int chunkSize)
    {
        foreach(Chunk chunk in waterChunks.Values)
        {
            int cx = chunk.X;
            int cy = chunk.Y;
            for(int x = 0; x < chunkSize; x++)
            {
                for(int y = 0; y < chunkSize; y++)
                {
                    int neighbourWaterTiles = GetSurroundingWater(cx, cy, x, y, chunkSize);

                    if(neighbourWaterTiles > 4)
                    {
                        waterChunks[GetChunkKey(cx, cy)].ChunkTiles[x, y] = waterTile;
                        waterTilemap.SetTile(ChunkToWorldPos(cx, cy, x, y, chunkSize), tileIndex.GetTile(waterTile));
                    }
                    else if(neighbourWaterTiles < 4)
                    {
                        waterChunks[GetChunkKey(cx, cy)].ChunkTiles[x, y] = deepWaterTile;
                        waterTilemap.SetTile(ChunkToWorldPos(cx, cy, x, y, chunkSize), tileIndex.GetTile(deepWaterTile));
                    }
                }
            }
        }
    }

    int GetSurroundingWater(int cx, int cy, int tx, int ty, int chunkSize)
    {
        int tileCount = 0;
        int ncx;
        int ncy;
        int ntx;
        int nty;

        for(int x = tx - 1; x <= tx + 1; x++)
        {
            for(int y = ty - 1; y <= ty + 1; y++)
            {
                // ignore the original tile
                if(x != tx || y != ty)
                {
                    ncx = cx;
                    ncy = cy;
                    ntx = x;
                    nty = y;
                    // check if the neighbouring tile is in a different chunk
                    if(x < 0 || x >= chunkSize || y < 0 || y >= chunkSize)
                    {
                        if(x < 0)
                        {
                            ncx--;
                            ntx = chunkSize + x;
                        }
                        else if(x >= chunkSize)
                        {
                            ncx++;
                            ntx = x - chunkSize;
                        }
                        if(y < 0)
                        {
                            ncy--;
                            nty = chunkSize + y;
                        }
                        else if(y >= chunkSize)
                        {
                            ncy++;
                            nty = y - chunkSize;
                        }
                    }
                    if(waterChunks.ContainsKey(GetChunkKey(ncx, ncy)))
                    {
                        if(waterChunks[GetChunkKey(ncx, ncy)].ChunkTiles[ntx, nty] == waterTile)
                        {
                            tileCount++;
                        }
                    }
                }
            }
        }
        return tileCount;
    }

    float currentTime = 0f;
    float cooldown = 0.1f;
    void Update()
    {
        if(gameLoop)
        {
            if(Time.time - currentTime > cooldown)
            {
                // get random water tile coordinates
                Tuple<int, int> coords = waterEventTiles[UnityEngine.Random.Range(0, waterCoords.Count)];
                int x = coords.Item1;
                int y = coords.Item2;

                // they will all have the same name because i dont want to add a string to the tuple in tile index
                GameObject waterEventObject = new GameObject("water event");
                // set parent so i dont have to look at them all in the inspector
                waterEventObject.transform.parent = waterEventsParent.transform;
                // set position and rotation to be correct
                waterEventObject.transform.position = new Vector3Int(x, y, 0);
                waterEventObject.transform.rotation = Quaternion.Euler(0, 0, 0);

                SpriteRenderer spriteRenderer = waterEventObject.AddComponent<SpriteRenderer>();
                spriteRenderer.sortingLayerID = waterEventsParent.transform.GetComponent<SpriteRenderer>().sortingLayerID;

                // pick a random water event to occur
                int eventIndex = UnityEngine.Random.Range(0, tileIndex.GetWaterEventCount());
                Tuple<AnimationClip, AnimatorController> selectedAnimation = tileIndex.GetWaterEvent(eventIndex);
                AnimatorController controller = selectedAnimation.Item2;
                Animator animator = waterEventObject.AddComponent<Animator>();
                animator.runtimeAnimatorController = controller;

                AnimatorControllerLayer layer = controller.layers[0];
                AnimatorStateMachine stateMachine = layer.stateMachine;
                AnimatorState state = stateMachine.defaultState;
                state.motion = selectedAnimation.Item1;
                stateMachine.defaultState = state;

                animator.speed = playbackSpeed;

                waterEventObject.AddComponent<AutoDestroy>().animationClip = selectedAnimation.Item1;

                currentTime = Time.time;
            }
        }
    }

    public class AutoDestroy : MonoBehaviour
    {
        public AnimationClip animationClip;

        void Start()
        {
            float adjustedDuration = animationClip.length / GetComponent<Animator>().speed;

            // Destroy the GameObject after the adjusted animation duration
            Destroy(gameObject, adjustedDuration);
        }
    }

    private int GetSeafoamIndex(ITilemap tilemap, Vector3Int position)
    {
        bool waterLeft = HasTile(tilemap, position + Vector3Int.left);
        bool waterRight = HasTile(tilemap, position + Vector3Int.right);
        bool waterUp = HasTile(tilemap, position + Vector3Int.up);
        bool waterDown = HasTile(tilemap, position + Vector3Int.down);

        if (!waterLeft && waterRight && waterUp && waterDown) return 0; // Seafoam on left edge
        if (!waterRight && waterLeft && waterUp && waterDown) return 1; // Seafoam on right edge
        if (!waterUp && waterLeft && waterRight && waterDown) return 2; // Seafoam on top edge
        if (!waterDown && waterLeft && waterRight && waterUp) return 3; // Seafoam on bottom edge

        // Handle corners
        if (!waterLeft && !waterUp && waterRight && waterDown) return 4; // Top-left corner
        if (!waterRight && !waterUp && waterLeft && waterDown) return 5; // Top-right corner
        if (!waterLeft && !waterDown && waterRight && waterUp) return 6; // Bottom-left corner
        if (!waterRight && !waterDown && waterLeft && waterUp) return 7; // Bottom-right corner

        // Handle 3 sides
        if (!waterLeft && !waterRight && !waterUp && waterDown) return 8; // down open
        if (!waterLeft && !waterRight && waterUp && !waterDown) return 9; // up open
        if (!waterLeft && waterRight && !waterUp && !waterDown) return 10; // right open
        if (waterLeft && !waterRight && !waterUp && !waterDown) return 11; // left open

        // Handle 2 sides
        if (!waterLeft && !waterRight && waterUp && waterDown) return 12; // seafoam left and right
        if (waterLeft && waterRight && !waterUp && !waterDown) return 13; // seafoam up and down

        return -1; // No seafoam needed
    }

    private bool HasTile(ITilemap tilemap, int x, int y)
    {
        return tilemap.GetTile(new Vector3Int(x, y, 0)) != null;
    }

    private bool HasTile(ITilemap tilemap, Vector3Int position)
    {
        return tilemap.GetTile(position) != null;
    }
}
