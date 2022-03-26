using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEngine.Pool;

public class ChunkGenerator : MonoBehaviour
{
    public static ChunkGenerator instance;
    public static int Seed = 75675;
    public static int ChunkSize = 30;
    public static int ChunkRenderDistance = 3;
    public static float Scale = 250f * 1f;
    public static float ElevationAmplitude = 300f * 1f;
    public static float MountainMapScale = 312f * 2f;
    public static float ElevationMapScale = 1000f;
    public static int TemperatureMapScale = 300;
    public static int HumidityMapScale = 300;
    public static float meter = 1f / ElevationAmplitude;
    public static float FlatLevel = .85f;
    public static float SeaLevel = .8495f;
    public static float SnowLevel = .91f;
    //public static float SnowLevel = float.MaxValue;
    public static float GrassNormal = .99f;
    public static float SnowNormalMin = .8f;
    public static float SnowNormalMax = 1f;
    public static float CaveNormal = .4f;
    public static bool LoadingChunks, DeloadingChunks;
    public static GameObject Chunk;
    public static GameObject Terrain;
    public static GameObject Water;

    public Transform playerTransform;
    public Vector3 playerRawPosition;
    public Vector2 playerChunkCoordinate;


    [SerializeField] MeshFilter TerrainMeshFilter;
    [SerializeField] MeshFilter WaterMeshFilter;
    Mesh TerrainMesh;
    Mesh WaterMesh;


    [SerializeField] PhysicMaterial physicMaterial;

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
    [SerializeField] float waterAlpha;

    float[,] TemperatureMap;
    float[,] HumidityMap;
    float[,] ElevationMap;
    float[,] MountainMap;
    float[,] FreshWaterMap;
    float[,] WetnessMap;
    float[,] HeightMap;
    bool[,] TreeMap;

    public int octaves;
    [Range(0, 1)] public float persistance;
    public float lacunarity;
    [Range(0, 1)] public float RockProtrusion;



    // features and creatures
    Transform FeaturesParent;
    public static List<GameObject> activeCPUCreatures;
    public static List<GameObject> Features, Creatures, Humans, Items;
    public static bool humanSpawned = false;
    public static readonly float cpuCreatureDespawnTimestep = 5f;
    public static float cpuCreatureDespawnTime = 0f;


