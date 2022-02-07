using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileGenerator : MonoBehaviour
{
    [Header("Parameters")]
    public int noiseSampleSize;
    public float scale;
    public int textureResolution=1;
    public float maxtHeight=1.0f;

    [Header("Terrain Types")]
    public TerrainType[] heightTerrainTypes;

    private MeshRenderer tileMeshRenderer;
    private MeshFilter tileMeshFilter;
    private MeshCollider tileMeshCollider;

    void Start()
    {
        //get the tile components
        tileMeshRenderer = GetComponent<MeshRenderer>();
        tileMeshFilter = GetComponent<MeshFilter>();
        tileMeshCollider = GetComponent<MeshCollider>();

        GenerateTile();
    }

    void GenerateTile()
    {
        //generate a new heightmap
        float[,] heightMap = NoiseGenerator.GenerateNoiseMap(noiseSampleSize, scale);

        float[,] hdHeightMap = NoiseGenerator.GenerateNoiseMap(noiseSampleSize, scale, textureResolution);

        Vector3[] verts = tileMeshFilter.mesh.vertices;

        for (int i = 0; i < noiseSampleSize; i++)
        {
            for (int j = 0; j < noiseSampleSize; j++)
            {
                verts[i * noiseSampleSize + j].y = heightMap[i, j] * maxtHeight;
            }
        }

        tileMeshFilter.mesh.vertices = verts;
        tileMeshFilter.mesh.RecalculateBounds();
        tileMeshFilter.mesh.RecalculateNormals();

        tileMeshCollider.sharedMesh = tileMeshFilter.mesh;

        //create the heightmap texture
        Texture2D heightMapTexture = TextureBuilder.BuildTexture(hdHeightMap, heightTerrainTypes);
        //apply the texture to the tile
        tileMeshRenderer.material.mainTexture = heightMapTexture;
    }

}

[System.Serializable]
public class TerrainType
{
    [Range(0.0f, 1.0f)]
    public float threshold;
    public Color color;
}