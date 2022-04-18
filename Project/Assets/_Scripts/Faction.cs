using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;


public enum FactionRole{ Leader, Follower }
public enum FactionStartingItemsTier{ PlayerTest, Nothing, One, Weak, Medium, Strong }

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
    public bool markedForDestruction;

    public Material clothingMaterial;

    public Dictionary<Item, int> ItemPhysicalOverflowDict;

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


    public void SendPartyCommand(ActionParameters ap, List<EntityHandle> specifiedHandles)
    {
        foreach(EntityHandle commandeeHandle in specifiedHandles.ToArray()){
            //Debug.Log("party member do shit");
            if(commandeeHandle != null)
            {
                ActionParameters newAp = ActionParameters.Clone(ap);
                ap.doerHandle = commandeeHandle;
                commandeeHandle.entityBehavior.InsertActionAndExecuteAsap(newAp, true);  
            }
        }
    }

    public void SendPartyCommandToAll(ActionParameters ap)
    {
        SendPartyCommand(ap, partyHandles);
    }


    public void SendIndividualCommand(EntityHandle calleeHandle, string command, Vector3 callPosition)
    {
        calleeHandle.entityBehavior.InsertActionAndExecuteAsap(ActionParameters.GenerateActionParameters(command, calleeHandle), true);
    }


    // transfer leader status to new leader
    public void SetLeader(EntityHandle newLeaderHandle)
    {
        if(leaderHandle != null)
        {
            leaderHandle.entityInfo.SetFactionRole(FactionRole.Follower);
        }
        newLeaderHandle.entityInfo.SetFactionRole(FactionRole.Leader);
        leaderHandle = newLeaderHandle;
    }


    public void AddMember(EntityHandle newMemberHandle, FactionRole factionRole, bool addToparty)
    {

        EntityInfo newMemberInfo = newMemberHandle.entityInfo;

        newMemberInfo.faction = this;
        newMemberInfo.SetFactionRole(factionRole);
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


        if(camp != null)
        {
            yield return new WaitUntil(() => !camp.placingCampComponents);
        }
        
        itemLogisticsHappening = true;

        // wait for delay
        yield return new WaitForSecondsRealtime(delay);

        //Debug.Log("Adding item: " + item.nme);

        int countOwned = GetItemCount(item);
        int campTotalCapacity = Camp.GetItemPhysicalCapacity(item);
        int maximumPhysicalToAdd = Mathf.Max(0, campTotalCapacity - countOwned);
        int countToAddPhysically = Mathf.Min(count, maximumPhysicalToAdd);
        int countToAddOverflow = count - countToAddPhysically;
        // Debug.Log("count owned: " + countOwned);
        // Debug.Log("camp total capacity: " + campTotalCapacity);
        // Debug.Log("physical add: " + countToAddPhysically);
        // Debug.Log("overflow add: " + countToAddOverflow);
        // Debug.Log("");

        ownedItems.AddItem(item, count);

        // if item is a camp component, add it to the camp and break;
        if(item.type.Equals(ItemType.CampComponent))
        {
            if (camp != null)
            {
                for(int i = 0; i < count; ++i)
                {
                    camp.AddCampComponentItem(item);
                }
            }
            itemLogisticsHappening = false;
            yield break;
        }

        // if faction item count is less than camp maximum physical capacity, add the physical object to camp
        if (camp != null)
        {
            if (rack == null)
            {

                ItemCollection newItems;
                if(countToAddPhysically > 0)
                {
                    newItems = new ItemCollection();
                    newItems.AddItem(item, countToAddPhysically);
                    camp.AddItemsToCamp(newItems, originT, true);
                }

                if(countToAddOverflow > 0)
                {
                    Debug.Log("Adding not physically... " + countToAddOverflow);
                    newItems = new ItemCollection();
                    newItems.AddItem(item, countToAddOverflow);
                    camp.AddItemsToCamp(newItems, originT, false);
                }
                
            }
            else
            {

                if(countToAddPhysically > 0)
                {  
                    int zeroRacksRef_0 = 0;
                    rack.AddObjects(item, ref countToAddPhysically, originT, ref zeroRacksRef_0, true);
                }

                if(countToAddOverflow > 0)
                {
                    int zeroRacksRef_1 = 0;
                    rack.AddObjects(item, ref countToAddOverflow, originT, ref zeroRacksRef_1, false);
                }
                
            }
        }

        for(int i = 0; i < countToAddOverflow; ++i)
        {
            IncrementOverflowItem(item);
        }

    

    
        itemLogisticsHappening = false;
    }


    public void RemoveObjectOwnedOfType(ItemType itemType, int count, ObjectRack rackToRemoveFrom, bool moveToAnotherPlace, object destination)
    {
        Item[] itemsOfType = Item.Items.Values.Where(item => item.type == itemType).ToArray();

        int countToRemove = count;
        int countOfItem;
        foreach(Item item in itemsOfType)
        {
            countOfItem = this.GetItemCount(item);
            if(countOfItem > 0)
            {
                int r = Math.Min(countOfItem, countToRemove);
                this.RemoveItemOwned(item, r, rackToRemoveFrom, moveToAnotherPlace, destination);
                countToRemove -= r;
            }
        }
    }
    

    public void RemoveItemOwned(Item item, int count, ObjectRack rackToRemoveFrom, bool moveToAnotherPlace, object destination)
    {

        if(destination != null)
        {
            if(destination is Workbench)
            {
                ownedItems.RemoveItem(item, count);
            }
        }
        else
        {
            ownedItems.RemoveItem(item, count);
        }


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


    public void RemoveAllItemsOwnedFromWorld()
    {
        Dictionary<Item, int> removeDict = new Dictionary<Item, int>(ownedItems.items);
        foreach(KeyValuePair<Item, int> kvp in removeDict)
        {
            RemoveItemOwned(kvp.Key, kvp.Value, null, false, null);
        }
    }

    // returns total number of items owned
    public int GetItemCountAbsolute(Item item)
    {
        return ownedItems.GetItemCount(item);
    }
    public int GetItemCountAbsolute(Enum itemType)
    {
        return ownedItems.GetItemCount(itemType);
    }

    // returns total number of items not on workbench
    public int GetItemCount(Item item)
    {
        int rawCount = GetItemCountAbsolute(item);
        int countOnWorkbench = 0;
        if(camp != null)
        {
            if(camp.workbench != null)
            {
                countOnWorkbench = camp == null ? 0 : camp.workbench.GetObjectCountsOnRackThatAreItemCount(item);
            }
        }
        return rawCount - countOnWorkbench;
    }

    public int GetItemCount(Enum itemType)
    {
        int rawCount = GetItemCountAbsolute(itemType);
        int countOnWorkbench = 0;
        if(camp != null)
        {
            if(camp.workbench != null)
            {
                countOnWorkbench = camp == null ? 0 : camp.workbench.GetObjectCountsOnRackThatAreItemCount(itemType);
            }
        }

        //Debug.Log(rawCount - countOnWorkbench);
        return rawCount - countOnWorkbench;
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
        foreach(GameObject gameObject in targetedObjects.ToArray())
        {
            if(ReferenceEquals(gameObject, o))
            {
                return true;
            }
        }
        return false;
    }

    public void OnPopulationChange(){
        //camp.UpdateTentCount();
    }

    public void UpdateLeaderCampStatus()
    {
        leaderInCamp = leaderHandle.entityPhysics.isInsideCamp;
        leaderHandle.entityBehavior.ResetFollowPosition();
        foreach(EntityHandle partyHandle in partyHandles)
        {
            partyHandle.entityBehavior.ResetFollowPosition();
        }
    }

    // add starting items (and tribe members) to faction, mainly used for spawning AI factions
    public void AddStartingResources(FactionStartingItemsTier tier)
    {

        //Debug.Log("AddStartingResources()");

        int memberCount;
        int itemCount;
        switch (tier)
        {
            case FactionStartingItemsTier.PlayerTest :
                memberCount = 0;
                itemCount = 0;
                break;
            case FactionStartingItemsTier.Nothing :
                memberCount = 2;
                itemCount = 0;
                break;
            case FactionStartingItemsTier.One :
                memberCount = 1;
                itemCount = 2;
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



        // spawn items
        //AddItemOwned(Item.ClothingTest, itemCount, null, leaderHandle.transform, 0f);
        AddItemOwned(Item.Meat, itemCount, null, leaderHandle.transform, 0f);
        AddItemOwned(Item.SpearStone, itemCount/2, null, leaderHandle.transform, 0f);
        AddItemOwned(Item.AxeStone, itemCount/2, null, leaderHandle.transform, 0f);
        AddItemOwned(Item.SpearBone, itemCount/2, null, leaderHandle.transform, 0f);
        AddItemOwned(Item.AxeBone, itemCount/2, null, leaderHandle.transform, 0f);
        AddItemOwned(Item.WoodPiece, itemCount, null, leaderHandle.transform, 0f);
        AddItemOwned(Item.BonePiece, itemCount, null, leaderHandle.transform, 0f);
        AddItemOwned(Item.StoneSmall, itemCount, null, leaderHandle.transform, 0f);
        AddItemOwned(Item.PeltBear, itemCount/2, null, leaderHandle.transform, 0f);


        // spawn tribe members
        bool spawnWithGear = !tier.Equals(FactionStartingItemsTier.Nothing);
        //spawnWithGear = false;
        for(int i = 0; i < memberCount; ++i)
        {
            StartCoroutine(ClientCommand.instance.SpawnCharacterAsFollowerWhenReady(leaderHandle, leaderHandle.transform.position, spawnWithGear));
        }
    }

    public void IncrementOverflowItem(Item item)
    {
        try
        {
            ItemPhysicalOverflowDict[item] += 1;
        }
        catch(KeyNotFoundException)
        {
            ItemPhysicalOverflowDict.Add(item, 1);
        }
    }
    public void DecrementOverflowItem(Item item)
    {
        try
        {
            ItemPhysicalOverflowDict[item] -= 1;
        }
        catch(KeyNotFoundException)
        {
            ItemPhysicalOverflowDict[item] = 0;
        }
    }

    public int GetOverflowItemCount(Item item)
    {
        try
        {
            return ItemPhysicalOverflowDict[item];
        }
        catch(KeyNotFoundException)
        {
            return 0;
        }
    }


    // pack up all faction items from camp and (todo: add to backpack)
    public void PackUp()
    {
        camp.Dismantle();
        UpdateLeaderCampStatus();
    }

    public void DestroyFaction()
    {

        // // destroy camp and all items in it
        // RemoveAllItemsOwnedFromWorld();

        // // pack up camp
        // if(camp != null)
        // {
        //     PackUp();
        // }

        // remove all members from the world
        foreach(EntityHandle memberHandle in memberHandles.ToArray())
        {
            memberHandle.RemoveFromWorld();
        }

        // destroy this
        Destroy(this);
    }

    public void MarkForDestruction()
    {
        markedForDestruction = true;
    }

    public bool IsMarkedForDestruction()
    {
        return markedForDestruction;
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
        f.ItemPhysicalOverflowDict = new Dictionary<Item, int>();
        f.clothingMaterial = MaterialController.instance.GetRandomClothingMaterial();
        return f;
    }


    public static bool FactionCampExists(Faction faction)
    {
        return !(faction.camp == null);
    }
}
