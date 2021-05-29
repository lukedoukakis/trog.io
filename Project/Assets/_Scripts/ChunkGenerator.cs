using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class ChunkGenerator : MonoBehaviour
{
    public static ChunkGenerator current;
    public static int Seed = -1;
    public static int ChunkSize = 100;
    public static int ChunkRenderDistance = 10;
    public static float ElevationAmplitude = 1800f;
    public static float MinElevation = -.292893219f;
    public static float MaxElevation = .224744871f;
    public static int MountainMapScale = 400;
    public static float ElevationMapScale = 2000;
    public static int TemperatureMapScale = 4000;
    public static int HumidityMapScale = 2000;
    public static bool LoadingChunks, DeloadingChunks;
    static GameObject Chunk;
    static GameObject Terrain;
    static GameObject Water;

    public Transform playerT;
    public Vector3 playerPos;
    public Vector2 playerPos_chunkSpace;
    public Vector2 currentChunkCoord;


    [SerializeField] MeshFilter TerrainMeshFilter;
    [SerializeField] MeshFilter WaterMeshFilter;
    Mesh TerrainMesh;
    Mesh WaterMesh;


    [SerializeField] PhysicMaterial physicMaterial;
    public LayerMask layerMask_terrain;

    public GameObject chunkPrefab;

    int xIndex;
    int zIndex;
    int xOffset;
    int zOffset;

    static List<ChunkData> ChunkDataToLoad;
    static List<ChunkData> ChunkDataLoaded;

    static Vector3[] TerrainVertices;
    static int[] TerrainTriangles;
    static Vector2[] TerrainUvs;
    static Color[] TerrainColors;
    static Vector3[] WaterVertices;
    static int[] WaterTriangles;
    static Vector2[] WaterUvs;
    static Color[] WaterColors;

    [SerializeField] float waterAlpha;

    float[,] TemperatureMap;
    float[,] HumidityMap;
    float[,] ElevationMap;
    float[,] MountainMap;
    int[,] BiomeMap;
    float[,] FreshWaterMap;
    float[,] WetnessMap;
    float[,] HeightMap;
    bool[,] TreeMap;

    public float scale;
    public int octaves;
    [Range(0, 1)] public float persistance;
    public float lacunarity;
    public float posterize_variationScale;

    public float flatLevel;
    public float seaLevel;
    public float snowLevel;


    // feature
    GameObject Trees;
    [Range(0f, 1f)] public float treeDensity;



    // Start is called before the first frame update
    void Awake()
    {
        current = this;
        Init();
        Biome.Init();

    }

    private void Update()
    {
        if (Biome.initialized && playerT != null)
        {
            if(!LoadingChunks && !DeloadingChunks){
                LoadingChunks = true;
                DeloadingChunks = true;
                UpdateChunksToLoad();
                StartCoroutine(LoadChunks());
                StartCoroutine(DeloadChunks());
            }
        }

    }

    void Init()
    {
        Seed = UnityEngine.Random.Range(0, 1000);
        //Seed = 100;
        if (Seed == -1) { Seed = UnityEngine.Random.Range(-100000, 100000); }
        Debug.Log("seed: " + Seed.ToString());

        //RiverGenerator.Generate();

        ChunkDataToLoad = new List<ChunkData>();
        ChunkDataLoaded = new List<ChunkData>();

        TerrainMesh = new Mesh();
        WaterMesh = new Mesh();
        TerrainMeshFilter.mesh = TerrainMesh;
        WaterMeshFilter.mesh = WaterMesh;
        currentChunkCoord = Vector2.positiveInfinity;

        layerMask_terrain = LayerMask.GetMask("Terrain");
    }


    void UpdateChunksToLoad()
    {


        playerPos = playerT.position;
        playerPos_chunkSpace = ToChunkSpace(playerPos);
        currentChunkCoord = new Vector2(Mathf.Floor(playerPos_chunkSpace.x), Mathf.Floor(playerPos_chunkSpace.y));


        // get neighbor chunk coordinates
        Vector2 halfVec = Vector3.one / 2f;
        Vector2[] neighborChunkCoords = new Vector2[(int)Mathf.Pow(ChunkRenderDistance * 2, 2)];
        int i = 0;
        for (int z = (int)currentChunkCoord.y - ChunkRenderDistance; z < (int)currentChunkCoord.y + ChunkRenderDistance; z++)
        {
            for (int x = (int)currentChunkCoord.x - ChunkRenderDistance; x < (int)currentChunkCoord.x + ChunkRenderDistance; x++)
            {
                neighborChunkCoords[i] = new Vector2(x, z);
                i++;
            }
        }


        // remove chunks out of rendering range from ChunksToLoad
        foreach (ChunkData cd in ChunkDataToLoad.ToArray())
        {
            if (Vector2.Distance(playerPos_chunkSpace, cd.coord + halfVec) >= ChunkRenderDistance)
            {
                ChunkDataToLoad.Remove(cd);
            }
        }

        // add chunks in rendering range to ChunksToLoad
        foreach (Vector2 chunkCoord in neighborChunkCoords)
        {
            if (Vector2.Distance(playerPos_chunkSpace, chunkCoord + halfVec) < ChunkRenderDistance)
            {

                int index = ChunkDataToLoad.FindIndex(cd => cd.coord == chunkCoord);
                if (index < 0)
                {
                    ChunkDataToLoad.Add(new ChunkData(chunkCoord));
                }
            }
        }

    }

    IEnumerator LoadChunks()
    {

        IEnumerator load;
        foreach (ChunkData cd in ChunkDataToLoad.OrderBy(c => Vector3.Distance(ToChunkSpace(playerT.position), c.coord)).ToArray())
        {
            if (!cd.loaded)
            {
                load = LoadChunk(cd);
                yield return StartCoroutine(load);
                ChunkDataLoaded.Add(cd);
            }
            if(ToChunkSpace(playerT.position) != playerPos_chunkSpace){
                break;
            }
        }
        LoadingChunks = false;
    }



    IEnumerator LoadChunk(ChunkData cd)
    {

        cd.Init(chunkPrefab);
        UnityEngine.Random.InitState(cd.randomState);
        Chunk = cd.chunk;
        Terrain = cd.terrain;
        Water = cd.water;
        TerrainMesh = cd.terrainMesh;
        WaterMesh = cd.waterMesh;
        Trees = cd.trees;
        xIndex = (int)(cd.coord.x);
        zIndex = (int)(cd.coord.y);
        xOffset = xIndex * ChunkSize;
        zOffset = zIndex * ChunkSize;


        yield return StartCoroutine(GenerateTerrainMaps());
        cd.TemperatureMap = TemperatureMap;
        cd.HumidityMap = HumidityMap;
        cd.ElevationMap = ElevationMap;
        cd.MountainMap = MountainMap;
        cd.BiomeMap = BiomeMap;
        cd.FreshWaterMap = FreshWaterMap;
        cd.WetnessMap = WetnessMap;
        cd.HeightMap = HeightMap;
        cd.TreeMap = TreeMap;

        PlaceTerrainAndWater();
        PlaceFeatures();

        yield return null;

    }

    IEnumerator DeloadChunks()
    {

        foreach (ChunkData loadedCd in ChunkDataLoaded.ToArray())
        {
            int index = ChunkDataToLoad.FindIndex(cd => cd.coord == loadedCd.coord);
            if (index < 0)
            {
                loadedCd.Deload();
                ChunkDataLoaded.Remove(loadedCd);
                yield return null;
            }
        }
        DeloadingChunks = false;

    }


    IEnumerator GenerateTerrainMaps()
    {

        TemperatureMap = new float[ChunkSize + 2, ChunkSize + 2];
        HumidityMap = new float[ChunkSize + 2, ChunkSize + 2];
        ElevationMap = new float[ChunkSize + 2, ChunkSize + 2];
        MountainMap = new float[ChunkSize + 2, ChunkSize + 2];
        BiomeMap = new int[ChunkSize + 2, ChunkSize + 2];
        FreshWaterMap = new float[ChunkSize + 2, ChunkSize + 2];
        WetnessMap = new float[ChunkSize + 2, ChunkSize + 2];
        HeightMap = new float[ChunkSize + 2, ChunkSize + 2];
        TreeMap = new bool[ChunkSize + 2, ChunkSize + 2];

        float temperatureValue, humidityValue, elevationValue, mountainValue, freshWaterValue, wetnessValue, heightValue;
        int biomeValue;
        bool treeValue;

        float tMod, rough;

        // loop start
        for (int z = 0; z < ChunkSize + 2; z++)
        {
            for (int x = 0; x < ChunkSize + 2; x++)
            {

                float e = Mathf.PerlinNoise((x + xOffset - Seed + .01f) / ElevationMapScale, (z + zOffset - Seed + .01f) / ElevationMapScale);
                //rough = Mathf.Pow(Mathf.PerlinNoise((x + xOffset + .01f) / 50f, (z + zOffset + .01f) / 50f) * 2f - 1f, .1f);
                //e += rough;
                elevationValue = Mathf.Pow(e + .5f, .5f) - 1f;
                float maxE = Mathf.Pow(1f + .5f, .5f) - 1f;
                float minE = Mathf.Pow(0f + .5f, .5f) - 1f;
                // -------------------------------------------------------

                // MountainMap [0, 1]
                mountainValue = Mathf.PerlinNoise((x + xOffset - Seed + .01f) / MountainMapScale + 5000f, (z + zOffset - Seed + .01f) / MountainMapScale + 5000f);
                mountainValue = Mathf.InverseLerp(.35f, .65f, mountainValue);
                //mountainValue = .99f;
                mountainValue *= .75f;
                mountainValue = Mathf.Pow(mountainValue, 2f);
                mountainValue = Mathf.InverseLerp(0f, Mathf.Pow(.75f, 2f), mountainValue);
                mountainValue *= Mathf.InverseLerp(minE, maxE+1f, elevationValue);

                // -------------------------------------------------------


                // TemperatureMap [0, 1]
                //temperatureValue = 1.2f - (e);
                temperatureValue = 1f - (e);
                rough = Mathf.Pow(Mathf.PerlinNoise((x + xOffset + .01f) / 50f, (z + zOffset + .01f) / 50f) + .5f, .1f) - 1f;
                temperatureValue += rough;

                float latitudeMod;
                latitudeMod = ((Mathf.PerlinNoise((x + xOffset + .01f) / TemperatureMapScale, (z + zOffset + .01f) / TemperatureMapScale) + .5f) - 1f) * 1.5f;

                temperatureValue += latitudeMod;
                temperatureValue = Mathf.Clamp01(temperatureValue);

                //temperatureValue = .3f;


                // -------------------------------------------------------

                // HumidityMap [0, 1]
                humidityValue = Mathf.PerlinNoise((x + xOffset - Seed + .01f) / HumidityMapScale, (z + zOffset - Seed + .01f) / HumidityMapScale);
                humidityValue += mountainValue * .5f;
                humidityValue = Mathf.Clamp01(humidityValue);
                humidityValue = Mathf.InverseLerp(0f, 1f, humidityValue);
                //humidityValue = .9f;
                // -------------------------------------------------------



                // FreshWaterMap [0, 1]
                float riverWindingScale = 180f;
                freshWaterValue = Mathf.PerlinNoise((x + xOffset - Seed + .01f) / riverWindingScale, (z + zOffset - Seed + .01f) / riverWindingScale) * 2f - 1f;
                rough = Mathf.PerlinNoise((x + xOffset + .01f) / 40f, (z + zOffset + .01f) / 40f);
                freshWaterValue *= Mathf.Pow(rough, .075f);
                //freshWaterValue -= Mathf.PerlinNoise((x + xOffset + .01f) / 400f, (z + zOffset + .01f) / 400f) / 2f;
                freshWaterValue = Mathf.Abs(freshWaterValue);
                freshWaterValue *= -1f;
                freshWaterValue += 1f;
                freshWaterValue = Mathf.Clamp01(freshWaterValue);
                //freshWaterValue = Mathf.Pow(freshWaterValue, .13f);
            
                //freshWaterValue *= Mathf.InverseLerp(.55f, .85f, 1f - mountainValue);
                freshWaterValue = 0f;


    
                // -------------------------------------------------------

                // WetnessMap [0, 1]
                wetnessValue = freshWaterValue;
                float fwThreshhold = 1f + Mathf.Pow(Mathf.PerlinNoise((x + xOffset + .01f) / 10f, (z + zOffset + .01f) / 10f) + .5f, 1f) - 1f;
                fwThreshhold = Mathf.Clamp01(fwThreshhold);
                if (freshWaterValue < fwThreshhold)
                {
                    if (elevationValue < .02f)
                    {
                        wetnessValue = Mathf.Max(wetnessValue, fwThreshhold);
                    }
                }
                float mtnMod = Mathf.Pow(mountainValue + .5f, 1f) - 1f;
                wetnessValue += mtnMod;

                wetnessValue = Mathf.Clamp01(wetnessValue);

                // -------------------------------------------------------

                // BiomeMap
                biomeValue = Biome.GetBiome(temperatureValue, humidityValue);

                // -------------------------------------------------------

                // HeightMap

                heightValue = 0f;
                float amplitude = 1;
                float frequency = 1;
                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = (x + xOffset) / scale * frequency + Seed;
                    float sampleZ = (z + zOffset) / scale * frequency + Seed;
                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleZ) * 2 - 1;
                    heightValue += perlinValue * amplitude;
                    amplitude *= persistance;
                    frequency *= lacunarity;
                }

                heightValue *= (40f * Mathf.PerlinNoise(((x + xOffset) / scale), ((z + zOffset) / scale)));

                //ABS and INVERT, and normalize value
                heightValue = Mathf.Abs(heightValue);
                heightValue *= -1f;
                heightValue = Mathf.InverseLerp(-75f, .01f, heightValue);


                // reduce hills
                if (heightValue < flatLevel)
                {
                    heightValue = flatLevel;
                }
                else
                {
                    float flat = (1f - mountainValue) * .99f;
                    heightValue = Mathf.Lerp(heightValue, flatLevel, flat);
                }

                // apply ElevationMap
                heightValue += elevationValue * .1f;


                // create ocean and rivers
                if (heightValue < flatLevel)
                {
                    float ocean = Mathf.InverseLerp(0f, .004f, flatLevel - heightValue);
                    freshWaterValue = ocean;
                }
                else
                {
                    heightValue = Mathf.Lerp(heightValue, seaLevel - .0001f, freshWaterValue);
                    //heightValue = Mathf.Lerp(heightValue, heightValue - .01f, freshWaterValue);
                }



                // posterize terrain

                float steps, stepHeight, h, thresh;
                steps = 2800f * (1f - (Mathf.Pow(mountainValue, 1f)));
                stepHeight = (1f - flatLevel) / steps;
                h = 0f;
                for (int i = 0; i < steps; i++)
                {
                    thresh = flatLevel + stepHeight * i;
                    thresh = Mathf.Max(thresh + stepHeight * 5f * (Mathf.PerlinNoise(((x + xOffset) / posterize_variationScale) + i * 10000, ((z + zOffset) / posterize_variationScale) + i * 10000) * 2f - 1f), flatLevel);
                    if (heightValue >= thresh)
                    {
                        h = thresh;
                    }
                    else
                    {
                        break;
                    }
                }
                float lerpRate = 1f;
                heightValue = Mathf.Lerp(heightValue, h, lerpRate);


                // create flatland
                float psgScale, psg;
                psgScale = 600f;
                psg = flatLevel + (.28f *  mountainValue * e * Mathf.PerlinNoise(((x + xOffset) / psgScale) + .01f, ((z + zOffset) / psgScale) + .01f));
                //psg = Mathf.Max(psg, e);
                if(heightValue < psg){
                    if(heightValue >= flatLevel){
                        float c = .003f * Mathf.Pow(mountainValue, .1f);
                        if(heightValue >= flatLevel + c){
                            heightValue = psg;
                        }else{
                            heightValue = Mathf.Lerp(heightValue, psg, (heightValue - flatLevel)/c);
                            //heightValue = Mathf.Lerp(heightValue, psg, .5f);
                        }
                    }
                }

                if((biomeValue == (int)Biome.BiomeType.SnowyTaiga || biomeValue == (int)Biome.BiomeType.Tundra) && heightValue > seaLevel){
                    heightValue += .0002f;
                }




                // create slight roughness in terrain
                if (biomeValue == (int)Biome.BiomeType.Desert)
                {
                    float duneMag = .0008f * (1f - Mathf.Pow(Mathf.Clamp01(freshWaterValue), 1.3f)) * (1f - Mathf.Pow(wetnessValue, 1.2f));
                    heightValue += duneMag * (1f - Mathf.Abs(Mathf.Sin((x + xOffset - Seed + .01f + Mathf.Sin(z+zOffset)*8f) / 15f)));
                }

                if(heightValue > snowLevel - .0005f){
                    heightValue = snowLevel;
                }

                // dip
                if(heightValue < seaLevel - .0001f){
                    heightValue = seaLevel - (.0005f);
                }


                // -------------------------------------------------------

                // TreeMap
                if (heightValue > seaLevel)
                {
                    treeValue = true;
                }
                else { treeValue = false; }

                // -------------------------------------------------------

                TemperatureMap[x, z] = temperatureValue;
                HumidityMap[x, z] = humidityValue;
                MountainMap[x, z] = mountainValue;
                ElevationMap[x, z] = elevationValue;
                BiomeMap[x, z] = biomeValue;
                FreshWaterMap[x, z] = freshWaterValue;
                WetnessMap[x, z] = wetnessValue;
                HeightMap[x, z] = heightValue;
                TreeMap[x, z] = treeValue;

            }

            yield return new WaitForSecondsRealtime(.00001f);
            
        }

        
    }

    Color SetVertexColor(int biome, float height, float mountain, float temperature, float humidity, float wetness, float fw)
    {

        
        Color c = new Color();
        

        // if(height >= snowLevel){
        //     c.b = 255f;
        //     return c;
        // }

    
        if(biome == (int)Biome.BiomeType.SnowyTaiga || biome == (int)Biome.BiomeType.Tundra){
            if(height > seaLevel){
                c.b = 255f;
                return c;
            }

        }

        // wetness (darkness of land)
        if(height > seaLevel){
            c.g = 230f * (1f - temperature);
        }
        
    
        // yellowness

        c.r = 230f * (1f - humidity);

        return c;
    }



    void PlaceTerrainAndWater()
    {

        // initialize properties for meshes
        TerrainVertices = new Vector3[(ChunkSize + 2) * (ChunkSize + 2)];
        TerrainTriangles = new int[(ChunkSize + 2) * (ChunkSize + 2) * 6];
        TerrainUvs = new Vector2[TerrainVertices.Length];
        TerrainColors = new Color[TerrainVertices.Length];
        WaterVertices = new Vector3[(ChunkSize + 2) * (ChunkSize + 2)];
        WaterTriangles = new int[(ChunkSize + 1) * (ChunkSize + 1) * 6];
        WaterUvs = new Vector2[TerrainVertices.Length];
        WaterColors = new Color[TerrainVertices.Length];


        // set terrain vertices according to HeightMap
        for (int i = 0, z = 0; z < ChunkSize + 2; z++)
        {
            for (int x = 0; x < ChunkSize + 2; x++)
            {
                TerrainVertices[i] = new Vector3(x + xOffset, HeightMap[x, z] * ElevationAmplitude, z + zOffset);
                TerrainColors[i] = SetVertexColor(BiomeMap[x, z], HeightMap[x, z], MountainMap[x, z], TemperatureMap[x, z], HumidityMap[x, z], WetnessMap[x, z], FreshWaterMap[x, z]);
                WaterVertices[i] = new Vector3(x + xOffset, seaLevel * ElevationAmplitude, z + zOffset);
                Color waterColor = new Color();
                waterColor.a = waterAlpha;
                WaterColors[i] = waterColor;
                i++;
            }
        }


        // set up triangles
        int vert = 0;
        int tris = 0;
        for (int z = 0; z < ChunkSize + 1; z++)
        {
            for (int x = 0; x < ChunkSize + 1; x++)
            {
                TerrainTriangles[tris + 0] = vert + 0;
                TerrainTriangles[tris + 1] = vert + ChunkSize + 2;
                TerrainTriangles[tris + 2] = vert + 1;
                TerrainTriangles[tris + 3] = vert + 1;
                TerrainTriangles[tris + 4] = vert + ChunkSize + 2;
                TerrainTriangles[tris + 5] = vert + ChunkSize + 3;
                WaterTriangles[tris + 0] = vert + 0;
                WaterTriangles[tris + 1] = vert + ChunkSize + 2;
                WaterTriangles[tris + 2] = vert + 1;
                WaterTriangles[tris + 3] = vert + 1;
                WaterTriangles[tris + 4] = vert + ChunkSize + 2;
                WaterTriangles[tris + 5] = vert + ChunkSize + 3;

                vert++;
                tris += 6;
            }
            vert++;
        }

        for (int i = 0, z = 0; z < ChunkSize + 1; z++)
        {
            for (int x = 0; x < ChunkSize + 1; x++)
            {
                TerrainUvs[i] = new Vector2((float)x + xOffset, (float)z + zOffset);
                WaterUvs[i] = TerrainUvs[i];
                i++;
            }
        }

        // update meshes
        TerrainMesh.Clear();
        TerrainMesh.vertices = TerrainVertices;
        TerrainMesh.triangles = TerrainTriangles;
        TerrainMesh.uv = TerrainUvs;
        TerrainMesh.colors = TerrainColors;
        TerrainMesh.RecalculateNormals();
        WaterMesh.Clear();
        WaterMesh.vertices = WaterVertices;
        WaterMesh.triangles = WaterTriangles;
        WaterMesh.uv = WaterUvs;
        WaterMesh.colors = WaterColors;
        WaterMesh.RecalculateNormals();

        MeshCollider mc = Terrain.AddComponent<MeshCollider>();
        BoxCollider bc = Water.AddComponent<BoxCollider>();
        mc.sharedMaterial = physicMaterial;
        bc.isTrigger = true;

    }


    void PlaceFeatures()
    {
        int biome;
        float wetness;
        float temperature;
        float height;
        float fw;

        for (int z = 0; z < ChunkSize + 2; z += 10)
        {
            for (int x = 0; x < ChunkSize + 2; x += 10)
            {

                biome = BiomeMap[x, z];
                wetness = WetnessMap[x, z];
                temperature = TemperatureMap[x, z];
                height = HeightMap[x, z];
                fw = FreshWaterMap[x, z];
                bool onWater;

            
                if (TreeMap[x, z])
                {
                    if(seaLevel >= height-.00025f){
                        onWater = seaLevel - height <= .0018f;
                    }
                    else{ onWater = false; }

                    // tree placement
                    var treeTuple = Biome.GetTree(biome, wetness, fw);
                    if (treeTuple != null && treeTuple.Item2.Item2 > 0f)
                    {
                        PlaceTreeBundle(treeTuple, wetness, onWater, x, z);
                    }

                    // feature placement
                    var featureTuple = Biome.GetFeature(biome, wetness, fw, onWater);
                    if(featureTuple != null)
                    {
                        PlaceTreeBundle(featureTuple, wetness, onWater, x, z);
                    }
                }
            }
        }
    }

    void PlaceTreeBundle(Tuple<GameObject, Tuple<float, float, float, float, float>> treeTuple, float wetness, bool onWater, int x, int z)
    {
        GameObject tree = treeTuple.Item1;
        float treeScale = treeTuple.Item2.Item1;
        int densityMultipler = (int)(treeTuple.Item2.Item2 * 10f);
        float treeMinYNormal = treeTuple.Item2.Item3;
        float treeAngleMultiplier = treeTuple.Item2.Item4;
        float spreadMultiplier = treeTuple.Item2.Item5;
        Quaternion uprightRot;
        Quaternion slantedRot;
        Vector3 castVec, point;
        float castLength;

        int passes = (int)(wetness * densityMultipler * treeDensity);
        passes = Mathf.Clamp(passes, (int)(UnityEngine.Random.value + .25f), passes);
        if (passes > 0)
        {
            for(int p = 0; p < passes; p++)
            {
                castVec = new Vector3(x + xOffset + (UnityEngine.Random.value * 2f - 1f) * spreadMultiplier * 10, ElevationAmplitude, z + zOffset + (UnityEngine.Random.value * 2f - 1f) * spreadMultiplier * 10);
                
                if(onWater){ castLength = ElevationAmplitude - ((seaLevel - .003f)* ElevationAmplitude); }
                else{ castLength = ElevationAmplitude - ((seaLevel + .002f)* ElevationAmplitude); }
                if (Physics.Raycast(castVec, Vector3.down, out RaycastHit hit, castLength, layerMask_terrain))
                {

                    point = hit.point;
                    float minY, maxY;
                    float seaY = seaLevel * ElevationAmplitude;
                    if(onWater){
                        minY = seaY - .1f;
                        maxY = seaY + .1f;
                    }
                    else{
                        minY = seaY + .02f;
                        maxY = float.MaxValue;
                    }
                    //if (hit.normal.y > treeMinYNormal)
                    if (point.y >= minY && point.y <= maxY && hit.normal.y > .8f)
                    {
                        uprightRot = Quaternion.AngleAxis(UnityEngine.Random.value * 360f, Vector3.up);
                        slantedRot = Quaternion.FromToRotation(transform.up, hit.normal);
                        tree = GameObject.Instantiate(tree, point, Quaternion.Slerp(uprightRot, slantedRot, treeAngleMultiplier), Trees.transform);
                        tree.transform.localScale = Vector3.one * treeScale * Mathf.Pow(UnityEngine.Random.value + .5f, .75f);
                    }
                }
            }
        }
    }


    // returns given position translated to chunk coordinates, based on chunkSize
    public static Vector2 ToChunkSpace(Vector3 position)
    {
        return new Vector2(position.x / (ChunkSize), position.z / (ChunkSize));
    }

    // retrieve ChunkData in ChunkDataLoaded associated with the chunk coordinate
    public static ChunkData GetChunk(Vector2 chunkCoord)
    {
        //Debug.Log(chunkCoord);
        foreach (ChunkData cd in ChunkDataLoaded.ToArray())
        {
            if (cd.coord == chunkCoord)
            {
                return cd;
            }
        }
        Debug.Log("ChunkGenerator: chunk from given position is not loaded!");
        return null;
    }

    // retrieve ChunkData in ChunkDataLoaded associated with the raw position given
    public static ChunkData GetChunk(Vector3 position)
    {
        Vector2 position_chunkSpace = ToChunkSpace(position);
        Vector2 chunkCoord = new Vector2((int)position_chunkSpace.x, (int)(position_chunkSpace.y));
        return GetChunk(chunkCoord);
    }

    public float Posterize(float value, float maxValue, int sections)
    {
        float ret = -1;

        float sectionSize = maxValue / sections;
        for (int i = 0; i < sections; i++)
        {
            ret = sectionSize * (i + 1);
            if (value <= ret)
            {
                return ret;
            }
        }




        return ret;
    }

}