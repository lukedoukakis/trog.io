using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Workbench : ObjectRack
{

    public static float NEW_OBJECT_CRAFT_FLIP_TIME = .5f;
    public static float NEW_OBJECT_CRAFT_FLIP_FORCE = 5000f;
    public static float NEW_OBJECT_CRAFT_UPWARD_TRANSLATION = 1f;

    public List<Item> itemsOnTable;            // the current items on the table
    public List<Item> potentialCraftedItems;   // possible items that use at least the current items on the table
    public Item currentCraftableItem;          // item that will be crafted if the craft action is taken
    Transform hammerT;

    


    public void SetWorkbench(Camp camp){
        base.SetObjectRack(camp, ItemType.Any);
        itemsOnTable = new List<Item>();
        potentialCraftedItems = new List<Item>();
        currentCraftableItem = null;
        hammerT = Utility.FindDeepChild(worldObject.transform, "Hammer");
        UpdateRecipes();
    }


    // workbench override for add objects
    public override void AddObjects(Item item, ref int countToAdd, Transform originT, ref int newRacksCount){
        //Debug.Log("Workbench: AddObjects()");
        
        // do the regular thing
        base.AddObjects(item, ref countToAdd, originT, ref newRacksCount);

        itemsOnTable.Add(item);

        // give the player another instance of the same item from their camp, if available
        EntityItems leaderItems = camp.faction.leaderHandle.entityItems;
        camp.faction.RemoveItemOwned(item, 1, null, true, leaderItems);

        UpdateRecipes();


    }


    public override void RemoveObjects(Item item, ref int countToRemove, bool moveToAnotherPlace, object destination)
    {

        base.RemoveObjects(item, ref countToRemove, moveToAnotherPlace, destination);

        if (itemsOnTable.Contains(item))
        {
            itemsOnTable.Remove(item);
        }
    
        UpdateRecipes();

    }




    void UpdateRecipes(){

        //Debug.Log("items on table count: " + itemsOnTable.Count);
        CraftingRecipe currentRecipe = new CraftingRecipe(itemsOnTable.Count > 0 ? itemsOnTable[0] : null, itemsOnTable.Count > 1 ? itemsOnTable[1] : null, itemsOnTable.Count > 2 ? itemsOnTable[2] : null);
        currentCraftableItem = CraftingRecipe.GetMatchingItem(currentRecipe);
        //Utility.FindDeepChild(hammerT, "HoverTrigger").GetComponent<BoxCollider>().enabled = !(currentCraftableItem == null);
        potentialCraftedItems = CraftingRecipe.GetOnTheWayItems(currentRecipe);

    }

    // logic following when the player prompts to craft an item
    public void OnCraft()
    {

        if(currentCraftableItem == null){ return; }

        StartCoroutine(_OnCraft());

        IEnumerator _OnCraft()
        {
            // create object and consume ingredients
            Item craftedItem = currentCraftableItem;
            Transform orientation = this.worldObject_orientationParent.Find("ItemOrientationCraftedItem");
            GameObject craftedObject = Utility.InstantiateSameName(craftedItem.worldObjectPrefab, orientation.position, Quaternion.identity);
            Utility.ToggleObjectPhysics(craftedObject, false, false, false, false);
            ConsumeRecipeObjects();

            yield return StartCoroutine(Utility.FlipForTime(craftedObject, NEW_OBJECT_CRAFT_UPWARD_TRANSLATION, NEW_OBJECT_CRAFT_FLIP_FORCE, NEW_OBJECT_CRAFT_FLIP_TIME));
            yield return new WaitForSecondsRealtime(.1f);
            Destroy(craftedObject);

            // add to faction
            Transform tempT = new GameObject().transform;
            tempT.SetPositionAndRotation(craftedObject.transform.position, craftedObject.transform.rotation);
            camp.faction.AddItemOwned(craftedItem, 1, null, tempT, 0f);
            Utility.DestroyInSeconds(tempT.gameObject, 5f);
            

            yield return new WaitUntil( () => !camp.faction.itemLogisticsHappening);
            StockWithItemsFromRecipe(craftedItem);

            yield return new WaitUntil( () => !camp.faction.itemLogisticsHappening);
            UpdateRecipes();

        }



    }

    void ConsumeRecipeObjects()
    {

        // Debug.Log("ConsumeRecipeItems()");

        foreach(Item item in itemsOnTable.ToArray()){
            camp.faction.RemoveItemOwned(item, 1, this, false, null);
        }

        //itemsOnTable.Clear();

        // Debug.Log("ConsumeRecipeItems() DONE");
        // Debug.Log("itemsOnTable count: " + itemsOnTable.Count);
        // Debug.Log("objectsOnRack count: " + objectsOnRack.Count);
        
    }

    void StockWithItemsFromRecipe(Item item)
    {
        // Debug.Log("StockWithItemsFromRecipe()");

        CraftingRecipe recipe = item.craftingRecipe;

        // clear the table and restock with the necessary items by moving objects from any available racks in camp
        ClearTable();
        foreach(Item recipeItem in recipe.requiredItems)
        {
            if (recipeItem != null)
            {
                if (camp.faction.GetItemCount(recipeItem) > 0)
                {
                    camp.faction.RemoveItemOwned(recipeItem, 1, null, true, this);
                }
            }
        }


    }

    // moves each item on the table to any available racks in camp
    public void ClearTable()
    {
        // Debug.Log("ClearTable()");

        Item item;
        foreach(GameObject worldObject in objectsOnRack.ToArray())
        {
            item = Item.GetItemByName(worldObject.name);
            camp.faction.RemoveItemOwned(item, 1, this, true, null);
        }
        //itemsOnTable.Clear();

        // Debug.Log("ClearTable() DONE");
        // Debug.Log("itemsOnTable count: " + itemsOnTable.Count);
        // Debug.Log("objectsOnRack count: " + objectsOnRack.Count);
    }



}

