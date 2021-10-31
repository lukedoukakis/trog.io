using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class ChunkGenerator : MonoBehaviour
{
    public static ChunkGenerator current;
    public static int Seed = 75675;
    public static int ChunkSize = 30;
    public static int ChunkRenderDistance = 7;
    public static float Scale = 120f;
    public static float ElevationAmplitude = 5400f;
    public static float MountainMapScale = 80f * 2f;
    public static float ElevationMapScale = 3000f;
    public static int TemperatureMapScale = 600;
    public static int HumidityMapScale = 600;
    public static float meter = 1f / ElevationAmplitude;
    public static float FlatLevel = .85f;
    public static float SeaLevel = FlatLevel - (meter * .06f); //0.849985f;
    public static float BankLevel = SeaLevel + meter;
    public static float SnowLevel = .871f;
    //public static float SnowLevel = float.MaxValue;
    public static float GrassNormalMin = .6f;
    public static float GrassNormalMax = .98f;
    public static float SnowNormal = .57f;
    public static float CaveNormal = .4f;
    public static bool LoadingChunks, DeloadingChunks;
    static GameObject Chunk;
    static GameObject Terrain;
    static GameObject Water;

    public Transform playerT;
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
    public float mountainSize;



    // features and creatures
    Transform FeaturesParent;
    public static List<GameObject> activeCreatures;
    public static List<GameObject> Features, Creatures, Items;
    public static readonly float creatureDespawnTimestep = 5f;
    public static float creatureDespawnTime = 0f;
    



    // Start is called before the first frame update
    void Awake()
    {
        current = this;
        Init();

    }

    private void Update()
    {
        if (playerT != null)
        {
            if(!LoadingChunks && !DeloadingChunks){
                LoadingChunks = true;
                DeloadingChunks = true;
                StartCoroutine(CallForSpawnGeneration());
                UpdateChunksToLoad();
                StartCoroutine(LoadChunks());
                StartCoroutine(DeloadChunks());
            }

            creatureDespawnTime += Time.deltaTime;
            if(creatureDespawnTime >= creatureDespawnTimestep){
                DespawnCreatures();
                creatureDespawnTime = creatureDespawnTime - creatureDespawnTimestep;
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
        Items = Item.Items.Values.Select(item => item.worldObjectPrefab).Where(o => o != null).ToList();

        activeCreatures = new List<GameObject>();

    
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


        playerRawPosition = playerT.position;
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
        foreach (ChunkData cd in ChunkDataToLoad.OrderBy(c => Vector3.Distance(GetChunkCoordinate(playerT.position), c.coordinate)).ToArray())
        {
            if (!cd.loaded)
            {
                load = LoadChunk(cd);
                yield return StartCoroutine(load);
                ChunkDataLoaded.Add(cd);
            }
            if(GetChunkCoordinate(playerT.position) != playerChunkCoordinate){
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

                //temperatureValue = Mathf.InverseLerp(.4f, .6f, temperatureValue);
                temperatureValue = Mathf.Clamp01(temperatureValue);

                // lock temperature
                //temperatureValue = .99f;
                //temperatureValue = .25f;






                // ElevationMap

                float e = Mathf.PerlinNoise((x + xOffset - Seed + 100000.01f) / ElevationMapScale, (z + zOffset - Seed + 100000.01f) / ElevationMapScale);
                e = Mathf.Clamp01(e);
                elevationValue = Mathf.Pow(e + .5f, .5f) - 1f;
                if(elevationValue > .00005f){
                    elevationValue = .00005f;
                }
                float bm0 = Mathf.PerlinNoise((x + xOffset + 100000.01f) / 1600f, (z + zOffset + 100000.01f) / 1600f) * 2f - 1f;
                float bm1 = Mathf.PerlinNoise((x + xOffset + 200000.01f) / 1600f, (z + zOffset + 200000.01f) / 1600f) * 2f - 1f;
                float bigMound = Mathf.Min(bm0, bm1);
                bigMound = Mathf.Pow(Mathf.Abs(bigMound) * -1f + 1f, 6f);
                float bigMoundCap = .04f;
                bigMound *= bigMoundCap;
                bigMound *= (1f - Mathf.InverseLerp(.25f, .75f, temperatureValue));
                //bigMound = 0f;
                elevationValue += bigMound;
                float maxE = Mathf.Pow(1f + .5f, .5f) - 1f;
                float minE = Mathf.Pow(0f + .5f, .5f) - 1f;
                // -------------------------------------------------------

                // HumidityMap [0, 1]
                humidityValue = Mathf.PerlinNoise((x + xOffset - Seed + .01f) / HumidityMapScale, (z + zOffset - Seed + .01f) / HumidityMapScale);
                //humidityValue += (mountainValue / mtnCap) * .5f;
                //humidityValue = Mathf.Clamp01(humidityValue);
                //humidityValue = Mathf.InverseLerp(.4f, .6f, humidityValue);

                // lock humidity
                humidityValue = 0f;
                //humidityValue = .99f;
                // -------------------------------------------------------

                // MountainMap [0, 1]
                float mtn0 = Mathf.PerlinNoise((x + xOffset - Seed + .01f) / MountainMapScale, (z + zOffset - Seed + .01f) / MountainMapScale);
                float mtn1 = Mathf.PerlinNoise((x + xOffset - Seed + 100000.01f) / MountainMapScale, (z + zOffset - Seed + 100000.01f) / MountainMapScale);
                float mtn2 = Mathf.PerlinNoise((x + xOffset - Seed + 200000.01f) / MountainMapScale, (z + zOffset - Seed + 200000.01f) / MountainMapScale);

                mountainValue = Mathf.Min(mtn0, mtn1, mtn2);
                //mountainValue = Mathf.InverseLerp(.3f, .7f, mountainValue);
                //mountainValue = .99f;
                //mountainValue *= .75f;
                mountainValue = Mathf.Pow(mountainValue, 2f);
                //mountainValue = Mathf.InverseLerp(0f, Mathf.Pow(.75f, 2f), mountainValue);
                mountainValue *= Mathf.InverseLerp(minE, maxE+1f, elevationValue);
                mountainValue *= 1f - CalculateDesertness(temperatureValue, humidityValue);
                if(mountainValue > mountainSize){
                    mountainValue = mountainSize;
                }
                //Debug.Log(mountainValue);


                // -------------------------------------------------------



                // FreshWaterMap [0, 1]

                if (bigMound < .1f)
                {
                    float riverScale = 180f;
                    riverScale = 900f;

                    // main river path
                    freshWaterValue = Mathf.PerlinNoise((x + xOffset - Seed + .01f) / riverScale, (z + zOffset - Seed + .01f) / riverScale) * 2f - 1f;

                    // give rivers character
                    float character = .1f;
                    rough = Mathf.PerlinNoise((x + xOffset + .01f) / 40f, (z + zOffset + .01f) / 40f) * 2f - 1f;
                    freshWaterValue += Mathf.Max(0f, rough) * character;

                    // give rivers roughness
                    if(freshWaterValue > .99f){
                        rough = Mathf.PerlinNoise((x + xOffset + .01f) / 1f, (z + zOffset + .01f) / 1f) * 2f - 1f;
                        freshWaterValue += rough * .1f;
                    }
                    

                    // ridgify
                    freshWaterValue = Mathf.Abs(freshWaterValue);
                    freshWaterValue *= -1f;
                    freshWaterValue += 1f;

                    freshWaterValue = Mathf.Clamp01(freshWaterValue);
                    //Debug.Log(freshWaterValue);
                    freshWaterValue = Mathf.Pow(freshWaterValue, Mathf.Lerp(3f, 10f, CalculateDesertness(temperatureValue, humidityValue)));

                    // reduce fresh water value proportionally to mound height
                    //freshWaterValue *= 1f - (Mathf.InverseLerp(.25f, 1f, (bigMound / bigMoundCap)));

                }
                else{
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

                heightValue = 0f;
                float amplitude = 1;
                float frequency = 1;
                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = (x + xOffset) / Scale * frequency + Seed;
                    float sampleZ = (z + zOffset) / Scale * frequency + Seed;
                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleZ) * 2 - 1;
                    heightValue += perlinValue * amplitude;
                    amplitude *= persistance * Mathf.Lerp(.25f, 1f, freshWaterValue) * (1f - CalculateDesertness(temperatureValue, humidityValue));
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
                // float elevationFactor = Convert.ToSingle(elevationValue > 0) * 2f - 1f;
                // heightValue += elevationFactor * .00005f;
                heightValue += elevationValue * .5f;

                // badland effect in deserts
                float desertness = CalculateDesertness(temperatureValue, humidityValue);
                if(desertness > 0f)
                {   
                    if(heightValue > FlatLevel + meter){
                        float postHeight = Posterize(FlatLevel + meter, 1f, heightValue, 100, .95f);
                        float badland = desertness;
                        heightValue = Mathf.Lerp(heightValue, postHeight, badland);
                    }

                    float duneMagnitude = meter * 4f;
                    float duneHeightModifier = duneMagnitude * Mathf.Pow(Mathf.Abs(Mathf.Sin((x + xOffset + (30f * Mathf.PerlinNoise(0, (z + zOffset) / 60f)) + .01f) / 15f)) * -1f + 1f, 2f);
                    heightValue += duneHeightModifier;
                
                }


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
                    //heightValue = Mathf.Lerp(heightValue, SeaLevel - .0001f, freshWaterValue * (1f - Mathf.InverseLerp(0f, .1f, bigMound)));
                }


                // -------------------------------------------------------

                //posterize all land
                //heightValue = Posterize(SeaLevel - .0001f, 1f, heightValue, 350, .1f);
                //heightValue = Posterize(SeaLevel - .0001f, 1f, heightValue, 750, .75f);


                // dip
                if(heightValue < SeaLevel - .0001f){
                    heightValue = SeaLevel - (.0005f);
                }

                // TreeMap
                treeValue = true;



                // completely flatten terrain
                //heightValue = FlatLevel + .001f;




                // -------------------------------------------------------

                TemperatureMap[x, z] = temperatureValue;
                HumidityMap[x, z] = humidityValue;
                MountainMap[x, z] = mountainValue / mountainSize;
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
        float desertness = Mathf.Min(Mathf.InverseLerp(.75f, 1f, temp), Mathf.InverseLerp(.75f, 1f, (1f - humid)));
        //Debug.Log(desertness);
        return desertness;
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

    Color SetVertexColor(int x, int z, float height, float mountain, float temperature, float humidity, float wetness, float fw, float rockiness)
    {

        Color c = new Color();

        // desertness (c.r)
        c.r = CalculateDesertness(temperature, humidity);
        //Debug.Log(c.r);

        return c;

        
    }



    public static IEnumerator GenerateSpawns(ChunkData cd)
    {

        float _xOffset = (int)cd.coordinate.x * ChunkSize;
        float _zOffset = (int)cd.coordinate.y * ChunkSize;

        for (int z = 0; z < ChunkSize; z++)
        {
            for (int x = 0; x < ChunkSize; x++)
            {

                float yNormal = cd.YNormalsMap[x, z];
                float skewHoriz = cd.SkewHorizMap[x, z];
                float height = cd.HeightMap[x, z];
                float temp = cd.TemperatureMap[x, z];
                float humid = cd.HumidityMap[x, z];

                SpawnParameters spawnParameters;
                float placementDensity;
                float randomDivisorOffset;
                string bundleName;
                string bundleName_last = "";
                float bundleIteration = 0f;
                Vector3 randomPositionOffset, spawnPosition, spawnScale;
                GameObject o;

                //yield return new WaitUntil(() => !cd.IsEdgeChunk());
                //yield return new WaitForSecondsRealtime(2f);

                foreach (GameObject feature in Features.Concat(Items))
                {

                    // break if chunk not loaded
                    if (cd == null || (cd.featuresParent == null)) { break; }

                    spawnParameters = SpawnParameters.GetSpawnParameters(feature.name);
                    if (spawnParameters != null)
                    {
                        //Debug.Log(feature.name);
                        placementDensity = SpawnParameters.GetPlacementDensity(spawnParameters, temp, humid, height, yNormal);
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
                                bundleName = SpawnParameters.GetBundleName(feature.name);
                                for (int i = 0; i < spawnParameters.numberToSpawn; ++i)
                                {
                                    randomPositionOffset = 2f * ((Vector3.right * (UnityEngine.Random.value * 2f - 1f)) + (Vector3.forward * (UnityEngine.Random.value * 2f - 1f)));
                                    Vector3 rawHorizontalPosition = new Vector3(x + _xOffset + skewHoriz + randomPositionOffset.x, 0f, z + _zOffset + skewHoriz + randomPositionOffset.z);
                                    ChunkData chunkAtPosition = GetChunkFromRawPosition(new Vector3(rawHorizontalPosition.x, 0f, rawHorizontalPosition.z));
                                    if (chunkAtPosition != null)
                                    {
                                        Vector2 coordinatesInChunk = GetCoordinatesInChunk(rawHorizontalPosition);
                                        int posChunkX = (int)coordinatesInChunk.x;
                                        int posChunkZ = (int)coordinatesInChunk.y;
                                        float posChunkHeight = chunkAtPosition.HeightMap[posChunkX, posChunkZ];
                                        float posChunkSkewHoriz = chunkAtPosition.SkewHorizMap[posChunkX, posChunkZ];
                                        float posChunkYNormal = chunkAtPosition.YNormalsMap[posChunkX, posChunkZ];
                                        placementDensity = SpawnParameters.GetPlacementDensity(spawnParameters, temp, humid, posChunkHeight, posChunkYNormal);
                                        if (placementDensity > 0)
                                        {
                                            spawnPosition = rawHorizontalPosition + Vector3.up * (posChunkHeight * ChunkGenerator.ElevationAmplitude) + Vector3.right * posChunkSkewHoriz + Vector3.forward * posChunkSkewHoriz;
                                            spawnScale = Vector3.one * spawnParameters.scale;
                                            o = Utility.InstantiateSameName(feature, spawnPosition, Quaternion.AngleAxis(UnityEngine.Random.Range(0f, 360f), Vector3.up));
                                            o.transform.SetParent(cd.featuresParent);
                                            o.transform.localScale = spawnScale * UnityEngine.Random.Range(.5f, 1.25f);

                                            bool noBundle = (bundleName == bundleName_last && !spawnParameters.bundle);
                                            bundleName_last = bundleName;

                                            if (noBundle)
                                            {
                                                bundleIteration = 0f;
                                                //break;
                                            }
                                            else
                                            {
                                                ++bundleIteration;
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
                        placementDensity = SpawnParameters.GetPlacementDensity(spawnParameters, temp, humid, height, yNormal);
                        int placementOffsetX = (int)((Mathf.InverseLerp(Int32.MinValue, Int32.MaxValue, creature.name.GetHashCode()) * 2f - 1f) * 50f);
                        int placementOffsetZ = (int)((Mathf.InverseLerp(Int32.MinValue, Int32.MaxValue, (creature.name + "_").GetHashCode()) * 2f - 1f) * 50f);
                        //Debug.Log(placementOffsetX);
                        //Debug.Log(placementOffsetZ);
                        //placementDensity = .1f;
                        if (placementDensity > 0f)
                        {
                            randomDivisorOffset = 0;
                            int divisor = (int)(Mathf.Lerp(5f, 100f, 1f - placementDensity) + randomDivisorOffset);
                            if (divisor < 1) { divisor = 1; }
                            if ((x + _xOffset + placementOffsetX) % divisor == 0 && (z + _zOffset + placementOffsetZ) % divisor == 0)
                            {
                                bundleName = SpawnParameters.GetBundleName(creature.name);
                                spawnPosition = new Vector3(x + _xOffset, height * ElevationAmplitude + 10f, z + _zOffset);
                                spawnScale = Vector3.one * spawnParameters.scale;
                                o = Utility.InstantiateSameName(creature, spawnPosition, Quaternion.identity);
                                o.transform.localScale = spawnScale * UnityEngine.Random.Range(.75f, 1.25f);
                                activeCreatures.Add(o);

                                bool breaker = (bundleName == bundleName_last && !spawnParameters.bundle);
                                bundleName_last = bundleName;

                                if (breaker) { break; }
                            }
                        }
                    }

                }

            }
            yield return null;
        }


    }


    void DespawnCreatures(){
        float despawnDistance = ChunkSize * ChunkRenderDistance;
        GameObject creature;
        for(int i = 0; i < activeCreatures.Count; ++i){
            creature = activeCreatures[i];
            if(creature != null){
                if(Vector3.Distance(playerRawPosition, activeCreatures[i].transform.position) > despawnDistance){
                    activeCreatures.RemoveAt(i);
                    GameObject.Destroy(creature);
                }
            }
            else{
                activeCreatures.RemoveAt(i);
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
                rockiness += (Mathf.PerlinNoise((x + xOffset) / 2f, (z + zOffset) / 2f) * 2f - 1f) * (.05f * (1f - MountainMap[x, z]));
                rockiness *= Mathf.InverseLerp(0f, .1f, MountainMap[x, z]);
                rockiness *= 1f - (CalculateDesertness(temperature, humidity));
                skewHoriz = ((rockiness + .5f) * 2f - 1f) * 18f * RockProtrusion;
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
        float yNormal;
        Vector3[] normals = TerrainMesh.normals;
        float[,] yNormalsMap = new float[ChunkSize + 2, ChunkSize + 2];

        for (int i = 0, z = 0; z < ChunkSize + 2; z++)
        {
            for (int x = 0; x < ChunkSize + 2; x++)
            {

                // uv
                TerrainUvs[i] = new Vector2((float)x + xOffset, (float)z + zOffset);

                // features
                if(z > 0 && x > 0){
                    normalIndex = (z * (ChunkSize + 2)) + x;
                    yNormal = normals[normalIndex].y;
                    yNormalsMap[x, z] = yNormal;
                    //StartCoroutine(GenerateSpawns(cd, yNormal, x, z, xOffset, zOffset, skewHorizMap[x, z]));
                    //StartCoroutine(cd.GenerateSpawns(yNormal, x, z, xOffset, zOffset, skewHorizMap[x, z]));
                }
                
                
                i++;
            }
        }
        TerrainMesh.uv = TerrainUvs;

        MeshCollider mc = Terrain.AddComponent<MeshCollider>();
        mc.sharedMaterial = physicMaterial;

        cd.YNormalsMap = yNormalsMap;
        cd.SkewHorizMap = skewHorizMap;

    }


    void UpdateWaterPosition(){
        Vector3 pos = playerT.position;
        pos.y = SeaLevel * ElevationAmplitude;
        Water.transform.position = pos;
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