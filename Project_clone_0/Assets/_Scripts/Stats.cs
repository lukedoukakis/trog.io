using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stats : ScriptableObject
{
    public enum StatType{
        Health, Attack, AttackSpeed, Speed, Swim, Agility, ArmorBase, ArmorBlunt, armorSlash, armorPierce
    }

    public float health, attack, attackSpeed, speed, swim, agility, armorBase, armorBlunt, armorSlash, armorPierce;
}

public class StatsHandler : MonoBehaviour{



    // ----
    // definitions
    public static Stats BASE_HUMAN = InitializeStats(
        .5f,
        .5f,
        .5f,
        .5f,
        .5f,
        .5f,
        .5f,
        .5f,
        .5f,
        .5f
    );


    // ----



    public static float GetStatValue(Stats stats, Enum statType){
        switch (statType) {
            case Stats.StatType.Health:
                return stats.health;
            case Stats.StatType.Attack:
                return stats.attack;
            case Stats.StatType.AttackSpeed:
                return stats.attackSpeed;
            case Stats.StatType.Speed:
                return stats.speed;
            case Stats.StatType.Swim:
                return stats.swim;
            case Stats.StatType.Agility:
                return stats.agility;
            case Stats.StatType.ArmorBase:
                return stats.armorBase;
            case Stats.StatType.ArmorBlunt:
                return stats.armorBlunt;
            case Stats.StatType.armorSlash:
                return stats.armorSlash;
            case Stats.StatType.armorPierce:
                return stats.armorPierce;
            default:
                Debug.Log("Invalid stat type");
                return -1;
        }
    }
    public static string GetStatName(Stats stats, Enum statType){
        switch (statType) {
            case Stats.StatType.Health:
                return "Health";
            case Stats.StatType.Attack:
                return "Attack";
            case Stats.StatType.AttackSpeed:
                return "Attack Speed";
            case Stats.StatType.Speed:
                return "Speed";
            case Stats.StatType.Swim:
                return "Swim";
            case Stats.StatType.Agility:
                return "Agility";
            case Stats.StatType.ArmorBase:
                return "Base Damage Resistance";
            case Stats.StatType.ArmorBlunt:
                return "Blunt Damage Resistance";
            case Stats.StatType.armorSlash:
                return "Slash Damage Resistance";
            case Stats.StatType.armorPierce:
                return "Pierce Armor Resistance";
            default:
                Debug.Log("Invalid stat type");
               return "ERROR: INVALID STAT TYPE";
        }
    }

    public static void SetStatValue(Stats stats, Enum statType, float value){
        switch (statType) {
            case Stats.StatType.Health:
                stats.health = value;
                break;
            case Stats.StatType.Attack:
                stats.attack = value;
                break;
            case Stats.StatType.AttackSpeed:
                stats.attackSpeed = value;
                break;
            case Stats.StatType.Speed:
                stats.speed = value;
                break;
            case Stats.StatType.Swim:
                stats.swim = value;
                break;
            case Stats.StatType.Agility:
                stats.agility = value;
                break;
            case Stats.StatType.ArmorBase:
                stats.armorBase = value;
                break;
            case Stats.StatType.ArmorBlunt:
                stats.armorBlunt = value;
                break;
            case Stats.StatType.armorSlash:
                stats.armorSlash = value;
                break;
            case Stats.StatType.armorPierce:
                stats.armorPierce = value;
                break;
            default:
                Debug.Log("Invalid stat type");
                break;
        }


    }

    public static Stats InitializeStats(float health, float attack, float attackSpeed, float speed, float swim, float agility, float armorBase, float armorBlunt, float armorSlash, float armorPierce){
        Stats stats = ScriptableObject.CreateInstance<Stats>();
        stats.health = health;
        stats.attack = attack;
        stats.attackSpeed = attackSpeed;
        stats.speed = speed;
        stats.swim = swim;
        stats.agility = agility;
        stats.armorBase = armorBase;
        stats.armorBlunt = armorBlunt;
        stats.armorSlash = armorSlash;
        stats.armorPierce = armorPierce;
        return stats;
    }

    public static Stats GetEntityBaseStats(string species){
        return BASE_STATS_MAP[species];
    }
    static Dictionary<string, Stats> BASE_STATS_MAP = new Dictionary<string, Stats>(){
        { "Human", BASE_HUMAN },
    };




    


}
