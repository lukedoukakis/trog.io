
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class SpawnParameters
{
    public float scale;
    public float heightMin, heightMax;
    public float temperatureMin, temperatureMax;
    public float humidityMin, humidityMax;
    public float densityMin, densityMax;
    public bool bundle;

    public SpawnParameters(float scale, Vector2 heightRange, Vector2 temperatureRange, Vector2 humidityRange, Vector2 densityRange, bool bundle)
    {
        this.scale = scale;
        this.heightMin = heightRange.x;
        this.heightMax = heightRange.y;
        this.temperatureMin = temperatureRange.x;
        this.temperatureMax = temperatureRange.y;
        this.humidityMin = humidityRange.x;
        this.humidityMax = humidityRange.y;
        this.densityMin = densityRange.x;
        this.densityMax = densityRange.y;
        this.bundle = bundle;
    }

    public static SpawnParameters GetSpawnParameters(string name)
    {

        string bundleName = GetBundleName(name);
        //Debug.Log(name);
        SpawnParameters spawnParameters = SpawnParametersDict[bundleName];
        return spawnParameters;
    }

    public static string GetBundleName(string name){
        return name.Split(' ')[0];
    }


    static Vector2 all = new Vector2(0f, 1f);
    static Vector2 q1 = new Vector2(0f, .25f);
    static Vector2 q2 = new Vector2(.25f, .5f);
    static Vector2 q3 = new Vector2(.5f, .75f);
    static Vector2 q4 = new Vector2(.75f, 1f);
    static Vector2 h1 = new Vector2(0f, .5f);
    static Vector2 h2 = new Vector2(.5f, 1f);
    static Vector2 hgtWater = new Vector2(ChunkGenerator.SeaLevel - ChunkGenerator.meter * .05f, ChunkGenerator.SeaLevel);
    static Vector2 hgtBank = new Vector2(ChunkGenerator.SeaLevel, ChunkGenerator.BankLevel);
    static Vector2 hgtDry = new Vector2(ChunkGenerator.BankLevel, 1f);
    public static Dictionary<string, SpawnParameters> SpawnParametersDict = new Dictionary<string, SpawnParameters>(){

        // terrain features
        {"TreeAcacia", new SpawnParameters(.5f, hgtDry, q3, q3, new Vector2(.1f, .1f), false)},
        {"TreeJungle", new SpawnParameters(.5f, hgtDry, q4, q4, new Vector2(.7f, .7f), false)},
        {"TreeFir", new SpawnParameters(1f, hgtDry, h1, h2, new Vector2(.1f, .5f), false)},
        {"TreePalm", new SpawnParameters(.625f, hgtDry, q4, h2, new Vector2(.1f, .1f), false)},
        //{"TreeOak", new FeatureAttributes(2f, hgtDry, h1, q1, new Vector2(.1f, .4f), false)},
        {"Grass", new SpawnParameters(.5f, hgtWater, q3, q3, new Vector2(.1f, .1f), true)},
        {"Plant", new SpawnParameters(.25f, hgtDry, h2, h2, new Vector2(.1f, .8f), true)},
        {"Reed", new SpawnParameters(.25f, hgtWater, all, all, new Vector2(.42f, .42f), true)},
        {"Mushroom", new SpawnParameters(.75f, hgtDry, h2, q3, new Vector2(.1f, .1f), true)},
        {"Bush", new SpawnParameters(.375f, hgtDry, h1, all, new Vector2(.1f, .5f), true)},
        {"DeadBush", new SpawnParameters(.75f, hgtDry, h1, h1, new Vector2(.15f, .15f), true)},
        {"Cactus", new SpawnParameters(.625f, hgtDry, q1, q1, new Vector2(.1f, .1f), false)},

        // creatures
        {"WildBear", new SpawnParameters(1f, hgtDry, h1, h2, new Vector2(.6f, .6f), true)},
        {"WildDeer", new SpawnParameters(1f, hgtDry, h1, h2, new Vector2(0f, 0f), true)},
        
    };


    public static float GetPlacementDensity(SpawnParameters sp, float temp, float humid, float height){

        float dHeight, dTemp, dHumid;
        if(Utility.IsBetween(height, sp.heightMin, sp.heightMax)){
            dHeight = 1f;
            //Debug.Log("dHeight: " + dHeight);
        }
        else{
            return -1f;
        }
        if(Utility.IsBetween(temp, sp.temperatureMin, sp.temperatureMax)){
            dTemp = Mathf.Min(sp.temperatureMax - temp, temp - sp.temperatureMin) / (sp.temperatureMax - sp.temperatureMin) * 2f;
            //Debug.Log("dTemp: " + dTemp);
        }
        else
        {
            return -1;
        }
        if(Utility.IsBetween(humid, sp.humidityMin, sp.humidityMax)){
            dHumid = Mathf.Min(sp.humidityMax - humid, humid - sp.humidityMin) / (sp.humidityMax - sp.humidityMin) * 2f;
            //Debug.Log("dHumid: " + dHumid);
        }
        else
        {
            return -1;
        }
        
        float dCombined = Mathf.Min(dHeight, dTemp, dHumid);
        return Mathf.Lerp(sp.densityMin, sp.densityMax, dCombined);

    }


}

