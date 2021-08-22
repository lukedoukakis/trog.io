
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeInfo : MonoBehaviour
{

    public string type;



    public static Tuple<float, float, float, float, float, float, float> GetPlacementParameters(string name, float wetness, float fw)
    {
        //Debug.Log("TreeInfo: type is: " + type);

        float scale;
        float density;
        float normal_min;
        float normal_max;
        float slant;
        float spread;
        float vertOffset;

        switch (name)
        {
            case "Acacia Tree":
                scale = 1.7f * ChunkGenerator.current.treeScale;
                density = .1f;
                normal_min = .998f;
                normal_max = 1f;
                slant = 0f;
                spread = 2f;
                vertOffset = 0f;
                break;
            case "Jungle Tree":
                scale = 2.2f * ChunkGenerator.current.treeScale;
                density = 1f;
                normal_min = .6f;
                normal_max = 1f;
                slant = .5f;
                spread = 1.5f;
                vertOffset = 0f;
                break;
            case "Fir Tree":
                scale = 1.5f * ChunkGenerator.current.treeScale;
                density = 1f;
                normal_min = .25f;
                normal_max = 1f;
                slant = .12f;
                spread = 2f;
                vertOffset = 0f;
                break;
            case "Snowy Fir Tree":
                scale = 1.5f * ChunkGenerator.current.treeScale;
                density = 1f;
                normal_min = .6f;
                normal_max = 1f;
                slant = .12f;
                spread = 2f;
                vertOffset = 0f;
                break;
            case "Palm Tree":
                scale = 2f * ChunkGenerator.current.treeScale;
                if(fw > .9f){
                    density = .2f;
                }
                else{
                    density = -1f;
                }
                normal_min = .98f;
                normal_max = 1f;
                slant = 0f;
                spread = 1f;
                vertOffset = 0f;
                break;
            case "Oak Tree":
                scale = 3f * ChunkGenerator.current.treeScale;
                density = 1f;
                normal_min = .25f;
                normal_max = 1f;
                slant = .12f;
                spread = 2f;
                vertOffset = 0f;
                break;
            case string str when name.StartsWith("Grass"):
                scale = .75f;
                density = 50f;
                normal_min = .995f;
                normal_max = 1f;
                slant = .5f;
                spread = 2f;
                vertOffset = 0f;
                break;
            case string str when name.StartsWith("Plant"):
                scale = .5f;
                density = 100f;
                normal_min = .9f;
                normal_max = 1f;
                slant = 1f;
                spread = 2f;
                vertOffset = 0f;
                break;
            case string str when name.StartsWith("Reed"):
                scale = 1f;
                density = 4f;
                normal_min = .99f;
                normal_max = 1f;
                slant = .5f;
                spread = .65f;
                vertOffset = 0f;
                break;
            case string str when name.StartsWith("Mushroom"):
                scale = 1.2f;
                density = .1f;
                normal_min = .7f;
                normal_max = 1f;
                slant = 1f;
                spread = .5f;
                vertOffset = 0f;
                break;
            case string str when name.StartsWith("Bush"):
                scale = .4f;
                density = 7f;
                normal_min = .6f;
                normal_max = 1f;
                slant = .8f;
                spread = .7f;
                vertOffset = 0f;
                break;
            case string str when name.StartsWith("Dead Bush"):
                scale = 2f;
                density = .02f;
                normal_min = .6f;
                normal_max = 1f;
                slant = .8f;
                spread = 5f;
                vertOffset = 0f;
                break;
            case string str when name.StartsWith("Cactus"):
                scale = 1f;
                density = .1f;
                normal_min = .98f;
                normal_max = 1f;
                slant = .18f;
                spread = 5f;
                vertOffset = 0f;
                break;
            case string str when name.StartsWith("Rock"):
                scale = 1f;
                density = 2f;
                normal_min = .9f;
                normal_max = .92f;
                slant = 1f;
                spread = 2f;
                vertOffset = -.5f;
                break;
            case string str when name.StartsWith("RockShore"):
                scale = .75f;
                density = 5f;
                normal_min = .5f;
                normal_max = .7f;
                slant = 1f;
                spread = 2f;
                vertOffset = 0f;
                break;
            default:
                scale = -1f;
                density = -1f;
                normal_min = -1f;
                normal_max = -1f;
                slant = -1f;
                spread = -1f;
                vertOffset = 0f;
                break;
        }

        density *= (wetness + .5f);

        return Tuple.Create(scale, density, normal_min, normal_max, slant, spread, vertOffset);


    }



   
}
