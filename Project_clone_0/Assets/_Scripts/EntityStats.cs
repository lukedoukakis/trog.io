using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityStats : EntityComponent
{


    public List<Stats> statsModifiers;
    public Stats statsCombined;
    public ItemCollection drops;


    public static float BASE_AMOUNT_HP = 100;
    public float BASE_AMOUNT_STAMINA = 100;
    public static float hpLossFromHit_base = 8;
    public int hp;
    public int stamina;


    protected override void Awake(){

        base.Awake();

        statsModifiers = new List<Stats>();
        statsCombined = Stats.InstantiateStats(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);

        if(entityInfo != null){
            AddSpeciesBaseModifier();
            SetBaseHpAndStamina();
            drops = entityInfo.speciesInfo.baseDrop;
        }
    

    }

    public void AddSpeciesBaseModifier(){
        AddStatsModifier(entityInfo.speciesInfo.baseStats);
    }

    public void SetBaseHpAndStamina(){
        hp = (int)(BASE_AMOUNT_HP * Stats.GetStatValue(statsCombined, Stats.StatType.Health));
        stamina = (int)(BASE_AMOUNT_STAMINA * Stats.GetStatValue(statsCombined, Stats.StatType.Stamina));
    }


    public void AddStatsModifier(Stats statsToAdd){
        statsModifiers.Add(statsToAdd);
        Enum statType;
        foreach(int statNum in Enum.GetValues(typeof(Stats.StatType))){
            statType = (Stats.StatType)statNum;
            Stats.SetStatValue(statsCombined, statType, Stats.GetStatValue(statsCombined, statType) + Stats.GetStatValue(statsToAdd, statType));
        }
        OnStatsChange();
        //Debug.Log("Added stats modifier");
    }
    public void RemoveStatsModifier(Stats statsToRemove){
        if (statsToRemove != null)
        {
            bool removed = statsModifiers.Remove(statsToRemove);
            if (removed)
            {
                Enum statType;
                foreach (int statNum in Enum.GetValues(typeof(Stats.StatType)))
                {
                    statType = (Stats.StatType)statNum;
                    Stats.SetStatValue(statsCombined, statType, Stats.GetStatValue(statsCombined, statType) - Stats.GetStatValue(statsToRemove, statType));
                }
            }
            else
            {
                Debug.Log("Error: couldn't find specified stats modifier to remove from stats modifiers");
            }

            OnStatsChange();
            //Debug.Log("Removed stats modifier");
        }


    }

    void OnStatsChange(){
        // todo: other stuff when entity's stats are changed
    }


    public void TakeDamage(EntityHandle attackerHandle){

        if(isLocalPlayer && Testing.instance.godMode){
            return;
        }

        // get attacker's relevant stats and (if applicable) weapon
        Stats attackerStats = attackerHandle.entityStats.statsCombined;
        EntityItems attackerItems = attackerHandle.entityItems;
        Item attackerWeapon = attackerItems == null ? Item.None : attackerItems.weaponEquipped_item;
        float attackerAttack = Stats.GetStatValue(attackerStats, Stats.StatType.Attack);

        // get this entity's relevant stats
        float armorBase = Stats.GetStatValue(this.statsCombined, Stats.StatType.ArmorBase);
        Enum armorStatType;
        switch (attackerWeapon.damageType)
        {
            case Item.ItemDamageType.Blunt:
                armorStatType = Stats.StatType.ArmorBlunt;
                break;
            case Item.ItemDamageType.Slash:
                armorStatType = Stats.StatType.ArmorSlash;
                break;
            case Item.ItemDamageType.Pierce:
                armorStatType = Stats.StatType.ArmorPierce;
                break;
            default:
                armorStatType = Stats.StatType.ArmorBase;
                break;
        }
        float armorFromWeaponType = Stats.GetStatValue(this.statsCombined, armorStatType);

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
        //Debug.Log("HP: " + hp.ToString());
        if(hp <= 0){
            OnHealthEmptied(attackerHandle);
        }

    }

    void OnHealthEmptied(EntityHandle attackerHandle){
        //Debug.Log("DED");

        // todo: death 'animation'/being destroyed visuals
        GameObject.Destroy(this.gameObject);

        SpawnDrops(this.transform.position, attackerHandle);

    }


    // drop drops
    void SpawnDrops(Vector3 dropSpot, EntityHandle receiverHandle){

        Debug.Log("Adding drops to entity \'" + receiverHandle.entityInfo.nickname + "\' faction: " + drops.ToString());
        // todo: add supplemental drops based on specific properties of this entity (?)

        Item item;
        GameObject worldObject;
        int count;
        bool inCamp = Camp.EntityIsInsideCamp(receiverHandle);

        List<Tuple<Item, GameObject>> totalDropsList = new List<Tuple<Item, GameObject>>();

        // first, get a list of each item and spawned gameobject dropped on the ground
        foreach(KeyValuePair<Item, int> kvp in drops.items)
        {
            item = kvp.Key;
            count = kvp.Value;

            float placementHeightOffset = 0f;
            for (int i = 0; i < count; ++i)
            {
                worldObject = Utility.InstantiatePrefabSameName(item.worldObject);
                worldObject.transform.position = dropSpot + (Vector3.up * placementHeightOffset);
                worldObject.transform.rotation = Utility.GetRandomRotation(360f);
                Vector3 randomDirection = Utility.GetRandomVector(300f);
                randomDirection.y = 0f;
                worldObject.GetComponent<Rigidbody>().AddForce(randomDirection + Vector3.up * 600f);
                placementHeightOffset += .3f;
                //yield return new WaitForSecondsRealtime(ObjectRack.OBJECT_PLACEMENT_DELAY_TIMESTEP);
                totalDropsList.Add(Tuple.Create<Item, GameObject>(item, worldObject));
            }
        }
        //Debug.Log("Finished dropping on ground...");

        
        // for each object on the ground
        for(int i = 0; i < totalDropsList.Count; ++i)
        {
            item = totalDropsList[i].Item1;
            worldObject = totalDropsList[i].Item2;
            if(inCamp){
                // if attacker is in their camp
                if(Item.IsRackable(item)){
                    Debug.Log("adding drops straight to faction");
                    // if the item is rackable, add item straight to racks
                    float delay = .5f + (i * ObjectRack.OBJECT_PLACEMENT_DELAY_TIMESTEP);
                    GameObject temp = new GameObject();
                    temp.transform.position = worldObject.transform.position;
                    Faction.AddItemOwned(receiverHandle.entityInfo.faction, item, 1, null, temp.transform, delay);
                    Utility.DestroyInSeconds(worldObject, delay);
                    Utility.DestroyInSeconds(temp, 5f);
                }
                else
                {
                    Debug.Log("drops staying on ground");
                    // do nothing
                }
            }
            else
            {
                Debug.Log("adding drops to inventory");
                // if not in camp, add to reciver's inventory and delay small timestep
                receiverHandle.entityItems.AddToInventory(item, worldObject, .5f + i * ObjectRack.OBJECT_PLACEMENT_DELAY_TIMESTEP);
            }
        }
        

    }



    public string CreateStatsList()
    {
        string list = "";
        string _name = entityInfo.nickname;
        string _type = entityInfo.species.ToString();
        if (_name != "") { list += _name + " (" + _type + ")"; }
        else { list += _type; }
        list += "\n";

        foreach (int i in Enum.GetValues(typeof(Stats.StatType)))
        {
            Enum statType = (Stats.StatType)i;
            list += Stats.GetStatName(statsCombined, statType) + ": " + Stats.GetStatValue(statsCombined, statType);
        }
        
        return list;
    }


}


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

    
    public static Stats ITEM_WIELDERMODIFIER_TESTFOOD = InstantiateStats(1f,1f,1f,0f,0f,0f,0f,0f,0f,0f,0f,0f);

    public static Stats ITEM_WIELDERMODIFIER_SPEAR = InstantiateStats(0f,0f,3f,0f,0f,0f,0f,0f,0f,0f,0f,0f);

    public static Stats ITEM_WIELDERMODIFIER_AXE = InstantiateStats(0f,0f,3f,0f,0f,0f,0f,0f,0f,0f,0f,0f);

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
