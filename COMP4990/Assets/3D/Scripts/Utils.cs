using UnityEditor;
using UnityEngine;

public class Utils
{
    public static long GetChunkKey(int x, int y)
    {
        return ((long)x << 32) | (uint)y;
    }

    public static void GetChunkCoordsFromKey(long key, out int x, out int y)
    {
        // dont worry about it
        x = (int)(key >> 32);
        y = (int)(key & 0XFFFFFFFF);
    }

    public static Vector3Int ChunkToWorldPos(int cx, int cy, int tx, int ty, int layer, int chunkSize)
    {
        // layer is y, y is z
        return new Vector3Int(cx * chunkSize + tx, cy * chunkSize + ty, layer);
    }

    public static (int, int, int, int) WorldToChunkPos(int x, int y, int chunkSize)
    {
        // Calculate chunk coordinates (cx, cy) correctly for negative values
        int cx = x >= 0 ? x / chunkSize : (x + 1) / chunkSize - 1;
        int cy = y >= 0 ? y / chunkSize : (y + 1) / chunkSize - 1;

        // Calculate block coordinates within the chunk
        int bx = (x % chunkSize + chunkSize) % chunkSize;
        int by = (y % chunkSize + chunkSize) % chunkSize;

        return (cx, cy, bx, by);
    }
    public static Vector3Int GetChunkAt(int x, int y, int chunkSize)
    {
        int cx = x >= 0 ? x / chunkSize : (x + 1) / chunkSize - 1;
        int cy = y >= 0 ? y / chunkSize : (y + 1) / chunkSize - 1;
        return new Vector3Int(cx, cy);
    }
}

public class ReadOnlyAttribute : PropertyAttribute {}

[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Disable editing of the property
        GUI.enabled = false;
        EditorGUI.PropertyField(position, property, label);
        GUI.enabled = true;
    }
}