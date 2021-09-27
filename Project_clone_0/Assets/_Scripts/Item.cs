using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Item : ScriptableObject
{

    public enum ItemType{
        Any, Food, Weapon, Clothing, Wood, Bone, Misc
    }

    public enum ItemHoldStyle{
        Hug, UnderArm, Spear, Axe, OverShoulder, Torch
    }

    public enum ItemDamageType{
        Blunt, Pierce, Slash
    };




    // Item defs
    public static Item None = InitiailizeItem(0, "None", ItemType.Weapon, Stats.NONE, ItemHoldStyle.Torch, ItemDamageType.Blunt, EmptyItemCollection, Resources.Load<GameObject>("Items/Torch/Gameobject/Torch"), Resources.Load<Image>("Items/Torch/Image"));
    public static Item Torch = InitiailizeItem(1, "Torch", ItemType.Weapon, Stats.NONE, ItemHoldStyle.Torch, ItemDamageType.Blunt, EmptyItemCollection, Resources.Load<GameObject>("Items/Torch/Gameobject/Torch"), Resources.Load<Image>("Items/Torch/Image"));
    public static Item Stone = InitiailizeItem(2, "Stone", ItemType.Misc, Stats.NONE, ItemHoldStyle.UnderArm, ItemDamageType.Blunt, EmptyItemCollection, Resources.Load<GameObject>("Items/Stone/Gameobject/Stone"), Resources.Load<Image>("Items/Stone/Image"));
    public static Item SmallStone = InitiailizeItem(3, "SmallStone", ItemType.Misc, Stats.NONE, ItemHoldStyle.Torch, ItemDamageType.Blunt, EmptyItemCollection, Resources.Load<GameObject>("Items/SmallStone/Gameobject/SmallStone"), Resources.Load<Image>("Items/SmallStone/Image"));
    public static Item Spear = InitiailizeItem(4, "Spear", ItemType.Weapon, Stats.WEAPON_SPEAR, ItemHoldStyle.Spear, ItemDamageType.Pierce, EmptyItemCollection, Resources.Load<GameObject>("Items/Spear/Gameobject/Spear"), Resources.Load<Image>("Items/Spear/Image"));
    public static Item Axe = InitiailizeItem(5, "Axe", ItemType.Weapon, Stats.WEAPON_AXE, ItemHoldStyle.Axe, ItemDamageType.Slash, EmptyItemCollection, Resources.Load<GameObject>("Items/Axe/Gameobject/Axe"), Resources.Load<Image>("Items/Axe/Image"));
    public static Item LogFir = InitiailizeItem(6, "LogFir", ItemType.Misc, Stats.NONE, ItemHoldStyle.Hug, ItemDamageType.Blunt, EmptyItemCollection, Resources.Load<GameObject>("Items/LogFir/Gameobject/LogFir"), Resources.Load<Image>("Items/LogFir/Image"));
    public static Item CarcassBear = InitiailizeItem(7, "CarcassBear", ItemType.Misc, Stats.NONE, ItemHoldStyle.UnderArm, ItemDamageType.Blunt, EmptyItemCollection, Resources.Load<GameObject>("Items/CarcassBear/Gameobject/CarcassBear"), Resources.Load<Image>("Items/CarcassBear/Image"));

    // testing items
    public static Item ClothingTest = InitiailizeItem(8, "ClothingTest", ItemType.Clothing, Stats.CLOTHING_TESTCLOTHING, ItemHoldStyle.UnderArm, ItemDamageType.Blunt, EmptyItemCollection, Resources.Load<GameObject>("Items/ClothingTest/Gameobject/ClothingTest"), Resources.Load<Image>("Items/ClothingTest/Image"));
    public static Item FoodTest = InitiailizeItem(9, "FoodTest", ItemType.Food, Stats.FOOD_TESTFOOD, ItemHoldStyle.Torch, ItemDamageType.Blunt, EmptyItemCollection, Resources.Load<GameObject>("Items/FoodTest/Gameobject/FoodTest"), Resources.Load<Image>("Items/FoodTest/Image"));
    public static Item WoodPiece = InitiailizeItem(10, "WoodPiece", ItemType.Wood, Stats.WOOD_PIECE, ItemHoldStyle.UnderArm, ItemDamageType.Blunt, EmptyItemCollection, Resources.Load<GameObject>("Items/WoodPiece/Gameobject/WoodPiece"), Resources.Load<Image>("Items/WoodPiece/Image"));




    public static Dictionary<string, Item> Items = new Dictionary<string, Item>
    {
        { "Torch", Torch },
        { "Stone", Stone },
        { "SmallStone", SmallStone },
        { "Spear", Spear },
        { "Axe", Axe },
        { "LogFir", LogFir },
        { "CarcassBear", CarcassBear },
        
        { "ClothingTest", ClothingTest },
        { "FoodTest", FoodTest },
        { "WoodPiece", WoodPiece },

    };
    public static Item GetItemByName(string _nme){

        //Debug.Log("searching for key: " + _nme);

        return Items[_nme];
       
    }

    public static bool IsRackable(Item item){
        return item.type.Equals(Item.ItemType.Food) || item.type.Equals(Item.ItemType.Weapon) || item.type.Equals(Item.ItemType.Clothing) || item.type.Equals(Item.ItemType.Wood) || item.type.Equals(Item.ItemType.Bone);
    }




    public int id;
    public string nme;
    public Enum type;
    public Stats stats;
    public Enum holdStyle;
    public Enum damageType;
    public ItemCollection contents;
    public GameObject worldObject;
    public Image image;


    public static Item InitiailizeItem(int _id, string _nme, Enum _type, Stats _stats, Enum _holdStyle, Enum _damageType, ItemCollection _contents, GameObject _gameobject, Image _image){
        Item i = ScriptableObject.CreateInstance<Item>();
        i.id = _id;
        i.nme = _nme;
        i.type = _type;
        i.stats = _stats;
        i.holdStyle = _holdStyle;
        i.damageType = _damageType;
        i.contents = _contents;
        i.worldObject = _gameobject;
        i.image = _image;
        return i;
    }

    public static ItemCollection EmptyItemCollection = new ItemCollection(new Dictionary<Item, int>());

}

// structure of 3 items
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
        { new CraftingRecipe(Item.LogFir, null, null), Item.Torch },
        { new CraftingRecipe(Item.LogFir, Item.SmallStone, null), Item.Spear },
        { new CraftingRecipe(Item.LogFir, Item.Stone, null), Item.Axe },
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