    // fill map
    public FillMap fillMap;
    



    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
        Init();

    }

    private void Update()
    {
        if (playerTransform != null)
        {
            if(!LoadingChunks && !DeloadingChunks){
                LoadingChunks = true;
                DeloadingChunks = true;
                StartCoroutine(CallForSpawnGeneration());
                UpdateChunksToLoad();
                StartCoroutine(LoadChunks());
                StartCoroutine(DeloadChunks());
            }

            cpuCreatureDespawnTime += Time.deltaTime;
            if(cpuCreatureDespawnTime >= cpuCreatureDespawnTimestep){
                DespawnCPUCreatures();
                cpuCreatureDespawnTime = 0f;
            }

            UpdateWaterPosition();

        }
    }

    void Init()
    {

        //Debug.Log("seed: " + Seed.ToString());

        //RiverGenerator.Generate();

        ChunkDataToLoad = new List<ChunkData>();
        ChunkDataLoaded = new List<ChunkData>();
        TerrainMesh = new Mesh();
        TerrainMeshFilter.mesh = TerrainMesh;

        Water = Instantiate(waterPrefab);


        Features = new List<GameObject>(Resources.LoadAll<GameObject>("Terrain/Features"));
        Creatures = new List<GameObject>(Resources.LoadAll<GameObject>("Terrain/Creatures"));
        Humans = new List<GameObject>(Resources.LoadAll<GameObject>("Terrain/Humans"));
        Items = Item.Items.Values.Select(item => item.worldObjectPrefab).Where(o => o != null).ToList();
        Features = Features.OrderBy(feature => SpawnParameters.GetSpawnParameters(feature.name).loadOrder).ToList();

        activeCPUCreatures = new List<GameObject>();

        fillMap = new FillMap();

    
    }


    public void SetPlayerTransform(Transform t)
    {
        Debug.Log("Setting playerTransform: " + t);
        playerTransform = t;
    }


    IEnumerator CallForSpawnGeneration()
    {
        foreach (ChunkData loadedCd in ChunkDataLoaded.ToArray())
        {
            if (!loadedCd.spawnsPlaced)
            {
                if (!loadedCd.IsEdgeChunk())
                {
                    loadedCd.spawnsPlaced = true;
                    yield return StartCoroutine(GenerateSpawns(loadedCd));
                }
            }
        }
    }


    void UpdateChunksToLoad()
    {


        playerRawPosition = playerTransform.position;
        playerChunkCoordinate = GetChunkCoordinate(playerRawPosition);
        Vector2 currentChunkCoord = new Vector2((int)playerChunkCoordinate.x, (int)playerChunkCoordinate.y);


        // get neighbor chunk coordinates
        Vector2 halfVec = Vector3.one / 2f;
        List<Vector2> neighborChunkCoords = new List<Vector2>();
        int i = 0;
        for (int z = (int)(currentChunkCoord.y - ChunkRenderDistance); z < (int)(currentChunkCoord.y + ChunkRenderDistance); ++z)
        {
            for (int x = (int)(currentChunkCoord.x - ChunkRenderDistance); x < (int)(currentChunkCoord.x + ChunkRenderDistance); ++x)
            {
                neighborChunkCoords.Add(new Vector2(x, z));
                ++i;
            }
        }


        // remove chunks out of rendering range from ChunksToLoad
        foreach (ChunkData cd in ChunkDataToLoad.ToArray())
        {
            if (Vector2.Distance(playerChunkCoordinate, cd.coordinate + halfVec) >= ChunkRenderDistance)
            {
                ChunkDataToLoad.Remove(cd);
            }
        }

        // add chunks in rendering range to ChunksToLoad
        foreach (Vector2 chunkCoord in neighborChunkCoords)
        {
            if (Vector2.Distance(playerChunkCoordinate, chunkCoord + halfVec) < ChunkRenderDistance)
            {
                int index = ChunkDataToLoad.FindIndex(cd => cd.coordinate.Equals(chunkCoord));
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
        foreach (ChunkData cd in ChunkDataToLoad.OrderBy(c => Vector3.Distance(GetChunkCoordinate(playerTransform.position), c.coordinate)).ToArray())
        {
            if (!cd.loaded)
            {
                load = LoadChunk(cd);
                yield return StartCoroutine(load);
                ChunkDataLoaded.Add(cd);
            }
            if(GetChunkCoordinate(playerTransform.position) != playerChunkCoordinate){
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
        FeaturesParent = cd.featuresParent;
        xIndex = (int)(cd.coordinate.x);
        zIndex = (int)(cd.coordinate.y);
        xOffset = xIndex * ChunkSize;
        zOffset = zIndex * ChunkSize;


        yield return StartCoroutine(GenerateTerrainMaps());
        cd.TemperatureMap = TemperatureMap;
        cd.HumidityMap = HumidityMap;
        cd.ElevationMap = ElevationMap;
        cd.MountainMap = MountainMap;
        cd.FreshWaterMap = FreshWaterMap;
        cd.WetnessMap = WetnessMap;
        cd.HeightMap = HeightMap;
        cd.TreeMap = TreeMap;

        PlaceTerrainAndWater(cd);

    }

    IEnumerator DeloadChunks()
    {

        foreach (ChunkData loadedCd in ChunkDataLoaded.ToArray())
        {
            int index = ChunkDataToLoad.FindIndex(cd => cd.coordinate == loadedCd.coordinate);
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
        FreshWaterMap = new float[ChunkSize + 2, ChunkSize + 2];
        WetnessMap = new float[ChunkSize + 2, ChunkSize + 2];
        HeightMap = new float[ChunkSize + 2, ChunkSize + 2];
        TreeMap = new bool[ChunkSize + 2, ChunkSize + 2];

        float temperatureValue, humidityValue, elevationValue, mountainValue, freshWaterValue, wetnessValue, heightValue, heightValue_water;
        bool treeValue;
        float rough;

        // loop start
        for (int z = 0; z < ChunkSize + 2; z++)
        {
            for (int x = 0; x < ChunkSize + 2; x++)
            {



                 // TemperatureMap [0, 1]

                temperatureValue = 1.25f * Mathf.PerlinNoise((x + xOffset + .001f) / TemperatureMapScale, (z + zOffset + .001f) / TemperatureMapScale);
                //temperatureValue -= 1f * (mountainValue / mtnCap);
                // temperatureValue +=  (Mathf.PerlinNoise((x + xOffset + .001f) / TemperatureMapScale, (z + zOffset + .001f) / TemperatureMapScale) * 2f - 1f) * (3f * (1f - mountainValue/mtnCap));

                temperatureValue = Mathf.InverseLerp(.2f, .8f, temperatureValue);
                temperatureValue = Mathf.Clamp01(temperatureValue);

                // lock temperature
                //temperatureValue = .99f;
                temperatureValue = .25f;






                // ElevationMap
                elevationValue = Mathf.PerlinNoise((x + xOffset - Seed + 100000.01f) / ElevationMapScale, (z + zOffset - Seed + 100000.01f) / ElevationMapScale) * 2f - 1f;
                float elevationValueWithRoughness = elevationValue + (Mathf.PerlinNoise((x + xOffset - Seed + .01f) / 15f, (z + zOffset - Seed + .01f) / 15f) * 2f - 1f) * .03f;
                //elevationValueWithRoughness = Posterize(-1, 1f, elevationValueWithRoughness, 8, .9f);
                //elevationValue = elevationValueWithRoughness;

                //elevationValue = -1f;
                float bm0 = Mathf.PerlinNoise((x + xOffset + 100000.01f) / 1600f, (z + zOffset + 100000.01f) / 1600f) * 2f - 1f;
                float bm1 = Mathf.PerlinNoise((x + xOffset + 200000.01f) / 1600f, (z + zOffset + 200000.01f) / 1600f) * 2f - 1f;
                float bigMound = Mathf.Min(bm0, bm1);
                bigMound = Mathf.Pow(Mathf.Abs(bigMound) * -1f + 1f, 1f);
                float bigMoundCap = .04f;
                bigMound *= bigMoundCap;
                bigMound *= (1f - Mathf.InverseLerp(.25f, .75f, temperatureValue));
                bigMound = 0;
                elevationValue += bigMound;
                // -------------------------------------------------------

                // HumidityMap [0, 1]
                humidityValue = Mathf.PerlinNoise((x + xOffset - Seed + .01f) / HumidityMapScale, (z + zOffset - Seed + .01f) / HumidityMapScale);
                //humidityValue += (mountainValue / mtnCap) * .5f;
                //humidityValue = Mathf.Clamp01(humidityValue);
                humidityValue = Mathf.InverseLerp(.2f, .8f, humidityValue);

                // lock humidity
                //humidityValue = 0f;
                //humidityValue = .99f;
                // -------------------------------------------------------


                // MountainMap [0, 1]
                float mountainElev = 0f;
                float mtn0 = Mathf.PerlinNoise((x + xOffset - Seed + .01f) / MountainMapScale, (z + zOffset - Seed + .01f) / MountainMapScale);
                float mtn1 = Mathf.PerlinNoise((x + xOffset - Seed + 100000.01f) / MountainMapScale, (z + zOffset - Seed + 100000.01f) / MountainMapScale);
                float mtn2 = Mathf.PerlinNoise((x + xOffset - Seed + 200000.01f) / MountainMapScale, (z + zOffset - Seed + 200000.01f) / MountainMapScale);
                if (elevationValueWithRoughness > 0f)
                {

                    mountainValue = Mathf.Min(mtn0, 1);
                    mountainValue *= Mathf.Lerp(.02f, 1f, Mathf.InverseLerp(.25f, .5f, 1f - temperatureValue));
                    mountainElev = mtn0 * Mathf.Lerp(.1f, 1f, Mathf.InverseLerp(.25f, .5f, 1f - temperatureValue));

            
                    //mountainValue = Mathf.InverseLerp(.25f, .75f, mountainValue);

                    //mountainValue = Mathf.Pow(mountainValue, 1.5f);
                    //mountainValue *= 1f - CalculateDesertness(temperatureValue, humidityValue);

                    //mountainValue *= Mathf.InverseLerp(0f, .35f, elevationValue);
                    mountainValue = Mathf.Clamp01(mountainValue);

                    
                    //mountainValue = 0f;
                    //Debug.Log(mountainValue);
                
                }
                else
                {
                    mountainValue = 0f;
                }

                //mountainValue = .5f;



                // -------------------------------------------------------


                // FreshWaterMap [0, 1]

                float freshWaterElev = 0f;
                if (false)
                {
                    float riverScale = 500f;

                    // main river path
                    freshWaterValue = Mathf.PerlinNoise((x + xOffset - Seed + .01f) / riverScale, (z + zOffset - Seed + .01f) / riverScale) * 2f - 1f;

                    // give rivers character
                    float character = .5f;
                    rough = Mathf.PerlinNoise((x + xOffset + .01f) / 100f, (z + zOffset + .01f) / 100f);
                    freshWaterValue += Mathf.Max(0f, rough) * character;

                    float riverWidthFactor = Mathf.PerlinNoise((x + xOffset - Seed + .01f) / 200f, (z + zOffset - Seed + .01f) / 200f);


                    // ridgify
                    freshWaterValue = Mathf.Abs(freshWaterValue);
                    freshWaterValue *= -1f;
                    freshWaterValue += 1f;

                    freshWaterValue = Mathf.Clamp01(freshWaterValue);

    
                    // give rivers roughness
                    float roughValue, roughElev;
                    roughValue = roughElev = Mathf.PerlinNoise((x + xOffset + .01f) / 3f, (z + zOffset + .01f) / 3f);
                    roughValue = roughValue * 2f - 1f;
                    roughElev *= -1f;


                    freshWaterElev = Mathf.Pow(freshWaterValue, Mathf.Lerp(5f, 10f, riverWidthFactor));
                    freshWaterElev = Mathf.InverseLerp(0f, .7f, freshWaterElev);
                    freshWaterElev += roughElev * .07f;
                    freshWaterElev = Mathf.Clamp01(freshWaterElev);
                    

                    freshWaterValue += roughValue * .01f;
                    freshWaterValue = Mathf.Pow(freshWaterValue, Mathf.Lerp(100f, 200f, riverWidthFactor));
                    //Debug.Log(freshWaterValue)

                    //freshWaterValue = Posterize(0f, 1f, freshWaterValue, 4);


                }
                else
                {
                    freshWaterValue = 0f;
                }


                
                


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

                // heightValue = Mathf.PerlinNoise((x + xOffset + .01f) / Scale, (z + zOffset - Seed + .01f) / Scale);
                // heightValue = Mathf.Lerp(FlatLevel, 1f, heightValue);


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

        
                //ABS and INVERT, and normalize value
                heightValue = Mathf.Abs(heightValue);
                heightValue *= -1f;
                heightValue = Mathf.InverseLerp(-1f, 0f, heightValue);
                heightValue = Mathf.Pow(heightValue, .5f);


                // decrease height fluctuation from mountain map
                if (heightValue < FlatLevel)
                {
                    heightValue = FlatLevel;
                }
                else
                {
                    heightValue = Mathf.Lerp(heightValue, FlatLevel, 1f - mountainValue);
                }

                // decrease height fluctuation from fresh water map
                if(elevationValueWithRoughness > 0f)
                {
                    heightValue = Mathf.Lerp(heightValue, FlatLevel, freshWaterElev);
                }

                
                // apply ElevationMap
                float totalRaise = 0f;
                totalRaise += elevationValueWithRoughness * meter * 1.5f * (1f - (freshWaterElev * Convert.ToSingle(elevationValueWithRoughness > 0f)));
                totalRaise += (mountainElev) * meter * 20f * elevationValueWithRoughness * (1f - freshWaterElev);
                //totalRaise += (mountainValue) * meter * 15f * elevationValue;
                heightValue += totalRaise;

                // create ocean and rivers
                float oceanFloorLevel = SeaLevel - (meter * 1f);
                float riverFloorLevel = SeaLevel - (meter * 1f);

                heightValue = Mathf.Lerp(heightValue, riverFloorLevel, freshWaterValue);

                heightValue = Mathf.Clamp01(heightValue);



                // -------------------------------------------------------

                // //posterize all land
                if (heightValue >= FlatLevel)
                {

                    // badland effect in deserts
                    float desertness = CalculateDesertness(temperatureValue, humidityValue);
                    if (desertness > 0f)
                    {
                        if (heightValue > FlatLevel)
                        {
                            float postHeight = Posterize(FlatLevel + meter, 1f, heightValue, 500);
                            heightValue = Mathf.Lerp(heightValue, postHeight, desertness);
                        }
                    }

                    // non desert posterize
                    else
                    {

                        // float postVariance = 70f;
                        // int stepsMin = 4;
                        // int stepsMax = 12;
                        // int stepSteps = 100;

                        // int posterizeSteps = (int)Mathf.Lerp(stepsMin, stepsMax, Mathf.InverseLerp(0f, 1f, (Perlin.Noise((x + xOffset + .01f) / postVariance, (heightValue * ElevationAmplitude / postVariance), (z + zOffset - Seed + .01f) / postVariance)) + 1f) / 2f);
                        // posterizeSteps = (int)Posterize(stepsMin, stepsMax, posterizeSteps, stepSteps);

                        // heightValue = Posterize(FlatLevel, 1f, heightValue, posterizeSteps);

                        heightValue = PosterizeSoft(FlatLevel, 1f, heightValue, 2);
                    }

                }
                else
                {
                    // float postVariance = 60f;
                    // float perlin = Mathf.PerlinNoise((x + xOffset + .01f) / postVariance, (z + zOffset - Seed + .01f) / postVariance);
                    // perlin = Perlin.Noise((x + xOffset + .01f) / postVariance, (heightValue * ElevationAmplitude) * .5f, (z + zOffset - Seed + .01f) / postVariance);

                    // int posterizeSteps = (int)Mathf.Lerp(100, 1500, Mathf.InverseLerp(0f, 1f, perlin));
                    // posterizeSteps = (int)Posterize(100, 1500, posterizeSteps, 4);
                    // posterizeSteps = 10;
                    // heightValue = Posterize(oceanFloorLevel, SeaLevel - meter, heightValue, posterizeSteps);

                    heightValue = Mathf.Lerp(heightValue, oceanFloorLevel, Mathf.Clamp(((FlatLevel - heightValue) / (FlatLevel - oceanFloorLevel)) * 100f, 0f, 1f));
                }
                
                

                // TreeMap
                float t;
                float tScale = 500f;
                t = Mathf.PerlinNoise((x + xOffset - Seed + .01f) / tScale, (z + zOffset - Seed + .01f) / tScale) * 2f - 1f;

                // character
                float c = .25f;
                rough = Mathf.PerlinNoise((x + xOffset + .01f) / 100f, (z + zOffset + .01f) / 100f);
                t += Mathf.Max(0f, rough) * c;

                // ridgify
                t = Mathf.Abs(t);
                t *= -1f;
                t += 1f;

                t = Mathf.Clamp01(t);
                //Debug.Log(t);
                treeValue = t >= .985f;



                // completely flatten terrain
                //heightValue = FlatLevel + .001f;




                // -------------------------------------------------------

                TemperatureMap[x, z] = temperatureValue;
                HumidityMap[x, z] = humidityValue;
                MountainMap[x, z] = mountainValue;
                ElevationMap[x, z] = elevationValue;
                FreshWaterMap[x, z] = freshWaterValue;
                WetnessMap[x, z] = wetnessValue;
                HeightMap[x, z] = heightValue;
                TreeMap[x, z] = treeValue;

            }

            yield return new WaitForSecondsRealtime(.0000001f);
            
        }

        
    }

    public static float CalculateDesertness(float temp, float humid)
    {
        //float desertness = Mathf.Min(Mathf.InverseLerp(.75f, 1f, temp), Mathf.InverseLerp(.75f, 1f, (1f - humid)));
        float desertness = Mathf.InverseLerp(.75f, .9f, 1f - humid);
        return desertness;
    }

    float PosterizeSoft(float min, float max, float val, int steps)
    {

        if(val < min)
        {
            return min;
        }
        if(val > max)
        {
            return max;
        }

        float stepHeight = (max - min) / steps;

        float level = min;
        while (level < max)
        {
            level += stepHeight;
            if (level >= val)
            {
                float nextLevel = level;
                level -= stepHeight;
                float midpt = Mathf.Lerp(level, nextLevel, 1f);
                //float compliance = 1f - (Mathf.Pow(Mathf.Abs(midpt - val) / (stepHeight / 2f), Mathf.Lerp(0f, 20f, rigidity)));
                float compliance = 1f - Mathf.InverseLerp(level, midpt, val);
                compliance = Mathf.Pow(compliance, .5f);
                val = Mathf.Lerp(val, level, compliance);


                return val;
            }
        }
        return -1f;

    }

    float Posterize(float min, float max, float val, int steps)
    {

        if(val < min)
        {
            return min;
        }
        if(val > max)
        {
            return max;
        }

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
                float compliance = 1;
                val = Mathf.Lerp(val, level, compliance);


                return val;
            }
        }
        return -1f;
    }



    Color SetVertexColor(int x, int z, float height, float mountain, float temperature, float humidity, float wetness, float fw, float rockiness)
    {

        Color c = new Color();

        // desertness (c.r)
        c.r = CalculateDesertness(temperature, humidity);

        // mountainness (c.g)
        c.g = mountain;


        //Debug.Log(c.r);

        return c;

        
    }



    public static IEnumerator GenerateSpawns(ChunkData cd)
    {

        float _xOffset = (int)cd.coordinate.x * ChunkSize;
        float _zOffset = (int)cd.coordinate.y * ChunkSize;

        for (int z = 0; z < ChunkSize + 2; z++)
        {
            for (int x = 0; x < ChunkSize + 2; x++)
            {

                if (!cd.TreeMap[x, z])
                {
                    Vector3 normal = cd.YNormalsMap[x, z];
                    float skewHoriz = cd.SkewHorizMap[x, z];
                    float height = cd.HeightMap[x, z];
                    float temp = cd.TemperatureMap[x, z];
                    float humid = cd.HumidityMap[x, z];

                    SpawnParameters spawnParameters;
                    float placementDensity;
                    float randomDivisorOffset;
                    Vector3 randomPositionOffset, spawnPosition, spawnScale;
                    GameObject worldObject;
                    ObjectPool<GameObject> pool;

                    //yield return new WaitUntil(() => !cd.IsEdgeChunk());
                    //yield return new WaitForSecondsRealtime(2f);

                    foreach (GameObject feature in Features.Concat(Items))
                    {

                        // break if chunk not loaded
                        if (cd == null || (cd.featuresParent == null)) { break; }

                        spawnParameters = SpawnParameters.GetSpawnParameters(feature.name);
                        if (spawnParameters != null)
                        {
                            placementDensity = SpawnParameters.GetPlacementDensity(spawnParameters, temp, humid, height, normal.y);
                            if (placementDensity > 0)
                            {
                                randomDivisorOffset = 15f * (Mathf.PerlinNoise((x + _xOffset + .01f) / 2f, (z + _zOffset + .01f) / 2f) * 2f - 1f);
                                int divisor = (int)(Mathf.Lerp(1f, 20f, 1f - placementDensity) + randomDivisorOffset);
                                if (divisor < 1)
                                {
                                    divisor = 1;
                                }
                                if ((x + _xOffset) % divisor == 0 && (z + _zOffset) % divisor == 0)
                                {
                                    for (int i = 0; i < spawnParameters.numberToSpawn; ++i)
                                    {
                                        randomPositionOffset = 2f * ((Vector3.right * (UnityEngine.Random.value * 2f - 1f)) + (Vector3.forward * (UnityEngine.Random.value * 2f - 1f)));
                                        Vector3 rawHorizontalPosition = new Vector3(x + _xOffset + skewHoriz + randomPositionOffset.x, 0f, z + _zOffset + skewHoriz + randomPositionOffset.z);
                                        Vector2 rawHorizontalPositionV2 = new Vector2(rawHorizontalPosition.x, rawHorizontalPosition.z);
                                        //if (!instance.fillMap.MapFilled(rawHorizontalPositionV2))
                                        if (true)
                                        {
                                            ChunkData chunkAtPosition = GetChunkFromRawPosition(new Vector3(rawHorizontalPosition.x, 0f, rawHorizontalPosition.z));
                                            if (chunkAtPosition != null)
                                            {
                                                Vector2 coordinatesInChunk = GetCoordinatesInChunk(rawHorizontalPosition);
                                                int posChunkX = (int)coordinatesInChunk.x;
                                                int posChunkZ = (int)coordinatesInChunk.y;
                                                if (!cd.TreeMap[posChunkX, posChunkZ])
                                                {
                                                    float posChunkHeight = chunkAtPosition.HeightMap[posChunkX, posChunkZ];
                                                    float posChunkSkewHoriz = chunkAtPosition.SkewHorizMap[posChunkX, posChunkZ];
                                                    float posChunkYNormal = chunkAtPosition.YNormalsMap[posChunkX, posChunkZ].y;
                                                    placementDensity = SpawnParameters.GetPlacementDensity(spawnParameters, temp, humid, posChunkHeight, posChunkYNormal);
                                                    if (placementDensity > 0)
                                                    {
                                                        spawnPosition = rawHorizontalPosition + Vector3.up * (posChunkHeight * ChunkGenerator.ElevationAmplitude) + Vector3.right * posChunkSkewHoriz + Vector3.forward * posChunkSkewHoriz;
                                                        spawnScale = Vector3.one * spawnParameters.scale;
                                                        pool = PoolHelper.GetPool(feature);
                                                        worldObject = pool.Get();
                                                        worldObject.transform.position = spawnPosition + (Vector3.up * spawnParameters.heightOffset);
                                                        worldObject.transform.SetParent(cd.featuresParent);
                                                        worldObject.transform.localScale = spawnScale * UnityEngine.Random.Range(.5f, 1.25f);
                                                        if (spawnParameters.slantMagnitude > 0f)
                                                        {
                                                            worldObject.transform.rotation = Quaternion.Slerp(Quaternion.identity, Quaternion.FromToRotation(Vector3.up, cd.YNormalsMap[x, z]), spawnParameters.slantMagnitude);
                                                        }
                                                        worldObject.transform.Rotate(worldObject.transform.up, UnityEngine.Random.Range(0, 360f));
                                                        if(worldObject.transform.GetComponent<Rigidbody>() != null)
                                                        {
                                                            Utility.ToggleObjectPhysics(worldObject, true, true, false, false);
                                                        }
                                                        //instance.fillMap.AddFillPoint(cd, rawHorizontalPositionV2, spawnParameters.fillRadius);
                                                    }
                                                }
                                                
                                            }
                                        }


                                    }




                                }
                            }
                        }

                    }

                    foreach (GameObject creature in Creatures)
                    {

                        // break if chunk not loaded
                        if (cd == null) { break; }

                        spawnParameters = SpawnParameters.GetSpawnParameters(creature.name);
                        if (spawnParameters != null)
                        {
                            placementDensity = SpawnParameters.GetPlacementDensity(spawnParameters, temp, humid, height, normal.y);
                            int placementOffsetX = (int)((Mathf.InverseLerp(Int32.MinValue, Int32.MaxValue, creature.name.GetHashCode()) * 2f - 1f) * 50f);
                            int placementOffsetZ = (int)((Mathf.InverseLerp(Int32.MinValue, Int32.MaxValue, (creature.name + "_").GetHashCode()) * 2f - 1f) * 50f);
                            if (placementDensity > 0f)
                            {
                                randomDivisorOffset = 0;
                                int divisor = (int)(Mathf.Lerp(5f, 100f, 1f - placementDensity) + randomDivisorOffset);
                                if (divisor < 1) { divisor = 1; }
                                if ((x + _xOffset + placementOffsetX) % divisor == 0 && (z + _zOffset + placementOffsetZ) % divisor == 0)
                                {
                                    spawnPosition = new Vector3(x + _xOffset, height * ElevationAmplitude + 10f, z + _zOffset);
                                    spawnScale = Vector3.one * spawnParameters.scale;
                                    worldObject = Utility.InstantiateSameName(creature, spawnPosition, Quaternion.identity);
                                    //pool = PoolHelper.GetPool(creature);
                                    //worldObject = pool.Get();
                                    //worldObject.transform.position = spawnPosition;
                                    worldObject.transform.localScale = spawnScale * UnityEngine.Random.Range(.75f, 1.25f);
                                    AddActiveCPUCreature(worldObject);
                                }
                            }
                        }

                    }

                    foreach (GameObject human in Humans)
                    {

                        // break if chunk not loaded
                        if (cd == null) { break; }

                        if (humanSpawned) { break; }

                        spawnParameters = SpawnParameters.GetSpawnParameters(human.name);
                        if (spawnParameters != null)
                        {
                            placementDensity = SpawnParameters.GetPlacementDensity(spawnParameters, temp, humid, height, normal.y);
                            int placementOffsetX = (int)((Mathf.InverseLerp(Int32.MinValue, Int32.MaxValue, human.name.GetHashCode()) * 2f - 1f) * 50f);
                            int placementOffsetZ = (int)((Mathf.InverseLerp(Int32.MinValue, Int32.MaxValue, (human.name + "_").GetHashCode()) * 2f - 1f) * 50f);
                            if (placementDensity > 0f)
                            {
                                randomDivisorOffset = 0;
                                int divisor = (int)(Mathf.Lerp(5f, 100f, 1f - placementDensity) + randomDivisorOffset);
                                if (divisor < 1) { divisor = 1; }
                                if ((x + _xOffset + placementOffsetX) % divisor == 0 && (z + _zOffset + placementOffsetZ) % divisor == 0)
                                {
                                    spawnPosition = new Vector3(x + _xOffset, height * ElevationAmplitude + 10f, z + _zOffset);
                                    spawnScale = Vector3.one * spawnParameters.scale;

                                    //Debug.Log("WILD NPC");
                                    instance.StartCoroutine(ClientCommand.instance.SpawnCharacterAsLeaderWhenReady(spawnPosition, true, FactionStartingItemsTier.Weak));


                                    //o.transform.localScale = spawnScale * UnityEngine.Random.Range(.75f, 1.25f);

                                    //humanSpawned = true;
                                }
                            }
                        }

                    }
                }



            }
            yield return null;
        }


    }


    void DespawnCPUCreatures()
    {

        //Debug.Log("DespawnCpuCreatures(): " + activeCPUCreatures.Count);

        float despawnDistance = ChunkSize * ChunkRenderDistance;
        GameObject cpuCreature;
        EntityHandle cpuCreatureHandle;
        for(int i = 0; i < activeCPUCreatures.Count; ++i){
            cpuCreature = activeCPUCreatures[i];
            if(cpuCreature != null)
            {
                cpuCreatureHandle = cpuCreature.GetComponent<EntityHandle>();
                if(Vector3.Distance(playerRawPosition, activeCPUCreatures[i].transform.position) > despawnDistance)
                {
                    activeCPUCreatures.RemoveAt(i);
                    Faction creatureFaction = cpuCreature.GetComponent<EntityInfo>().faction;
                    if (creatureFaction != null && !ReferenceEquals(creatureFaction, ClientCommand.instance.clientPlayerCharacterHandle.entityInfo.faction))
                    {
                        // if creature's faction is a faction besides its species' base faction, and the faction is not already marked for destruction, destroy the whole faction
                        if (!ReferenceEquals(creatureFaction, SpeciesInfo.GetSpeciesInfo(cpuCreatureHandle.entityInfo.species)) && !creatureFaction.IsMarkedForDestruction())
                        {
                            creatureFaction.MarkForDestruction();
                            creatureFaction.DestroyFaction();
                        }
                        // else, remove only the creature from the world
                        else
                        {
                            cpuCreatureHandle.RemoveFromWorld();
                        }
                    }
                    else
                    {
                        cpuCreatureHandle.RemoveFromWorld();
                    }

                    
                }
            }
            else{
                activeCPUCreatures.RemoveAt(i);
            }
        }
    }

    void PlaceTerrainAndWater(ChunkData cd)
    {

        TerrainMesh.Clear();

        // initialize properties for meshes
        TerrainVertices = new Vector3[(ChunkSize + 2) * (ChunkSize + 2)];
        TerrainTriangles = new int[(ChunkSize + 2) * (ChunkSize + 2) * 6];
        TerrainUvs = new Vector2[TerrainVertices.Length];
        TerrainColors = new Color[TerrainVertices.Length];


        // set terrain vertices according to HeightMap, and set colors
        // NOTE: vertex index = (z * (ChunkSize + 2) + x
        float height;
        float temperature;
        float humidity;
        float rockiness;
        float skewHoriz;
        float[,] skewHorizMap = new float[ChunkSize + 2, ChunkSize + 2];
        for (int i = 0, z = 0; z < ChunkSize + 2; z++)
        {
            for (int x = 0; x < ChunkSize + 2; x++)
            {
                height = HeightMap[x, z] * ElevationAmplitude;
                temperature = TemperatureMap[x, z];
                humidity = HumidityMap[x, z];
                rockiness = Mathf.Pow(Mathf.PerlinNoise((x + xOffset) / 50f, (z + zOffset) / 50f), .5f);
                //rockiness *= Mathf.PerlinNoise(((height) / 50f), 0f);
                rockiness += (Mathf.PerlinNoise((x + xOffset) / 2f, (z + zOffset) / 2f) * 2f - 1f) * .01f;
                //rockiness *= Mathf.InverseLerp(0f, .1f, MountainMap[x, z]);
                rockiness *= 1f - (CalculateDesertness(temperature, humidity));
                skewHoriz = ((rockiness + .5f) * 2f - 1f) * 18f * RockProtrusion;
                //skewHoriz = 0f;
                skewHorizMap[x, z] = skewHoriz;
                TerrainVertices[i] = new Vector3(x + xOffset + skewHoriz, height, z + zOffset + skewHoriz);
                //TerrainVertices[i] = new Vector3(x + xOffset, height, z + zOffset);
                TerrainColors[i] = SetVertexColor(x + xOffset, z + zOffset, HeightMap[x, z], MountainMap[x, z], temperature, humidity, WetnessMap[x, z], FreshWaterMap[x, z], rockiness);
                i++;
            }
        }
        TerrainMesh.vertices = TerrainVertices;
        TerrainMesh.colors = TerrainColors;

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
        TerrainMesh.triangles = TerrainTriangles;

        // set up normals
        TerrainMesh.RecalculateNormals();


        // set up UVs, and place features based on normal value
        int normalIndex;
        Vector3 normal;
        Vector3[] normals = TerrainMesh.normals;
        Vector3[,] normalsMap = new Vector3[ChunkSize + 2, ChunkSize + 2];

        for (int i = 0, z = 0; z < ChunkSize + 2; z++)
        {
            for (int x = 0; x < ChunkSize + 2; x++)
            {

                // uv
                TerrainUvs[i] = new Vector2((float)x + xOffset, (float)z + zOffset);

                // features
                if(z > 0 && x > 0){
                    normalIndex = (z * (ChunkSize + 2)) + x;
                    normal = normals[normalIndex];
                    normalsMap[x, z] = normal;
                }
                
                
                i++;
            }
        }
        TerrainMesh.uv = TerrainUvs;

        MeshCollider mc = Terrain.AddComponent<MeshCollider>();
        mc.sharedMaterial = physicMaterial;

        cd.YNormalsMap = normalsMap;
        cd.SkewHorizMap = skewHorizMap;

    }


    void UpdateWaterPosition(){
        Vector3 pos = playerTransform.position;
        pos.y = SeaLevel * ElevationAmplitude;
        Water.transform.position = pos;
    }

    public static void AddActiveCPUCreature(GameObject creature)
    {
        activeCPUCreatures.Add(creature);
    }


    // returns given position translated to chunk coordinates, based on chunkSize
    public static Vector2 GetChunkCoordinate(Vector3 position)
    {
        return new Vector2(Mathf.Floor(position.x / ChunkSize), Mathf.Floor(position.z / ChunkSize));
    }

    // retrieve ChunkData in ChunkDataLoaded associated with the chunk coordinate
    public static ChunkData GetChunkFromCoordinate(Vector2 chunkCoord)
    {
        //Debug.Log(chunkCoord);
        foreach (ChunkData cd in ChunkDataLoaded.ToArray())
        {
            if (cd.coordinate.Equals(chunkCoord))
            {
                return cd;
            }
        }
        //Debug.Log("ChunkGenerator: chunk from given position is not loaded!");
        //Debug.Log(chunkCoord);
        return null;
    }

    // retrieve ChunkData in ChunkDataLoaded associated with the raw position given
    public static ChunkData GetChunkFromRawPosition(Vector3 rawPosition)
    {
        Vector2 chunkCoord = GetChunkCoordinate(rawPosition);
        return GetChunkFromCoordinate(chunkCoord);
    }

    // retrieve the x and z points of the given position on the chunk it's in
    public static Vector2 GetCoordinatesInChunk(Vector3 rawPosition)
    {
        int x = (int)(rawPosition.x % ChunkSize);
        int z = (int)(rawPosition.z % ChunkSize);

        if(x < 0)
        {
            x = (int)(ChunkSize - x * -1f);
        }
        if(z < 0)
        {
            z = (int)(ChunkSize - z * -1f);
        }

        Vector2 coordinatesInChunk = new Vector2(x, z);
        //Debug.Log(coordinatesInChunk);

        return coordinatesInChunk;
    }


}

