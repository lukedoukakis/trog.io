using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public enum FactionStartingItemsTier{ Nothing, Weak, Medium, Strong }

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

    public Material clothingMaterial;

    public bool itemLogisticsHappening;




    // send command to all party members within radius
    public void SendPartyCommand(string command)
    {
        foreach(EntityHandle commandeeHandle in partyHandles.ToArray()){
            //Debug.Log("party member do shit");
            if(commandeeHandle != null)
            {
                commandeeHandle.entityBehavior.InsertActionAndExecuteAsap(ActionParameters.GenerateActionParameters(command, commandeeHandle), true);  
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
                commandeeHandle.entityBehavior.InsertActionAndExecuteAsap(newAp, true);  
            }
        }
    }

    public void SendIndividualCommand(EntityHandle calleeHandle, string command, Vector3 callPosition)
    {
        calleeHandle.entityBehavior.InsertActionAndExecuteAsap(ActionParameters.GenerateActionParameters(command, calleeHandle), true);
    }


    public void AddMember(EntityHandle newMemberHandle, bool addToparty)
    {
        newMemberHandle.entityInfo.faction = this;
        memberHandles.Add(newMemberHandle);
        if(addToparty)
        {
            AddToParty(newMemberHandle);
        }
    }
    public void RemoveMember(EntityHandle removedMemberHandle)
    {
        removedMemberHandle.entityInfo.faction = null;
        memberHandles.Remove(removedMemberHandle);
        RemoveFromParty(removedMemberHandle);
    }

    public void AddToParty(EntityHandle newPartyMemberHandle){
        if(!partyHandles.Contains(newPartyMemberHandle)){
            partyHandles.Add(newPartyMemberHandle);
        }
    }
    public void RemoveFromParty(EntityHandle removedPartyMemberHandle){
        if(partyHandles.Contains(removedPartyMemberHandle)){
            partyHandles.Remove(removedPartyMemberHandle);
        }
    }

    public void AddObjectTargeted(GameObject worldObjectTargeted){
        targetedObjects.Add(worldObjectTargeted);
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

        itemLogisticsHappening = false;
    }
    

    public void RemoveItemOwned(Item item, int count, ObjectRack rackToRemoveFrom, bool moveToAnotherPlace, object destination)
    {

        bool r = true;
        if(destination != null)
        {
            if(destination is EntityItems)
            {
                r = false;
            }
        }

        if(r){ ownedItems.RemoveItem(item, count); }

        //ownedItems.RemoveItem(item, count);

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

        //Debug.Log("Faction " + item.nme + " count: " + GetItemCount(item));


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
        //camp.UpdateTentCount();
    }

    public void UpdateLeaderCampStatus()
    {
        leaderInCamp = leaderHandle.entityPhysics.isInsideCamp;
        foreach(EntityHandle partyHandle in partyHandles)
        {
            partyHandle.entityBehavior.UpdateHomePosition(leaderInCamp);
        }
    }

    // add starting items (and tribe members) to faction, mainly used for spawning AI factions
    public void AddStartingResources(FactionStartingItemsTier tier)
    {
        int memberCount;
        int itemCount;
        switch (tier)
        {
            case FactionStartingItemsTier.Nothing :
                memberCount = 2;
                itemCount = 0;
                break;
            case FactionStartingItemsTier.Weak :
                memberCount = 5;
                itemCount = 5;
                break;
            case FactionStartingItemsTier.Medium :
                memberCount = 12;
                itemCount = 12;
                break;
            case FactionStartingItemsTier.Strong :
                memberCount = 25;
                itemCount = 25;
                break;
            default :
                memberCount = 2;
                itemCount = 0;
                break;
        }

        // spawn tribe members
        bool spawnWithGear = !tier.Equals(FactionStartingItemsTier.Nothing);
        //spawnWithGear = false;
        for(int i = 0; i < memberCount; ++i)
        {
            StartCoroutine(ClientCommand.instance.SpawnNpcFollowerWhenReady(leaderHandle, leaderHandle.transform.position, spawnWithGear));
        }


        // spawn items
        //AddItemOwned(Item.ClothingTest, itemCount, null, leaderHandle.transform, 0f);
        AddItemOwned(Item.Meat, itemCount, null, leaderHandle.transform, 0f);
        AddItemOwned(Item.Spear, itemCount / 2, null, leaderHandle.transform, 0f);
        AddItemOwned(Item.Axe, itemCount / 2, null, leaderHandle.transform, 0f);
        AddItemOwned(Item.WoodPiece, itemCount, null, leaderHandle.transform, 0f);
        AddItemOwned(Item.BonePiece, itemCount, null, leaderHandle.transform, 0f);
        AddItemOwned(Item.StoneSmall, itemCount, null, leaderHandle.transform, 0f);
    }


    // pack up all faction items from camp and (todo: add to backpack)
    public void PackUp()
    {
        camp.Dismantle();
    }

    public void DestroyFaction()
    {
        // destroy camp and all items in it
        List<Item> items = new List<Item>(ownedItems.items.Keys);
        List<int> counts = new List<int>(ownedItems.items.Values);
        for(int i = 0; i < items.Count; ++i)
        {
            RemoveItemOwned(items[i], counts[i], null, false, null);
        }

        // destroy all members
        foreach(EntityHandle memberHandle in memberHandles)
        {
            memberHandle.DestroyEntity();
        }

        if(camp != null)
        {
            camp.Dismantle();
        }

        // destroy this
        Destroy(this);
    }



    public static Faction InstantiateFaction(string _factionName){
        Faction f = GameManager.instance.gameObject.AddComponent<Faction>();
        f.id = UnityEngine.Random.Range(0, int.MaxValue);
        f.factionName = _factionName;
        f.leaderHandle = null;
        f.memberHandles = new List<EntityHandle>();
        f.partyHandles = new List<EntityHandle>();
        f.ownedItems = new ItemCollection();
        f.targetedObjects = new List<GameObject>();
        f.clothingMaterial = MaterialController.instance.GetRandomClothingMaterial();
        return f;
    }


    public static bool FactionCampExists(Faction faction)
    {
        return !(faction.camp == null);
    }
}
