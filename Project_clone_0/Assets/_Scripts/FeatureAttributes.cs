
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class FeatureAttributes
{
    public float scale;
    public float density;
    public bool bundle;

    public FeatureAttributes(float scale, float density, bool bundle)
    {
        this.scale = scale;
        this.density = density;
        this.bundle = bundle;
    }

    public static FeatureAttributes GetFeatureAttributes(string name, float humidity)
    {

        string bundleName = GetBundleName(name);
        FeatureAttributes featureAttributes = FeatureAttributesMap[bundleName];
        return featureAttributes;
    }

    public static string GetBundleName(string name){
        return name.Split(' ')[0];
    }

    public static Dictionary<string, FeatureAttributes> FeatureAttributesMap = new Dictionary<string, FeatureAttributes>(){
        {"AcaciaTree", new FeatureAttributes(2f, .05f, false)},
        {"JungleTree", new FeatureAttributes(2.2f, .4f, false)},
        {"FirTree", new FeatureAttributes(2.5f, .4f, false)},
        {"SnowyFirTree", new FeatureAttributes(1.5f, .4f, false)},
        {"PalmTree", new FeatureAttributes(2f, .1f, false)},
        {"OakTree", new FeatureAttributes(2f, .4f, false)},
        {"Grass", new FeatureAttributes(0f, 0f, true)},
        {"Plant", new FeatureAttributes(.5f, 1f, true)},
        {"Reed", new FeatureAttributes(1f, .7f, true)},
        {"Mushroom", new FeatureAttributes(1f, .1f, true)},
        {"BushChaparral", new FeatureAttributes(1.5f, .5f, true)},
        {"BushSavannah", new FeatureAttributes(1.5f, .2f, true)},
        {"BushForest", new FeatureAttributes(1.5f, .2f, true)},
        {"DeadBush", new FeatureAttributes(3f, .3f, true)},
        {"Cactus", new FeatureAttributes(3f, .05f, false)},
        {"Rock", new FeatureAttributes(.5f, .1f, true)},
    };

}

