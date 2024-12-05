using UnityEngine;

public class NoiseSampleGenerator : MonoBehaviour
{
    public NoiseGenerator noiseGenerator;
    public int textureWidth = 256; // Width of the noise texture
    public int textureHeight = 256; // Height of the noise texture
    public SpriteRenderer displaySpriteRenderer; // A SpriteRenderer to display the noise

    void Start()
    {
        if (noiseGenerator == null)
        {
            Debug.LogError("NoiseGenerator reference is not set!");
            return;
        }

        if (displaySpriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer reference is not set!");
            return;
        }

        GenerateAndDisplayNoiseTexture();
    }

    void GenerateAndDisplayNoiseTexture()
    {
        // Create a new Texture2D
        Texture2D texture = new Texture2D(textureWidth, textureHeight);

        // Loop through each pixel and generate noise
        for (int x = 0; x < textureWidth; x++)
        {
            for (int y = 0; y < textureHeight; y++)
            {
                Color color = noiseGenerator.CalculateColour(x, y); // Use the NoiseGenerator to calculate color
                texture.SetPixel(x, y, color);
            }
        }

        // Apply the changes to the texture
        texture.Apply();

        // Create a sprite from the texture and assign it to the SpriteRenderer
        Sprite noiseSprite = Sprite.Create(texture, new Rect(0, 0, textureWidth, textureHeight), new Vector2(0.5f, 0.5f));
        displaySpriteRenderer.sprite = noiseSprite;
    }
}