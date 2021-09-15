
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class FeatureAttributes
{
    public float scale;
    public float heightMin, heightMax;
    public float temperatureMin, temperatureMax;
    public float humidityMin, humidityMax;
    public float densityMin, densityMax;
    public bool bundle;

    public FeatureAttributes(float scale, Vector2 heightRange, Vector2 temperatureRange, Vector2 humidityRange, Vector2 densityRange, bool bundle)
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

    public static FeatureAttributes GetFeatureAttributes(string name)
    {

        string bundleName = GetBundleName(name);
        //Debug.Log(name);
        FeatureAttributes featureAttributes = FeatureAttributesMap[bundleName];
        return featureAttributes;
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
    public static Dictionary<string, FeatureAttributes> FeatureAttributesMap = new Dictionary<string, FeatureAttributes>(){
        {"TreeAcacia", new FeatureAttributes(2f, hgtDry, q3, q3, new Vector2(.1f, .1f), false)},
        {"TreeJungle", new FeatureAttributes(2f, hgtDry, q4, q4, new Vector2(.7f, .7f), false)},
        {"TreeFir", new FeatureAttributes(2.5f, hgtDry, h1, h2, new Vector2(.1f, .4f), false)},
        {"TreePalm", new FeatureAttributes(2.5f, hgtDry, q4, h2, new Vector2(.1f, .1f), false)},
        //{"TreeOak", new FeatureAttributes(2f, hgtDry, h1, q1, new Vector2(.1f, .4f), false)},
        {"Grass", new FeatureAttributes(2f, hgtWater, q3, q3, new Vector2(.1f, .1f), false)},
        {"Plant", new FeatureAttributes(1f, hgtDry, h2, h2, new Vector2(.1f, .8f), false)},
        {"Reed", new FeatureAttributes(1f, hgtWater, all, all, new Vector2(.42f, .42f), false)},
        {"Mushroom", new FeatureAttributes(1.5f, hgtDry, h2, q3, new Vector2(.1f, .1f), false)},
        {"Bush", new FeatureAttributes(1.5f, hgtDry, h1, all, new Vector2(.1f, .5f), false)},
        {"DeadBush", new FeatureAttributes(3f, hgtDry, h1, h1, new Vector2(.15f, .15f), false)},
        {"Cactus", new FeatureAttributes(2.5f, hgtDry, q1, q1, new Vector2(.1f, .1f), false)},
    };


    public static float GetPlacementDensity(FeatureAttributes fa, float temp, float humid, float height){

        float dHeight, dTemp, dHumid;
        if(Utility.IsBetween(height, fa.heightMin, fa.heightMax)){
            dHeight = Mathf.Min(fa.heightMax - height, height - fa.heightMin);
        }
        else{
            return -1f;
        }
        if(Utility.IsBetween(temp, fa.temperatureMin, fa.temperatureMax)){
            dTemp = Mathf.Min(fa.temperatureMax - temp, temp - fa.temperatureMin);
        }
        else
        {
            return -1;
        }
        if(Utility.IsBetween(humid, fa.humidityMin, fa.humidityMax)){
            dHumid = Mathf.Min(fa.humidityMax - humid, humid - fa.humidityMin);
        }
        else
        {
            return -1;
        }
        
        float dCombined = Mathf.Min(dHeight, dTemp, dHumid);
        return Mathf.Lerp(fa.densityMin, fa.densityMax, dCombined);

    }


}

