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
    public List<EntityHandle> party;
    public ItemCollection ownedItems;
    public Camp camp;
    public List<GameObject> targetedObjects; // items being pursued by members of this faction





    // send command to all party members within radius
    public void SendPartyCommand(string command, Vector3 callPosition, float radius){
        EntityBehavior behavior;
        foreach(EntityHandle handle in party.ToArray()){
            //Debug.Log("party member do shit");
            if(Vector3.Distance(callPosition, handle.transform.position) <= radius)
            {
                behavior = handle.entityBehavior;
                behavior.InsertActionImmediate(Action.GenerateAction(command, behavior.entityHandle), true);
            }
        }
        //Debug.Log("Commands sent!");
    }


    public void AddMember(EntityHandle handle, bool addToparty){
        handle.entityInfo.faction = this;
        members.Add(handle);
        if(addToparty)
        {
            AddToParty(handle);
        }
    }

    public void AddToParty(EntityHandle handle){
        if(!party.Contains(handle)){
            party.Add(handle);
        }
    }
    public void RemoveFromParty(EntityHandle handle){
        if(party.Contains(handle)){
            party.Remove(handle);
        }
    }

    public static void AddItemTargeted(Faction fac, GameObject o){
        fac.targetedObjects.Add(o);
    }

    public static void AddItemOwned(Faction fac, ItemCollection itemCollection, ObjectRack rack){
        foreach(KeyValuePair<Item, int> kvp in itemCollection.items){
            AddItemOwned(fac, kvp.Key, kvp.Value, rack);
        }
    }

    public static void AddItemOwned(Faction fac, Item item, int count, ObjectRack rack)
    {

        fac.ownedItems.AddItem(item, count);

        if (fac.camp != null)
        {
            if (rack == null)
            {
                ItemCollection newItems = new ItemCollection();
                newItems.AddItem(item, count);
                fac.camp.AddItemsToCamp(newItems);
            }
            else
            {
                rack.AddObjects(item, ref count);
            }
        }
    }

    public static void RemoveItemOwned(Faction fac, Item item, int count, ObjectRack rack)
    {

        fac.ownedItems.RemoveItem(item, count);

        if (fac.camp != null)
        {
            if (rack == null)
            {
                ItemCollection itemsToRemove = new ItemCollection();
                itemsToRemove.AddItem(item, count);
                fac.camp.RemoveItemsFromCamp(itemsToRemove);
            }
            else
            {
                rack.RemoveObjects(item, ref count);
            }
        }
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

    public static void OnPopulationChange(Faction fac){
        fac.camp.UpdateTentCount();
    }



    public static Faction InstantiateFaction(string _factionName, bool _isPlayerFaction){
        Faction f = ScriptableObject.CreateInstance<Faction>();
        f.id = UnityEngine.Random.Range(0, int.MaxValue);
        f.factionName = _factionName;
        f.members = new List<EntityHandle>();
        f.party = new List<EntityHandle>();
        f.ownedItems = new ItemCollection();
        f.isPlayerFaction = _isPlayerFaction;
        f.targetedObjects = new List<GameObject>();
        return f;
    }
}
