using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityStats : EntityComponent
{


    public List<Stats> statsModifiers;
    public Stats statsCombined;


    protected override void Awake(){

        base.Awake();

        statsModifiers = new List<Stats>();
        statsCombined = ScriptableObject.CreateInstance<Stats>();
        AddStatsModifier(StatsHandler.GetEntityBaseStats(entityInfo.species));
        UpdateCombinedStats();


    }


    public void AddStatsModifier(Stats stats){
        statsModifiers.Add(stats);
        Debug.Log("Added stats modifier");
    }
    public void RemoveStatsModifier(Stats stats){
        bool removed = statsModifiers.Remove(stats);
        if(!removed){
            Debug.Log("Error: couldn't find specified stats modifier to remove from stats modifiers");
        }
    }

    void UpdateCombinedStats(Enum statType){
        float calc = 1f;
        foreach(Stats stats in statsModifiers){
            calc *= StatsHandler.GetStatValue(stats, statType);
        }
        StatsHandler.SetStatValue(statsCombined, statType, calc);
    }


    void UpdateCombinedStats(){
        foreach(int i in Enum.GetValues(typeof(Stats.StatType))){
            UpdateCombinedStats((Stats.StatType)i);
        }
    }



    public string CreateStatsList()
    {
        string list = "";
        string _name = entityInfo.nickname;
        string _type = entityInfo.species;
        if (_name != "") { list += _name + " (" + _type + ")"; }
        else { list += _type; }
        list += "\n";

        foreach (int i in Enum.GetValues(typeof(Stats.StatType)))
        {
            Enum statType = (Stats.StatType)i;
            list += StatsHandler.GetStatName(statsCombined, statType) + ": " + StatsHandler.GetStatValue(statsCombined, statType);
        }
        
        return list;
    }


}
