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
        statsCombined = StatsHandler.InitializeStats(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
        AddStatsModifier(StatsHandler.GetEntityBaseStats(entityInfo.species));


    }


    public void AddStatsModifier(Stats statsToAdd){
        statsModifiers.Add(statsToAdd);
        Enum statType;
        foreach(int statNum in Enum.GetValues(typeof(Stats.StatType))){
            statType = (Stats.StatType)statNum;
            StatsHandler.SetStatValue(statsCombined, statType, StatsHandler.GetStatValue(statsCombined, statType) + StatsHandler.GetStatValue(statsToAdd, statType));
        }
        OnStatsChange();
        //Debug.Log("Added stats modifier");
    }
    public void RemoveStatsModifier(Stats statsToRemove){
        bool removed = statsModifiers.Remove(statsToRemove);
        if (removed)
        {
            Enum statType;
            foreach (int statNum in Enum.GetValues(typeof(Stats.StatType)))
            {
                statType = (Stats.StatType)statNum;
                StatsHandler.SetStatValue(statsCombined, statType, StatsHandler.GetStatValue(statsCombined, statType) - StatsHandler.GetStatValue(statsToRemove, statType));
            }
        }
        else
        {
            Debug.Log("Error: couldn't find specified stats modifier to remove from stats modifiers");
        }

        OnStatsChange();
        //Debug.Log("Removed stats modifier");

    }

    void OnStatsChange(){
        // todo: other stuff when entity's stats are changed
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
