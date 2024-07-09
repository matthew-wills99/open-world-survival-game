using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static Utils;

using Tree = Utils.Tree;
using System.IO;
using System;
using System.Linq;

public class SaveWorldScript : MonoBehaviour
{
    // need to store:
    // player position for all players
    // every chunk and every tile in chunk (including variant)
    // every tree position, every cactus position, every rock position and variant
    // every structure position and type
    // every enemy within x distance from each player <-- do when have enemy

    string extension = "json";
    string directory = "Assets/Saved Worlds/";

    public void SaveWorld(string worldName, int seed, MapSize worldSize, int playerX, int playerY, Dictionary<string, Chunk> aboveGroundChunks, Dictionary<string, Chunk> groundChunks, Dictionary<string, Chunk> underGroundChunks, Dictionary<string, Tree> trees, Dictionary<string, Rock> rocks, Dictionary<string, Cactus> cacti, List<Structure> structures)
    {
        var allObjects = new
        {
            Seed = seed,
            WorldSize = worldSize,
            PlayerX = playerX,
            PlayerY = playerY,
            AboveGroundChunks = aboveGroundChunks,
            GroundChunks = groundChunks,
            UnderGroundChunks = underGroundChunks,
            Trees = trees,
            Rocks = rocks,
            Cacti = cacti,
            Structures = structures
        };

        var settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            Converters = new List<JsonConverter> { new SingleLineListConverter()}
        };

        string json = JsonConvert.SerializeObject(allObjects, settings);
        File.WriteAllTextAsync($"{directory}{worldName}.{extension}", json);
        Debug.Log($"Saved world");
    }

    public class SingleLineListConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(int[,]);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var array = (int[,])value;
            int rows = array.GetLength(0);

            writer.WriteStartArray();
            writer.WriteRaw("\n");
            for(int i = 0; i < rows; i++)
            {
                writer.WriteRaw("[ " + string.Join(", ", GetRow(array, i)) + " ]");
                if(i < rows - 1)
                {
                    writer.WriteRaw(",\n");
                }
            }
            writer.WriteRaw("\n");
            writer.WriteEndArray();
        }

        private IEnumerable<int> GetRow(int[,] array, int row)
        {
            for(int i = 0; i < array.GetLength(1); i++)
            {
                yield return array[row, i];
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JArray array = JArray.Load(reader);
            int rows = array.Count;
            int cols = array[0].Count();
            int[,] result = new int[rows, cols];

            for(int i = 0; i < rows; i++)
            {
                for(int j = 0; j < cols; j++)
                {
                    result[i, j] = (int)array[i][j];
                }
            }
            return result;
        }
    }

    public List<string> GetAllWorlds()
    {
        List<string> worlds = new List<string>();
        var worldPaths = Directory.EnumerateFiles(directory, "*.*", SearchOption.AllDirectories).Where(s => extension == Path.GetExtension(s).TrimStart('.').ToLowerInvariant()).ToList();
        foreach(var path in worldPaths)
        {
            worlds.Add(Path.GetFileNameWithoutExtension(path));
        }
        return worlds;
    }
}
