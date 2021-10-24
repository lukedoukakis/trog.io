using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ChunkData
{
 
    public Vector2 coordinate;
    public bool loaded;
    public bool spawnsPlaced;
    public int randomState;

    public GameObject chunk;
    public GameObject terrain;
    public Transform featuresParent;

    public MeshFilter terrainMeshFilter;
    public MeshRenderer terrainMeshRenderer;
    public Mesh terrainMesh;

    public float[,] TemperatureMap;
    public float[,] HumidityMap;
    public float[,] ElevationMap;
    public float[,] MountainMap;
    public float[,] FreshWaterMap;
    public float[,] WetnessMap;
    public float[,] HeightMap;
    public bool[,] TreeMap;

    public float[,] YNormalsMap;
    public float[,] SkewHorizMap;

    public Dictionary<Vector2, ChunkData> neighbors;

    // ----

    public static Vector2 up = Vector2.up;
    public static Vector2 down = Vector2.down;
    public static Vector2 right = Vector2.right;
    public static Vector2 left = Vector2.left;
    public static Vector2 upRight = up + right;
    public static Vector2 upLeft = up + left;
    public static Vector2 downRight = down + right;
    public static Vector2 downLeft = down + left;

    public ChunkData(Vector2 _coord)
    {
        coordinate = _coord;
        loaded = false;
        spawnsPlaced = false;
    }

    public void Init(GameObject chunkPrefab)
    {
        randomState = (int)(coordinate.x + coordinate.y * 10f);

        chunk = GameObject.Instantiate(chunkPrefab);
        chunk.transform.SetParent(GameObject.Find("Chunk Generator").transform);
        terrain = chunk.transform.Find("Terrain").gameObject;
        chunk.transform.position = Vector3.zero;
        featuresParent = new GameObject().transform;
        featuresParent.SetParent(chunk.transform);

        terrainMeshRenderer = terrain.GetComponent<MeshRenderer>();
        terrainMeshFilter = terrain.GetComponent<MeshFilter>();
        terrainMesh = new Mesh();
        terrainMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        terrainMeshFilter.mesh = terrainMesh;

        neighbors = new Dictionary<Vector2, ChunkData>();
        neighbors.Add(up, null);
        neighbors.Add(down, null);
        neighbors.Add(right, null);
        neighbors.Add(left, null);
        neighbors.Add(upRight, null);
        neighbors.Add(upLeft, null);
        neighbors.Add(downRight, null);
        neighbors.Add(downLeft, null);
        FetchAllNeighbors();

        loaded = true;
    }


    void FetchAllNeighbors()
    {
        List<Vector2> keys = new List<Vector2>(neighbors.Keys);
        List<ChunkData> values = new List<ChunkData>(neighbors.Values);
        for(int i = 0; i < keys.Count; ++i)
        {
            ChunkData targetCd = ChunkGenerator.GetChunk(this.coordinate + keys[i]);
            AddNeighbor(keys[i], targetCd);

            if(targetCd != null)
            {
                targetCd.AddNeighbor(keys[i] * -1f, this);
            }
        }
    }

    public void AddNeighbor(Vector2 position, ChunkData cd)
    {
        neighbors[position] = cd;
    }

    void RemoveReferencesToThisAsANeighbor()
    {

        List<Vector2> keys = new List<Vector2>(neighbors.Keys);
        List<ChunkData> values = new List<ChunkData>(neighbors.Values);

        for(int i = 0; i < keys.Count; ++i)
        {
            if(values[i] != null)
            {
                values[i].RemoveNeighbor(keys[i] * -1f);
            }
        }
    }

    public void RemoveNeighbor(Vector2 position)
    {
        neighbors[position] = null;
    }

    public ChunkData GetNeighbor(Vector2 position)
    {
        return neighbors[position];
    }

    public bool IsEdgeChunk()
    {
        return neighbors.ContainsValue(null);
    }


    public void Deload()
    {
        RemoveReferencesToThisAsANeighbor();
        Component.Destroy(terrainMesh);
        GameObject.Destroy(chunk);
        GameObject.Destroy(featuresParent);
        loaded = false;

    }

}
