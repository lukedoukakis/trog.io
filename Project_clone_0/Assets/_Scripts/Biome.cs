using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Biome : MonoBehaviour
{

    public static bool initialized;

    
    public static List<GameObject> Trees;
    public static List<GameObject> Features;
    public static List<GameObject> Creatures;

    public static void Init()
    {

        Trees = new List<GameObject>(Resources.LoadAll<GameObject>("Terrain/Trees"));
        Features = new List<GameObject>(Resources.LoadAll<GameObject>("Terrain/Features"));
        Creatures = new List<GameObject>(Resources.LoadAll<GameObject>("Terrain/Creatures"));

        initialized = true;
    }

    public static float GetSnowHeight(float snowLevel, float temperature){

        return snowLevel;

    }



}
