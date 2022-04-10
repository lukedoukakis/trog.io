using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum StatsSlot{ Base, Weapon, Clothing, Food, Tonic }

public class EntityStats : EntityComponent
{



    Dictionary<StatsSlot, Stats> statsSlots;
    List<Stats> activeStatsModifiers;
    public Stats combinedStats;
    public ItemCollection drops;


    public static float BASE_AMOUNT_HP = 100;
    public float BASE_AMOUNT_STAMINA = 100;
    public static float BASE_HP_LOSS_FROM_HIT = 8;
    public float health, maxHealth;
    public float stamina, maxStamina;


    protected override void Awake()
    {

        this.fieldName = "entityStats";

        base.Awake();

        statsSlots = new Dictionary<StatsSlot, Stats>(){
            { StatsSlot.Base, Stats.NONE },
            { StatsSlot.Weapon, Stats.NONE },
            { StatsSlot.Food, Stats.NONE },
            { StatsSlot.Clothing, Stats.NONE },
        };

        activeStatsModifiers = new List<Stats>();
        combinedStats = Stats.InstantiateStats(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);

        if(entityInfo != null){
            SetStatsSlot(StatsSlot.Base, entityInfo.speciesInfo.baseStats);
            SetBaseHpAndStamina();
            drops = entityInfo.speciesInfo.baseDrop;
        }
    

    }


    public void SetStatsSlot(StatsSlot slot, Stats newStats)
    {

        Stats oldStats = statsSlots[slot];
        if(!ReferenceEquals(oldStats, Stats.NONE))
        {
            RemoveStatsModifier(oldStats);
        }
        statsSlots[slot] = newStats;
        AddStatsModifier(newStats);

        OnStatsChange();
    }


    public void SetBaseHpAndStamina()
    {
        maxHealth = health = (BASE_AMOUNT_HP * Stats.GetStatValue(combinedStats, Stats.StatType.Health));
        maxStamina = stamina = BASE_AMOUNT_STAMINA;
    }

    public bool IsHealthFull()
    {
        return health >= maxHealth;
    }

    public bool IsStaminaFull()
    {
        return stamina >= maxStamina;
    }


