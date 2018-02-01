using UnityEngine;

public class PerlinNoise
{
    public enum NormaliseMode { Local, Global }
    private int seed;
    private int octaves = 3;
    private float amplitude = 60f;
    private float roughness = 0.3f;
    private Vector2 offset;
    private Vector2[] octaveOffsets;
    private NormaliseMode normaliseMode;
    private float maxPossibleHeight = 0;

    public PerlinNoise(int seed, int octaves, float amplitude, float roughness, Vector2 offset, NormaliseMode normaliseMode)
    {
        this.seed = seed;
        this.octaves = octaves;
        this.amplitude = amplitude;
        this.roughness = roughness;
        this.offset = offset;
        this.normaliseMode = normaliseMode;

        System.Random prng = new System.Random(this.seed);
        this.octaveOffsets = new Vector2[this.octaves];
        for (int i = 0; i < octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000) + this.offset.x;
            float offsetY = prng.Next(-100000, 100000) + this.offset.y;

            this.octaveOffsets[i] = new Vector2(offsetX, offsetY);

            maxPossibleHeight += (float)Mathf.Pow(this.roughness, i) * this.amplitude;
        }
    }

    public PerlinNoise(int octaves, float amplitude, float roughness, Vector2 offset, NormaliseMode normaliseMode)
    {
        this.seed = new System.Random().Next(1000000000);
        this.octaves = octaves;
        this.amplitude = amplitude;
        this.roughness = roughness;
        this.offset = offset;
        this.normaliseMode = normaliseMode;

        System.Random prng = new System.Random(this.seed);
        this.octaveOffsets = new Vector2[this.octaves];
        for (int i = 0; i < octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000) + this.offset.x;
            float offsetY = prng.Next(-100000, 100000) - this.offset.y;

            this.octaveOffsets[i] = new Vector2(offsetX, offsetY);

            maxPossibleHeight += (float)Mathf.Pow(this.roughness, i) * this.amplitude;
        }
    }

    public int GetSeed() { return this.seed; }
    public float GetAmplitude() { return this.amplitude; }

    public float[,] GenerateNoiseMap(int mapSize)
    {
        float[,] noiseMap = new float[mapSize, mapSize];

        float minLocalNoiseHeight = float.MaxValue;
        float maxLocalNoiseHeight = float.MinValue;

        for (int y = 0; y < mapSize; y++)
        {
            for (int x = 0; x < mapSize; x++)
            {
                float perlinVal = GetPerlinNoise(x, y);

                if (perlinVal > maxLocalNoiseHeight) maxLocalNoiseHeight = perlinVal;
                else if (perlinVal < minLocalNoiseHeight) minLocalNoiseHeight = perlinVal;

                noiseMap[x, y] = perlinVal;
            }
        }

        for (int y = 0; y < mapSize; y++)
        {
            for (int x = 0; x < mapSize; x++)
            {
                if (this.normaliseMode == NormaliseMode.Local)
                {
                    noiseMap[x, y] = Mathf.InverseLerp(minLocalNoiseHeight, maxLocalNoiseHeight, noiseMap[x, y]);
                } else if (this.normaliseMode == NormaliseMode.Global)
                {
                    float normalisedHeight = (noiseMap[x, y] + 1) / (maxPossibleHeight);
                    noiseMap[x, y] = normalisedHeight;
                }
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
            float sampleX = (x + this.octaveOffsets[i].x) * freq;
            float sampleY = (y + this.octaveOffsets[i].y) * freq;
            total += GetInterpolatedNoise(sampleX, sampleY) * amp;
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