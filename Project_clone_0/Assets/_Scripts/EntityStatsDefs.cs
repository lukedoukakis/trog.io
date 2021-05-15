using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityStatsDefs : MonoBehaviour
{
    

    public static EntityStatsDefs current;
    
    public Dictionary<string, Dictionary<string, float>> EntityStats;
    Dictionary<string, float> human, tree;

    public bool init;


    void Awake(){
        init = true;
        current = this;

        human = new Dictionary<string, float>()
        {
            {"hp", .5f},
            {"speed", .5f},
            {"strength", .5f},
            {"stamina", .5f},
        };

        tree = new Dictionary<string, float>()
        {
            {"hp", 2f},
            {"speed", -1f},
            {"strength", -1f},
            {"stamina", -1f},
        };


        EntityStats = new Dictionary<string, Dictionary<string, float>>()
        {
            {"human", human},
            {"tree", tree},
        };

    }


    public Dictionary<string, float> GetStats(string type){
        Dictionary<string, float> stats;
        if(EntityStats.TryGetValue(type, out stats))
        {
            //Debug.Log("EntityStatsDefs: Got stats for type: " + type);
            return stats;
        }
        else{
            Debug.Log("EntityStatsDefs: no dictionary entry for type: " + type);
            return null;
        }

    }





}
