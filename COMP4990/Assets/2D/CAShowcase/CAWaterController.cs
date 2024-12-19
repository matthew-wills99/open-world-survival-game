using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Tilemaps;
using static EUtils;

public class CAWaterController : MonoBehaviour
{
    public CompositeCollider2D compositeCollider;

    public Tilemap seafoamTilemap;

    public int deepWaterSmoothingPasses = 1;
    public int waterThreshold = 4;
    
    public MapGen mapManager;

    int mapSizeInChunks;

    public TileIndex tileIndex;
    bool gameLoop = false;
    List<string> waterCoords;
    List<Tuple<int, int>> waterEventTiles;
    Dictionary<string, Chunk> waterChunks;

    int chunkSize;

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

    public void Setup(Tilemap waterTilemap, Dictionary<string, Chunk> wc, int chunkSize, int mapSizeInChunks)
    {
        StartCoroutine(Go(waterTilemap, wc, chunkSize, mapSizeInChunks));
    }

    IEnumerator Go(Tilemap waterTilemap, Dictionary<string, Chunk> wc, int chunkSize, int mapSizeInChunks)
    {
        this.mapSizeInChunks = mapSizeInChunks;
        waterChunks = wc;
        this.chunkSize = chunkSize;
        yield return StartCoroutine(SetSeafoam(waterTilemap, chunkSize));

        yield return StartCoroutine(SetDepth(waterTilemap, chunkSize, 2));

        // TODO: make this work it is horrible
        for(int i = 0; i < deepWaterSmoothingPasses; i++)
        {
            yield return StartCoroutine(SmoothDeepWater(waterTilemap, chunkSize));
        }

        mapManager.SetWaterChunks(waterChunks);
        gameLoop = true;

        //CleanUpEdges(waterTilemap, chunkSize);

        //Debug.Log($"yea: {HasTile(waterTilemap, ChunkToWorldPos(mapSizeInChunks/2, mapSizeInChunks/2, 16, 16, chunkSize))}");
        //Debug.Log($"yea: {waterTilemap.GetTile(ChunkToWorldPos(0,0, 0,0, chunkSize))}");

        compositeCollider.GenerateGeometry();
    }

