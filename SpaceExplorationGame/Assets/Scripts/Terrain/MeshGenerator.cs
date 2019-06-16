using UnityEngine;

public static class MeshGenerator
{
    public static MeshData GenerateTerrainMesh(Terrain terrain, BiomeHelper biomeHelper, float heightMultiplier, AnimationCurve meshHeightCurve, int levelOfDetail)
    {
        AnimationCurve heightCurve = new AnimationCurve(meshHeightCurve.keys);
        int size = terrain.heightMap.GetLength(0);
        float topLeftX = (size - 1) / -2f;
        float topLeftZ = (size - 1) / 2f;

        int meshSimplificationIncrement = levelOfDetail == 0 ? 1 : levelOfDetail * 2;
        int verticesPerLine = (size - 1) / meshSimplificationIncrement + 1;

        MeshData meshData = new MeshData(verticesPerLine);
        int vertexIndex = 0;

        for (int y = 0; y < size; y += meshSimplificationIncrement)
        {
            for (int x = 0; x < size; x += meshSimplificationIncrement)
            {
                Biome biome = biomeHelper.GetBiome(terrain.biomeMap[x, y]);
                meshData.vertices[vertexIndex] = new Vector3(topLeftX + x, heightCurve.Evaluate(terrain.heightMap[x, y]) * heightMultiplier * (biome != null ? biome.heightMultiplier : 1), topLeftZ - y);
                meshData.uvs[vertexIndex] = new Vector2(x / (float)size, y / (float)size);

                if (x < size - 1 && y < size - 1)
                {
                    meshData.AddTriangle(vertexIndex, vertexIndex + verticesPerLine + 1, vertexIndex + verticesPerLine);
                    meshData.AddTriangle(vertexIndex + verticesPerLine + 1, vertexIndex, vertexIndex + 1);
                }

                vertexIndex++;
            }
        }

        meshData.SetColours(terrain.colourMap);
        meshData.ApplyFlatShading();

        return meshData;
    }
}

public class MeshData
{
    public Vector3[] vertices;
    public int[] triangles;
    public Vector2[] uvs;
    public Color[] meshColours;

    int triangleIndex;
    bool useFlatShading;

    public MeshData(int meshSize)
    {
        vertices = new Vector3[meshSize * meshSize];
        uvs = new Vector2[meshSize * meshSize];
        triangles = new int[(meshSize - 1) * (meshSize - 1) * 6];
    }

    public void SetColours(Color[] meshColours)
    {
        this.meshColours = meshColours;
    }

    public void AddTriangle(int a, int b, int c)
    {
        triangles[triangleIndex] = a;
        triangles[triangleIndex + 1] = b;
        triangles[triangleIndex + 2] = c;
        triangleIndex += 3;
    }

    public Mesh CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        //mesh.colors = this.meshColours;
        mesh.RecalculateNormals();

        return mesh;
    }

    public void ApplyFlatShading()
    {
        Vector3[] flatShadedVertices = new Vector3[triangles.Length];
        Vector2[] flatShadedUvs = new Vector2[triangles.Length];

        for (int i = 0; i < triangles.Length; i++)
        {
            flatShadedVertices[i] = vertices[triangles[i]];
            flatShadedUvs[i] = uvs[triangles[i]];
            triangles[i] = i;
        }

        vertices = flatShadedVertices;
        uvs = flatShadedUvs;
    }
}