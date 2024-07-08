using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using static Utils;

using Tree = Utils.Tree;
using System.IO;
using System;

public class SaveWorldScript : MonoBehaviour
{
    // need to store:
    // player position for all players
    // every chunk and every tile in chunk (including variant)
    // every tree position, every cactus position, every rock position and variant
    // every structure position and type
    // every enemy within x distance from each player <-- do when have enemy
    public void SaveWorld(string worldName, int seed, MapSize worldSize, List<Vector3> players, Dictionary<string, Chunk> chunks, Dictionary<string, Tree> trees, Dictionary<string, Rock> rocks, Dictionary<string, Cactus> cacti, List<Structure> structures)
    {
        var allObjects = new
        {
            Seed = seed,
            WorldSize = worldSize,
            //Players = players,
            Chunks = chunks,
            Trees = trees,
            //Rocks = rocks,
            Cacti = cacti,
            //Structures = structures
        };

        string json = JsonConvert.SerializeObject(allObjects, Formatting.Indented);
        File.WriteAllText($"{worldName}.json", json);
        Debug.Log("Saved world");
    }
}
