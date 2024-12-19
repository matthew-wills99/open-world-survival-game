using System.Collections.Generic;
using UnityEngine;

using static Utils;

public class Block
{
    public BlockScriptable blockScriptable;

    public Vector3 position;

    private Texture2D textureAtlas;
    private Mesh mesh;
    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();
    private List<Vector2> uvs = new List<Vector2>();

    private int lastVertex;
    
    private static Dictionary<BlockScriptable, Texture2D> textureAtlases = new Dictionary<BlockScriptable, Texture2D>();

    private void InitializeTextureAtlas(BlockScriptable blockScriptable)
    {
        if (!textureAtlases.ContainsKey(blockScriptable))
        {
            int textureWidth = blockScriptable.topFaceTexture.width;
            Texture2D atlas = new Texture2D(textureWidth * 2, textureWidth);

            for (int x = 0; x < textureWidth; x++)
            {
                for (int y = 0; y < textureWidth; y++)
                {
                    atlas.SetPixel(x, y, blockScriptable.sideFaceTexture.GetPixel(x, y));
                    atlas.SetPixel(x + textureWidth, y + textureWidth, blockScriptable.topFaceTexture.GetPixel(x, y));
                }
            }
            atlas.Apply();

            textureAtlases[blockScriptable] = atlas;
        }
    }

    // Constructor to initialize the block
    public Block(Vector3 position, BlockScriptable blockScriptable)
    {
        this.position = position;
        this.blockScriptable = blockScriptable;

        InitializeTextureAtlas(blockScriptable);

        mesh = new Mesh();

        DrawBlock();

        textureAtlas = textureAtlases[blockScriptable];

        // Assign generated data to the mesh
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.SetUVs(0, uvs);
        mesh.RecalculateNormals();
    }

    public Mesh GetMesh()
    {
        return mesh;
    }

    public Texture2D GetTextureAtlas()
    {
        return textureAtlas;
    }

    private void DrawBlock()
    {
        if(IsFaceVisible(Vector3Int.forward)) Front_GenerateFace();
        if(IsFaceVisible(Vector3Int.back)) Back_GenerateFace();
        if(IsFaceVisible(Vector3Int.left)) Left_GenerateFace();
        if(IsFaceVisible(Vector3Int.right)) Right_GenerateFace();
        // top face should always be drawn
        Top_GenerateFace();

        // No need to generate the bottom face
    }

    private bool IsFaceVisible(Vector3Int dir)
    {
        Vector3Int neighbourPos = Vector3Int.FloorToInt(position) + dir;

        int chunkSize = GameManager.Instance.chunkSize;
        (int cx, int cy, int bx, int by) = WorldToChunkPos(neighbourPos.x, neighbourPos.z, chunkSize);

        long neighbourChunkKey = GetChunkKey(cx, cy);

        if(!GameManager.Instance.chunks.ContainsKey(neighbourChunkKey))
        {
            return true;
        }

        if(GameManager.Instance.chunks.TryGetValue(neighbourChunkKey, out int[,] neighbourChunk))
        {
            if(bx >= 0 && bx < chunkSize && by >= 0 && by < chunkSize)
            {
                return neighbourChunk[bx, by] == 0;
            }
        }

        return true;
    }

    private void GenerateSideUvs()
    {
        uvs.Add(new Vector2(0, 0));
        uvs.Add(new Vector2(0, 1));
        uvs.Add(new Vector2(0.5f, 1));
        uvs.Add(new Vector2(0.5f, 0));
    }

    private void GenerateTopUvs()
    {
        uvs.Add(new Vector2(0.5f, 0));
        uvs.Add(new Vector2(0.5f, 1));
        uvs.Add(new Vector2(1, 1));
        uvs.Add(new Vector2(1, 0));
    }

    private void Front_GenerateFace()
    {
        lastVertex = vertices.Count;

        vertices.Add(position + Vector3.forward);
        vertices.Add(position + Vector3.forward + Vector3.up);
        vertices.Add(position + Vector3.forward + Vector3.up + Vector3.right);
        vertices.Add(position + Vector3.forward + Vector3.right);

        triangles.Add(lastVertex + 2);
        triangles.Add(lastVertex + 1);
        triangles.Add(lastVertex);

        triangles.Add(lastVertex);
        triangles.Add(lastVertex + 3);
        triangles.Add(lastVertex + 2);

        GenerateSideUvs();
    }

    private void Back_GenerateFace()
    {
        lastVertex = vertices.Count;

        vertices.Add(position + Vector3.right);
        vertices.Add(position + Vector3.up + Vector3.right);
        vertices.Add(position + Vector3.up);
        vertices.Add(position);

        triangles.Add(lastVertex + 2);
        triangles.Add(lastVertex + 1);
        triangles.Add(lastVertex);

        triangles.Add(lastVertex);
        triangles.Add(lastVertex + 3);
        triangles.Add(lastVertex + 2);

        GenerateSideUvs();
    }

    private void Left_GenerateFace()
    {
        lastVertex = vertices.Count;

        vertices.Add(position);
        vertices.Add(position + Vector3.up);
        vertices.Add(position + Vector3.forward + Vector3.up);
        vertices.Add(position + Vector3.forward);

        triangles.Add(lastVertex + 2);
        triangles.Add(lastVertex + 1);
        triangles.Add(lastVertex);

        triangles.Add(lastVertex);
        triangles.Add(lastVertex + 3);
        triangles.Add(lastVertex + 2);

        GenerateSideUvs();
    }

    private void Right_GenerateFace()
    {
        lastVertex = vertices.Count;

        vertices.Add(position + Vector3.right + Vector3.forward);
        vertices.Add(position + Vector3.one);
        vertices.Add(position + Vector3.right + Vector3.up);
        vertices.Add(position + Vector3.right);

        triangles.Add(lastVertex + 2);
        triangles.Add(lastVertex + 1);
        triangles.Add(lastVertex);

        triangles.Add(lastVertex);
        triangles.Add(lastVertex + 3);
        triangles.Add(lastVertex + 2);

        GenerateSideUvs();
    }

    private void Top_GenerateFace()
    {
        lastVertex = vertices.Count;

        vertices.Add(position + Vector3.up + Vector3.right);
        vertices.Add(position + Vector3.one);
        vertices.Add(position + Vector3.up + Vector3.forward);
        vertices.Add(position + Vector3.up);

        triangles.Add(lastVertex + 2);
        triangles.Add(lastVertex + 1);
        triangles.Add(lastVertex);

        triangles.Add(lastVertex);
        triangles.Add(lastVertex + 3);
        triangles.Add(lastVertex + 2);

        GenerateTopUvs();
    }
}