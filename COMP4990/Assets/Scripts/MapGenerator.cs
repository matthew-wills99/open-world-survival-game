using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    Dictionary<int, GameObject> tileset;
    Dictionary<int, GameObject> tile_groups = null;
    public GameObject pfb_plains;
    public GameObject pfb_forest;
    //public GameObject pfb_desert;
    //public GameObject pfb_lake;
    public GameObject pfb_ocean;
    //public GameObject pfb_river;
    public GameObject pfb_snow;
    //public GameObject pfb_swamp;

    int map_width = 160;
    int map_height = 90;

    List<List<int>> noise_grid = null;
    List<List<GameObject>> tile_grid = null;

    public float magnification = 7.0f;
    public int x_offset = 0;
    public int y_offset = 0;

    void Start()
    {
        Generate();
    }

    [ContextMenu("Generate Map")]
    public void Generate()
    {
        if(tile_grid != null)
        {
            foreach(List<GameObject> row in tile_grid)
            {
                foreach(GameObject obj in row)
                {
                    Destroy(obj);
                }
            }
        }

        if(tile_groups != null)
        {
            foreach(KeyValuePair<int, GameObject> group in tile_groups)
            {
                Destroy(group.Value);
            }
        }

        noise_grid = new List<List<int>>();
        tile_grid = new List<List<GameObject>>();

        CreateTileset();
        GenerateTileGroups();
        GenerateMap();
    }

    void CreateTileset()
    {
        tileset = new Dictionary<int, GameObject>
        {
            { 0, pfb_plains },
            { 1, pfb_forest },
            { 2, pfb_ocean },
            { 3, pfb_snow }
        };
    }

    public void GenerateTileGroups()
    {
        tile_groups = new Dictionary<int, GameObject>();
        foreach(KeyValuePair<int, GameObject> prefab_pair in tileset)
        {
            GameObject tile_group = new GameObject(prefab_pair.Value.name);
            tile_group.transform.parent = gameObject.transform;
            tile_group.transform.localPosition = new Vector3(0, 0, 0);
            tile_groups.Add(prefab_pair.Key, tile_group);
        }
    }

    void GenerateMap()
    {
        for(int x = 0; x < map_width; x++)
        {
            noise_grid.Add(new List<int>());
            tile_grid.Add(new List<GameObject>());
            for(int y = 0; y < map_height; y++)
            {
                int tile_id = GetIdUsingPerlin(x, y);
                noise_grid[x].Add(tile_id);
                CreateTile(tile_id, x, y);
            }
        }
    }

    int GetIdUsingPerlin(int x, int y)
    {
        float raw_perlin = Mathf.PerlinNoise(
            (x - x_offset) / magnification, 
            (y - y_offset) / magnification
        );

        float clamped_perlin = Mathf.Clamp(raw_perlin, 0.0f, 1.0f);
        float scaled_perlin = clamped_perlin * tileset.Count;
        if(scaled_perlin == tileset.Count)
        {
            scaled_perlin = tileset.Count - 1;
        }
        return Mathf.FloorToInt(scaled_perlin);
    }

    void CreateTile(int tile_id, int x, int y)
    {
        GameObject tile_prefab = tileset[tile_id];
        GameObject tile_group = tile_groups[tile_id];
        GameObject tile = Instantiate(tile_prefab, tile_group.transform);

        tile.name = string.Format("tile_x[0]_y[1]", x, y);
        tile.transform.localPosition = new Vector3(x, y, 0);
        
        tile_grid[x].Add(tile);
    }
}
