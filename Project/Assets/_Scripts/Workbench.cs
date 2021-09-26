using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Workbench : ObjectRack
{


    public List<Item> itemsOnTable;            // the current items on the table
    public List<Item> potentialCraftedItems;   // possible items that use at least the current items on the table
    public Item currentCraftableItem;          // item that will be crafted if the craft action is taken
    Transform hammerT;

    


    public void SetWorkbench(Camp camp){
        base.SetObjectRack(camp, Item.ItemType.Any);
        itemsOnTable = new List<Item>();
        potentialCraftedItems = new List<Item>();
        currentCraftableItem = null;
        hammerT = Utility.FindDeepChild(worldObject.transform, "Hammer");
        UpdateRecipes();
    }


    // workbench override for add objects
    public override void AddObjects(Item item, ref int countToAdd){
        //Debug.Log("Workbench: AddObjects()");
        
        // do the regular thing
        base.AddObjects(item, ref countToAdd);

        // todo: workbench magic: look at recipes and highlight other items that can be added
        itemsOnTable.Add(item);
        //Debug.Log("added item to table");

        UpdateRecipes();


    }

    public override void RemoveObjects(Item item, ref int countToRemove){

        base.RemoveObjects(item, ref countToRemove);

        if (itemsOnTable.Contains(item))
        {
            itemsOnTable.Remove(item);
        }
    

        UpdateRecipes();

    }




    void UpdateRecipes(){

        //Debug.Log("items on table count: " + itemsOnTable.Count);
        CraftingRecipe currentRecipe = new CraftingRecipe(itemsOnTable.Count > 0 ? itemsOnTable[0] : null, itemsOnTable.Count > 1 ? itemsOnTable[1] : null, itemsOnTable.Count > 2 ? itemsOnTable[2] : null);
        
        // string s = "";
        // foreach (Item item in currentRecipe.requiredItems)
        // {
        //     if (item == null)
        //     {
        //         s += "null, ";
        //     }
        //     else
        //     {
        //         s += item.nme + ", ";
        //     }
        // }
        // Debug.Log("items on table: " + s);

        currentCraftableItem = CraftingRecipe.GetMatchingItem(currentRecipe);
        //Utility.FindDeepChild(hammerT, "HoverTrigger").GetComponent<BoxCollider>().enabled = !(currentCraftableItem == null);

        potentialCraftedItems = CraftingRecipe.GetOnTheWayItems(currentRecipe);

        //Debug.Log("current craftable item: " + (currentCraftableItem == null ? "null" : currentCraftableItem.nme));



        // if(currentCraftableItem == null){
        //     Debug.Log("Matching item: null");
        // }
        // else{
        //     Debug.Log("Matching item: " + currentCraftableItem.nme);

        // }
        // Debug.Log("Possible items: ");
        // foreach(Item item in potentialCraftedItems){
        //     Debug.Log(item.nme);
        // }

    }

    // logic following when the player prompts to craft an item
    public void OnCraft(){

        Transform orientation = this.worldObject_orientationParent.Find("ItemOrientationCraftedItem");
        GameObject obj = Utility.InstantiatePrefabSameName(currentCraftableItem.worldObject);
        obj.transform.position = orientation.position;
        obj.transform.rotation = orientation.rotation;

        ConsumeRecipeObjects();
        UpdateRecipes();

    }

    void ConsumeRecipeObjects(){

        foreach(Item item in itemsOnTable.ToArray()){
            Faction.RemoveItemOwned(camp.faction, item, 1, this);
        }
        itemsOnTable.Clear();
        
    }



}

