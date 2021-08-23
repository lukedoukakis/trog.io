using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Faction : ScriptableObject
{


    public bool isPlayerFaction;
    public int id;
    public string factionName;
    public int population;
    public ItemCollection ownedItems;
    public Camp camp;
    public List<EntityHandle> members;
    public List<GameObject> targetedObjects; // items being pursued by members of this factio




    void Init(){
        members = new List<EntityHandle>();
    }
    public void AddMember(EntityHandle handle){
        members.Add(handle);
        handle.entityInfo.faction = this;
        ++population;
    }

    public static void AddItemTargeted(GameObject o, Faction fac){
        fac.targetedObjects.Add(o);
    }
    public static void RemoveItemTargeted(GameObject o, Faction fac){
        fac.targetedObjects.Remove(o);
    }


    public override string ToString(){
        string str = factionName + ": ";
        foreach(EntityHandle handle in members){
            str += handle.entityInfo.nickname + ", ";
        }
        return str;
    }



    public static void OnItemPickup(Item i, GameObject o, Faction fac){
        
    }

    public static bool ItemIsTargetedByFaction(GameObject o, Faction fac){
        return fac.targetedObjects.Contains(o);
    }



    public static Faction GenerateFaction(string _factionName, bool _isPlayerFaction){
        Faction f = ScriptableObject.CreateInstance<Faction>();
        f.id = UnityEngine.Random.Range(0, int.MaxValue);
        f.factionName = _factionName;
        f.population = 0;
        f.ownedItems = new ItemCollection();
        f.isPlayerFaction = _isPlayerFaction;
        f.targetedObjects = new List<GameObject>();
        f.Init();
        return f;
    }
}
