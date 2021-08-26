using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Biome : MonoBehaviour
{

    public static bool initialized;

    public enum BiomeType
    {
        Desert,
        Chaparral,
        Jungle,
        Savannah,
        Plains,
        Tundra,
        Forest,
        Taiga,
        SnowyTaiga,
        Ocean
    }


    // [temperature, humidity]
    static int[][] BiomeTable;
    public static GameObject[][] TreePool;
    public static GameObject[][] FeaturePool;
    public static GameObject[][] WaterFeaturePool;



    public static float MaxTemp_Snow = 4f / 11f;

    public static void Init()
    {



        BiomeTable = new int[][]
        {
        new int[]{ (int)BiomeType.Tundra, (int)BiomeType.Tundra, (int)BiomeType.Tundra, (int)BiomeType.Tundra, (int)BiomeType.Tundra, (int)BiomeType.Tundra, (int)BiomeType.SnowyTaiga, (int)BiomeType.SnowyTaiga, (int)BiomeType.SnowyTaiga, (int)BiomeType.SnowyTaiga, (int)BiomeType.SnowyTaiga },
        new int[]{ (int)BiomeType.Tundra, (int)BiomeType.Tundra, (int)BiomeType.Tundra, (int)BiomeType.Tundra, (int)BiomeType.Tundra, (int)BiomeType.Tundra, (int)BiomeType.SnowyTaiga, (int)BiomeType.SnowyTaiga, (int)BiomeType.SnowyTaiga, (int)BiomeType.SnowyTaiga, (int)BiomeType.SnowyTaiga },
        new int[]{ (int)BiomeType.Tundra, (int)BiomeType.Tundra, (int)BiomeType.Tundra, (int)BiomeType.Tundra, (int)BiomeType.Tundra, (int)BiomeType.Tundra, (int)BiomeType.SnowyTaiga, (int)BiomeType.SnowyTaiga, (int)BiomeType.SnowyTaiga, (int)BiomeType.SnowyTaiga, (int)BiomeType.SnowyTaiga },
        new int[]{ (int)BiomeType.Plains, (int)BiomeType.Plains, (int)BiomeType.Plains, (int)BiomeType.Plains, (int)BiomeType.Plains, (int)BiomeType.Taiga, (int)BiomeType.Taiga, (int)BiomeType.Taiga, (int)BiomeType.Taiga, (int)BiomeType.Taiga, (int)BiomeType.Taiga },
        new int[]{ (int)BiomeType.Plains, (int)BiomeType.Plains, (int)BiomeType.Plains, (int)BiomeType.Plains, (int)BiomeType.Plains, (int)BiomeType.Taiga, (int)BiomeType.Taiga, (int)BiomeType.Taiga, (int)BiomeType.Taiga, (int)BiomeType.Taiga, (int)BiomeType.Taiga },
        new int[]{ (int)BiomeType.Plains, (int)BiomeType.Plains, (int)BiomeType.Plains, (int)BiomeType.Plains, (int)BiomeType.Plains, (int)BiomeType.Plains, (int)BiomeType.Taiga, (int)BiomeType.Taiga, (int)BiomeType.Taiga, (int)BiomeType.Taiga, (int)BiomeType.Taiga },
        new int[]{ (int)BiomeType.Desert, (int)BiomeType.Desert, (int)BiomeType.Chaparral, (int)BiomeType.Chaparral, (int)BiomeType.Savannah, (int)BiomeType.Savannah, (int)BiomeType.Savannah, (int)BiomeType.Savannah, (int)BiomeType.Forest, (int)BiomeType.Forest, (int)BiomeType.Forest },
        new int[]{ (int)BiomeType.Desert, (int)BiomeType.Desert, (int)BiomeType.Chaparral, (int)BiomeType.Chaparral, (int)BiomeType.Savannah, (int)BiomeType.Savannah, (int)BiomeType.Savannah, (int)BiomeType.Savannah, (int)BiomeType.Forest, (int)BiomeType.Forest, (int)BiomeType.Forest },
        new int[]{ (int)BiomeType.Desert, (int)BiomeType.Desert, (int)BiomeType.Desert, (int)BiomeType.Chaparral, (int)BiomeType.Savannah, (int)BiomeType.Savannah, (int)BiomeType.Savannah, (int)BiomeType.Savannah, (int)BiomeType.Forest, (int)BiomeType.Forest, (int)BiomeType.Forest },
        new int[]{ (int)BiomeType.Desert, (int)BiomeType.Desert, (int)BiomeType.Desert, (int)BiomeType.Desert, (int)BiomeType.Desert, (int)BiomeType.Desert, (int)BiomeType.Savannah, (int)BiomeType.Savannah, (int)BiomeType.Savannah, (int)BiomeType.Savannah, (int)BiomeType.Savannah },
        new int[]{ (int)BiomeType.Desert, (int)BiomeType.Desert, (int)BiomeType.Desert, (int)BiomeType.Desert, (int)BiomeType.Desert, (int)BiomeType.Desert, (int)BiomeType.Savannah, (int)BiomeType.Savannah, (int)BiomeType.Savannah, (int)BiomeType.Savannah, (int)BiomeType.Savannah },
        };



        string biomeName;
        string path;
        TreePool = new GameObject[10][];
        FeaturePool = new GameObject[10][];
        WaterFeaturePool = new GameObject[10][];
        for (int i = 0; i < TreePool.Length; i++)
        {
            biomeName = ((BiomeType)i).ToString();

            // trees
            path = "Terrain/" + biomeName + "/Trees";
            TreePool[i] = Resources.LoadAll<GameObject>(path);

            // features
            path = "Terrain/" + biomeName + "/Features";
            FeaturePool[i] = Resources.LoadAll<GameObject>(path);

            // water features
            path = "Terrain/" + biomeName + "/Features Water";
            WaterFeaturePool[i] = Resources.LoadAll<GameObject>(path);


            //Debug.Log(TreePool[i].Length);
        }

        //Debug.Log("initialized");
        initialized = true;
    }


    public static int GetBiome(float temp, float humid, float mtn)
    {
        if(mtn >= .75f){
            if(humid >= .5f){
                return (int)BiomeType.SnowyTaiga;
            }
            else{
                return (int)BiomeType.SnowyTaiga;
            }
        }

        int temperature = (int)((temp * 10f) + 0.5f);
        int humidity = (int)((humid * 10f) + 0.5f);
        //Debug.Log(temp);
        int biome = BiomeTable[temperature][humidity];


        if(mtn < .75f){
            if(biome == (int)BiomeType.SnowyTaiga){
                biome = (int)BiomeType.Taiga;
            }
            else if(biome == (int)BiomeType.Tundra){
                biome = (int)BiomeType.Plains;
            }
        }

        return biome;


    }

    public static Tuple<GameObject, Tuple<float, float, float, float, float, float, float>> GetTree(int biomeType, float wetness, float fw)
    {
        //Debug.Log(((BiomeType)biomeType).ToString());
        GameObject[] trees = TreePool[biomeType];
        if(trees.Length > 0)
        {
            GameObject tree = trees[UnityEngine.Random.Range(0, trees.Length)];
            return Tuple.Create(tree, TreeInfo.GetPlacementParameters(tree.name, wetness, fw));
        }
        return null;
    }


    public static Tuple<GameObject, Tuple<float, float, float, float, float, float, float>> GetFeature(int biomeType, float wetness, float fw, bool onWater){
        
        GameObject[] features;
        if(onWater){ features = WaterFeaturePool[biomeType]; }
        else{ features = FeaturePool[biomeType]; }
        if(features.Length > 0)
        {
            GameObject feature = features[UnityEngine.Random.Range(0, features.Length)];
            return Tuple.Create(feature, TreeInfo.GetPlacementParameters(feature.name, wetness, fw));
        }
        return null;
    }



}
