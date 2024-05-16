using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public bool useColour = false;

    public int width = 256;
    public int height = 256;

    public float scale = 20f;
    public float offsetX = 100f;
    public float offsetY = 100f;

    // TODO fix this shit
    public Color[] biomes;
    [Range(0f, 1f)]
    public float[] biomeThresholds; // should be in ascending order and equal in length to biomes
    
    private new Renderer renderer;

    void Start()
    {
        renderer = GetComponent<Renderer>();
    }

    void Update()
    {
        renderer.material.mainTexture = GenerateTexture();
    }

    Texture2D GenerateTexture()
    {
        Texture2D texture = new Texture2D(width, height);

        for(int x = 0; x < width; x++)
        {
            for(int y = 0; y < height; y++)
            {
                Color colour = CalculateColour(x, y);
                texture.SetPixel(x, y, colour);
            }
        }

        texture.Apply();
        return texture;
    }

    Color CalculateColour(int x, int y)
    {
        float xCoord = (float)x / width * scale + offsetX;
        float yCoord = (float)y / height * scale + offsetY;

        float sample = Mathf.Clamp(Mathf.PerlinNoise(xCoord, yCoord), 0f, 1f);

        if(useColour)
        {
            // iterate thru biomes descending order
            for(int i = biomes.Length - 1; i >= 0; i--)
            {
                if(sample >= biomeThresholds[i])
                {
                    return biomes[i]; // ? i think?
                }
            }
        }
        return new Color(sample, sample, sample);
    }
}
