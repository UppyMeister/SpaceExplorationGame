using UnityEngine;

public class Terrain
{
    public float[,] heightMap;
    public float[,] biomeMap;
    public Color[] colourMap;

    public Terrain(float[,] heightMap, float[,] biomeMap, Color[] colourMap)
    {
        this.heightMap = heightMap;
        this.biomeMap = biomeMap;
        this.colourMap = colourMap;
    }
}