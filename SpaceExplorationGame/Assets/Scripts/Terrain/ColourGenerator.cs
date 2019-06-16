using UnityEngine;
using System.Collections.Generic;

public class ColourGenerator
{
    private BiomeHelper biomeHelper;

    public ColourGenerator() { }

    public Color[] GenerateColours(float[,] heightMap, float[,] biomeMap, BiomeHelper biomeHelper, int size)
    {
        this.biomeHelper = biomeHelper;

        Color[] colourMap = new Color[Mathf.RoundToInt(5.8806f * (size * size))];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                colourMap[y * size + x] = BlendColours(GetNeighbourColours(x, y, heightMap, biomeMap));
            }
        }

        return colourMap;
    }

    private List<Color> GetNeighbourColours(int x, int y, float[,] heightMap, float[,] biomeMap)
    {
        List<Color> colours = new List<Color>();
        
        for (int dy = -1; dy <= 1; dy++)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                if ((x + dx >= 0 && x + dx < heightMap.GetLength(0)) && (y + dy >= 0 && y + dy < heightMap.GetLength(1)))
                {
                    Biome biome = biomeHelper.GetBiome(biomeMap[x + dx, y + dy]);
                    if (biome != null)
                    {
                        TerrainType terrainType = biomeHelper.GetTerrainType(biome, heightMap[x + dx, y + dy]);
                        if (terrainType != null)
                        {
                            colours.Add(terrainType.colour);
                        }
                        else
                        {
                            Debug.LogError("No Terrain Type found!");
                        }
                    }
                    else
                    {
                        Debug.LogError("No Biome found!");
                    }
                } else
                {
                    //Debug.LogWarning("Index exists in other chunk");
                }
            }
        }

        return colours;
    }

    private Color BlendColours(List<Color> colours)
    {
        Color t = default(Color);
        foreach (Color c in colours)
        {
            t += c;
        }

        t /= colours.Count;
        t.a = 1;

        return t;
    }
}