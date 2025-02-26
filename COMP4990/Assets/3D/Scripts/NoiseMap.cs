using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

using static Utils;

public static class NoiseMap
{

    public static Dictionary<long, float[,]> Generate(System.Random random)
    {
        int xOff = random.Next(-100000, 100000);
        int yOff = random.Next(-100000, 100000);

        float scale = 0.1f;

        Dictionary<long, float[,]> noiseMap = new Dictionary<long, float[,]>();
        CreateNoiseMap(noiseMap, xOff, yOff, scale);

        SaveNoiseMapToFile(noiseMap, "C:/Projects/COMP4990/COMP4990/COMP4990/Assets/map.txt");

        return noiseMap;
    }

    private static void CreateNoiseMap(Dictionary<long, float[,]> noiseMap, int xOff, int yOff, float scale)
    {
        int halfMap = GameManager.Instance.mapGenerator.halfMap;
        int chunkSize = GameManager.Instance.mapGenerator.GetChunkSize();

        for(int cx = -halfMap; cx < halfMap; cx++)
        {
            for(int cy = -halfMap; cy < halfMap; cy++)
            {
                noiseMap.Add(GetChunkKey(cx, cy), GenerateNoiseArray(chunkSize, cx, cy, xOff, yOff, scale));
            }
        }   
    }

    private static float[,] GenerateNoiseArray(int chunkSize, int cx, int cy, int xOff, int yOff, float scale)
    {
        float[,] noiseArray = new float[chunkSize, chunkSize];
        for(int x = 0; x < chunkSize; x++)
        {
            for(int y = 0; y < chunkSize; y++)
            {
                int xChange = (cx*chunkSize) + x;
                int yChange = (cy*chunkSize) + y;
                noiseArray[x, y] = Mathf.PerlinNoise((xOff + xChange) * scale, (yOff + yChange) * scale);
            }
        }
        return noiseArray;
    }

    // Method to save the noise map to a text file
    private static void SaveNoiseMapToFile(Dictionary<long, float[,]> noiseMap, string filePath)
    {
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            foreach (var entry in noiseMap)
            {
                long chunkKey = entry.Key;
                float[,] noiseArray = entry.Value;

                writer.WriteLine($"Chunk: {chunkKey}");
                for (int x = 0; x < noiseArray.GetLength(0); x++)
                {
                    for (int y = 0; y < noiseArray.GetLength(1); y++)
                    {
                        writer.Write(noiseArray[x, y].ToString("F6") + " ");  // Format the float to 6 decimal places
                    }
                    writer.WriteLine();
                }
                writer.WriteLine();  // Add a blank line after each chunk
            }
        }
        Debug.Log("Noise map saved to " + filePath);
    }
}

