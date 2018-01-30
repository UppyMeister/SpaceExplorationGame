using UnityEngine;

[System.Serializable]
public class Biome
{
    public string name;
    public float height;
    public float heightMultiplier;
    public TerrainType[] terrainTypes;
}

[System.Serializable]
public class TerrainType
{
    public string name;
    public float height;
    public Color colour;
}