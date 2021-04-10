using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityStats : EntityComponent
{

    Dictionary<string, float> stats_base;
    Dictionary<string, float> stats_modifiers;
    Dictionary<string, float> stats_current;



    public static float modifierRange = .15f;


    void Awake(){
        handle = GetComponent<EntityHandle>();
        handle.entityStats = this;

        Init();
    }

    void Start(){
        
    }

    public void Init(){

        bool CREATENEW = true;
        // TODO: createNew if doesnt exist in memory


        stats_base = new Dictionary<string, float>(EntityStatsDefs.current.GetStats(handle.entityInfo.TYPE));

        if(CREATENEW){
            stats_modifiers = CreateModifiers(stats_base);
        }
        else{
            //stats_modifiers = SavesManager.GetStatsModifiers(info.ID);
            // init from memory
        }
        
    }

    public Dictionary<string, float> GetStats(){
        return stats_base;
    }

    public float GetStat(string stat){
        float value;
        //Debug.Log(stats_base == null);
        if(stats_base.TryGetValue(stat, out value))
        {
            return value;
        }
        else{
            Debug.Log("ObjectStats: no dictionary entry for stat: " + stat);
            return -1;
        }

    }

    public Dictionary<string, float> CreateModifiers(Dictionary<string, float> bas){
        Dictionary<string, float> mod = new Dictionary<string, float>();
        foreach(KeyValuePair<string, float> kvp in bas){
            mod.Add(kvp.Key, Random.Range(1f - modifierRange, 1f + modifierRange));
        }
        return mod;
    }

    public string CreateStatsList(){
        string list = "";
        string _name = handle.entityInfo.NAME;
        string _type = handle.entityInfo.TYPE;
        if(_name != ""){ list += _name + " (" + _type + ")"; }
        else{ list += _type; }
        list += "\n";
        if(stats_base != null){
            foreach(KeyValuePair<string, float> kvp in stats_base){
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
