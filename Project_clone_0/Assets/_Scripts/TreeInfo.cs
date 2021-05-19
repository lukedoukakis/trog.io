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
                scale = .08f*1.5f;
                density = .1f;
                normal = .998f;
                slant = 0f;
                spread = 20f;
                break;
            case "Jungle Tree":
                scale = .12f;
                density = 1f;
                normal = .7f;
                slant = .5f;
                spread = 15f;
                break;
            case "Fir Tree":
                scale = .07f;
                density = .75f;
                normal = .7f;
                slant = .18f;
                spread = 20f;
                break;
            case "Snowy Fir Tree":
                scale = .07f;
                density = .7f;
                normal = .7f;
                slant = .18f;
                spread = 20f;
                break;
            case "Palm Tree":
                scale = .09f*1.5f;
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
                scale = .08f*1.5f;
                density = 1f*.75f;
                normal = .9f;
                slant = .18f;
                spread = 20f;
                break;
            case "Plains Oak Tree":
                scale = .09f*1.5f;
                density = .01f;
                normal = .9985f;
                slant = .18f;
                spread = 1f;
                break;
            case string str when name.StartsWith("Grass"):
                scale = .17f;
                density = 12f;
                normal = .99f;
                slant = 1f;
                spread = 5f;
                break;
            case string str when name.StartsWith("Reed"):
                scale = .1f;
                density = 4f;
                normal = .99f;
                slant = .5f;
                spread = .65f;
                break;
            case string str when name.StartsWith("Mushroom"):
                scale = .12f;
                density = .1f;
                normal = .7f;
                slant = 1f;
                spread = .5f;
                break;
            case string str when name.StartsWith("Bush"):
                scale = .04f;
                density = 7f;
                normal = .99f;
                slant = .8f;
                spread = .7f;
                break;
            case string str when name.StartsWith("Dead Bush"):
                scale = .2f;
                density = .02f;
                normal = .6f;
                slant = .8f;
                spread = 5f;
                break;
            case string str when name.StartsWith("Cactus"):
                scale = .09f;
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