    /// <summary>
    /// Replaces water bordering terrain with the correct
    /// seafoam tile.
    /// </summary>
    /// <param name="waterTilemap">Tilemap that water is stored in</param>
    /// <param name="chunkSize">Length and width of a chunk in tiles</param>
    IEnumerator SetSeafoam(Tilemap waterTilemap, int chunkSize)
    {
        //TODO: add corners, seafoam should not be placed around the border of the map.

        // Store the camera's original position and orthographic size
        Vector3 originalCameraPosition = Camera.main.transform.position;
        float originalOrthographicSize = Camera.main.orthographicSize;

        // Set camera zoom for closer view
        float targetZoom = 11f; // Adjust this value for the desired close-up zoom
        Camera.main.orthographicSize = targetZoom;

        waterCoords = new List<string>();
        waterEventTiles = new List<Tuple<int, int>>();
        int border = mapSizeInChunks / 2;
        foreach(Chunk chunk in waterChunks.Values)
        {
            int cx = chunk.X;
            int cy = chunk.Y;

            Vector3 chunkCenter = new Vector3(cx * chunkSize + chunkSize / 2f, cy * chunkSize + chunkSize / 2f, originalCameraPosition.z);

            // Smoothly move the camera to the chunk center
            float elapsedTime = 0f;
            float moveDuration = 0.75f; // Adjust for camera movement speed
            Vector3 startCameraPosition = Camera.main.transform.position;

            while (elapsedTime < moveDuration)
            {
                elapsedTime += Time.deltaTime;
                Camera.main.transform.position = Vector3.Lerp(startCameraPosition, chunkCenter, elapsedTime / moveDuration);
                yield return null;
            }

            for(int tx = 0; tx < chunkSize; tx++)
            {
                for(int ty = 0; ty < chunkSize; ty++)
                {
                    if(chunk.ChunkTiles[tx, ty] != -1)
                    {
                        if(tx == 0 || tx == chunkSize - 1 || ty == 0 || ty == chunkSize - 1)
                        {
                            yield return new WaitForSeconds(0.05f);
                        }

                        Vector3Int pos = ChunkToWorldPos(chunk.X, chunk.Y, tx, ty, chunkSize);

                        int sfIndex = GetSeafoamIndex(waterTilemap, pos);
                        if(sfIndex != -1)
                        {
                            seafoamTilemap.SetTile(pos, tileIndex.GetSeafoam(sfIndex));
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

        Camera.main.transform.position = new Vector3(0, 0, -10);
        Camera.main.orthographicSize = 65;
    }

    /// <summary>
    /// Replaces any water x tiles from terrain in all directions with deep water.
    /// </summary>
    /// <param name="waterTilemap">Tilemap that water is stored in</param>
    /// <param name="waterChunks">List of chunks containing water</param>
    /// <param name="chunkSize">Length and width of a chunk in tiles</param>
    /// <param name="distanceFromShore">Distance in tiles from shore required for deep water</param>
    IEnumerator SetDepth(Tilemap waterTilemap, int chunkSize, int distanceFromShore=2)
    {
        foreach(Chunk chunk in waterChunks.Values)
        {
            for(int tx = 0; tx < chunkSize; tx++)
            {
                for(int ty = 0; ty < chunkSize; ty++)
                {
                    yield return new WaitForSeconds(000f);
                    if(chunk.ChunkTiles[tx, ty] != -1)
                    {
                        if(IsDeepWater(chunk.X, chunk.Y, tx, ty, chunkSize, distanceFromShore))
                        {
                            waterChunks[GetChunkKey(chunk.X, chunk.Y)].ChunkTiles[tx, ty] = deepWaterTile;
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
                        if(waterChunks[GetChunkKey(ncx, ncy)].ChunkTiles[ntx, nty] != deepWaterTile)
                        {
                            if(waterChunks[GetChunkKey(ncx, ncy)].ChunkTiles[ntx, nty] != waterTile)
                            {
                                return false;
                            }
                        }
                    }
                    else
                    {
                        //return false;
                    }
                }
            }
        }
        return true;
    }

    IEnumerator SmoothDeepWater(Tilemap waterTilemap, int chunkSize)
    {
        foreach(Chunk chunk in waterChunks.Values)
        {
            int cx = chunk.X;
            int cy = chunk.Y;
            for(int x = 0; x < chunkSize; x++)
            {
                for(int y = 0; y < chunkSize; y++)
                {
                    yield return new WaitForSeconds(000f);
                    if(chunk.ChunkTiles[x, y] == deepWaterTile)
                    {
                        int neighbourWaterTiles = GetSurroundingWater(cx, cy, x, y, chunkSize);

                        if(neighbourWaterTiles > waterThreshold)
                        {
                            waterChunks[GetChunkKey(cx, cy)].ChunkTiles[x, y] = waterTile;
                            waterTilemap.SetTile(ChunkToWorldPos(cx, cy, x, y, chunkSize), tileIndex.GetTile(waterTile));
                        }
                    }
                }
            }
        }
    }

    void CleanUpEdges(Tilemap waterTilemap, int chunkSize)
    {
        int border = mapSizeInChunks / 2;
        foreach(Chunk chunk in waterChunks.Values)
        {
            int cx = chunk.X;
            int cy = chunk.Y;
            // if the chunk is a border chunk
            if(cx == -border || cx == border-1 || cy == -border || cy == border-1)
            {
                for(int tx = 0; tx < chunkSize; tx++)
                {
                    for(int ty = 0; ty < chunkSize; ty++)
                    {
                        // if the tile is a border tile
                        if(cx == -border && tx == 0 || cx == border-1 && tx == chunkSize-1 || cy == -border && ty == 0 || cy == border-1 && ty == chunkSize-1)
                        {
                            Vector3Int pos = ChunkToWorldPos(chunk.X, chunk.Y, tx, ty, chunkSize);
                            int sfIndex = GetSeafoamIndex(waterTilemap, pos);
                            if(sfIndex != -1)
                            {
                                waterTilemap.SetTile(pos, tileIndex.GetSeafoam(sfIndex));
                            }
                        }
                    }
                }
            }
        }
    }

    int GetSurroundingWater(int cx, int cy, int tx, int ty, int chunkSize)
    {
        int waterCount = 0;
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
                        if(waterChunks[GetChunkKey(ncx, ncy)].ChunkTiles[ntx, nty] != deepWaterTile)
                        {
                            waterCount++;
                        }
                    }
                }
            }
        }
        return waterCount;
    }

    float currentTime = 0f;
    float cooldown = 0.1f;
    void Update()
    {
        /*if(gameLoop)
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
        }*/
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
        (int cx, int cy, int tx, int ty) yea = WorldToChunkPos(position.x, position.y, chunkSize);
        int cx = yea.cx;
        int cy = yea.cy;
        int tx = yea.tx;
        int ty = yea.ty;

        int border = mapSizeInChunks / 2;

        bool waterLeft = HasTile(tilemap, position + Vector3Int.left);
        bool waterRight = HasTile(tilemap, position + Vector3Int.right);
        bool waterUp = HasTile(tilemap, position + Vector3Int.up);
        bool waterDown = HasTile(tilemap, position + Vector3Int.down);
 
        bool waterUpLeft = HasTile(tilemap, position + Vector3Int.up + Vector3Int.left);
        bool waterUpRight = HasTile(tilemap, position + Vector3Int.up + Vector3Int.right);
        bool waterDownLeft = HasTile(tilemap, position + Vector3Int.down + Vector3Int.left);
        bool waterDownRight = HasTile(tilemap, position + Vector3Int.down + Vector3Int.right);

        bool upLeftCorner = !waterUpLeft && waterLeft && waterUp;
        bool upRightCorner = !waterUpRight && waterRight && waterUp;
        bool downLeftCorner = !waterDownLeft && waterLeft && waterDown;
        bool downRightCorner = !waterDownRight && waterRight && waterDown;

        //Debug.Log($"{cx}, {cy}");

        // left border
        if(cx == -border && tx == 0)
        {
            if(!waterRight)
            {
                return 1;
            }
            return -1;
        }
        // right border
        else if(cx == border-1 && tx == chunkSize-1)
        {
            if(!waterLeft)
            {
                return 0;
            }
            return -1;
        }
        // bottom border
        else if(cy == -border && ty == 0)
        {
            if(!waterUp)
            {
                return 2;
            }
            return -1;
        }
        // top border
        else if(cy == border-1 && ty == chunkSize-1)
        {
            if(!waterDown)
            {
                return 3;
            }
            return -1;
        }

        // corners of block and corners of sfc:
        if (upLeftCorner && !waterRight && !waterDown) return 29;
        if (upLeftCorner && !waterRight) return 30;
        if (upLeftCorner && !waterDown) return 31;

        if (upRightCorner && !waterLeft && !waterDown) return 32;
        if (upRightCorner && !waterLeft) return 33;
        if (upRightCorner && !waterDown) return 34;

        if (downLeftCorner && !waterRight && !waterUp) return 35;
        if (downLeftCorner && !waterRight) return 36;
        if (downLeftCorner && !waterUp) return 37;

        if (downRightCorner && !waterLeft && !waterUp) return 38;
        if (downRightCorner && !waterLeft) return 39;
        if (downRightCorner && !waterUp) return 40;
        // not done yet

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

        // corners of sf

        if (!upLeftCorner && !upRightCorner && !downLeftCorner && downRightCorner) return 14; /* Only downRightCorner is true */ 
        if (!upLeftCorner && !upRightCorner && downLeftCorner && !downRightCorner) return 15; /* Only downLeftCorner is true */ 
        if (!upLeftCorner && !upRightCorner && downLeftCorner && downRightCorner) return 16; /* Only downLeftCorner and downRightCorner are true */ 
        if (!upLeftCorner && upRightCorner && !downLeftCorner && !downRightCorner) return 17; /* Only upRightCorner is true */ 
        if (!upLeftCorner && upRightCorner && !downLeftCorner && downRightCorner) return 18; /* Only upRightCorner and downRightCorner are true */ 
        if (!upLeftCorner && upRightCorner && downLeftCorner && !downRightCorner) return 19; /* Only upRightCorner and downLeftCorner are true */ 
        if (!upLeftCorner && upRightCorner && downLeftCorner && downRightCorner) return 20; /* Only upRightCorner, downLeftCorner, and downRightCorner are true */ 
        if (upLeftCorner && !upRightCorner && !downLeftCorner && !downRightCorner) return 21; /* Only upLeftCorner is true */ 
        if (upLeftCorner && !upRightCorner && !downLeftCorner && downRightCorner) return 22; /* Only upLeftCorner and downRightCorner are true */ 
        if (upLeftCorner && !upRightCorner && downLeftCorner && !downRightCorner) return 23; /* Only upLeftCorner and downLeftCorner are true */ 
        if (upLeftCorner && !upRightCorner && downLeftCorner && downRightCorner) return 24; /* Only upLeftCorner, downLeftCorner, and downRightCorner are true */ 
        if (upLeftCorner && upRightCorner && !downLeftCorner && !downRightCorner) return 25;/* Only upLeftCorner and upRightCorner are true */ 
        if (upLeftCorner && upRightCorner && !downLeftCorner && downRightCorner) return 26; /* Only upLeftCorner, upRightCorner, and downRightCorner are true */ 
        if (upLeftCorner && upRightCorner && downLeftCorner && !downRightCorner) return 27; /* Only upLeftCorner, upRightCorner, and downLeftCorner are true */ 
        if (upLeftCorner && upRightCorner && downLeftCorner && downRightCorner) return 28; /* All corners are true */ 

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

    private bool IsWorldPositionOutOfBounds(int cx, int cy, int tx, int ty, int chunkSize)
    {
        // Calculate the half size of the map in chunks
        int halfMapSizeInChunks = mapSizeInChunks / 2;

        // Check if chunk coordinates are out of bounds
        if (cx < -halfMapSizeInChunks || cx > halfMapSizeInChunks ||
            cy < -halfMapSizeInChunks || cy > halfMapSizeInChunks)
        {
            return true; // Chunk coordinates are out of bounds
        }

        // Check if tile coordinates are out of bounds
        if (tx < 0 || tx >= chunkSize || ty < 0 || ty >= chunkSize)
        {
            return true; // Tile coordinates are out of bounds
        }

        // Both chunk and tile coordinates are within bounds
        return false;
    }
}
