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
    public static Item GetMatchingItem(CraftingRecipe compareRecipe){
        foreach(CraftingRecipe key in recipeDict.Keys){
            //Debug.Log("Comparing recipe: " + key.ToString());
            bool match = true;
            for(int i = 0; i < key.requiredItems.Count; ++i){
                if(key.requiredItems[i] == null){
                    if(compareRecipe.requiredItems[i] != null){
                        match = false;
                        break;
                    }
                }
                else
                {
                    if (!key.requiredItems[i].Equals(compareRecipe.requiredItems[i]))
                    {
                        match = false;
                        break;
                    }
                }
            }
            if(match){ return recipeDict[key]; }

        }
        return null;
    }

    // return list of items whose recipes have at least all the items of given recipe
    public static List<Item> GetOnTheWayItems(CraftingRecipe compareRecipe){

        List<Item> items = new List<Item>();
        if(compareRecipe.Equals(CraftingRecipe.None)){ return items; }
        
        // loop through all recipes, adding them to list if their recipes contain all non-null items in compareRecipe
        CraftingRecipe recipe;
        Item item;
        bool match;
        foreach(KeyValuePair<CraftingRecipe, Item> kvp in recipeDict){
            recipe = kvp.Key;
            item = kvp.Value;
            match = true;
            foreach(Item recipeItem in compareRecipe.requiredItems.Where(i => i != null)){
                if(!recipe.requiredItems.Contains(recipeItem)){
                    match = false;
                    break;
                }
            }
            if(match){
                items.Add(kvp.Value);
            }
        }

        return items;
    }

    public static Dictionary<CraftingRecipe, Item> recipeDict = new Dictionary<CraftingRecipe, Item>()
    {
        { new CraftingRecipe(Item.WoodPiece, null, null), Item.Torch },
        { new CraftingRecipe(Item.WoodPiece, Item.SmallStone, null), Item.Spear },
        { new CraftingRecipe(Item.WoodPiece, Item.Stone, null), Item.Axe },
    };

    // public override bool Equals(object obj)
    // {

    //     if (obj == null || GetType() != obj.GetType())
    //     {
    //         return false;
    //     }

    //     return requiredItems.Equals(((CraftingRecipe)obj).requiredItems);

    // }
    // public override int GetHashCode()
    // {
    //     return base.GetHashCode();
    // }

    public static CraftingRecipe None = new CraftingRecipe(null, null, null);

}
