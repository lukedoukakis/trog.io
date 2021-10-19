using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CraftingRecipe
{

    public List<Item> requiredItems;


    public CraftingRecipe(Item i0, Item i1, Item i2){
        requiredItems = new List<Item>{ i0, i1, i2 };
        requiredItems = requiredItems.OrderBy(item => (item == null ? int.MaxValue : item.id)).ToList();
    }

    // return item whose recipe matches the one given
    public static Item GetMatchingItem(CraftingRecipe recipe)
    {
        
        CraftingRecipe compareRecipe;
        bool match;
        foreach (Item item in Item.Items.Values)
        {
            compareRecipe = item.craftingRecipe;
            if (compareRecipe != null)
            {
                match = true;
                for (int i = 0; i < compareRecipe.requiredItems.Count; ++i)
                {
                    if (compareRecipe.requiredItems[i] == null)
                    {
                        if (recipe.requiredItems[i] != null)
                        {
                            match = false;
                            break;
                        }
                    }
                    else
                    {
                        if (!compareRecipe.requiredItems[i].Equals(recipe.requiredItems[i]))
                        {
                            match = false;
                            break;
                        }
                    }
                }
                if (match) { return item; }
            }

        }
            
        return null;
    }

    // return list of items whose recipes have at least all the items of given recipe
    public static List<Item> GetOnTheWayItems(CraftingRecipe recipe)
    {

        List<Item> items = new List<Item>();
        if(recipe.Equals(CraftingRecipe.None)){ return items; }
        
        // loop through all recipes, adding them to list if their recipes contain all non-null items in compareRecipe

        CraftingRecipe compareRecipe;
        bool match;
        foreach (Item item in Item.Items.Values)
        {
            compareRecipe = item.craftingRecipe;
            if (compareRecipe != null)
            {
                match = true;
                foreach (Item recipeItem in recipe.requiredItems.Where(ri => ri != null))
                {
                    if (!compareRecipe.requiredItems.Contains(recipeItem))
                    {
                        match = false;
                        break;
                    }
                }
                if (match)
                {
                    items.Add(item);
                }
            }

        }

        return items;
    }


    public static CraftingRecipe None = new CraftingRecipe(null, null, null);

}
