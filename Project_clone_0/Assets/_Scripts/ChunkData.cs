using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ChunkData
{
 
    public Vector2 coord;
    public bool loaded;
    public int randomState;

    public GameObject chunk;
    public GameObject terrain;
    public GameObject trees;

    public MeshFilter terrainMeshFilter;
    public MeshRenderer terrainMeshRenderer;
    public Mesh terrainMesh;

    public float[,] TemperatureMap;
    public float[,] HumidityMap;
    public float[,] ElevationMap;
    public float[,] MountainMap;
    public int[,] BiomeMap;
    public float[,] FreshWaterMap;
    public float[,] WetnessMap;
    public float[,] HeightMap;
    public bool[,] TreeMap;

    public ChunkData(Vector2 _coord)
    {
        coord = _coord;
        loaded = false;
    }

    public void Init(GameObject chunkPrefab)
    {
        randomState = (int)(coord.x + coord.y * 10f);

        chunk = GameObject.Instantiate(chunkPrefab);
        chunk.transform.SetParent(GameObject.Find("Chunk Generator").transform);
        terrain = chunk.transform.Find("Terrain").gameObject;
        chunk.transform.position = Vector3.zero;
        trees = new GameObject();
        trees.transform.SetParent(chunk.transform);

        terrainMeshRenderer = terrain.GetComponent<MeshRenderer>();
        terrainMeshFilter = terrain.GetComponent<MeshFilter>();
        terrainMesh = new Mesh();
        terrainMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        terrainMeshFilter.mesh = terrainMesh;

        loaded = true;
    }

    public void Deload()
    {
        Component.Destroy(terrainMesh);
        GameObject.Destroy(chunk);
        GameObject.Destroy(trees);
        loaded = false;

    }

}
