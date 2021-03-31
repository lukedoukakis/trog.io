using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectStats : MonoBehaviour
{

    public string type;
    public string name;

    Dictionary<string, int> stats;


    void Awake(){
        InitStats(type);
    }

    public void InitStats(string type){
        //Debug.Log("Getting stats for type: " + type);
        stats = new Dictionary<string, int>(ObjectStatsDefs.current.GetStats(type));
    }

    public Dictionary<string, int> GetStats(){
        return stats;
    }

    public int GetStat(string stat){
        int value;
        if(stats.TryGetValue(stat, out value))
        {
            return value;
        }
        else{
            Debug.Log("ObjectStats: no dictionary entry for stat: " + stat);
            return -1;
        }

    }

    public string CreateStatsList(){
        string list = "";
        if(name != ""){ list += name + " (" + type + ")"; }
        else{ list += type; }
        list += "\n";
        if(stats != null){
            foreach(KeyValuePair<string, int> kvp in stats){
                if(kvp.Value != -1){
                    list += kvp.Key + ": " + kvp.Value + "\n";
                }
            }
        }
        else{
            Debug.Log("ObjectStats: can't create stats list, stats is null");
            list = "STATS IS NULL";
        }
        return list;
    }


}
