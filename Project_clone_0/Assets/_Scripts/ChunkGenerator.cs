using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class ChunkGenerator : MonoBehaviour
{



    public static ChunkGenerator current;
    public static int Seed = 455;
    public static int ChunkSize = 30;
    public static int ChunkRenderDistance = 15;
    public static float Scale = 1200f * 2.5f;
    public static float ElevationAmplitude = 5400f;
    public static float MinElevation = -.292893219f;
    public static float MaxElevation = .224744871f;
    public static int MountainMapScale = 800;
    public static float ElevationMapScale = 2000;
    public static int TemperatureMapScale = 800;
    public static int HumidityMapScale = 800;
    public static float MountainPolarity = 1f;
    public static float FlatLevel = .85f;
    public static float SeaLevel = 0.849985f;
    public static float BankLevel = SeaLevel + .0002f;
    public static float WaterFeatureLevel = .85f;
    public static float SnowLevel = .86f;
    public static float grassNormal = .87f;
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
    [SerializeField] Material terrainMaterial;
    [SerializeField] Material grassMaterial;


    [SerializeField] PhysicMaterial physicMaterial;
    public LayerMask layerMask_terrain;

    public GameObject chunkPrefab;
    public GameObject waterPrefab;

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

    public int octaves;
    [Range(0, 1)] public float persistance;
    public float lacunarity;
    [Range(0, 1)] public float RockProtrusion;



    // feature
    GameObject Trees;
    [Range(0f, 1f)] public float treeDensity;
    [Range(0f, 1f)] public float treeScale;



    static bool placing;
    static List<FeatureLocation> featureLocations;
    struct FeatureLocation{
        public GameObject featurePrefab;
        public Vector3 position;
        public Vector3 scale;
        public FeatureLocation(GameObject _featurePrefab, Vector3 _position, Vector3 _scale){
            featurePrefab = _featurePrefab;
            position = _position;
            scale = _scale;
        }
    }



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

            UpdateWaterPosition();

            if(!placing){
                placing = true;
                StartCoroutine(PlaceFeatures());
            }
    
        }

    }

    void Init()
    {
        Seed = UnityEngine.Random.Range(0, 100000);
        Seed = 1;
        if (Seed == -1) { Seed = UnityEngine.Random.Range(-100000, 100000); }
        Debug.Log("seed: " + Seed.ToString());

        //RiverGenerator.Generate();

        ChunkDataToLoad = new List<ChunkData>();
        ChunkDataLoaded = new List<ChunkData>();

        TerrainMesh = new Mesh();
        TerrainMeshFilter.mesh = TerrainMesh;
        currentChunkCoord = Vector2.positiveInfinity;

        Water = Instantiate(waterPrefab);

        layerMask_terrain = LayerMask.GetMask("Terrain");


        // set grass material parameters
        terrainMaterial.SetFloat("_WaterHeight", SeaLevel * ElevationAmplitude + .5f);
        terrainMaterial.SetFloat("_SnowHeight", SnowLevel * ElevationAmplitude + .5f);
        grassMaterial.SetFloat("_WaterHeight", SeaLevel * ElevationAmplitude + .5f);
        grassMaterial.SetFloat("_GrassNormal", .8f);

        featureLocations = new List<FeatureLocation>();

        Debug.Log("starting coroutine...");
    
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
        TerrainMesh = cd.terrainMesh;
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

        float temperatureValue, humidityValue, elevationValue, mountainValue, freshWaterValue, wetnessValue, heightValue, heightValue_water;
        int biomeValue;
        bool treeValue;
        float mtnCap;

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
                mountainValue = Mathf.InverseLerp(.5f - .15f * MountainPolarity, .5f +  .15f * MountainPolarity, mountainValue);
                //mountainValue = .99f;
                mountainValue *= .75f;
                mountainValue = Mathf.Pow(mountainValue, 2f);
                mountainValue = Mathf.InverseLerp(0f, Mathf.Pow(.75f, 2f), mountainValue);
                mountainValue *= Mathf.InverseLerp(minE, maxE+1f, elevationValue);
                mountainValue *= .5f;
                mtnCap = .3f;
                if(mountainValue > mtnCap){
                    mountainValue = mtnCap;
                }
                //Debug.Log(mountainValue);

                // -------------------------------------------------------


                // TemperatureMap [0, 1]

                temperatureValue = 1.5f * Mathf.PerlinNoise((x + xOffset + .001f) / TemperatureMapScale, (z + zOffset + .001f) / TemperatureMapScale);
                temperatureValue -= 1f * (mountainValue / mtnCap);
                // temperatureValue +=  (Mathf.PerlinNoise((x + xOffset + .001f) / TemperatureMapScale, (z + zOffset + .001f) / TemperatureMapScale) * 2f - 1f) * (3f * (1f - mountainValue/mtnCap));

                //temperatureValue = Mathf.InverseLerp(.4f, .6f, temperatureValue);
                temperatureValue = Mathf.Clamp01(temperatureValue);

                //temperatureValue = .01f;



                // -------------------------------------------------------

                // HumidityMap [0, 1]
                humidityValue = Mathf.PerlinNoise((x + xOffset - Seed + .01f) / HumidityMapScale, (z + zOffset - Seed + .01f) / HumidityMapScale);
                humidityValue += mountainValue * .5f;
                humidityValue = Mathf.Clamp01(humidityValue);
                humidityValue = Mathf.InverseLerp(.3f, .6f, humidityValue);
                //humidityValue = .9f;
                // -------------------------------------------------------



                // FreshWaterMap [0, 1]
                float riverScale = 180f;
                freshWaterValue = Mathf.PerlinNoise((x + xOffset - Seed + .01f) / riverScale, (z + zOffset - Seed + .01f) / riverScale) * 2f - 1f;
                float rough = Mathf.PerlinNoise((x + xOffset + .01f) / 40f, (z + zOffset + .01f) / 40f);
                freshWaterValue *= Mathf.Pow(rough, 1f);
                rough = Mathf.PerlinNoise((x + xOffset + .01f) / 1f, (z + zOffset + .01f) / 1f) * 2f - 1f;
                freshWaterValue += rough * .1f;
                freshWaterValue -= Mathf.PerlinNoise((x + xOffset + .01f) / 400f, (z + zOffset + .01f) / 400f) / 2f;
                freshWaterValue = Mathf.Abs(freshWaterValue);
                freshWaterValue *= -1f;
                freshWaterValue += 1f;
                freshWaterValue = Mathf.Clamp01(freshWaterValue);


                float thresh1 = .7f + .1f * (Mathf.PerlinNoise((x + xOffset + Seed + .01f) / 20f, (z + zOffset + Seed + .01f) / 20f) * 2f - 1f);
                float thresh2 = .85f + .1f * (Mathf.PerlinNoise((x + xOffset - Seed + .01f) / 20f, (z + zOffset - Seed + .01f) / 20f) * 2f - 1f);
                if (freshWaterValue > thresh1)
                {
                    float emod = Mathf.PerlinNoise((x + xOffset + Seed + .01f) / riverScale, (z + zOffset + Seed + .01f) / riverScale) * 2f - 1f;
                    if (freshWaterValue > thresh2)
                    {
                        float midpt = (thresh2 + 1f) / 2f;
                        float fac = 1f - Mathf.Pow(Mathf.Abs(midpt - freshWaterValue) / (Mathf.Abs(1f - thresh2) / 2f), 2f);
                        freshWaterValue = Mathf.Lerp(freshWaterValue, 1f, emod * 2f * fac);
                    }
                    else
                    {
                        float midpt = (thresh1 + thresh2) / 2f;
                        float fac = 1f - Mathf.Pow(Mathf.Abs(midpt - freshWaterValue) / (Mathf.Abs(thresh1 - thresh2) / 2f), 2f);
                        freshWaterValue = Mathf.Lerp(freshWaterValue, thresh2, emod * 2f * fac);
                    }
                }
                else
                {
                    freshWaterValue = Mathf.InverseLerp(0f, Mathf.Pow(thresh1, 3f), Mathf.Pow(freshWaterValue, 3f)) * thresh1;
                    //freshWaterValue = 0f;
                }

                freshWaterValue = Mathf.Pow(freshWaterValue, .9f);
                freshWaterValue *= Mathf.Pow(1f - Mathf.InverseLerp(.25f, .5f, mountainValue / mtnCap), 1f);      
                //freshWaterValue = 0f;





                // -------------------------------------------------------

                // WetnessMap [0, 1]
                // wetnessValue = freshWaterValue;
                // float fwThreshhold = 1f + Mathf.Pow(Mathf.PerlinNoise((x + xOffset + .01f) / 10f, (z + zOffset + .01f) / 10f) + .5f, 1f) - 1f;
                // fwThreshhold = Mathf.Clamp01(fwThreshhold);
                // if (freshWaterValue < fwThreshhold)
                // {
                //     if (elevationValue < .02f)
                //     {
                //         wetnessValue = Mathf.Max(wetnessValue, fwThreshhold);
                //     }
                // }
                // // float mtnMod = Mathf.Pow((mountainValue / mtnCap) + .5f, 1f) - 1f;
                // // wetnessValue += mtnMod;

                //wetnessValue = Mathf.Clamp01(wetnessValue);

                wetnessValue = 1f;

                // -------------------------------------------------------

                // HeightMap

                heightValue = 0f;
                float amplitude = 1;
                float frequency = 1;
                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = (x + xOffset) / Scale * frequency + Seed;
                    float sampleZ = (z + zOffset) / Scale * frequency + Seed;
                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleZ) * 2 - 1;
                    heightValue += perlinValue * amplitude;
                    amplitude *= persistance;
                    frequency *= lacunarity;
                }

                heightValue *= (40f * Mathf.PerlinNoise(((x + xOffset) / Scale), ((z + zOffset) / Scale)));

                //ABS and INVERT, and normalize value
                heightValue = Mathf.Abs(heightValue);
                heightValue *= -1f;
                heightValue = Mathf.InverseLerp(-75f, .01f, heightValue);


                // apply MountainMap
                if (heightValue < FlatLevel)
                {
                    heightValue = FlatLevel;
                }
                else
                {
                    float flat = (1f - mountainValue) * .99f;
                    heightValue = Mathf.Lerp(heightValue, FlatLevel, flat);
                }

                // apply ElevationMap
                heightValue += elevationValue * .1f;


                // create ocean and rivers
                if (heightValue < FlatLevel)
                {
                    heightValue_water = SeaLevel;
                    float ocean = Mathf.InverseLerp(0f, .004f, FlatLevel - heightValue);
                    freshWaterValue = ocean;
                }
                else
                {
                    heightValue_water = Mathf.Max(SeaLevel, Mathf.Lerp(SeaLevel, heightValue - .0001f, .5f));
                    heightValue = Mathf.Lerp(heightValue, SeaLevel - .0001f, freshWaterValue);
                    //heightValue = Mathf.Lerp(heightValue, heightValue - .01f, freshWaterValue);
                }


                // -------------------------------------------------------


                // create flatland
                float psgScale, psgNoise, psgSteps, psgStepHeight, psg, oldPsg;
                psgScale = 200f;
                psgSteps = 40f;
                psgStepHeight = (1f - FlatLevel) / psgSteps;
                
        

                oldPsg = FlatLevel;
                for(int i = 0; i < psgSteps; i++){
                    if(i == 0){
                        psgNoise = 0f;
                        psg = FlatLevel + .005f * Mathf.PerlinNoise((x + xOffset) / psgScale*2f, (z + zOffset) / psgScale*2f);
                    }
                    else{
                        //psgNoise = .007f * (Mathf.PerlinNoise((x + xOffset) / psgScale + (i * 1000f), (z + zOffset) / psgScale + (i * 1000f)) * 2f - 1f);
                        psgNoise = .0025f * (Mathf.PerlinNoise((x + xOffset) / psgScale + (i * 1000f), (z + zOffset) / psgScale + (i * 1000f)) * 2f - 1f);
                        psg = oldPsg + psgStepHeight + psgNoise;
                    }
                    if(psg >= 1f - psgStepHeight){
                        break;
                    }
                    if (heightValue < psg){
                        if (heightValue >= oldPsg){
                            //float c = .003f * (Mathf.Pow(mountainValue, .01f) - .1f);
                            float c = .003f;
                            if (heightValue >= oldPsg + c){
                                heightValue = psg;
                            }
                            else{
                                heightValue = Mathf.Lerp(heightValue, psg, ((heightValue - oldPsg) / c) * 1f);
                            }
                        }
                    }
                    oldPsg = psg - psgNoise;
                }


                // badland effect in deserts
                if(heightValue > SeaLevel + .0001f){
                    float postHeight = Posterize(SeaLevel + .0001f, 1f, heightValue, 100, .5f);
                    float badland = Mathf.InverseLerp(.6f, .9f, temperatureValue);
                    heightValue = Mathf.Lerp(heightValue, postHeight, badland);
                }

                // posterize all land
                float postNes = .75f;
                heightValue = Posterize(SeaLevel - .0001f, 1f, heightValue, 350, .9f + postNes);
                heightValue = Posterize(SeaLevel - .0001f, 1f, heightValue, 750, 0f + postNes);


                // dip
                if(heightValue < SeaLevel - .0001f){
                    heightValue = SeaLevel - (.0005f);
                }

                // TreeMap
                if (heightValue > FlatLevel)
                {
                    treeValue = true;
                }
                else { treeValue = false; }


                // -------------------------------------------------------

                // BiomeMap
                biomeValue = Biome.GetBiome(temperatureValue, humidityValue, heightValue);


                // -------------------------------------------------------

                TemperatureMap[x, z] = temperatureValue;
                HumidityMap[x, z] = humidityValue;
                MountainMap[x, z] = mountainValue / mtnCap;
                ElevationMap[x, z] = elevationValue;
                BiomeMap[x, z] = biomeValue;
                FreshWaterMap[x, z] = freshWaterValue;
                WetnessMap[x, z] = wetnessValue;
                HeightMap[x, z] = heightValue;
                TreeMap[x, z] = treeValue;

            }

            yield return new WaitForSecondsRealtime(.0000001f);
            
        }

        
    }

    float Posterize(float min, float max, float val, int steps, float softness)
    {
        float stepHeight = (max - min) / steps;

        float level = min;
        while (level < max)
        {
            level += stepHeight;
            if (level >= val)
            {
                float nextLevel = level;
                level -= stepHeight;
                float midpt = (level + nextLevel) / 2f;
                float compliance = 1f - (Mathf.Pow(Mathf.Abs(midpt - val) / (stepHeight / 2f), Mathf.Lerp(0f, 20f, 1f - softness)));
                val = Mathf.Lerp(val, level, compliance);


                return val;
            }
        }
        return -1f;

    }

    Color SetVertexColor(int x, int z, int biome, float height, float mountain, float temperature, float humidity, float wetness, float fw, float rockiness)
    {

        Color c = new Color();
        float snowHeight = Biome.GetSnowHeight(SnowLevel, temperature);
        float snow = Mathf.InverseLerp(snowHeight - .08f, snowHeight, height);
        c.b = Mathf.Max(0, snow) * 255f;
        //c.b = 0f;
        //c.b = 255f;

    
        // desertness (c.r)
        c.r = Mathf.Max(255f * temperature);

        return c;

        
    }



    public static void SetFeaturePlots(int biome, float wetness, float x, float y, float z){

        float randomOffsetDivisor;
        Vector3 randomOffsetPosition;
        FeatureAttributes featureAttributes;

        foreach(GameObject feature in Biome.TreePool[biome]){
            featureAttributes = FeatureAttributes.GetFeatureAttributes(feature.name, wetness);
            randomOffsetDivisor = 15f * (Mathf.PerlinNoise((x + .01f) / 2f, (z + .01f) / 2f) * 2f - 1f);
            randomOffsetPosition = (Vector3.right * (UnityEngine.Random.value * 2f - 1f)) + (Vector3.forward * (UnityEngine.Random.value * 2f - 1f));
            int divisor = (int)(Mathf.Lerp(1f, 20f, 1f - featureAttributes.density) + randomOffsetDivisor);
            if(divisor < 1){ divisor = 1; }
            if(x % divisor == 0 && z % divisor == 0){

                Vector3 featurePosition = new Vector3(x, y, z) + randomOffsetPosition;
                Vector3 featureScale = Vector3.one * featureAttributes.scale * ChunkGenerator.current.treeScale;
                featureLocations.Add(new FeatureLocation(feature, featurePosition, featureScale));

                // GameObject o = GameObject.Instantiate(feat, new Vector3(x, y, z) + randomOffsetPosition, Quaternion.identity, current.Trees.transform);
                // o.transform.localScale = Vector3.one * featureAttributes.scale * ChunkGenerator.current.treeScale;
            }
        }
        foreach(GameObject feature in Biome.FeaturePool[biome]){
            featureAttributes = FeatureAttributes.GetFeatureAttributes(feature.name, wetness);
            randomOffsetDivisor = 5f * (Mathf.PerlinNoise((x + .01f) / 2f, (z + .01f) / 2f) * 2f - 1f);
            randomOffsetPosition = (Vector3.right * (UnityEngine.Random.value * 2f - 1f)) + (Vector3.forward * (UnityEngine.Random.value * 2f - 1f));
            int divisor = (int)(Mathf.Lerp(1f, 20f, 1f - featureAttributes.density) + randomOffsetDivisor);
            if(divisor < 1){ divisor = 1; }
            if(x % divisor == 0 && z % divisor == 0){
                
                Vector3 featurePosition = new Vector3(x, y, z) + randomOffsetPosition;
                Vector3 featureScale = Vector3.one * featureAttributes.scale * ChunkGenerator.current.treeScale;
                featureLocations.Add(new FeatureLocation(feature, featurePosition, featureScale));

                // GameObject o = GameObject.Instantiate(feat, new Vector3(x, y, z) + randomOffsetPosition, Quaternion.identity, current.Trees.transform);
                // o.transform.localScale = Vector3.one * featureAttributes.scale * ChunkGenerator.current.treeScale;
            }
        }
        
    }


    void PlaceTerrainAndWater()
    {

        // initialize properties for meshes
        TerrainVertices = new Vector3[(ChunkSize + 2) * (ChunkSize + 2)];
        TerrainTriangles = new int[(ChunkSize + 2) * (ChunkSize + 2) * 6];
        TerrainUvs = new Vector2[TerrainVertices.Length];
        TerrainColors = new Color[TerrainVertices.Length];


        // set terrain vertices according to HeightMap
        float height, rockiness, skewHoriz;
        for (int i = 0, z = 0; z < ChunkSize + 2; z++)
        {
            for (int x = 0; x < ChunkSize + 2; x++)
            {


                height = HeightMap[x, z] * ElevationAmplitude;
                rockiness = Mathf.Pow(Mathf.PerlinNoise((x + xOffset) / 30f, (z + zOffset) / 30f) * Mathf.PerlinNoise(height / 50f, 0f), 2f);
                skewHoriz = (rockiness * 2f - 1f) * 36f * RockProtrusion;
               
                TerrainVertices[i] = new Vector3(x + xOffset + skewHoriz, height, z + zOffset + skewHoriz);


                // TerrainVertices[i] = new Vector3(x + xOffset, HeightMap[x, z] * ElevationAmplitude, z + zOffset);
                TerrainColors[i] = SetVertexColor(x + xOffset, z + zOffset, BiomeMap[x, z], HeightMap[x, z], MountainMap[x, z], TemperatureMap[x, z], HumidityMap[x, z], WetnessMap[x, z], FreshWaterMap[x, z], rockiness);
                
                // place features
                if(TreeMap[x, z] && z > 0 && x > 0){
                    SetFeaturePlots(BiomeMap[x, z], WetnessMap[x, z], x + xOffset, height, z + zOffset);
                }

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

                vert++;
                tris += 6;
            }
            vert++;
        }

        for (int i = 0, z = 0; z < ChunkSize + 2; z++)
        {
            for (int x = 0; x < ChunkSize + 2; x++)
            {
                TerrainUvs[i] = new Vector2((float)x + xOffset, (float)z + zOffset);
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
    
        MeshCollider mc = Terrain.AddComponent<MeshCollider>();
        mc.sharedMaterial = physicMaterial;

    }


    void UpdateWaterPosition(){
        Vector3 pos = playerT.position;
        pos.y = SeaLevel * ElevationAmplitude;
        Water.transform.position = pos;
    }


    IEnumerator PlaceFeatures(){
        //Debug.Log("checking feature placement...");
        while(true){


            featureLocations = featureLocations.OrderBy(c => Vector3.Distance(playerT.position, c.position)).ToList();


            //Debug.Log("featureLocs length: " + featureLocations.Count);
            if(featureLocations.Any()){

               // Debug.Log("placing...");

                FeatureLocation featureLocation = featureLocations[0];
                Vector3 featurePos = featureLocation.position;
                Vector3 featureScale = featureLocation.scale;

                Vector2 chunkCoords = ToChunkSpace(featurePos);
                Debug.Log(chunkCoords);
                float x = featurePos.x / (chunkCoords.x);
                float z = featurePos.z / (chunkCoords.y);
                //Debug.Log("x: " + x);
                //Debug.Log("z: " + z);
                
                ChunkData cd = GetChunk(featurePos);
                if (cd != null)
                {
                    if(true)
                    //if (Mathf.Abs(featurePos.y - cd.HeightMap[(int)x, (int)z] * ElevationAmplitude) < .2f)
                    {
                        GameObject o = GameObject.Instantiate(featureLocation.featurePrefab, featurePos, Quaternion.identity, cd.trees.transform);
                        o.transform.localScale = featureScale;
                    }
                }

                featureLocations.Remove(featureLocation);


            }

            //Debug.Log("...............");
            yield return new WaitForSeconds(.01f);

        }
        
    }


    // returns given position translated to chunk coordinates, based on chunkSize
    public Vector2 ToChunkSpace(Vector3 position)
    {
        return new Vector2(position.x / (ChunkSize), position.z / (ChunkSize));
    }

    // retrieve ChunkData in ChunkDataLoaded associated with the chunk coordinate
    public ChunkData GetChunk(Vector2 chunkCoord)
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
    public ChunkData GetChunk(Vector3 position)
    {
        Vector2 position_chunkSpace = ToChunkSpace(position);
        Vector2 chunkCoord = new Vector2((int)position_chunkSpace.x, (int)(position_chunkSpace.y));
        return GetChunk(chunkCoord);
    }


}