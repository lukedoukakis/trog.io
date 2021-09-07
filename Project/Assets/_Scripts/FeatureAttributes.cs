
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class FeatureAttributes
{
    public float scale;
    public float density;

    public FeatureAttributes(float scale, float density)
    {
        this.scale = scale;
        this.density = density;
    }

    public static FeatureAttributes GetFeatureAttributes(string name, float wetness)
    {

        FeatureAttributes featureAttributes;

        switch (name)
        {
            case "Acacia Tree":
                featureAttributes = new FeatureAttributes(2f, .1f);
                break;
            case "Jungle Tree":
                featureAttributes = new FeatureAttributes(2.2f, .7f);
                break;
            case "Fir Tree":
                featureAttributes = new FeatureAttributes(1.5f, .7f);
                break;
            case "Snowy Fir Tree":
                featureAttributes = new FeatureAttributes(1.5f, .7f);
                break;
            case "Palm Tree":
                featureAttributes = new FeatureAttributes(2f, .1f);
                break;
            case "Oak Tree":
                featureAttributes = new FeatureAttributes(2f, .7f);
                break;
            case string str when name.StartsWith("Grass"):
                featureAttributes = new FeatureAttributes(0f, 0f);
                break;
            case string str when name.StartsWith("Plant"):
                featureAttributes = new FeatureAttributes(.5f, 1f);
                break;
            case string str when name.StartsWith("Reed"):
                featureAttributes = new FeatureAttributes(1f, .7f);
                break;
            case string str when name.StartsWith("Mushroom"):
                featureAttributes = new FeatureAttributes(1f, .1f);
                break;
            case string str when name.StartsWith("Bush"):
                featureAttributes = new FeatureAttributes(1f, .4f);
                break;
            case string str when name.StartsWith("Dead Bush"):
                featureAttributes = new FeatureAttributes(2f, .3f);
                break;
            case string str when name.StartsWith("Cactus"):
                featureAttributes = new FeatureAttributes(3f, .0001f);
                break;
            case string str when name.StartsWith("Rock"):
                featureAttributes = new FeatureAttributes(.5f, .1f);
                break;
            default:
                featureAttributes = new FeatureAttributes(-1f, -1f);
                break;
        }

        //featureAttributes.density *= (wetness + .5f);

        return featureAttributes;


    }

}

