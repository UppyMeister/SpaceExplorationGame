using UnityEngine;

public class PerlinNoise
{
    private int seed;
    private int octaves = 3;
    private float amplitude = 60f;
    private float roughness = 0.3f;

    public PerlinNoise(int seed, int octaves, float amplitude, float roughness)
    {
        this.seed = seed;
        this.octaves = octaves;
        this.amplitude = amplitude;
        this.roughness = roughness;
    }

    public PerlinNoise(int octaves, float amplitude, float roughness)
    {
        this.seed = new System.Random().Next(1000000000);
        this.octaves = octaves;
        this.amplitude = amplitude;
        this.roughness = roughness;
    }

    public int GetSeed() { return this.seed; }
    public float GetAmplitude() { return this.amplitude; }

    public float[,] GenerateNoiseMap(int mapSize)
    {
        float[,] noiseMap = new float[mapSize, mapSize];

        float minNoiseHeight = float.MaxValue;
        float maxNoiseHeight = float.MinValue;

        for (int y = 0; y < mapSize; y++)
        {
            for (int x = 0; x < mapSize; x++)
            {
                float perlinVal = GetPerlinNoise(x, y);

                if (perlinVal > maxNoiseHeight) maxNoiseHeight = perlinVal;
                else if (perlinVal < minNoiseHeight) minNoiseHeight = perlinVal;

                noiseMap[x, y] = perlinVal;
            }
        }

        for (int y = 0; y < mapSize; y++)
        {
            for (int x = 0; x < mapSize; x++)
            {
                noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
            }
        }

        return noiseMap;
    }

    private float GetPerlinNoise(int x, int y)
    {
        float total = 0;
        float d = (float)Mathf.Pow(2, this.octaves - 1);

        for (int i = 0; i < this.octaves; i++)
        {
            float freq = (float)(Mathf.Pow(2, i) / d);
            float amp = (float)Mathf.Pow(this.roughness, i) * this.amplitude;
            total += GetInterpolatedNoise(x * freq, y * freq) * amp;
        }
        
        return total;
    }

    private float GetSmoothNoise(int x, int y)
    {
        float corners = (GetNoise(x - 1, y - 1) + GetNoise(x + 1, y - 1) + GetNoise(x - 1, y + 1) + GetNoise(x + 1, y + 1)) / 16f;
        float sides = (GetNoise(x - 1, y) + GetNoise(x + 1, y) + GetNoise(x, y - 1) + GetNoise(x, y + 1)) / 8f;
        float center = GetNoise(x, y) / 4f;
        return corners + sides + center;
    }

    private float GetNoise(int x, int y)
    {
        long n = x + (y * 57);
        n = (long)((n << 13) ^ n);
        return (float)(1.0 - ((n * (n * n * 15731 + 789221) + 1376312589) & 0x7fffffff) / 1073741824.0);
    }

    private float GetInterpolatedNoise(float x, float y)
    {
        int intX = (int)x;
        float fracX = x - intX;
        int intY = (int)y;
        float fracY = y - intY;

        float v1 = GetSmoothNoise(intX, intY);
        float v2 = GetSmoothNoise(intX + 1, intY);
        float v3 = GetSmoothNoise(intX, intY + 1);
        float v4 = GetSmoothNoise(intX + 1, intY + 1);
        float i1 = Interpolate(v1, v2, fracX);
        float i2 = Interpolate(v3, v4, fracX);
        return Interpolate(i1, i2, fracY);
    }

    private float Interpolate(float a, float b, float blend)
    {
        double theta = blend * Mathf.PI;
        float f = (float)((1f - Mathf.Cos((float)theta)) * 0.5f);
        return a * (1f - f) + b * f;
    }
}