using System;
using UnityEngine;
using System.Threading;
using System.Collections.Generic;

public class TerrainMaster : MonoBehaviour
{
    public enum DrawType { NoiseMap, Mesh }
    public const int chunkSize = 97;

    [Header("Noise")]
    public float scale = 0.3f;
    public int octaves = 4;
    [Range(0, 1)] public float persistance = 0.5f;
    public float lacunarity = 2f;
    public int seed;
    public Vector2 offset;
    public float meshHeightMultiplier;
    public AnimationCurve meshHeightCurve;

    [Space]

    [Header("Biomes")]
    public Biome[] biomes;
    public int biomeOctaves;
    [Range(0, 1)] public float biomePersistance = 0.5f;
    public float biomeLacunarity;
    public Vector2 biomeOffset;

    [Space]

    [Header("LOD")]
    [Range(0, 6)] public int editorLevelOfDetail;

    [Space]

    [Header("Other")]
    public DrawType drawType;
    public PerlinNoise.NormaliseMode normaliseMode;

    Queue<TerrainThreadInfo<Terrain>> terrainThreadInfoQueue = new Queue<TerrainThreadInfo<Terrain>>();
    Queue<TerrainThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<TerrainThreadInfo<MeshData>>();

    void Update()
    {
        if (terrainThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < terrainThreadInfoQueue.Count; i++)
            {
                TerrainThreadInfo<Terrain> threadInfo = terrainThreadInfoQueue.Dequeue();
                threadInfo.callback.Invoke(threadInfo.parameter);
            }
        }

        if (meshDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < meshDataThreadInfoQueue.Count; i++)
            {
                TerrainThreadInfo<MeshData> threadInfo = meshDataThreadInfoQueue.Dequeue();
                threadInfo.callback.Invoke(threadInfo.parameter);
            }
        }
    }

    Terrain GenerateTerrain(Vector2 centre)
    {
        PerlinNoise noise = new PerlinNoise(seed, octaves, 70, 0.3f, centre + offset, normaliseMode); // Change vals.
        PerlinNoise biomeNoise = new PerlinNoise(seed, biomeOctaves, 70, 0.3f, biomeOffset, normaliseMode); // Change vals.
        ColourGenerator colourGen = new ColourGenerator();

        TerrainGenerator terrainGen = new FlatTerrainGenerator(noise, biomeNoise, colourGen, new BiomeHelper(biomes));

        Terrain flatTerrain = terrainGen.GenerateTerrain(chunkSize);

        return flatTerrain;
    }

    public void DrawMapInEditor()
    {
        Terrain terrain = GenerateTerrain(Vector2.zero);
        TerrainDisplay display = FindObjectOfType<TerrainDisplay>();
        if (drawType == DrawType.NoiseMap)
        {
            display.DrawNoiseMap(terrain);
        }
        else if (drawType == DrawType.Mesh)
        {
            display.DrawMesh(MeshGenerator.GenerateTerrainMesh(terrain, new BiomeHelper(this.biomes), meshHeightMultiplier, meshHeightCurve, editorLevelOfDetail));
        }
    }

    public void RequestTerrain(Vector2 centre, Action<Terrain> callback)
    {
        ThreadStart threadStart = delegate
        {
            TerrainThread(centre, callback);
        };

        new Thread(threadStart).Start();
    }

    public void RequestMeshData(Terrain terrain, int lod, Action<MeshData> callback)
    {
        ThreadStart threadStart = delegate
        {
            MeshDataThread(terrain, lod, callback);
        };

        new Thread(threadStart).Start();
    }

    void TerrainThread(Vector2 centre, Action<Terrain> callback)
    {
        Terrain terrain = GenerateTerrain(centre);
        lock (terrainThreadInfoQueue)
        {
            terrainThreadInfoQueue.Enqueue(new TerrainThreadInfo<Terrain>(callback, terrain));
        }
    }

    void MeshDataThread(Terrain terrain, int lod, Action<MeshData> callback)
    {
        MeshData meshData = MeshGenerator.GenerateTerrainMesh(terrain, new BiomeHelper(this.biomes), meshHeightMultiplier, meshHeightCurve, lod);
        lock (meshDataThreadInfoQueue)
        {
            meshDataThreadInfoQueue.Enqueue(new TerrainThreadInfo<MeshData>(callback, meshData));
        }
    }

    void OnValidate()
    {
        if (lacunarity < 1) lacunarity = 1;
        if (octaves < 0) octaves = 0;
    }

    struct TerrainThreadInfo<T>
    {
        public readonly Action<T> callback;
        public readonly T parameter;

        public TerrainThreadInfo(Action<T> callback, T parameter)
        {
            this.callback = callback;
            this.parameter = parameter;
        }
    }
}