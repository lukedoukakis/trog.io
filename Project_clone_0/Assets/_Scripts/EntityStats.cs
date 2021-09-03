using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityStats : EntityComponent
{


    public List<Stats> statsModifiers;
    public Stats statsCombined;


    public static float hp_base = 100;
    public float stamina_base = 100;
    public static float hpLossFromHit_base = 8;
    public int hp;
    public int stamina;


    protected override void Awake(){

        base.Awake();

        statsModifiers = new List<Stats>();
        statsCombined = StatsHandler.InitializeStats(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
        EntityInfo info = GetComponent<EntityInfo>();
        string statsIdentifier = (info == null) ? statsIdentifier = tag : info.species;
        AddStatsModifier(StatsHandler.GetEntityBaseStats(statsIdentifier));
        hp = (int)(hp_base * StatsHandler.GetStatValue(statsCombined, Stats.StatType.Health));
        stamina = (int)(stamina_base * StatsHandler.GetStatValue(statsCombined, Stats.StatType.Stamina));

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


    public void TakeDamage(EntityStats attackerStats, Item attackerWeapon){

        // get attacker's relevant stats
        float attackerAttack = StatsHandler.GetStatValue(attackerStats.statsCombined, Stats.StatType.Attack);

        // get this entity's relevant stats
        float armorBase = StatsHandler.GetStatValue(this.statsCombined, Stats.StatType.ArmorBase);
        Enum armorStatType;
        switch (attackerWeapon.damageType) {
            case Item.DamageType.Blunt :
                armorStatType = Stats.StatType.ArmorBlunt;
                break;
            case Item.DamageType.Slash :
                armorStatType = Stats.StatType.ArmorSlash;
                break;
            case Item.DamageType.Pierce :
                armorStatType = Stats.StatType.ArmorPierce;
                break;
            default:
                armorStatType = Stats.StatType.ArmorBlunt;
                break;
        }
        float armorFromWeaponType = StatsHandler.GetStatValue(this.statsCombined, armorStatType);

        // calculate damage
        float hpLoss = hpLossFromHit_base;
        hpLoss *= attackerAttack;
        hpLoss *= 1f / Mathf.Max(armorBase, 1f);
        hpLoss *= 1f / Mathf.Max(armorFromWeaponType, 1f);
        
        // take away health
        hp -= (int)hpLoss;
        // Debug.Log("Attacker Weapon Type: " + attackerWeapon.damageType.ToString());
        // Debug.Log("Armor Type: " + armorStatType.ToString());
        // Debug.Log("Armor against this type: " + armorFromWeaponType);
        // Debug.Log("DAMAGE: " + (int)hpLoss);
        // Debug.Log("HP: " + hp.ToString());
        if(hp <= 0){
            OnDeath();
        }

    }

    void OnDeath(){
        Debug.Log("DED");
        // todo: death stuff
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
