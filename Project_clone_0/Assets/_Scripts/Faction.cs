using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Faction : ScriptableObject
{


    public bool isPlayerFaction;
    public int id;
    public string factionName;
    public List<EntityHandle> members;
    public ItemCollection ownedItems;
    public Camp camp;
    public List<GameObject> targetedObjects; // items being pursued by members of this faction



    public void AddMember(EntityHandle handle){
        members.Add(handle);
        handle.entityInfo.faction = this;
    }

    public static void AddItemTargeted(Faction fac, GameObject o){
        fac.targetedObjects.Add(o);
    }

    public static void AddItemOwned(Faction fac, GameObject o, int count){
        fac.ownedItems.AddItem(Item.GetItemByName(o.name), count);
    }
    public static void AddItemOwned(Faction fac, string itemName, int count){
        fac.ownedItems.AddItem(Item.GetItemByName(itemName), count);
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
        f.members = new List<EntityHandle>();
        f.ownedItems = new ItemCollection();
        f.isPlayerFaction = _isPlayerFaction;
        f.targetedObjects = new List<GameObject>();
        return f;
    }
}
