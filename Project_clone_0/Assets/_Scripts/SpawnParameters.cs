
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DensityCalculationType { Binary, DenserAtAverage, DenserAtMinimum, DenserAtMaximum }


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
    public bool bundle;

    public SpawnParameters(float scale, Vector2 heightRange, Vector2 temperatureRange, Vector2 humidityRange, Vector2 yNormalRange, Vector2 densityRange, Enum densityCalculationType_height, Enum densityCalculationType_temperature, Enum densityCalculationType_humidity, Enum densityCalculationType_yNormal, float slantMagnitude, float numberToSpawn, bool bundle)
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
        this.bundle = bundle;
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
    static Vector2 hgtBank = new Vector2(ChunkGenerator.SeaLevel, ChunkGenerator.BankLevel);
    static Vector2 hgtWaterAndBank = new Vector2(ChunkGenerator.SeaLevel - ChunkGenerator.meter * .25f, ChunkGenerator.BankLevel);
    static Vector2 hgtDry = new Vector2(ChunkGenerator.BankLevel, 1f);
    static Vector3 hgtDryNoSnow = new Vector2(ChunkGenerator.BankLevel, ChunkGenerator.SnowLevel - .005f);
    static Vector3 hgtBankAndDry = new Vector2(ChunkGenerator.SeaLevel, 1f);
    static Vector2 normGrass = new Vector2(ChunkGenerator.GrassNormalMin, 1f);
    static Vector2 normCliff = new Vector2(ChunkGenerator.CaveNormal, ChunkGenerator.GrassNormalMin);
    static Vector2 normCave = new Vector2(0f, ChunkGenerator.CaveNormal);
    public static Dictionary<string, SpawnParameters> SpawnParametersDict = new Dictionary<string, SpawnParameters>(){

        {"Empty", new SpawnParameters(0f, none, none, none, none, none, DensityCalculationType.Binary, DensityCalculationType.Binary, DensityCalculationType.Binary, DensityCalculationType.Binary, 0f, 0, false)},


        // trees
        {"TreeAcacia", new SpawnParameters(.5f, hgtDry, q3, q3, normGrass, new Vector2(.03f, .06f), DensityCalculationType.Binary, DensityCalculationType.DenserAtAverage, DensityCalculationType.DenserAtAverage, DensityCalculationType.Binary, 1f, 1, false)},
        {"TreeJungle", new SpawnParameters(.5f, hgtDry, q4, q4, normGrass, new Vector2(.7f, .7f), DensityCalculationType.Binary, DensityCalculationType.DenserAtAverage, DensityCalculationType.DenserAtAverage, DensityCalculationType.Binary, 1f, 1, false)},
        {"TreeFir", new SpawnParameters(1f, hgtDry, h1, h2, new Vector2(.85f, 1f), new Vector2(.25f, .5f), DensityCalculationType.Binary, DensityCalculationType.DenserAtMaximum, DensityCalculationType.DenserAtMaximum, DensityCalculationType.Binary, .25f, 3, false)},
        {"TreePalm", new SpawnParameters(.625f, hgtDry, q4, h2, normGrass, new Vector2(.1f, .1f), DensityCalculationType.Binary, DensityCalculationType.DenserAtAverage, DensityCalculationType.DenserAtAverage, DensityCalculationType.Binary, 1f, 1, false)},
        //{"TreeOak", new FeatureAttributes(2f, hgtDry, h1, q1, new Vector2(.1f, .4f), false)},

        // stones
        {"Stone", new SpawnParameters(1f, hgtBankAndDry, new Vector2(0f, .75f), all, new Vector2(.8f, 1f), new Vector2(.01f, .2f), DensityCalculationType.Binary, DensityCalculationType.DenserAtAverage, DensityCalculationType.DenserAtAverage, DensityCalculationType.DenserAtMinimum, 0f, 3, true)},
        {"StoneDesert", new SpawnParameters(5f, hgtBankAndDry, q4, q1, new Vector2(.75f, .8f), new Vector2(0f, .5f), DensityCalculationType.Binary, DensityCalculationType.Binary, DensityCalculationType.Binary, DensityCalculationType.Binary, 0f, 1, true)},
        {"StoneAll", new SpawnParameters(5f, hgtBankAndDry, all, all, new Vector2(.25f, .75f), new Vector2(.29f, .4f), DensityCalculationType.Binary, DensityCalculationType.Binary, DensityCalculationType.Binary, DensityCalculationType.Binary, 0f, 1, true)},

        // smaller plants
        //{"Grass", new SpawnParameters(.5f, hgtWaterAndBank, all, all, normGrass, new Vector2(.1f, .8f), DensityCalculationType.DenserAtMinimum, DensityCalculationType.DenserAtAverage, DensityCalculationType.DenserAtAverage, DensityCalculationType.Binary, 1f, 3, true)},
        {"Plant", new SpawnParameters(1f, hgtDryNoSnow, h1, h2, new Vector2(.95f, 1f), new Vector2(.99f, 1f), DensityCalculationType.Binary, DensityCalculationType.Binary, DensityCalculationType.Binary, DensityCalculationType.Binary, 1f, 1, true)},
        {"Reed", new SpawnParameters(1f, hgtWaterAndBank, all, all, new Vector2(.75f, .1f), new Vector2(.3f, .5f), DensityCalculationType.Binary, DensityCalculationType.Binary, DensityCalculationType.Binary, DensityCalculationType.DenserAtMaximum, 0f, 4, true)},
        //{"Mushroom", new SpawnParameters(1.5f, hgtDryNoSnow, all, h2, new Vector2(.95f, 1f), new Vector2(.09f, .1f), DensityCalculationType.Binary, DensityCalculationType.Binary, DensityCalculationType.Binary, DensityCalculationType.Binary, 1f, 1, true)},
        {"Bush", new SpawnParameters(.375f, hgtDryNoSnow, h1, all, normGrass, new Vector2(.07f, .25f), DensityCalculationType.Binary, DensityCalculationType.DenserAtAverage, DensityCalculationType.DenserAtAverage, DensityCalculationType.DenserAtMaximum, 1f, 2, true)},
        {"DeadBush", new SpawnParameters(.75f, hgtDryNoSnow, h2, h1, normGrass, new Vector2(.15f, .15f), DensityCalculationType.Binary, DensityCalculationType.Binary, DensityCalculationType.Binary, DensityCalculationType.Binary, 1, 1f, true)},
        {"Cactus", new SpawnParameters(.625f, hgtDryNoSnow, q4, q1, new Vector2(.98f, 1f), new Vector2(.049f, .05f), DensityCalculationType.Binary, DensityCalculationType.Binary, DensityCalculationType.Binary, DensityCalculationType.Binary, 1, 0f, false)},

        // creatures
        {"WildBear", new SpawnParameters(1f, hgtDry, h1, h2, normGrass, new Vector2(.007f, .007f), DensityCalculationType.Binary, DensityCalculationType.DenserAtAverage, DensityCalculationType.DenserAtAverage, DensityCalculationType.Binary, 0f, 1, true)},
        {"WildDeer", new SpawnParameters(1f, hgtDry, h1, h2, normGrass, new Vector2(.007f, .007f), DensityCalculationType.Binary, DensityCalculationType.DenserAtAverage, DensityCalculationType.DenserAtAverage, DensityCalculationType.Binary, 0f, 1, true)},
        
        // items
        {"StoneSmall", new SpawnParameters(1f, hgtBankAndDry, all, all, new Vector2(.8f, 1f), new Vector2(.01f, .05f), DensityCalculationType.Binary, DensityCalculationType.Binary, DensityCalculationType.Binary, DensityCalculationType.Binary, 0f, 3, true)},
        {"WoodPiece", new SpawnParameters(1f, hgtDry, h1, h2, new Vector2(ChunkGenerator.GrassNormalMin, .96f), new Vector2(.1f, .2f), DensityCalculationType.Binary, DensityCalculationType.DenserAtMaximum, DensityCalculationType.DenserAtMaximum, DensityCalculationType.Binary, 0f, 1, false)},
    
    
        // AI faction leader
        //{"Npc", new SpawnParameters(1f, hgtDry, all, all, new Vector2(.96f, 1f), new Vector2(.1f, .1f), DensityCalculationType.Binary, DensityCalculationType.Binary, DensityCalculationType.Binary, DensityCalculationType.Binary, 0f, 1, false)},


    
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

