using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Workbench : ObjectRack
{


    List<Item> itemsOnTable;
    


    public void SetWorkbench(Camp camp){
        base.SetObjectRack(camp, Item.Type.Any);
        itemsOnTable = new List<Item>();
    }


    // workbench override for add objects
    public override void AddObjects(Item item, ref int countToAdd){
        //Debug.Log("Workbench: AddObjects()");
        
        // do the regular thing
        base.AddObjects(item, ref countToAdd);


        // todo: workbench magic: look at recipes and highlight other items that can be added
        itemsOnTable.Add(item);
        Debug.Log("added item to table");

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

        Debug.Log("items on table count: " + itemsOnTable.Count);
        CraftingRecipe currentRecipe = new CraftingRecipe(itemsOnTable.Count > 0 ? itemsOnTable[0] : null, itemsOnTable.Count > 1 ? itemsOnTable[1] : null, itemsOnTable.Count > 2 ? itemsOnTable[2] : null);
        foreach (Item item in currentRecipe.requiredItems)
        {
            if (item == null)
            {
                Debug.Log("null");
            }
            else
            {
                Debug.Log(item.nme);

            }
        }

        Item matchingItem = CraftingRecipe.GetMatchingItem(currentRecipe);
        List<Item> possibleItems = CraftingRecipe.GetOnTheWayItems(currentRecipe);



        if(matchingItem == null){
            Debug.Log("Matching item: null");
        }
        else{
            Debug.Log("Matching item: " + matchingItem.nme);

        }
        // Debug.Log("\nPossible items: ");
        // foreach(Item item in possibleItems){
        //     Debug.Log(item.nme);
        // }

    }

    public Item CraftItem(Item item){
        // todo: craft item

        // consume items on table and generate new one



        return Item.None;
    }



}

