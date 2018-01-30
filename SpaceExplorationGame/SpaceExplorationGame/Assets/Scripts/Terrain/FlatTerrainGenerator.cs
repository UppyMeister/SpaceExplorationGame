using UnityEngine;

public class FlatTerrainGenerator : TerrainGenerator
{
    public FlatTerrainGenerator(PerlinNoise perlinNoise, PerlinNoise biomePerlinNoise, ColourGenerator colourGen, BiomeHelper biomeHelper)
        : base(perlinNoise, biomePerlinNoise, colourGen, biomeHelper) { }

    public override Terrain CreateTerrain(float[,] heightMap, float[,] biomeMap, Color[] colourMap)
    {
        return new Terrain(heightMap, biomeMap, colourMap);
    }
}