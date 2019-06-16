using System;
using UnityEngine;

public class BiomeHelper
{
    private Biome[] biomes;

    public BiomeHelper(Biome[] biomes)
    {
        this.biomes = biomes;
    }

    public Biome[] GetBiomes() { return this.biomes; }

    public Biome GetBiome(float height)
    {
        for (int i = 0; i < this.biomes.Length; i++)
        {
            if (height <= biomes[i].height)
            {
                return biomes[i];
            }
        }

        return null;
    }

    public TerrainType GetTerrainType(Biome biome, float height)
    {
        for (int i = 0; i < biome.terrainTypes.Length; i++)
        {
            if (height <= biome.terrainTypes[i].height)
            {
                return biome.terrainTypes[i];
            }
        }

        return null;
    }
}