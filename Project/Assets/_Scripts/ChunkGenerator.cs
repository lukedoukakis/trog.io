using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEngine.Pool;

public class ChunkGenerator : MonoBehaviour
{
    public static float TerrainScaleModifier = 1f;
    public static float IslandDiameter = 100000f;
    public static ChunkGenerator instance;
    public static int Seed = 75675;
    public static int ChunkSize = 10;
    public static int ChunkRenderDistance = 8;
    public static float Scale = 100f;
    public static float Amplitude = 160f;
    public static float MountainMapScale = 500f;
    public static float ElevationMapScale = 2000f;
    public static int TemperatureMapScale = 300;
    public static int HumidityMapScale = 300;
    public static float meter = 1f / Amplitude;
    public static float SeaLevel = .1f;
    public static float SnowLevel = .65f;
    //public static float SnowLevel = float.MaxValue;
    public static float GrassNormal = .9f;
    public static float SnowNormalMin = .75f;
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


    public static bool AllChunksLoaded;
    static List<Vector2> ChunkCoordsToBeActive;
    static List<ChunkData> ChunkDataLoaded;

    static Vector3[] TerrainVertices, WaterVertices;
    static int[] TerrainTriangles, WaterTriangles;
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
    float[,] HeightMapWater;

    [Range(0, 1)] public float RockProtrusion;



