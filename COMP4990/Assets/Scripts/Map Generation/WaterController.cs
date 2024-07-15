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
    List<Tuple<int, int>> waterCoords;

    const int waterTile = 34;

    public AnimationClip bubbles1Clip;
    public AnimationClip bubbles2Clip;
    public AnimationClip ripplesClip;

    public AnimatorController bubbles1Controller;
    public AnimatorController bubbles2Controller;
    public AnimatorController ripplesController;

    public GameObject waterEventsParent;

    public float playbackSpeed = 0.5f;

    public void Setup(Tilemap waterTilemap, List<Chunk> waterChunks, int chunkSize)
    {
        waterCoords = new List<Tuple<int, int>>();
        foreach(Chunk chunk in waterChunks)
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
                            waterCoords.Add(new Tuple<int, int>(pos.x, pos.y));
                        }
                    }
                }
            }
        }
        gameLoop = true;
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
                Tuple<int, int> coords = waterCoords[UnityEngine.Random.Range(0, waterCoords.Count)];
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
