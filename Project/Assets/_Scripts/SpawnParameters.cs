
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;

public enum DensityCalculationType { Binary, DenserAtAverage, DenserAtMinimum, DenserAtMaximum }

public enum LoadOrder{ Primary, Secondary, Tirtiary }


public class SpawnParameters
{
    public float scale;
    public float heightMin, heightMax;
    public float temperatureMin, temperatureMax;
    public float humidityMin, humidityMax;
    public float yNormalMin, yNormalMax;
    public float densityMin, densityMax;
    Enum densityCalculationType_height, densityCalculationType_temperature, densityCalculationType_humidity, densityCalculationType_yNormal;
    public float slantMagnitude;
    public float numberToSpawn;
    public float heightOffset;
    public LoadOrder loadOrder;

    public SpawnParameters(float scale, Vector2 heightRange, Vector2 temperatureRange, Vector2 humidityRange, Vector2 yNormalRange, Vector2 densityRange, Enum densityCalculationType_height, Enum densityCalculationType_temperature, Enum densityCalculationType_humidity, Enum densityCalculationType_yNormal, float slantMagnitude, float numberToSpawn, float heightOffset, LoadOrder loadOrder)
    {
        this.scale = scale;
        this.heightMin = heightRange.x;
        this.heightMax = heightRange.y;
        this.temperatureMin = temperatureRange.x;
        this.temperatureMax = temperatureRange.y;
        this.humidityMin = humidityRange.x;
        this.humidityMax = humidityRange.y;
        this.yNormalMin = yNormalRange.x;
        this.yNormalMax = yNormalRange.y;
        this.densityMin = densityRange.x;
        this.densityMax = densityRange.y;
        this.densityCalculationType_height = densityCalculationType_height;
        this.densityCalculationType_temperature = densityCalculationType_temperature;
        this.densityCalculationType_humidity = densityCalculationType_humidity;
        this.densityCalculationType_yNormal = densityCalculationType_yNormal;
        this.slantMagnitude = slantMagnitude;
        this.numberToSpawn = numberToSpawn;
        this.heightOffset = heightOffset;
        this.loadOrder = loadOrder;

    }

    public static SpawnParameters GetSpawnParameters(string name)
    {
        string bundleName = GetBundleName(name);
        if(SpawnParametersDict.ContainsKey(bundleName))
        {
            SpawnParameters spawnParameters = SpawnParametersDict[bundleName];
            return spawnParameters;
        }
        else
        {
            return null;
        }    
    }

    public static string GetBundleName(string name){
        return name.Split(' ')[0];
    }


    static Vector3 none = new Vector2(-1f, -1f);
    static Vector2 all = new Vector2(0f, 1f);
    static Vector2 q1 = new Vector2(0f, .25f);
    static Vector2 q2 = new Vector2(.25f, .5f);
    static Vector2 q3 = new Vector2(.5f, .75f);
    static Vector2 q4 = new Vector2(.75f, 1f);
    static Vector2 h1 = new Vector2(0f, .5f);
    static Vector2 h2 = new Vector2(.5f, 1f);
    static Vector2 hgtWater = new Vector2(ChunkGenerator.SeaLevel - ChunkGenerator.meter * .05f, ChunkGenerator.SeaLevel);
    static Vector2 hgtBank = new Vector2(ChunkGenerator.SeaLevel, ChunkGenerator.SeaLevel);
    static Vector2 hgtWaterAndBank = new Vector2(ChunkGenerator.SeaLevel - ChunkGenerator.meter * .25f, ChunkGenerator.SeaLevel);
    static Vector2 hgtWaterAndAbove = new Vector2(ChunkGenerator.SeaLevel - ChunkGenerator.meter * .25f, 1f);
    static Vector2 hgtDry = new Vector2(ChunkGenerator.SeaLevel, 1f);
    static Vector3 hgtDryNoSnow = new Vector2(ChunkGenerator.SeaLevel, ChunkGenerator.SnowLevel - .005f);
    static Vector3 hgtBankAndDry = new Vector2(ChunkGenerator.SeaLevel, 1f);
    static Vector2 normGrass = new Vector2(ChunkGenerator.GrassNormal, 1f);
    static Vector2 normCliff = new Vector2(ChunkGenerator.CaveNormal, ChunkGenerator.GrassNormal);
    static Vector2 normCave = new Vector2(0f, ChunkGenerator.CaveNormal);