    // features and creatures
    Transform FeaturesParent;
    public static List<GameObject> activeCPUCreatures;
    public static List<GameObject> Features, Creatures, Humans, Items;
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
                UpdateChunksToBeActive();
                StartCoroutine(ActivateChunks());
                StartCoroutine(FreezeChunks());
            }

            cpuCreatureDespawnTime += Time.deltaTime;
            if(cpuCreatureDespawnTime >= cpuCreatureDespawnTimestep){
                DespawnCPUCreatures();
                cpuCreatureDespawnTime = 0f;
            }

            //UpdateWaterPosition();

        }
    }

    void Init()
    {

        //Debug.Log("seed: " + Seed.ToString());

        //RiverGenerator.Generate();

        ChunkCoordsToBeActive = new List<Vector2>();
        ChunkDataLoaded = new List<ChunkData>();
        TerrainMesh = new Mesh();
        TerrainMeshFilter.mesh = TerrainMesh;
        WaterMesh = new Mesh();
        WaterMeshFilter.mesh = WaterMesh;

        Features = new List<GameObject>(Resources.LoadAll<GameObject>("Terrain/Features"));
        Creatures = new List<GameObject>(Resources.LoadAll<GameObject>("Terrain/Creatures"));
        Humans = new List<GameObject>(Resources.LoadAll<GameObject>("Terrain/Humans"));
        Items = Item.Items.Values.Select(item => item.worldObjectPrefab).Where(o => o != null).ToList();
        //Features = Features.OrderBy(feature => SpawnParameters.GetSpawnParameters(feature.name).loadOrder).ToList();

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


    void UpdateChunksToBeActive()
    {


        float maxCoordinateDistanceFromCenter = ((int)IslandDiameter / 2) / ChunkSize + 1;
        playerRawPosition = playerTransform.position / TerrainScaleModifier;
        playerChunkCoordinate = GetChunkCoordinate(playerRawPosition);
        Vector2 currentChunkCoord = new Vector2((int)playerChunkCoordinate.x, (int)playerChunkCoordinate.y);

        // get neighbor chunk coordinates
        Vector2 halfVec = Vector3.one / 2f;
        List<Vector2> neighborChunkCoords = new List<Vector2>();
        Vector2 neighborChunkCoord;
        for (int z = (int)(currentChunkCoord.y - ChunkRenderDistance); z < (int)(currentChunkCoord.y + ChunkRenderDistance); ++z)
        {
            for (int x = (int)(currentChunkCoord.x - ChunkRenderDistance); x < (int)(currentChunkCoord.x + ChunkRenderDistance); ++x)
            {
                neighborChunkCoord = new Vector2(x, z);
                if(Vector2.Distance(neighborChunkCoord, currentChunkCoord) <= ChunkRenderDistance && neighborChunkCoord.magnitude < maxCoordinateDistanceFromCenter)
                {
                    neighborChunkCoords.Add(neighborChunkCoord);
                }
            }
        }


        // remove chunks out of rendering range from ChunksToLoad
        foreach (Vector2 coordinate in ChunkCoordsToBeActive.ToArray())
        {
            if (Vector2.Distance(playerChunkCoordinate, coordinate + halfVec) >= ChunkRenderDistance)
            {
                ChunkCoordsToBeActive.Remove(coordinate);
            }
        }

        // add chunks in rendering range to ChunksToLoad
        foreach (Vector2 chunkCoord in neighborChunkCoords)
        {
            if (Vector2.Distance(playerChunkCoordinate, chunkCoord + halfVec) < ChunkRenderDistance)
            {  
                if (!ChunkCoordsToBeActive.Contains(chunkCoord))
                {
                    ChunkCoordsToBeActive.Add(chunkCoord);
                }
            }
        }

    }

    // for each chunk to be active, load it if it isn't loaded and unfreeze it if it's frozen
    IEnumerator ActivateChunks()
    {

        IEnumerator load;
        ChunkData cd;
        int index;
        foreach (Vector2 coordinate in ChunkCoordsToBeActive.OrderBy(c => Vector3.Distance(GetChunkCoordinate(playerTransform.position / TerrainScaleModifier), c)).ToArray())
        {
            index = ChunkDataLoaded.FindIndex(cd => cd.coordinate == coordinate);
            if(index < 0)
            {
                cd = new ChunkData(coordinate);
                load = LoadChunk(cd);
                ChunkDataLoaded.Add(cd);
                yield return StartCoroutine(load);
            }
            else
            {
                cd = ChunkDataLoaded[index];
                if(cd.frozen)
                {
                    cd.SetFrozen(false);
                }
            }
            
            // if(GetChunkCoordinate(playerTransform.position / TerrainScaleModifier) != playerChunkCoordinate){
            //     break;
            // }
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
        FeaturesParent = cd.featuresParent;
        xIndex = (int)(cd.coordinate.x);
        zIndex = (int)(cd.coordinate.y);
        xOffset = xIndex * ChunkSize;
        zOffset = zIndex * ChunkSize;


        GenerateTerrainMaps();
        cd.TemperatureMap = TemperatureMap;
        cd.HumidityMap = HumidityMap;
        cd.ElevationMap = ElevationMap;
        cd.MountainMap = MountainMap;
        cd.FreshWaterMap = FreshWaterMap;
        cd.WetnessMap = WetnessMap;
        cd.HeightMap = HeightMap;
        cd.HeightMapWater = HeightMapWater;

        PlaceTerrainAndWater(cd);
        yield return null;

    }


    // for all currently loaded and unfrozen chunks, freeze them if they are not included in the chunks that should be active
    IEnumerator FreezeChunks()
    {
        foreach (ChunkData loadedCd in ChunkDataLoaded.ToArray())
        {
            if(!loadedCd.frozen)
            {
                if (!ChunkCoordsToBeActive.Contains(loadedCd.coordinate))
                {
                    loadedCd.SetFrozen(true);
                    yield return null;
                }
            }
        }
        DeloadingChunks = false;

    }


    void GenerateTerrainMaps()
    {

        TemperatureMap = new float[ChunkSize + 2, ChunkSize + 2];
        HumidityMap = new float[ChunkSize + 2, ChunkSize + 2];
        ElevationMap = new float[ChunkSize + 2, ChunkSize + 2];
        MountainMap = new float[ChunkSize + 2, ChunkSize + 2];
        FreshWaterMap = new float[ChunkSize + 2, ChunkSize + 2];
        WetnessMap = new float[ChunkSize + 2, ChunkSize + 2];
        HeightMap = new float[ChunkSize + 2, ChunkSize + 2];
        HeightMapWater = new float[ChunkSize + 2, ChunkSize + 2];

        float temperatureValue, humidityValue, elevationValue, mountainValue, freshWaterValue, wetnessValue, heightValue, heightValue_water;
        float rough;
        float seaFloorHeight = SeaLevel - (meter * 1f);

        // loop start
        for (int z = 0; z < ChunkSize + 2; z++)
        {
            for (int x = 0; x < ChunkSize + 2; x++)
            {

                // ElevationMap
                elevationValue = 1 - Mathf.InverseLerp(0, IslandDiameter / 2, new Vector2(x + xOffset, z + zOffset).magnitude);
                //elevationValue = Mathf.PerlinNoise((x + xOffset - Seed + 100000.01f) / ElevationMapScale, (z + zOffset - Seed + 100000.01f) / ElevationMapScale) * 2f - 1f;
                float elevationValueWithRoughness = elevationValue + (Mathf.PerlinNoise((x + xOffset - Seed + .01f) / 15f, (z + zOffset - Seed + .01f) / 15f) * 2f - 1f) * .01f;


                // -------------------------------------------------------
                
                // MountainMap [0, 1]
                float mountainElev = 0f;
                mountainValue = Mathf.PerlinNoise((x + xOffset - Seed + 5000.01f) / MountainMapScale, (z + zOffset - Seed + 5000.01f) / MountainMapScale);
                //mountainValue *= Mathf.Lerp(.015f, 1f, Mathf.InverseLerp(0f, .1f, elevationValueWithRoughness));
                //mountainValue *= Mathf.Lerp(.02f, 1f, Mathf.InverseLerp(.25f, .5f, 1f - temperatureValue));
        
                mountainValue = Mathf.InverseLerp(.25f, .75f, mountainValue);
                //mountainValue = Mathf.Pow(mountainValue, 1.5f);
                mountainElev = mountainValue;
                //mountainValue *= 1f - CalculateDesertness(temperatureValue, humidityValue);

                mountainValue *= Mathf.InverseLerp(SeaLevel, .2f, elevationValueWithRoughness);
                mountainValue = Mathf.Clamp01(mountainValue);
                

                //mountainValue = 0f;
                //Debug.Log(mountainValue);



                //mountainValue = .99f;


                // -------------------------------------------------------

                // TemperatureMap [0, 1]

                temperatureValue = Mathf.PerlinNoise((x + xOffset + .001f) / TemperatureMapScale, (z + zOffset + .001f) / TemperatureMapScale);
                temperatureValue += (Mathf.PerlinNoise((x + xOffset + .001f) / 10f, (z + zOffset + .001f) / 10f) * 2f - 1f) * .05f;
                //temperatureValue -= 1f * (mountainValue / mtnCap);
                // temperatureValue += (Mathf.PerlinNoise((x + xOffset + .001f) / TemperatureMapScale, (z + zOffset + .001f) / TemperatureMapScale) * 2f - 1f) * (3f * (1f - mountainValue/mtnCap));

                temperatureValue = Mathf.InverseLerp(.2f, .8f, temperatureValue);
                temperatureValue *= (1f - mountainValue);

                temperatureValue = Mathf.Clamp01(temperatureValue);

                // lock temperature
                //temperatureValue = .99f;
                temperatureValue = .25f;


                // -------------------------------------------------------

                // HumidityMap [0, 1]
                humidityValue = Mathf.PerlinNoise((x + xOffset - Seed + .01f) / HumidityMapScale, (z + zOffset - Seed + .01f) / HumidityMapScale);
                humidityValue += (Mathf.PerlinNoise((x + xOffset + .001f) / 10f, (z + zOffset + .001f) / 10f) * 2f - 1f) * .05f;
                //humidityValue += (mountainValue / mtnCap) * .5f;
                //humidityValue = Mathf.Clamp01(humidityValue);
                humidityValue = Mathf.InverseLerp(.2f, .8f, humidityValue);

                // lock humidity
                //humidityValue = 0f;
                //humidityValue = .99f;
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


                // FreshWaterMap [0, 1]
                freshWaterValue = 0f;
                float riverScale = 600f;
                float riverWidthFactor = Mathf.PerlinNoise((x + xOffset - Seed + .01f) / 200f, (z + zOffset - Seed + .01f) / 200f);
                riverWidthFactor = 1;
                // --------------
                // main river path
                freshWaterValue = Mathf.PerlinNoise((x + xOffset - Seed + .01f) / riverScale, (z + zOffset - Seed + .01f) / riverScale) * 2f - 1f;
                // --------------
                // give rivers character
                float character = .5f;
                rough = Mathf.PerlinNoise((x + xOffset + .01f) / 75f, (z + zOffset + .01f) / 75f);
                freshWaterValue += Mathf.Max(0f, rough) * character;
                // --------------
                // ridgify
                freshWaterValue = Mathf.Abs(freshWaterValue);
                freshWaterValue *= -1f;
                freshWaterValue += 1f;
                freshWaterValue = Mathf.Clamp01(freshWaterValue);
                // --------------
                // give rivers roughness
                float roughValue, roughElev;
                roughValue = roughElev = Mathf.PerlinNoise((x + xOffset + .01f) / 3f, (z + zOffset + .01f) / 3f);
                roughValue = roughValue * 2f - 1f;
                roughElev *= -1f;
                freshWaterValue += roughValue * .01f;
                float freshWaterBroad = Mathf.Pow(freshWaterValue, Mathf.Lerp(5f, 10f, riverWidthFactor));
                freshWaterValue = Mathf.Pow(freshWaterValue, Mathf.Lerp(25f, 50f, riverWidthFactor));
                freshWaterValue = Mathf.InverseLerp(.1f, .2f, freshWaterValue);
                // --------------
                //if(freshWaterValue > .4f){ freshWaterValue = 1f; } else { freshWaterValue = 0f; }


                //Debug.Log(freshWaterValue)

                //freshWaterValue = Posterize(0f, 1f, freshWaterValue, 4);

                freshWaterValue = 0;
                freshWaterBroad = 0;


                // -------------------------------------------------------

                // HeightMap
                float heightFromElevation = Mathf.Min(elevationValueWithRoughness, SeaLevel + .11f);
                float heightFromMtn;
                float mtn0, mtn1, mtn2, mtnTotal;
                mtn0 = mountainValue;
                mtn1 = Mathf.InverseLerp(0f, .2f, mountainValue) * Mathf.Pow(Mathf.PerlinNoise((x + xOffset) / 55f, (z + zOffset) / 55f), 3.5f) * .8f;
                mtn1 += Mathf.PerlinNoise((x + xOffset) / 5f, (z + zOffset) / 5f) * .1f * Mathf.InverseLerp(0, .1f, mtn1);
                mtn1 *= (1 - freshWaterBroad);
                mtn2 = 0;
                mtnTotal = mtn0 + mtn1 + mtn2;
                heightFromMtn = (mtnTotal) * mountainValue * .5f;
                heightValue = 0;
                heightValue += heightFromElevation;
                heightValue += heightFromMtn;

                // -------------------------------------------------------

                //posterize all land
                float shoreHeight = SeaLevel + (meter * 1f);
                if (heightValue >= shoreHeight)
                {
                    // float postVariance = 100f;
                    // float posterizeStrength = Perlin.Noise((x + xOffset + .01f) / postVariance, (heightValue * Amplitude / postVariance), (z + zOffset - Seed + .01f) / postVariance);
                    // posterizeStrength = Mathf.InverseLerp(.4f, .6f, posterizeStrength);
                    // posterizeStrength = Mathf.Lerp(0f, .4f, posterizeStrength);
                    //heightValue = PosterizeSoft(shoreHeight, 1f, heightValue, 10, Mathf.InverseLerp(0, .05f, freshWaterValue), x + xOffset, z + zOffset);
                    //heightValue = Posterize(shoreHeight, 1f, heightValue, 50);

                }
                else
                {
                    //heightValue = Mathf.Lerp(heightValue, seaFloorHeight, Mathf.Clamp(((shoreHeight - heightValue) / (shoreHeight - seaFloorHeight)) * 100f, 0f, 1f));
                }


                // create ocean and rivers
                heightValue_water = Mathf.Lerp(SeaLevel, Mathf.Max(SeaLevel, heightValue - (meter * 1)), freshWaterValue);
                heightValue = Mathf.Lerp(heightValue - (meter * 1), heightValue, 1 - freshWaterValue);



                // bumpiness
                heightValue += (mtn1 > 0 && freshWaterValue <= 0) ? (Mathf.PerlinNoise((x + xOffset - Seed + .01f) / 1, (z + zOffset - Seed + .01f) / 1) * 2f - 1f) * .05f : 0;



               



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
                HeightMapWater[x, z] = SeaLevel;


            }

            //yield return new WaitForSecondsRealtime(.000000001f);
            
        }

        
    }

    public static float CalculateDesertness(float temp, float humid)
    {
        float desertness = Mathf.Min(Mathf.InverseLerp(.75f, 1f, temp), Mathf.InverseLerp(.75f, 1f, (1f - humid)));
        //float desertness = Mathf.InverseLerp(.75f, .9f, 1f - humid);
        return desertness;
    }

    float PosterizeSoft(float min, float max, float val, int steps, float strength, float x, float z)
    {

        bool vary = true;
        float variation = vary ? (Perlin.Noise((x / 20f), (val * Amplitude) / 2f, (z / 20f)) * 2f - 1f) * .01f : 0f;
        min += variation;
        max += variation;

        if(val < min)
        {
            return min;
        }
        if(val > max)
        {
            return val;
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
                compliance = Mathf.Pow(compliance, 1f - strength);
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



    Color SetVertexColor(int x, int z, float height, float mountain, float temperature, float humidity, float wetness, float fw)
    {

        Color c = new Color();

        // desertness (c.r)
        c.r = Mathf.InverseLerp(.1f, .9f, CalculateDesertness(temperature, humidity));

        // mountainness (c.g)
        c.g = mountain;

        c.b = fw;


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

                if(cd.FreshWaterMap[x, z] <= 0)
                {
                    float yNormal = cd.YNormalsMap[x, z];
                float height = cd.HeightMap[x, z];
                float temp = cd.TemperatureMap[x, z];
                float humid = cd.HumidityMap[x, z];

                SpawnParameters spawnParameters;
                float placementDensity;
                Vector3 randomPositionOffset, spawnPosition, spawnScale;
                GameObject worldObject;
                ObjectPool<GameObject> pool;


                //foreach (GameObject feature in Features.Concat(Items))
                foreach (GameObject feature in Features)
                {
                    //break;

                    // break if chunk not loaded
                    if (cd == null || (cd.featuresParent == null)) { break; }

                    spawnParameters = SpawnParameters.GetSpawnParameters(feature.name);
                    placementDensity = SpawnParameters.GetPlacementDensity(spawnParameters, temp, humid, height, yNormal);
                    if (placementDensity > 0)
                    {
                        if (UnityEngine.Random.value < placementDensity)
                        {
                            for (int i = 0; i < spawnParameters.numberToSpawn; ++i)
                            {
                                randomPositionOffset = 2f * ((Vector3.right * (UnityEngine.Random.value * 2f - 1f)) + (Vector3.forward * (UnityEngine.Random.value * 2f - 1f)));
                                Vector3 rawHorizontalPosition = new Vector3(x + _xOffset + randomPositionOffset.x, 0f, z + _zOffset + randomPositionOffset.z);
                                Vector2 rawHorizontalPositionV2 = new Vector2(rawHorizontalPosition.x, rawHorizontalPosition.z);
                                ChunkData chunkAtPosition = GetChunkFromRawPosition(new Vector3(rawHorizontalPosition.x, 0f, rawHorizontalPosition.z));
                                if (chunkAtPosition != null)
                                {
                                    Vector2 coordinatesInChunk = GetCoordinatesInChunk(rawHorizontalPosition);
                                    int posChunkX = (int)coordinatesInChunk.x;
                                    int posChunkZ = (int)coordinatesInChunk.y;

                                    float posChunkHeight = chunkAtPosition.HeightMap[posChunkX, posChunkZ];
                                    float posChunkYNormal = chunkAtPosition.YNormalsMap[posChunkX, posChunkZ];
                                    placementDensity = SpawnParameters.GetPlacementDensity(spawnParameters, temp, humid, posChunkHeight, posChunkYNormal);
                                    if (placementDensity > 0)
                                    {
                                        spawnPosition = rawHorizontalPosition + Vector3.up * (posChunkHeight * ChunkGenerator.Amplitude);
                                        spawnScale = Vector3.one * spawnParameters.scale;
                                        pool = PoolHelper.GetPool(feature);
                                        worldObject = pool.Get();
                                        worldObject.transform.position = (spawnPosition + (Vector3.up * spawnParameters.heightOffset)) * TerrainScaleModifier;
                                        worldObject.transform.SetParent(cd.featuresParent);
                                        worldObject.transform.localScale = spawnScale * UnityEngine.Random.Range(.5f, 1.25f);
                                        if (spawnParameters.slantMagnitude > 0f)
                                        {
                                            worldObject.transform.rotation = Quaternion.Slerp(Quaternion.identity, Quaternion.FromToRotation(Vector3.up, Vector3.up * cd.YNormalsMap[x, z]), spawnParameters.slantMagnitude);
                                        }
                                        worldObject.transform.Rotate(worldObject.transform.up, UnityEngine.Random.Range(0, 360f));
                                        if (worldObject.transform.GetComponent<Rigidbody>() != null)
                                        {
                                            worldObject.transform.position = (worldObject.transform.position + Vector3.up) * TerrainScaleModifier;
                                            //Utility.ToggleObjectPhysics(worldObject, true, true, false, false);
                                        }
                                        //instance.fillMap.AddFillPoint(cd, rawHorizontalPositionV2, spawnParameters.fillRadius);

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
                    placementDensity = SpawnParameters.GetPlacementDensity(spawnParameters, temp, humid, height, yNormal);
                    if (placementDensity > 0f)
                    {
                        if (UnityEngine.Random.value < placementDensity)
                        {
                            spawnPosition = new Vector3(x + _xOffset, height * Amplitude + 10f, z + _zOffset);
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

                foreach (GameObject human in Humans)
                {

                    // break if chunk not loaded
                    if (cd == null) { break; }

                    spawnParameters = SpawnParameters.GetSpawnParameters(human.name);
                    placementDensity = SpawnParameters.GetPlacementDensity(spawnParameters, temp, humid, height, yNormal);
                    if (placementDensity > 0f)
                    {
                        if (UnityEngine.Random.value < placementDensity)
                        {
                            spawnPosition = new Vector3(x + _xOffset, height * Amplitude + 10f, z + _zOffset);
                            spawnScale = Vector3.one * spawnParameters.scale;
                            //Debug.Log("WILD NPC");
                            ClientCommand.instance.SpawnCharacterAsLeader(spawnPosition, true, FactionStartingItemsTier.One);
                            //o.transform.localScale = spawnScale * UnityEngine.Random.Range(.75f, 1.25f);

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
                    Faction creatureFaction = cpuCreature.GetComponent<EntityInfo>().faction;
                    bool isFromPlayerFaction = ReferenceEquals(creatureFaction, ClientCommand.instance.clientPlayerCharacterHandle.entityInfo.faction);
                    if(!isFromPlayerFaction)
                    {
                        activeCPUCreatures.RemoveAt(i);
                        if (creatureFaction != null)
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
            }
            else
            {
                activeCPUCreatures.RemoveAt(i);
            }
        }
    }

    void PlaceTerrainAndWater(ChunkData cd)
    {

        TerrainMesh.Clear();
        WaterMesh.Clear();

        // initialize properties for meshes
        TerrainVertices = new Vector3[(ChunkSize + 2) * (ChunkSize + 2)];
        WaterVertices = new Vector3[(ChunkSize + 2) * (ChunkSize + 2)];
        TerrainTriangles = new int[(ChunkSize + 2) * (ChunkSize + 2) * 6];
        WaterTriangles = new int[(ChunkSize + 2) * (ChunkSize + 2) * 6];
        TerrainUvs = new Vector2[TerrainVertices.Length];
        TerrainColors = new Color[TerrainVertices.Length];


        // set terrain vertices according to HeightMap, and set colors
        // NOTE: vertex index = (z * (ChunkSize + 2) + x
        float height, height_water;
        float temperature;
        float humidity;
        for (int i = 0, z = 0; z < ChunkSize + 2; z++)
        {
            for (int x = 0; x < ChunkSize + 2; x++)
            {
                height = HeightMap[x, z] * Amplitude;
                height_water = HeightMapWater[x, z] * Amplitude;
                temperature = TemperatureMap[x, z];
                humidity = HumidityMap[x, z];
                TerrainVertices[i] = new Vector3(x + xOffset, height, z + zOffset);
                WaterVertices[i] = new Vector3(x + xOffset, height_water, z + zOffset);
                TerrainColors[i] = SetVertexColor(x + xOffset, z + zOffset, HeightMap[x, z], MountainMap[x, z], temperature, humidity, WetnessMap[x, z], FreshWaterMap[x, z]);
                i++;
            }
        }
        TerrainMesh.vertices = TerrainVertices;
        WaterMesh.vertices = WaterVertices;
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
        TerrainMesh.triangles = TerrainTriangles;
        WaterMesh.triangles = WaterTriangles;

        // set up normals
        TerrainMesh.RecalculateNormals();
        WaterMesh.RecalculateNormals();


        // set up UVs, and place features based on normal value
        int normalIndex;
        Vector3[] normals = TerrainMesh.normals;
        float[,] yNormals = new float[ChunkSize + 2, ChunkSize + 2];

        for (int i = 0, z = 0; z < ChunkSize + 2; z++)
        {
            for (int x = 0; x < ChunkSize + 2; x++)
            {

                // uv
                TerrainUvs[i] = new Vector2(x + xOffset, z + zOffset);

                // features
                if(z > 0 && x > 0){
                    normalIndex = (z * (ChunkSize + 2)) + x;
                    yNormals[x, z] = normals[normalIndex].y;
                }
                
                
                ++i;
            }
        }
        TerrainMesh.uv = TerrainUvs;

        MeshCollider mc = Terrain.AddComponent<MeshCollider>();
        mc.sharedMaterial = physicMaterial;

        cd.YNormalsMap = yNormals;



    }


    // void UpdateWaterPosition(){
    //     Vector3 pos = playerTransform.position;
    //     pos.y = SeaLevel * Amplitude * TerrainScaleModifier;
    //     Water.transform.position = pos;
    // }

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
        //Debug.Log("chunkCoord: " + chunkCoord);
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

