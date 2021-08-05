
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeInfo : MonoBehaviour
{

    public string type;



    public static Tuple<float, float, float, float, float, float> GetPlacementParameters(string name, float wetness, float fw)
    {
        //Debug.Log("TreeInfo: type is: " + type);

        float scale;
        float density;
        float normal_min;
        float normal_max;
        float slant;
        float spread;

        switch (name)
        {
            case "Acacia Tree":
                scale = 1.3f * ChunkGenerator.current.treeScale;
                density = .1f;
                normal_min = .998f;
                normal_max = 1f;
                slant = 0f;
                spread = 2f;
                break;
            case "Jungle Tree":
                scale = 2.2f * ChunkGenerator.current.treeScale;
                density = 1f;
                normal_min = .7f;
                normal_max = 1f;
                slant = .5f;
                spread = 1.5f;
                break;
            case "Fir Tree":
                scale = 1.5f * ChunkGenerator.current.treeScale;
                density = 1f;
                normal_min = .94f;
                normal_max = 1f;
                slant = .12f;
                spread = 2f;
                break;
            case "Snowy Fir Tree":
                scale = 1.5f * ChunkGenerator.current.treeScale;
                density = 1f;
                normal_min = .94f;
                normal_max = 1f;
                slant = .12f;
                spread = 2f;
                break;
            case "Palm Tree":
                scale = 1.2f * ChunkGenerator.current.treeScale;
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
                break;
            case "Oak Tree":
                scale = .5f * ChunkGenerator.current.treeScale;
                density = .05f;
                normal_min = .95f;
                normal_max = 1f;
                slant = .18f;
                spread = 2f;
                break;
            case "Plains Oak Tree":
                scale = 2.5f * ChunkGenerator.current.treeScale;
                density = .1f;
                normal_min = .95f;
                normal_max = 1f;
                slant = .18f;
                spread = 1f;
                break;
            case string str when name.StartsWith("Grass"):
                scale = 1.7f * ChunkGenerator.current.treeScale;
                density = 5f;
                normal_min = .95f;
                normal_max = 1f;
                slant = 1f;
                spread = .5f;
                break;
            case string str when name.StartsWith("Reed"):
                scale = 1f* ChunkGenerator.current.treeScale;
                density = 4f;
                normal_min = .99f;
                normal_max = 1f;
                slant = .5f;
                spread = .65f;
                break;
            case string str when name.StartsWith("Mushroom"):
                scale = 1.2f* ChunkGenerator.current.treeScale;
                density = .1f;
                normal_min = .7f;
                normal_max = 1f;
                slant = 1f;
                spread = .5f;
                break;
            case string str when name.StartsWith("Bush"):
                scale = .4f* ChunkGenerator.current.treeScale;
                density = 7f;
                normal_min = .99f;
                normal_max = 1f;
                slant = .8f;
                spread = .7f;
                break;
            case string str when name.StartsWith("Dead Bush"):
                scale = 2f* ChunkGenerator.current.treeScale;
                density = .02f;
                normal_min = .6f;
                normal_max = 1f;
                slant = .8f;
                spread = 5f;
                break;
            case string str when name.StartsWith("Cactus"):
                scale = 1f* ChunkGenerator.current.treeScale;
                if(fw > .92f){
                    density = 0f;
                }
                else{
                    density = .01f;
                }
                normal_min = .996f;
                normal_max = 1f;
                slant = .18f;
                spread = 5f;
                break;
            case string str when name.StartsWith("Rock"):
                scale = 3f* ChunkGenerator.current.treeScale;
                density = .2f;
                normal_min = .5f;
                normal_max = .8f;
                slant = 0f;
                spread = 5f;
                break;
            default:
                scale = -1f* ChunkGenerator.current.treeScale;
                density = -1f;
                normal_min = -1f;
                normal_max = -1f;
                slant = -1f;
                spread = -1f;
                break;
        }

        density *= (wetness + .5f);

        return Tuple.Create(scale, density, normal_min, normal_max, slant, spread);


    }



   
}