    public static Dictionary<string, SpawnParameters> SpawnParametersDict = new Dictionary<string, SpawnParameters>()
    {

        {"Empty", new SpawnParameters(0f, none, none, none, none, none, DensityCalculationType.Binary, DensityCalculationType.Binary, DensityCalculationType.Binary, DensityCalculationType.Binary, 0f, 0, -1f, LoadOrder.Primary)},


        // large features/stones
        {"StoneDesert", new SpawnParameters(5f, hgtBankAndDry, q4, q1, new Vector2(.75f, .8f), new Vector2(0f, .5f), DensityCalculationType.Binary, DensityCalculationType.Binary, DensityCalculationType.Binary, DensityCalculationType.Binary, 0f, 1, 0, LoadOrder.Primary)},
        {"StoneAll", new SpawnParameters(3f, hgtBankAndDry, all, all, new Vector2(.001f, .83f), new Vector2(.39f, .4f), DensityCalculationType.Binary, DensityCalculationType.Binary, DensityCalculationType.Binary, DensityCalculationType.Binary, 0f, 1, 0, LoadOrder.Primary)},
        {"StoneShore", new SpawnParameters(1.75f, hgtWaterAndBank, all, all, new Vector2(0f, .75f), new Vector2(.79f, .8f), DensityCalculationType.Binary, DensityCalculationType.Binary, DensityCalculationType.Binary, DensityCalculationType.Binary, 0f, 1, 0, LoadOrder.Primary)},
        {"Stone", new SpawnParameters(1f, hgtBankAndDry, new Vector2(0f, .75f), all, new Vector2(.75f, 1f), new Vector2(.01f, .05f), DensityCalculationType.Binary, DensityCalculationType.DenserAtAverage, DensityCalculationType.DenserAtAverage, DensityCalculationType.DenserAtMinimum, 0f, 3, 0, LoadOrder.Secondary)},

        // trees
        {"TreeAcacia", new SpawnParameters(.5f, hgtDry, q3, q3, new Vector2(.8f, 1f), new Vector2(.03f, .06f), DensityCalculationType.Binary, DensityCalculationType.DenserAtAverage, DensityCalculationType.DenserAtAverage, DensityCalculationType.Binary, 1f, 1, 0, LoadOrder.Secondary)},
        {"TreeFir", new SpawnParameters(.8f, hgtDry, h1, h2, new Vector2(.75f, 1f), new Vector2(.2f, .35f * 2.5f), DensityCalculationType.Binary, DensityCalculationType.DenserAtMaximum, DensityCalculationType.DenserAtMaximum, DensityCalculationType.Binary, 0f, 1, 0, LoadOrder.Secondary)},
        {"TreeBamboo", new SpawnParameters(.65f, hgtWaterAndAbove, q4, q4, new Vector2(.96f, 1f), new Vector2(.29f, .3f), DensityCalculationType.Binary, DensityCalculationType.Binary, DensityCalculationType.Binary, DensityCalculationType.Binary, 0f, 5, 0, LoadOrder.Secondary)},
        {"TreeBanana", new SpawnParameters(.65f, hgtBankAndDry, q4, q4, new Vector2(.96f, 1f), new Vector2(.19f, .2f), DensityCalculationType.Binary, DensityCalculationType.Binary, DensityCalculationType.Binary, DensityCalculationType.Binary, .25f, 3, 0, LoadOrder.Secondary)},
        {"TreeCoconut", new SpawnParameters(.65f, hgtBankAndDry, q4, h2, new Vector2(.96f, 1f), new Vector2(.12f, .13f), DensityCalculationType.Binary, DensityCalculationType.DenserAtAverage, DensityCalculationType.DenserAtAverage, DensityCalculationType.Binary, .25f, 3, 0, LoadOrder.Secondary)},
        {"TreeJungle", new SpawnParameters(.65f, hgtBankAndDry, q4, q4, new Vector2(.96f, 1f), new Vector2(.39f, .4f), DensityCalculationType.Binary, DensityCalculationType.Binary, DensityCalculationType.Binary, DensityCalculationType.Binary, 0f, 2, 0, LoadOrder.Secondary)},


        // smaller plants
        {"Grass", new SpawnParameters(.5f, hgtWaterAndBank, all, all, normGrass, new Vector2(.1f, .8f), DensityCalculationType.DenserAtMinimum, DensityCalculationType.DenserAtAverage, DensityCalculationType.DenserAtAverage, DensityCalculationType.Binary, 1f, 3, 0, LoadOrder.Tirtiary)},
        {"Plant", new SpawnParameters(2f, hgtWaterAndAbove, q4, q4, new Vector2(.95f, 1f), new Vector2(.29f, .3f), DensityCalculationType.Binary, DensityCalculationType.Binary, DensityCalculationType.Binary, DensityCalculationType.Binary, 1f, 1, 0, LoadOrder.Tirtiary)},
        {"Reed", new SpawnParameters(.8f, hgtWaterAndBank, all, all, new Vector2(.75f, 1f), new Vector2(.05f, .2f), DensityCalculationType.DenserAtMinimum, DensityCalculationType.Binary, DensityCalculationType.Binary, DensityCalculationType.Binary, 0f, 4, 0, LoadOrder.Tirtiary)},
        {"Mushroom", new SpawnParameters(1.5f, hgtDryNoSnow, all, h2, new Vector2(.95f, 1f), new Vector2(.09f, .1f), DensityCalculationType.Binary, DensityCalculationType.Binary, DensityCalculationType.Binary, DensityCalculationType.Binary, 1f, 1, 0, LoadOrder.Tirtiary)},
        {"BerryBush", new SpawnParameters(.375f, hgtDry, h1, all, new Vector2(.9f, 1f), new Vector2(.015f, .08f), DensityCalculationType.Binary, DensityCalculationType.DenserAtAverage, DensityCalculationType.DenserAtAverage, DensityCalculationType.DenserAtMaximum, 1f, 4, 0, LoadOrder.Secondary)},
        {"DeadBush", new SpawnParameters(.75f, hgtDry, all, h1, new Vector2(.96f, 1f), new Vector2(.15f, .15f), DensityCalculationType.Binary, DensityCalculationType.Binary, DensityCalculationType.Binary, DensityCalculationType.Binary, 1, 1, 0, LoadOrder.Secondary)},
        {"Cactus", new SpawnParameters(.625f, hgtDryNoSnow, all, q1, new Vector2(.96f, 1f), new Vector2(.049f, .05f), DensityCalculationType.Binary, DensityCalculationType.Binary, DensityCalculationType.Binary, DensityCalculationType.Binary, 1, 1, 0, LoadOrder.Secondary)},
        

        // // creatures
        //{"WildBear", new SpawnParameters(1f, hgtDry, h1, h2, normGrass, new Vector2(.007f, .007f), DensityCalculationType.Binary, DensityCalculationType.DenserAtAverage, DensityCalculationType.DenserAtAverage, DensityCalculationType.Binary, 0f, 1, 0, LoadOrder.Primary)},
        //{"WildDeer", new SpawnParameters(1f, hgtDry, h1, h2, normGrass, new Vector2(.007f, .007f), DensityCalculationType.Binary, DensityCalculationType.DenserAtAverage, DensityCalculationType.DenserAtAverage, DensityCalculationType.Binary, 0f, 1, 0, LoadOrder.Primary)},
        {"WildHorse", new SpawnParameters(1f, hgtDry, all, all, all, new Vector2(.05f, .07f), DensityCalculationType.Binary, DensityCalculationType.DenserAtAverage, DensityCalculationType.DenserAtAverage, DensityCalculationType.Binary, 0f, 1, 0, LoadOrder.Primary)},


        // // items
        {"StoneSmall", new SpawnParameters(2f, hgtBankAndDry, all, all, new Vector2(.8f, 1f), new Vector2(.005f, .005f), DensityCalculationType.Binary, DensityCalculationType.Binary, DensityCalculationType.Binary, DensityCalculationType.Binary, 0f, 3, 0, LoadOrder.Tirtiary)},
        {"WoodPiece", new SpawnParameters(1f, hgtDry, all, h2, new Vector2(.8f, 1f), new Vector2(.1f, .2f), DensityCalculationType.Binary, DensityCalculationType.DenserAtMaximum, DensityCalculationType.DenserAtMaximum, DensityCalculationType.Binary, 0f, 1, 0, LoadOrder.Tirtiary)},
    
    
        // AI faction leader
        //{"Character", new SpawnParameters(1f, hgtDry, all, all, new Vector2(.96f, 1f), new Vector2(.002f, .002f), DensityCalculationType.Binary, DensityCalculationType.Binary, DensityCalculationType.Binary, DensityCalculationType.Binary, 0f, 1, -1f, LoadOrder.Tirtiary)},


    
    };


