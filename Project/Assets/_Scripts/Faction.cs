using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Faction : MonoBehaviour
{


    public int id;
    public string factionName;
    public EntityHandle leaderHandle;
    public List<EntityHandle> memberHandles;
    public List<EntityHandle> partyHandles;
    public ItemCollection ownedItems;
    public Camp camp;
    public List<GameObject> targetedObjects; // items being pursued by members of this faction
    public bool leaderInCamp;

    public bool itemLogisticsHappening;





    // send command to all party members within radius
    public void SendPartyCommand(string command)
    {
        foreach(EntityHandle commandeeHandle in partyHandles.ToArray()){
            //Debug.Log("party member do shit");
            if(commandeeHandle != null)
            {
                commandeeHandle.entityBehavior.InsertActionImmediate(ActionParameters.GenerateActionParameters(command, commandeeHandle), true);  
            }
        }
        //Debug.Log("Commands sent!");
    }
    public void SendPartyCommand(ActionParameters ap)
    {
        foreach(EntityHandle commandeeHandle in partyHandles.ToArray()){
            //Debug.Log("party member do shit");
            if(commandeeHandle != null)
            {
                ActionParameters newAp = ActionParameters.Clone(ap);
                ap.doerHandle = commandeeHandle;
                commandeeHandle.entityBehavior.InsertActionImmediate(newAp, true);  
            }
        }
    }

    public void SendIndividualCommand(EntityHandle calleeHandle, string command, Vector3 callPosition)
    {
        calleeHandle.entityBehavior.InsertActionImmediate(ActionParameters.GenerateActionParameters(command, calleeHandle), true);
    }


    public void AddMember(EntityHandle handle, bool addToparty){
        handle.entityInfo.faction = this;
        memberHandles.Add(handle);
        if(addToparty)
        {
            AddToParty(handle);
        }
    }

    public void AddToParty(EntityHandle handle){
        if(!partyHandles.Contains(handle)){
            partyHandles.Add(handle);
        }
    }
    public void RemoveFromParty(EntityHandle handle){
        if(partyHandles.Contains(handle)){
            partyHandles.Remove(handle);
        }
    }

    public void AddItemTargeted(GameObject o){
        targetedObjects.Add(o);
    }

    public void AddItemsOwned(ItemCollection itemCollection, ObjectRack rack, Transform originT, float delay)
    {
        foreach (KeyValuePair<Item, int> kvp in itemCollection.items)
        {
            AddItemOwned(kvp.Key, kvp.Value, rack, originT, delay);
        }

    }
    public void AddItemOwned(Item item, int count, ObjectRack rack, Transform originT, float delay)
    {
        StartCoroutine(_AddItemOwned(item, count, rack, originT, delay));
    }
    IEnumerator _AddItemOwned(Item item, int count, ObjectRack rack, Transform originT, float delay)
    {
        
        itemLogisticsHappening = true;

        // wait for delay
        yield return new WaitForSecondsRealtime(delay);

        //Debug.Log("Adding item: " + item.nme);
        ownedItems.AddItem(item, count);

        if (camp != null)
        {
            if (rack == null)
            {
                ItemCollection newItems = new ItemCollection();
                newItems.AddItem(item, count);
                camp.AddItemsToCamp(newItems, originT);
            }
            else
            {
                int zeroRacksRef = 0;
                rack.AddObjects(item, ref count, originT, ref zeroRacksRef);
            }
        }
        yield return null;

        itemLogisticsHappening = false;
    }
    

    public void RemoveItemOwned(Item item, int count, ObjectRack rackToRemoveFrom, bool moveToAnotherPlace, object destination)
    {

        ownedItems.RemoveItem(item, count);

        if (camp != null)
        {
            if (rackToRemoveFrom == null)
            {
                ItemCollection itemsToRemove = new ItemCollection();
                itemsToRemove.AddItem(item, count);
                camp.RemoveItemsFromCamp(itemsToRemove, moveToAnotherPlace, destination);
            }
            else
            {
                rackToRemoveFrom.RemoveObjects(item, ref count, moveToAnotherPlace, destination);
            }
        }
    }


    public int GetItemCount(Item item){
        return ownedItems.GetItemCount(item);

    }

    public void RemoveItemTargeted(GameObject o){
        targetedObjects.Remove(o);
    }


    public override string ToString(){
        string str = factionName + ": ";
        foreach(EntityHandle handle in memberHandles){
            str += handle.entityInfo.nickname + ", ";
        }
        return str;
    }



    public static void OnItemPickup(Item i, GameObject o, Faction fac)
    {
        
    }

    public bool ItemIsTargetedByThisFaction(GameObject o)
    {
        return targetedObjects.Contains(o);
    }

    public void OnPopulationChange(){
        camp.UpdateTentCount();
    }

    public void UpdateLeaderCampStatus()
    {
        leaderInCamp = leaderHandle.entityPhysics.isInsideCamp;
    }



    public static Faction InstantiateFaction(string _factionName){
        Faction f = GameManager.current.gameObject.AddComponent<Faction>();
        f.id = UnityEngine.Random.Range(0, int.MaxValue);
        f.factionName = _factionName;
        f.leaderHandle = null;
        f.memberHandles = new List<EntityHandle>();
        f.partyHandles = new List<EntityHandle>();
        f.ownedItems = new ItemCollection();
        f.targetedObjects = new List<GameObject>();
        return f;
    }


    public static bool FactionCampExists(Faction faction)
    {
        return !(faction.camp == null);
    }
}