    void AddStatsModifier(Stats statsToAdd){
        activeStatsModifiers.Add(statsToAdd);
        Enum statType;
        foreach(int statNum in Enum.GetValues(typeof(Stats.StatType))){
            statType = (Stats.StatType)statNum;
            Stats.SetStatValue(combinedStats, statType, Stats.GetStatValue(combinedStats, statType) + Stats.GetStatValue(statsToAdd, statType));
        }
        //Debug.Log("Added stats modifier");
    }
    void RemoveStatsModifier(Stats statsToRemove){
        if (statsToRemove != null)
        {
            bool removed = activeStatsModifiers.Remove(statsToRemove);
            if (removed)
            {
                Enum statType;
                foreach (int statNum in Enum.GetValues(typeof(Stats.StatType)))
                {
                    statType = (Stats.StatType)statNum;
                    Stats.SetStatValue(combinedStats, statType, Stats.GetStatValue(combinedStats, statType) - Stats.GetStatValue(statsToRemove, statType));
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


    public void TakeDamage(EntityHandle attackerHandle, Item attackerWeapon, Projectile projectile, bool instantKill)
    {

        // get attacker's relevant stats and (if applicable) weapon
        Stats attackerStats;
        if(projectile == null)
        {
            attackerStats = attackerHandle.entityStats.combinedStats;
        }
        else
        {
            attackerStats = projectile.stats;
            attackerWeapon = projectile.item;
        }

        // calculate damage
        float hpLoss;
        if (!instantKill)
        {
            hpLoss = Stats.CalculateDamage(entityStats.combinedStats, attackerHandle.entityStats, attackerWeapon);
        }
        else
        {
            hpLoss = float.MaxValue;
        }

        // take away health
        ApplyHealthIncrement(hpLoss * -1f, attackerHandle);

        // Debug.Log("Attacker Weapon Type: " + attackerWeapon.damageType.ToString());
        // Debug.Log("Armor Type: " + armorStatType.ToString());
        // Debug.Log("Armor against this type: " + armorFromWeaponType);
        // Debug.Log("DAMAGE: " + (int)hpLoss);
        //Debug.Log("HP: " + hp.ToString());


    }

    public void ApplyHealthIncrement(float amount, EntityHandle entityThatCaused)
    {
        health += amount; 
        if(health > maxHealth)
        {
            health = maxHealth;
        }
        else if(health <= 0)
        {
            OnHealthEmptied(entityThatCaused);
        }
    }

    public void ApplyStaminaIncrement(float amount)
    {
        stamina += amount;
        if(stamina > maxStamina)
        {
            stamina = maxStamina;
        }
        else if(stamina < 0)
        {
            stamina = 0;
        }
    }

    void OnHealthEmptied(EntityHandle entityThatCaused)
    {
        Die(entityThatCaused);
    }

    void Die(EntityHandle attackerHandle)
    {
        //Debug.Log("DED");

        // drop any items the entity is carrying onto the ground
        if(entityItems != null)
        {
            entityItems.DropEverything();
        }

        // give entity drops to faction leader of attacker
        EntityHandle dropReceiverHandle;
        Faction attackerFaction = attackerHandle.entityInfo.faction;
        if(attackerFaction.leaderHandle != null)
        {
            dropReceiverHandle = attackerFaction.leaderHandle;
        }
        else
        {
            dropReceiverHandle = attackerHandle;
        }
        SpawnDrops(this.transform.position, dropReceiverHandle);

        // remove the entity from the world
        RemoveFromWorld();
    }


    // drop drops
    void SpawnDrops(Vector3 dropSpot, EntityHandle receiverHandle){

        //Debug.Log("Adding drops to entity \'" + receiverHandle.entityInfo.nickname + "\' faction: " + drops.ToString());
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
                worldObject = Utility.InstantiateSameName(item.worldObjectPrefab);
                worldObject.transform.position = dropSpot + (Vector3.up * placementHeightOffset);
                if(item.isRackable)
                {
                    worldObject.transform.rotation = Utility.GetRandomRotation(360f);
                    Vector3 randomDirection = Utility.GetRandomVector(1f);
                    randomDirection.y = 0f;
                    worldObject.GetComponent<Rigidbody>().AddForce((randomDirection * 300f) + (Vector3.up * 3000f));
                    totalDropsList.Add(Tuple.Create<Item, GameObject>(item, worldObject));
                }
                placementHeightOffset += .3f;
                //yield return new WaitForSecondsRealtime(ObjectRack.OBJECT_PLACEMENT_DELAY_TIMESTEP);
                
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
                if(item.isRackable){
                    //Debug.Log("adding drops straight to faction");
                    // if the item is rackable, add item straight to racks
                    float delay = .5f + (i * ObjectRack.OBJECT_PLACEMENT_DELAY_TIMESTEP);
                    GameObject temp = new GameObject();
                    temp.transform.position = worldObject.transform.position;
                    receiverHandle.entityInfo.faction.AddItemOwned(item, 1, null, temp.transform, delay);
                    Utility.DestroyInSeconds(worldObject, delay);
                    Utility.DestroyInSeconds(temp, 5f);
                }
                else
                {
                    //Debug.Log("drops staying on ground");
                    // do nothing
                }
            }
            else
            {
                if(item.isRackable)
                {
                    //Debug.Log("adding drops to inventory");
                    // if not in camp, add to reciver's inventory and delay small timestep
                    receiverHandle.entityItems.AddToInventory(item, worldObject, false, .5f + i * ObjectRack.OBJECT_PLACEMENT_DELAY_TIMESTEP);
                }
                
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
            list += Stats.GetStatName(combinedStats, statType) + ": " + Stats.GetStatValue(combinedStats, statType);
        }
        
        return list;
    }


}

