using System;
using UnityEngine;
using System.Collections.Generic;

public class InfiniteTerrain : MonoBehaviour
{
    [Header("Base")]
    [SerializeField] private Transform viewer;
    [SerializeField] private Material chunkMaterial;

    [Space]

    [Header("Levels of Detail")]
    [SerializeField] private LODInfo[] levelsOfDetail;

    public static Vector2 viewerPos;
    Vector2 viewerPosOld;
    public static float maxViewDistance;
    int chunkSize;
    int chunksVisibleInViewDistance;

    const float viewerMoveThresholdForChunkUpdate = 25f;
    const float sqrViewerMoveThresholdForChunkUpdate = viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;
    const float scale = 1f;

    static TerrainMaster terrainMaster;

    Dictionary<Vector2, Chunk> chunkDictionary = new Dictionary<Vector2, Chunk>();
    static List<Chunk> chunksVisibleLastUpdate = new List<Chunk>();

    void Start()
    {
        terrainMaster = GetComponent<TerrainMaster>();

        maxViewDistance = levelsOfDetail[levelsOfDetail.Length - 1].visibleDistanceThreshold;
        chunkSize = TerrainMaster.chunkSize - 1;
        chunksVisibleInViewDistance = Mathf.RoundToInt(maxViewDistance / chunkSize);

        UpdateVisibleChunks();
    }

    void Update()
    {
        viewerPos = new Vector2(viewer.position.x, viewer.position.z) / scale;

        if ((viewerPosOld - viewerPos).sqrMagnitude > sqrViewerMoveThresholdForChunkUpdate)
        {
            viewerPosOld = viewerPos;
            UpdateVisibleChunks();
        }
    }

    void UpdateVisibleChunks()
    {
        for (int i = 0; i < chunksVisibleLastUpdate.Count; i++)
        {
            chunksVisibleLastUpdate[i].SetVisible(false);
        }

        chunksVisibleLastUpdate.Clear();

        int currentChunkCoordX = Mathf.RoundToInt(viewerPos.x / chunkSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPos.y / chunkSize);

        for (int yOffset = -chunksVisibleInViewDistance; yOffset <= chunksVisibleInViewDistance; yOffset++)
        {
            for (int xOffset = -chunksVisibleInViewDistance; xOffset <= chunksVisibleInViewDistance; xOffset++)
            {
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

                if (chunkDictionary.ContainsKey(viewedChunkCoord))
                {
                    chunkDictionary[viewedChunkCoord].UpdateChunk();
                } else
                {
                    chunkDictionary.Add(viewedChunkCoord, new Chunk(viewedChunkCoord, chunkSize, levelsOfDetail, transform, chunkMaterial));
                }
            }
        }
    }

    public class Chunk
    {
        Vector2 pos;
        GameObject meshObj;
        Bounds bounds;

        MeshRenderer meshRenderer;
        MeshFilter meshFilter;
        MeshCollider meshCollider;

        LODInfo[] detailLevels;
        LODMesh[] lodMeshes;
        LODMesh collisionLodMesh;

        Terrain terrain;
        bool terrainReceived;
        int previousLodIndex = -1;

        public Chunk(Vector2 coord, int size, LODInfo[] detailLevels, Transform parent, Material mat)
        {
            this.detailLevels = detailLevels;
            this.pos = coord * size;
            bounds = new Bounds(this.pos, Vector2.one * size);
            Vector3 positionV3 = new Vector3(this.pos.x, 0, this.pos.y);

            meshObj = new GameObject("Chunk");
            meshRenderer = meshObj.AddComponent<MeshRenderer>();
            meshFilter = meshObj.AddComponent<MeshFilter>();
            meshCollider = meshObj.AddComponent<MeshCollider>();
            meshRenderer.material = mat;

            meshObj.transform.position = positionV3 * scale;
            meshObj.transform.parent = parent;
            meshObj.transform.localScale = Vector3.one * scale;

            SetVisible(false);

            lodMeshes = new LODMesh[this.detailLevels.Length];
            for (int i = 0; i < lodMeshes.Length; i++)
            {
                lodMeshes[i] = new LODMesh(this.detailLevels[i].lod, UpdateChunk);
                if (this.detailLevels[i].useForCollider)
                {
                    collisionLodMesh = lodMeshes[i];
                }
            }

            terrainMaster.RequestTerrain(this.pos, OnTerrainReceived);
        }

        void OnTerrainReceived(Terrain terrain)
        {
            this.terrain = terrain;
            this.terrainReceived = true;

            UpdateChunk();
        }

        public void UpdateChunk()
        {
            if (this.terrainReceived)
            {
                float viewerDistanceFromNearestChunkEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPos));
                bool visible = viewerDistanceFromNearestChunkEdge <= maxViewDistance;

                if (visible)
                {
                    int lodIndex = 0;

                    for (int i = 0; i < this.detailLevels.Length - 1; i++)
                    {
                        if (viewerDistanceFromNearestChunkEdge > this.detailLevels[i].visibleDistanceThreshold)
                        {
                            lodIndex = i + 1;
                        }
                        else break;
                    }

                    if (lodIndex != previousLodIndex)
                    {
                        LODMesh lodMesh = lodMeshes[lodIndex];
                        if (lodMesh.hasMesh)
                        {
                            previousLodIndex = lodIndex;
                            meshFilter.mesh = lodMesh.mesh;
                        }
                        else if (!lodMesh.hasRequestedMesh)
                        {
                            lodMesh.RequestMesh(this.terrain);
                        }
                    }

                    if (lodIndex == 0)
                    {
                        if (collisionLodMesh.hasMesh)
                        {
                            meshCollider.sharedMesh = collisionLodMesh.mesh;
                        } else if (!collisionLodMesh.hasRequestedMesh)
                        {
                            collisionLodMesh.RequestMesh(this.terrain);
                        }
                    }

                    chunksVisibleLastUpdate.Add(this);
                }

                SetVisible(visible);
            }
        }

        public void SetVisible(bool visible)
        {
            meshObj.SetActive(visible);
        }

        public bool IsVisible() { return meshObj.activeSelf; }
    }

    class LODMesh
    {
        public Mesh mesh;
        public bool hasRequestedMesh;
        public bool hasMesh;
        int lod;
        Action updateCallback;

        public LODMesh(int lod, Action updateCallback)
        {
            this.lod = lod;
            this.updateCallback = updateCallback;
        }

        public void RequestMesh(Terrain terrain)
        {
            hasRequestedMesh = true;
            terrainMaster.RequestMeshData(terrain, this.lod, OnMeshDataReceived);
        }

        void OnMeshDataReceived(MeshData meshData)
        {
            mesh = meshData.CreateMesh();
            hasMesh = true;
            updateCallback.Invoke();
        }
    }
}