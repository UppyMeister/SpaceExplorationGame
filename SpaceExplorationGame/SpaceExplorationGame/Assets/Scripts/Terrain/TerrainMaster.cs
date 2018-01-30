using UnityEngine;

public class TerrainMaster : MonoBehaviour
{
    public enum DrawType { NoiseMap, Mesh }
    
    [Header("Noise")]
    public int mapSize = 100;
    public float scale = 0.3f;
    public int octaves = 4;
    [Range(0, 1)] public float persistance = 0.5f;
    public float lacunarity = 2f;
    public int seed;
    public Vector2 offset;
    public float meshHeightMultiplier;

    [Space]

    [Header("Biomes")]
    public Biome[] biomes;
    public int biomeOctaves;
    [Range(0, 1)] public float biomePersistance = 0.5f;
    public float biomeLacunarity;
    public Vector2 biomeOffset;

    [Space]

    [Header("Other")]
    public DrawType drawType;

    void Start ()
	{
        GenerateTerrain();
	}

    public void GenerateTerrain()
    {
        PerlinNoise noise = new PerlinNoise(octaves, 70, 0.3f); // Change vals.
        PerlinNoise biomeNoise = new PerlinNoise(biomeOctaves, 70, 0.3f); // Change vals.
        ColourGenerator colourGen = new ColourGenerator();

        TerrainGenerator terrainGen = new FlatTerrainGenerator(noise, biomeNoise, colourGen, new BiomeHelper(biomes));

        Terrain flatTerrain = terrainGen.GenerateTerrain(this.mapSize);

        TerrainDisplay display = FindObjectOfType<TerrainDisplay>();
        if (drawType == DrawType.NoiseMap)
        {
            display.DrawNoiseMap(flatTerrain);

        } else if (drawType == DrawType.Mesh)
        {
            display.DrawMesh(MeshGenerator.GenerateTerrainMesh(flatTerrain, new BiomeHelper(this.biomes), meshHeightMultiplier));
        }
    }

    void OnValidate()
    {
        if (mapSize < 1) mapSize = 1;
        if (lacunarity < 1) lacunarity = 1;
        if (octaves < 0) octaves = 0;
    }
}