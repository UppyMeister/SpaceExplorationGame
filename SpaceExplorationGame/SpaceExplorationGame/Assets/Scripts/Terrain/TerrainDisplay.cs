using UnityEngine;

public class TerrainDisplay : MonoBehaviour
{
    [SerializeField] private Renderer textureRenderer;
    [SerializeField] private MeshFilter meshFilter;
    [SerializeField] private MeshRenderer meshRenderer;

    public void DrawNoiseMap(Terrain terrain)
    {
        int size = terrain.heightMap.GetLength(0);

        Texture2D texture = new Texture2D(size, size);
        texture.SetPixels(terrain.colourMap);
        texture.Apply();

        textureRenderer.sharedMaterial.mainTexture = texture;
        textureRenderer.transform.localScale = new Vector3(size, 1, size);
    }

    public void DrawMesh(MeshData meshData)
    {
        meshFilter.sharedMesh = meshData.CreateMesh();
        //meshRenderer.sharedMaterial.mainTexture = null;
    }
}