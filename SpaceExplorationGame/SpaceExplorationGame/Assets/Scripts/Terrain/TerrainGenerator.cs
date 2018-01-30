using UnityEngine;

public abstract class TerrainGenerator
{
    private PerlinNoise perlinNoise;
    private PerlinNoise biomePerlinNoise;
    private ColourGenerator colourGen;
    private BiomeHelper biomeHelper;

    public TerrainGenerator(PerlinNoise perlinNoise, PerlinNoise biomePerlinNoise, ColourGenerator colourGen, BiomeHelper biomeHelper)
    {
        this.perlinNoise = perlinNoise;
        this.biomePerlinNoise = biomePerlinNoise;
        this.colourGen = colourGen;
        this.biomeHelper = biomeHelper;
    }

    public Terrain GenerateTerrain(int mapSize)
    {
        float[,] heights = GenerateHeights(mapSize, perlinNoise);
        float[,] biomeMap = GenerateBiomeMap(mapSize, biomePerlinNoise);
        Color[] colours = colourGen.GenerateColours(heights, biomeMap, biomeHelper, mapSize);
        return CreateTerrain(heights, biomeMap, colours);
    }

    private float[,] GenerateHeights(int mapSize, PerlinNoise perlinNoise)
    {
        return perlinNoise.GenerateNoiseMap(mapSize);
    }

    private float[,] GenerateBiomeMap(int mapSize, PerlinNoise perlinNoise)
    {
        return perlinNoise.GenerateNoiseMap(mapSize);
    }

    public abstract Terrain CreateTerrain(float[,] heightMap, float[,] biomeMap, Color[] colourMap);
}