    public static float GetPlacementDensity(SpawnParameters sp, float temp, float humid, float height, float yNorm)
    {

        //Debug.Log("Y NORM: " + yNorm);

        float dHeight, dTemp, dHumid, dYNorm;

        // height
        dHeight = CalculateDensity(sp.heightMin, sp.heightMax, height, sp.densityCalculationType_height);
        //Debug.Log("dHeight: " + dHeight);
        if(dHeight <= 0f){ return -1f; }

        // temperature
        dTemp = CalculateDensity(sp.temperatureMin, sp.temperatureMax, temp, sp.densityCalculationType_temperature);
        //Debug.Log("dTemp: " + dTemp);
        if(dTemp <= 0f){ return -1f; }

        // humidity
        dHumid = CalculateDensity(sp.humidityMin, sp.humidityMax, humid, sp.densityCalculationType_humidity);
        //Debug.Log("dHumid: " + dHumid);
        if(dHumid <= 0f){ return -1f; }

        // y normal
        dYNorm = CalculateDensity(sp.yNormalMin, sp.yNormalMax, yNorm, sp.densityCalculationType_yNormal);
        //Debug.Log("dYNorm: " + dYNorm);
        if(dYNorm <= 0f){ return -1f; }



        // combined
        float dCombined = Mathf.Min(dHeight, dTemp, dHumid, dYNorm);

        return Mathf.Lerp(sp.densityMin, sp.densityMax, dCombined);

    }

    public static float CalculateDensity(float min, float max, float value, Enum densityCalculationType)
    {

        if(!Utility.IsBetween(value, min, max)){
            return -1f;
        }

        float density;

        switch (densityCalculationType)
        {
            case DensityCalculationType.Binary :
                density = 1f;
                break;
            case DensityCalculationType.DenserAtAverage :
                density = ProximityToAverage(min, max, value);
                break;
            case DensityCalculationType.DenserAtMinimum :
                density = ProximityToMinimum(min, max, value);
                break;
            case DensityCalculationType.DenserAtMaximum :
                //Debug.Log("claculating proximity to MAXIMUM");
                density = ProximityToMaximum(min, max, value);
                break;
            default :
                density = -1f;
                break;
        }

        return density;
    }

    // returns a value between 0 and 1 according to how close the value is to the average of min and max
    static float ProximityToAverage(float min, float max, float value)
    {
        return Mathf.Min(max - value, value - min) / (max - min) * 2f;
    }

    static float ProximityToMinimum(float min, float max, float value)
    {
        return 1f - Mathf.InverseLerp(min, max, value);
    }

    static float ProximityToMaximum(float min, float max, float value)
    {
        return Mathf.InverseLerp(min, max, value);
    }



}






    


