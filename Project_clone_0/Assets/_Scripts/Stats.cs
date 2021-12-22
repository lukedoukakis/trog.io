using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stats : ScriptableObject
{
    public enum StatType{
        Health, Stamina, Attack, AttackSpeed, Speed, Swim, Agility, ArmorBase, ArmorBlunt, ArmorSlash, ArmorPierce, ColdResist
    }

    public float health, stamina, attack, attackSpeed, speed, swim, agility, armorBase, armorBlunt, armorSlash, armorPierce, coldResist;



    public static Stats NONE = InstantiateStats(
        0f,
        0f,
        0f,
        0f,
        0f,
        0f,
        0f,
        0f,
        0f,
        0f,
        0f,
        0f
    );

    
    public static Stats ITEM_WIELDERMODIFIER_MEAT = InstantiateStats(1f,1f,1f,0f,0f,0f,0f,0f,0f,0f,0f,0f);

    public static Stats ITEM_WIELDERMODIFIER_SPEARSTONE = InstantiateStats(0f,0f,3f,0f,0f,0f,0f,0f,0f,0f,0f,0f);

    public static Stats ITEM_WIELDERMODIFIER_AXESTONE = InstantiateStats(0f,0f,3f,0f,0f,0f,0f,0f,0f,0f,0f,0f);
    
    public static Stats ITEM_WIELDERMODIFIER_SPEARBONE = InstantiateStats(0f,0f,6f,0f,0f,0f,0f,0f,0f,0f,0f,0f);

    public static Stats ITEM_WIELDERMODIFIER_AXEBONE = InstantiateStats(0f,0f,6f,0f,0f,0f,0f,0f,0f,0f,0f,0f);

    public static Stats ITEM_WIELDERMODIFIER_TESTCLOTHING = InstantiateStats(0f,0f,0f,0f,0f,0f,0f,3f,0f,0f,0f,1f);






    




    // ----



    public static float GetStatValue(Stats stats, Enum statType){
        switch (statType) {
            case Stats.StatType.Health:
                return stats.health;
            case Stats.StatType.Stamina:
                return stats.stamina;
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
            case Stats.StatType.ArmorSlash:
                return stats.armorSlash;
            case Stats.StatType.ArmorPierce:
                return stats.armorPierce;
            case Stats.StatType.ColdResist:
                return stats.coldResist;
            default:
                Debug.Log("Invalid stat type");
                return -1;
        }
    }
    public static string GetStatName(Stats stats, Enum statType){
        switch (statType) {
            case Stats.StatType.Health:
                return "Health";
            case Stats.StatType.Stamina:
                return "Stamina";
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
            case Stats.StatType.ArmorSlash:
                return "Slash Damage Resistance";
            case Stats.StatType.ArmorPierce:
                return "Pierce Armor Resistance";
            case Stats.StatType.ColdResist:
                return "Cold Resistance";
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
            case Stats.StatType.Stamina:
                stats.stamina = value;
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
            case Stats.StatType.ArmorSlash:
                stats.armorSlash = value;
                break;
            case Stats.StatType.ArmorPierce:
                stats.armorPierce = value;
                break;
            case Stats.StatType.ColdResist:
                stats.coldResist = value;
                break;
            default:
                Debug.Log("Invalid stat type");
                break;
        }


    }

    public static Stats InstantiateStats(float health, float stamina, float attack, float attackSpeed, float speed, float swim, float agility, float armorBase, float armorBlunt, float armorSlash, float armorPierce, float coldResist){
        Stats stats = ScriptableObject.CreateInstance<Stats>();
        stats.health = health;
        stats.stamina = stamina;
        stats.attack = attack;
        stats.attackSpeed = attackSpeed;
        stats.speed = speed;
        stats.swim = swim;
        stats.agility = agility;
        stats.armorBase = armorBase;
        stats.armorBlunt = armorBlunt;
        stats.armorSlash = armorSlash;
        stats.armorPierce = armorPierce;
        stats.coldResist = coldResist;
        return stats;
    }






    


}