using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectStatsDefs : MonoBehaviour
{
    

    public static ObjectStatsDefs current;
    
    public Dictionary<string, Dictionary<string, int>> ObjectStats;
    Dictionary<string, int> human, tree;


    void Awake(){
        current = this;

        human = new Dictionary<string, int>()
        {
            {"hp", 50},
            {"speed", 50},
            {"strength", 50},
            {"stamina", 100},
        };

        tree = new Dictionary<string, int>()
        {
            {"hp", 200},
            {"speed", -1},
            {"strength", -1},
            {"stamina", -1},
        };


        ObjectStats = new Dictionary<string, Dictionary<string, int>>()
        {
            {"human", human},
            {"tree", tree},
        };

    }


    public Dictionary<string, int> GetStats(string type){
        Dictionary<string, int> stats;
        if(ObjectStats.TryGetValue(type, out stats))
        {
            //Debug.Log("ObjectStatsDefs: Got stats for type: " + type);
            return stats;
        }
        else{
            Debug.Log("ObjectStatsDefs: no dictionary entry for type: " + type);
            return null;
        }

    }





}
