﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class ChunkGenerator : MonoBehaviour
{



    public static ChunkGenerator current;
    public static int Seed = 75675;
    public static int ChunkSize = 30;
    public static int ChunkRenderDistance = 4;
    public static float Scale = 120f;
    public static float ElevationAmplitude = 5400f;
    public static float MountainMapScale = 80f;
    public static float ElevationMapScale = 16000f;
    public static int TemperatureMapScale = 800;
    public static int HumidityMapScale = 800;
    public static float meter = 1f / ElevationAmplitude;
    public static float FlatLevel = .85f;
    public static float SeaLevel = FlatLevel - (meter * .06f); //0.849985f;
    public static float BankLevel = SeaLevel + meter;
    public static float SnowLevel = .861f;
    public static float GrassNormal = .8f;
    public static float SnowNormal = .8f;
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
    [SerializeField] Material[] SnowMaterials;
    [SerializeField] Material[] FadeMaterials;


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
    [Range(0, 1)] public float mountainSize;



    // features and creatures
    Transform FeaturesParent;
    public static List<GameObject> activeCreatures;
    public static List<GameObject> Features;
    public static List<GameObject> Creatures;
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

            foreach(Material mat in FadeMaterials)
            {
                mat.SetVector("_TargetVector", GameManager.current.localPlayerHandle.entityPhysics.obstacleHeightSense.position);
            }

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
        currentChunkCoord = Vector2.positiveInfinity;

        Water = Instantiate(waterPrefab);

        layerMask_terrain = LayerMask.GetMask("Terrain");


        // set grass material parameters
        terrainMaterial.SetFloat("_WaterHeight", SeaLevel * ElevationAmplitude + .5f);
        grassMaterial.SetFloat("_WaterHeight", SeaLevel * ElevationAmplitude + .5f);
        grassMaterial.SetFloat("_GrassNormal", GrassNormal);
        foreach(Material mat in SnowMaterials){
            //mat.SetFloat("_SnowMinimumSurfaceNormal", SnowNormal);
            mat.SetFloat("_SnowHeightStart", (ChunkGenerator.SnowLevel - .13f) * ChunkGenerator.ElevationAmplitude);
            mat.SetFloat("_SnowHeightCap", ChunkGenerator.SnowLevel * ChunkGenerator.ElevationAmplitude);
        }

        Features = new List<GameObject>(Resources.LoadAll<GameObject>("Terrain/Features"));
        Creatures = new List<GameObject>(Resources.LoadAll<GameObject>("Terrain/Creatures"));

        activeCreatures = new List<GameObject>();

    
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
        FeaturesParent = cd.featuresParent;
        xIndex = (int)(cd.coord.x);
        zIndex = (int)(cd.coord.y);
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

                temperatureValue = 1f * Mathf.PerlinNoise((x + xOffset + .001f) / TemperatureMapScale, (z + zOffset + .001f) / TemperatureMapScale);
                //temperatureValue -= 1f * (mountainValue / mtnCap);
                // temperatureValue +=  (Mathf.PerlinNoise((x + xOffset + .001f) / TemperatureMapScale, (z + zOffset + .001f) / TemperatureMapScale) * 2f - 1f) * (3f * (1f - mountainValue/mtnCap));

                temperatureValue = Mathf.InverseLerp(.25f, .75f, temperatureValue);
                //temperatureValue = Mathf.Clamp01(temperatureValue);

                temperatureValue = .25f;






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
                bigMound = Mathf.Pow(Mathf.Abs(bigMound) * -1f + 1f, 5f);
                float bigMoundCap = .036f;
                bigMound *= bigMoundCap * (1f - Mathf.InverseLerp(.25f, .75f, temperatureValue));
                elevationValue += bigMound;
                float maxE = Mathf.Pow(1f + .5f, .5f) - 1f;
                float minE = Mathf.Pow(0f + .5f, .5f) - 1f;
                // -------------------------------------------------------

                // MountainMap [0, 1]
                float mtn0 = Mathf.PerlinNoise((x + xOffset - Seed + .01f) / MountainMapScale, (z + zOffset - Seed + .01f) / MountainMapScale);
                float mtn1 = Mathf.PerlinNoise((x + xOffset - Seed + 100000.01f) / MountainMapScale, (z + zOffset - Seed + 100000.01f) / MountainMapScale);
                float mtn3 = Mathf.PerlinNoise((x + xOffset - Seed + 200000.01f) / MountainMapScale, (z + zOffset - Seed + 200000.01f) / MountainMapScale);

                mountainValue = Mathf.Min(mtn0, mtn1, mtn3);
                //mountainValue = Mathf.InverseLerp(.3f, .7f, mountainValue);
                //mountainValue = .99f;
                mountainValue *= .75f;
                mountainValue = Mathf.Pow(mountainValue, 2f);
                //mountainValue = Mathf.InverseLerp(0f, Mathf.Pow(.75f, 2f), mountainValue);
                mountainValue *= Mathf.InverseLerp(minE, maxE+1f, elevationValue);
                mountainValue *= 1f;
                if(mountainValue > mountainSize){
                    mountainValue = mountainSize;
                }
                //Debug.Log(mountainValue);


                // -------------------------------------------------------

                // HumidityMap [0, 1]
                humidityValue = Mathf.PerlinNoise((x + xOffset - Seed + .01f) / HumidityMapScale, (z + zOffset - Seed + .01f) / HumidityMapScale);
                //humidityValue += (mountainValue / mtnCap) * .5f;
                //humidityValue = Mathf.Clamp01(humidityValue);
                //humidityValue = Mathf.InverseLerp(.2f, .8f, humidityValue);
                humidityValue = .75f;
                // -------------------------------------------------------



                // FreshWaterMap [0, 1]

                if (bigMound < .1f)
                {
                    float riverScale = 180f;
                    riverScale = 900f;

                    // main river path
                    freshWaterValue = Mathf.PerlinNoise((x + xOffset - Seed + .01f) / riverScale, (z + zOffset - Seed + .01f) / riverScale) * 2f - 1f;

                    // give rivers character
                    rough = Mathf.PerlinNoise((x + xOffset + .01f) / 40f, (z + zOffset + .01f) / 40f) * 2f - 1f;
                    freshWaterValue += Mathf.Max(0f, rough) * .1f;

                    // give rivers roughness
                    if(freshWaterValue < .99f){
                        rough = Mathf.PerlinNoise((x + xOffset + .01f) / 1f, (z + zOffset + .01f) / 1f) * 2f - 1f;
                        //freshWaterValue += rough * .1f;
                    }
                    

                    // ridgify
                    freshWaterValue = Mathf.Abs(freshWaterValue);
                    freshWaterValue *= -1f;
                    freshWaterValue += 1f;

                    freshWaterValue = Mathf.Clamp01(freshWaterValue);
                    freshWaterValue = Mathf.Pow(freshWaterValue, 3f);

                    // reduce fresh water value proportionally to mound height
                    freshWaterValue *= 1f - (Mathf.InverseLerp(.25f, 1f, (bigMound / bigMoundCap)));

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
                    amplitude *= persistance * Mathf.Lerp(.25f, 1f, freshWaterValue);
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



                // create flatland
                // float psgScale, psgNoise, psgSteps, psgStepHeight, psg, oldPsg;
                // psgScale = 200f;
                // psgSteps = 40f;
                // psgStepHeight = (1f - FlatLevel) / psgSteps;
                
        

                // oldPsg = FlatLevel;
                // for(int i = 0; i < psgSteps; i++){
                //     if(i == 0){
                //         psgNoise = 0f;
                //         psg = FlatLevel + .005f * Mathf.PerlinNoise((x + xOffset) / psgScale*2f, (z + zOffset) / psgScale*2f);
                //     }
                //     else{
                //         //psgNoise = .007f * (Mathf.PerlinNoise((x + xOffset) / psgScale + (i * 1000f), (z + zOffset) / psgScale + (i * 1000f)) * 2f - 1f);
                //         psgNoise = .0025f * (Mathf.PerlinNoise((x + xOffset) / psgScale + (i * 1000f), (z + zOffset) / psgScale + (i * 1000f)) * 2f - 1f);
                //         psg = oldPsg + psgStepHeight + psgNoise;
                //     }
                //     if(psg >= 1f - psgStepHeight){
                //         break;
                //     }
                //     if (heightValue < psg){
                //         if (heightValue >= oldPsg){
                //             //float c = .003f * (Mathf.Pow(mountainValue, .01f) - .1f);
                //             float c = .003f;
                //             if (heightValue >= oldPsg + c){
                //                 heightValue = psg;
                //             }
                //             else{
                //                 heightValue = Mathf.Lerp(heightValue, psg, ((heightValue - oldPsg) / c) * 1f);
                //             }
                //         }
                //     }
                //     oldPsg = psg - psgNoise;
                // }


                // // badland effect in deserts
                // if(heightValue > SeaLevel + .0001f){
                //     float postHeight = Posterize(SeaLevel + .0001f, 1f, heightValue, 100, .5f);
                //     float badland = Mathf.InverseLerp(.6f, .9f, temperatureValue);
                //     heightValue = Mathf.Lerp(heightValue, postHeight, badland);
                // }

                //posterize all land
                // float postNes = .75f;
                // heightValue = Posterize(SeaLevel - .0001f, 1f, heightValue, 350, .9f + postNes);
                // heightValue = Posterize(SeaLevel - .0001f, 1f, heightValue, 750, 0f + postNes);


                // dip
                if(heightValue < SeaLevel - .0001f){
                    heightValue = SeaLevel - (.0005f);
                }

                // TreeMap
                if (heightValue > FlatLevel)
                {
                    float tree = Mathf.PerlinNoise((x + xOffset + .001f) / 100f, (z + zOffset + .001f) / 100f);
                    rough = Mathf.PerlinNoise((x + xOffset + .001f) / 20f, (z + zOffset + .001f) / 20f) * 2f - 1f;
                    tree += rough * .1f;
                    treeValue = tree > .4f;
                }
                else { treeValue = false; }


                heightValue = FlatLevel + .001f;




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
        float snow = Mathf.InverseLerp(SnowLevel - .13f, SnowLevel, height);
        c.b = Mathf.Max(0, snow) * 255f;
        //c.b = 0f;
        //c.b = 255f;

    
        // desertness (c.r)
        c.r = Mathf.Max(255f * temperature);

        return c;

        
    }



    public static IEnumerator GenerateSpawns(ChunkData cd, float temp, float humid, float wetness, float height, float x, float z, float xOffset, float zOffset, float skewHoriz){

        SpawnParameters spawnParameters;
        float placementDensity;
        float randomDivisorOffset;
        string bundleName;
        string bundleName_last = "";
        Vector3 randomPositionOffset, spawnPosition, spawnScale;
        GameObject o;

        foreach(GameObject feature in Features)
        {

            // break if chunk not loaded
            if(cd == null){ break; }

            spawnParameters = SpawnParameters.GetSpawnParameters(feature.name);
            placementDensity = SpawnParameters.GetPlacementDensity(spawnParameters, temp, humid, height);
            //placementDensity = .1f;
            if (placementDensity > 0f)
            {
                randomDivisorOffset = 15f * (Mathf.PerlinNoise((x + xOffset + .01f) / 2f, (z + zOffset + .01f) / 2f) * 2f - 1f);
                int divisor = (int)(Mathf.Lerp(1f, 20f, 1f - placementDensity) + randomDivisorOffset);
                if (divisor < 1) { divisor = 1; }
                if ((x + xOffset) % divisor == 0 && (z + zOffset) % divisor == 0)
                {
                    bundleName = SpawnParameters.GetBundleName(feature.name);
                    randomPositionOffset = 0f * (Vector3.right * (UnityEngine.Random.value * 2f - 1f)) + (Vector3.forward * (UnityEngine.Random.value * 2f - 1f));
                    spawnPosition = new Vector3(x + xOffset + skewHoriz, height * ElevationAmplitude, z + zOffset + skewHoriz) + randomPositionOffset;
                    spawnScale = Vector3.one * spawnParameters.scale;
                    o = GameObject.Instantiate(feature, spawnPosition, Quaternion.AngleAxis(UnityEngine.Random.Range(0f, 360f), Vector3.up), cd.featuresParent.transform);
                    o.transform.localScale = spawnScale * UnityEngine.Random.Range(.75f, 1.25f);

                    bool breaker = (bundleName == bundleName_last && !spawnParameters.bundle);
                    bundleName_last = bundleName;
        
                    if (breaker) { break; }
                }
            }
            yield return null;
        }

        foreach(GameObject creature in Creatures)
        {

            // break if chunk not loaded
            if(cd == null){ break; }

            spawnParameters = SpawnParameters.GetSpawnParameters(creature.name);
            placementDensity = SpawnParameters.GetPlacementDensity(spawnParameters, temp, humid, height);
            

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
                if ((x + xOffset + placementOffsetX) % divisor == 0 && (z + zOffset + placementOffsetZ) % divisor == 0)
                {
                    bundleName = SpawnParameters.GetBundleName(creature.name);
                    spawnPosition = new Vector3(x + xOffset, height * ElevationAmplitude + 10f, z + zOffset);
                    spawnScale = Vector3.one * spawnParameters.scale;
                    o = GameObject.Instantiate(creature, spawnPosition, Quaternion.identity, null);
                    o.transform.localScale = spawnScale * UnityEngine.Random.Range(.75f, 1.25f);
                    activeCreatures.Add(o);

                    // // test
                    // o.transform.position = current.playerPos;

                    bool breaker = (bundleName == bundleName_last && !spawnParameters.bundle);
                    bundleName_last = bundleName;
        
                    if (breaker) { break; }
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
                if(Vector3.Distance(playerPos, activeCreatures[i].transform.position) > despawnDistance){
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
        float rockiness;
        float skewHoriz;
        float[,] skewHorizMap = new float[ChunkSize + 2, ChunkSize + 2];
        for (int i = 0, z = 0; z < ChunkSize + 2; z++)
        {
            for (int x = 0; x < ChunkSize + 2; x++)
            {
                height = HeightMap[x, z] * ElevationAmplitude;
                rockiness = Mathf.Pow(Mathf.PerlinNoise((x + xOffset) / 50f, (z + zOffset) / 50f), 1f);
                rockiness *= Mathf.PerlinNoise(((height + (x + xOffset) + (z + zOffset)) / 20f), 0f);
                rockiness += (Mathf.PerlinNoise((x + xOffset) / 2f, (z + zOffset) / 2f) * 2f - 1f) * .02f;
                rockiness *= Mathf.InverseLerp(0f, .1f, MountainMap[x, z]);
                skewHoriz = (rockiness * 2f - 1f) * 36f * RockProtrusion;
                skewHoriz *= Mathf.InverseLerp(SeaLevel, SeaLevel + (meter * 20f), HeightMap[x, z]); // smooth down rockiness at sea level
                skewHorizMap[x, z] = skewHoriz;
                TerrainVertices[i] = new Vector3(x + xOffset + skewHoriz, height, z + zOffset + skewHoriz);
                //TerrainVertices[i] = new Vector3(x + xOffset, height, z + zOffset);
                TerrainColors[i] = SetVertexColor(x + xOffset, z + zOffset, HeightMap[x, z], MountainMap[x, z], TemperatureMap[x, z], HumidityMap[x, z], WetnessMap[x, z], FreshWaterMap[x, z], rockiness);
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
        Vector3[] normals = TerrainMesh.normals;
        for (int i = 0, z = 0; z < ChunkSize + 2; z++)
        {
            for (int x = 0; x < ChunkSize + 2; x++)
            {

                // uv
                TerrainUvs[i] = new Vector2((float)x + xOffset, (float)z + zOffset);

                // features
                if(z > 0 && x > 0 && TreeMap[x, z]){
                    normalIndex = (z * (ChunkSize + 2)) + x;
                    if(normals[normalIndex].y >= GrassNormal){
                        StartCoroutine(GenerateSpawns(cd, TemperatureMap[x, z], HumidityMap[x, z], WetnessMap[x, z], HeightMap[x, z], x, z, xOffset, zOffset, skewHorizMap[x, z]));
                    }
                }
                
                
                i++;
            }
        }
        TerrainMesh.uv = TerrainUvs;
        
    
    
        MeshCollider mc = Terrain.AddComponent<MeshCollider>();
        mc.sharedMaterial = physicMaterial;

    }


    void UpdateWaterPosition(){
        Vector3 pos = playerT.position;
        pos.y = SeaLevel * ElevationAmplitude;
        Water.transform.position = pos;
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