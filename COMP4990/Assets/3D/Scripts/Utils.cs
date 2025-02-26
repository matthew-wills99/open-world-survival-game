using System;
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

    public static Vector2Int ChunkToWorldPos(int cx, int cy, int tx, int ty, int chunkSize)
    {
        return new Vector2Int(cx * chunkSize + tx, cy * chunkSize + ty);
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

    [Serializable]
    public class BlockEntry
    {
        public int blockID; // The unique ID for the block
        public string blockName; // Name of the block
        public bool isTerrain; // (not water or air) (physical block the player stands on that is not a structure or something)
        [HideInInspector] public Biome biome;
        public BlockScriptable blockScriptable; // The associated ScriptableObject
    }

    [Serializable]
    public class Biome
    {
        public EBiome biome;
        public int biomeRadius;
        [Range(0, 100)] public int terrainChance;
    }


    public enum EBiome
    {
        Snow,
        Desert,
        Forest,
        Wasteland,
        Plains,
    }

    public enum ERiver
    {
        River,
        NotRiver,
    }

    public enum EObstacle
    {
        Obstacle,
        NotObstacle
    }
}

public class DistanceFunctions
{
    // id = 1
    public static float ManhattanDistance(Vector2Int point)
    {
        return Mathf.Abs(point.x) + Mathf.Abs(point.y);
    }

    // id = 2
    public static float EuclideanDistance(Vector2Int point)
    {
        return Mathf.Sqrt(point.x * point.x + point.y * point.y);
    }

    // id = 3
    public static float MinkowskiDistance(Vector2Int point)
    {
        // power 3 default
        return Mathf.Pow(Mathf.Pow(Mathf.Abs(point.x), 3) + Mathf.Pow(Mathf.Abs(point.y), 3), 1f / 3);
    }

    public static float UseDistanceFunctionById(Vector2Int point, int id)
    {
        switch(id)
        {
            case 1:
                return ManhattanDistance(point);
            case 2:
                return EuclideanDistance(point);
            case 3:
                return MinkowskiDistance(point);
            default:
                Debug.LogWarning($"What the sigma!! expected id 1 -> 3 but got ({id})!");
                return ManhattanDistance(point);
        }
    }
}

#if UNITY_EDITOR
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

[CustomPropertyDrawer(typeof(Utils.BlockEntry))]
public class BlockEntryDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Use a foldout for better organization
        property.isExpanded = EditorGUI.Foldout(
            new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight),
            property.isExpanded,
            property.displayName
        );

        if (!property.isExpanded) return;

        // Start drawing fields
        EditorGUI.indentLevel++;

        // Draw blockID
        SerializedProperty blockID = property.FindPropertyRelative("blockID");
        EditorGUI.PropertyField(
            new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight, position.width, EditorGUIUtility.singleLineHeight),
            blockID
        );

        // Draw blockName
        SerializedProperty blockName = property.FindPropertyRelative("blockName");
        EditorGUI.PropertyField(
            new Rect(position.x, position.y + 2 * EditorGUIUtility.singleLineHeight, position.width, EditorGUIUtility.singleLineHeight),
            blockName
        );

        // Draw isTerrain
        SerializedProperty isTerrain = property.FindPropertyRelative("isTerrain");
        EditorGUI.PropertyField(
            new Rect(position.x, position.y + 3 * EditorGUIUtility.singleLineHeight, position.width, EditorGUIUtility.singleLineHeight),
            isTerrain
        );

        // Conditionally show biome dropdown
        if (isTerrain.boolValue)
        {
            SerializedProperty biome = property.FindPropertyRelative("biome");
            EditorGUI.PropertyField(
                new Rect(position.x, position.y + 5 * EditorGUIUtility.singleLineHeight, position.width, EditorGUIUtility.singleLineHeight),
                biome
            );
        }

        EditorGUI.indentLevel--;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        int lines = property.isExpanded ? 5 : 1;

        // Add an extra line if isBiomeTerrain is true
        if (property.isExpanded && property.FindPropertyRelative("isTerrain").boolValue)
            lines++;

        return EditorGUIUtility.singleLineHeight * lines;
    }
}
#endif