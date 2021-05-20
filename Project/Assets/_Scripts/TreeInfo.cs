using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeInfo : MonoBehaviour
{

    public string type;



    public static Tuple<float, float, float, float, float> GetPlacementParameters(string name, float wetness, float fw)
    {
        //Debug.Log("TreeInfo: type is: " + type);

        float scale;
        float density;
        float normal;
        float slant;
        float spread;

        switch (name)
        {
            case "Acacia Tree":
                scale = 1.2f;
                density = .05f;
                normal = .998f;
                slant = 0f;
                spread = 2f;
                break;
            case "Jungle Tree":
                scale = 2.2f;
                density = 1.5f;
                normal = .7f;
                slant = .5f;
                spread = 1.5f;
                break;
            case "Fir Tree":
                scale = 1.75f;
                density = .75f;
                normal = .7f;
                slant = .18f;
                spread = 2f;
                break;
            case "Snowy Fir Tree":
                scale = 1.75f;
                density = .7f;
                normal = .7f;
                slant = .18f;
                spread = 2f;
                break;
            case "Palm Tree":
                scale = 1.2f;
                if(fw > .9f){
                    density = .2f;
                }
                else{
                    density = -1f;
                }
                normal = .98f;
                slant = 0f;
                spread = 1f;
                break;
            case "Oak Tree":
                scale = .5f;
                density = .05f;
                normal = .95f;
                slant = .18f;
                spread = 2f;
                break;
            case "Plains Oak Tree":
                scale = 2.5f;
                density = .1f;
                normal = .95f;
                slant = .18f;
                spread = 1f;
                break;
            case string str when name.StartsWith("Grass"):
                scale = 1.7f;
                density = 5f;
                normal = .99f;
                slant = 1f;
                spread = .5f;
                break;
            case string str when name.StartsWith("Reed"):
                scale = 1f;
                density = 4f;
                normal = .99f;
                slant = .5f;
                spread = .65f;
                break;
            case string str when name.StartsWith("Mushroom"):
                scale = 1.2f;
                density = .1f;
                normal = .7f;
                slant = 1f;
                spread = .5f;
                break;
            case string str when name.StartsWith("Bush"):
                scale = .4f;
                density = 7f;
                normal = .99f;
                slant = .8f;
                spread = .7f;
                break;
            case string str when name.StartsWith("Dead Bush"):
                scale = 2f;
                density = .02f;
                normal = .6f;
                slant = .8f;
                spread = 5f;
                break;
            case string str when name.StartsWith("Cactus"):
                scale = 1f;
                if(fw > .92f){
                    density = 0f;
                }
                else{
                    density = .01f;
                }
                normal = .996f;
                slant = .18f;
                spread = 5f;
                break;
            default:
                scale = -1f;
                density = -1f;
                normal = -1f;
                slant = -1f;
                spread = -1f;
                break;
        }

        density *= (wetness + .5f);

        return Tuple.Create(scale, density, normal, slant, spread);


    }



   
}
