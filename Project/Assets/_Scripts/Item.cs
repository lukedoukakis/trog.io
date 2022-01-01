using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public enum ItemType
{
    Any, Food, Weapon, Pelt, Wood, Bone, Stone, Feature, CampComponent, Misc
}

public enum ItemDamageType{
    None, Blunt, Pierce, Slash
};

public class Item : ScriptableObject
{


    public enum ItemHoldStyle{
        Hug, UnderArm, Spear, Axe, OverShoulder, Torch
    }


    static int idCounter = 0;
    public static int SetItemID(Item item){
        int id = idCounter;
        ++idCounter;
        return id;
    }


    // Item defs
    public static Item None = InitiailizeItem("None", ItemType.Any, false, Stats.NONE, Stats.NONE, ItemHoldStyle.Torch, ItemDamageType.Blunt, ItemCollection.EmptyItemCollection, null, null, Resources.Load<GameObject>("Items/Torch/Gameobject/Torch"), Resources.Load<Image>("Items/Torch/Image"));
    public static Item Meat = InitiailizeItem("Meat", ItemType.Food, true, Stats.NONE, Stats.ITEM_WIELDERMODIFIER_MEAT, ItemHoldStyle.Torch, ItemDamageType.Blunt, ItemCollection.EmptyItemCollection, null, null, Resources.Load<GameObject>("Items/Food/Meat/Gameobject/Meat"), Resources.Load<Image>("Items/Food/Meat/Image"));
    public static Item WoodPiece = InitiailizeItem("WoodPiece", ItemType.Wood, true, Stats.NONE, Stats.NONE, ItemHoldStyle.UnderArm, ItemDamageType.Blunt,ItemCollection. EmptyItemCollection, null, null, Resources.Load<GameObject>("Items/WoodPiece/Gameobject/WoodPiece"), Resources.Load<Image>("Items/WoodPiece/Image"));
    public static Item BonePiece = InitiailizeItem("BonePiece", ItemType.Bone, true, Stats.NONE, Stats.NONE, ItemHoldStyle.UnderArm, ItemDamageType.Blunt, ItemCollection.EmptyItemCollection, null, null, Resources.Load<GameObject>("Items/Bones/BonePiece/Gameobject/BonePiece"), Resources.Load<Image>("Items/Bones/BonePiece/Image"));
    public static Item StoneSmall = InitiailizeItem("StoneSmall", ItemType.Stone, true, Stats.NONE, Stats.NONE, ItemHoldStyle.Torch, ItemDamageType.Blunt, ItemCollection.EmptyItemCollection, null, null, Resources.Load<GameObject>("Items/StoneSmall/Gameobject/StoneSmall"), Resources.Load<Image>("Items/StoneSmall/Image"));
    public static Item Torch = InitiailizeItem("Torch", ItemType.Misc, false, Stats.NONE, Stats.NONE, ItemHoldStyle.Torch, ItemDamageType.Blunt, ItemCollection.EmptyItemCollection, new CraftingRecipe(Item.WoodPiece, null, null), null, Resources.Load<GameObject>("Items/Torch/Gameobject/Torch"), Resources.Load<Image>("Items/Torch/Image"));
    public static Item SpearStone = InitiailizeItem("SpearStone", ItemType.Weapon, true, Stats.NONE, Stats.ITEM_WIELDERMODIFIER_SPEARSTONE, ItemHoldStyle.Spear, ItemDamageType.Pierce, ItemCollection.EmptyItemCollection, new CraftingRecipe(Item.WoodPiece, Item.WoodPiece, Item.StoneSmall), null, Resources.Load<GameObject>("Items/Weapons/SpearStone/Gameobject/SpearStone"), Resources.Load<Image>("Items/Weapons/SpearStone/Image"));
    public static Item AxeStone = InitiailizeItem("AxeStone", ItemType.Weapon, true, Stats.NONE, Stats.ITEM_WIELDERMODIFIER_AXESTONE, ItemHoldStyle.Axe, ItemDamageType.Slash, ItemCollection.EmptyItemCollection, new CraftingRecipe(Item.WoodPiece, Item.StoneSmall, Item.StoneSmall), null, Resources.Load<GameObject>("Items/Weapons/AxeStone/Gameobject/AxeStone"), Resources.Load<Image>("Items/Weapons/AxeStone/Image"));
    public static Item SpearBone = InitiailizeItem("SpearBone", ItemType.Weapon, true, Stats.NONE, Stats.ITEM_WIELDERMODIFIER_SPEARBONE, ItemHoldStyle.Spear, ItemDamageType.Pierce, ItemCollection.EmptyItemCollection, new CraftingRecipe(Item.WoodPiece, Item.WoodPiece, Item.StoneSmall), null, Resources.Load<GameObject>("Items/Weapons/SpearBone/Gameobject/SpearBone"), Resources.Load<Image>("Items/Weapons/SpearBone/Image"));
    public static Item AxeBone = InitiailizeItem("AxeBone", ItemType.Weapon, true, Stats.NONE, Stats.ITEM_WIELDERMODIFIER_AXEBONE, ItemHoldStyle.Axe, ItemDamageType.Slash, ItemCollection.EmptyItemCollection, new CraftingRecipe(Item.WoodPiece, Item.StoneSmall, Item.StoneSmall), null, Resources.Load<GameObject>("Items/Weapons/AxeBone/Gameobject/AxeBone"), Resources.Load<Image>("Items/Weapons/AxeBone/Image"));
    public static Item PeltBear = InitiailizeItem("PeltBear", ItemType.Pelt, true, Stats.NONE, Stats.ITEM_WIELDERMODIFIER_PELTBEAR, ItemHoldStyle.Torch, ItemDamageType.Blunt, ItemCollection.EmptyItemCollection, null, null, Resources.Load<GameObject>("Items/Pelt/PeltBear/Gameobject/PeltBear"), Resources.Load<Image>("Items/Pelt/PeltBear/Image"));
    public static Item PeltDeer = InitiailizeItem("PeltDeer", ItemType.Pelt, true, Stats.NONE, Stats.ITEM_WIELDERMODIFIER_PELTDEER, ItemHoldStyle.Torch, ItemDamageType.Blunt, ItemCollection.EmptyItemCollection, null, null, Resources.Load<GameObject>("Items/Pelt/PeltDeer/Gameobject/PeltDeer"), Resources.Load<Image>("Items/Pelt/PeltDeer/Image"));
    public static Item CarcassBear = InitiailizeItem("CarcassBear", ItemType.Misc, false, Stats.InstantiateStats(.1f,0f,0f,0f,0f,0f,0f,0f,float.MaxValue,0f,float.MaxValue,0f), Stats.NONE, ItemHoldStyle.UnderArm, ItemDamageType.Blunt, new ItemCollection(new Dictionary<Item, int>(){{PeltBear, 1}, {BonePiece, 4}, {Meat, 2}}), null, ParticleController.instance.BloodSpatter, Resources.Load<GameObject>("Items/Carcasses/CarcassBear/Gameobject/CarcassBear"), Resources.Load<Image>("Items/Carcasses/CarcassBear/Image"));
    public static Item LogFir = InitiailizeItem("LogFir", ItemType.Misc, false, Stats.InstantiateStats(.1f,0f,0f,0f,0f,0f,0f,0f,float.MaxValue,0f,float.MaxValue,0f), Stats.NONE, ItemHoldStyle.Hug, ItemDamageType.Blunt, new ItemCollection(new Dictionary<Item, int>(){{WoodPiece, 4},}), null, ParticleController.instance.TreeDebris, Resources.Load<GameObject>("Items/LogFir/Gameobject/LogFir"), Resources.Load<Image>("Items/LogFir/Image"));
    public static Item Stone = InitiailizeItem("Stone", ItemType.Feature, true, Stats.InstantiateStats(3f,0f,0f,0f,0f,0f,0f,0f,float.MaxValue,0f,float.MaxValue,0f), Stats.NONE, ItemHoldStyle.UnderArm, ItemDamageType.Blunt, new ItemCollection(new Dictionary<Item, int>(){{StoneSmall, 5}}), null, ParticleController.instance.StoneDebris, null, null);
    public static Item Tree = InitiailizeItem("Tree", ItemType.Feature, true, Stats.InstantiateStats(3f,0f,0f,0f,0f,0f,0f,0f,float.MaxValue,0f,float.MaxValue,0f), Stats.NONE, ItemHoldStyle.UnderArm, ItemDamageType.Blunt, new ItemCollection(new Dictionary<Item, int>(){{WoodPiece, 4},}), null, ParticleController.instance.TreeDebris, null, null);
    public static Item TentBearPelt = InitiailizeItem("TentBearPelt", ItemType.CampComponent, true, Stats.NONE, Stats.NONE, ItemHoldStyle.UnderArm, ItemDamageType.Blunt, new ItemCollection(new Dictionary<Item, int>(){{WoodPiece, 2},{PeltBear, 1},}), new CraftingRecipe(Item.WoodPiece, Item.WoodPiece, Item.PeltBear), null, null, null);
    public static Item TentDeerPelt = InitiailizeItem("TentDeerPelt", ItemType.CampComponent, true, Stats.NONE, Stats.NONE, ItemHoldStyle.UnderArm, ItemDamageType.Blunt, new ItemCollection(new Dictionary<Item, int>(){{WoodPiece, 2},{PeltDeer, 1},}), new CraftingRecipe(Item.WoodPiece, Item.WoodPiece, Item.PeltDeer), null, null, null);



    public static Dictionary<string, Item> Items = new Dictionary<string, Item>
    {

        { "WoodPiece", WoodPiece },
        { "BonePiece", BonePiece },
        { "Torch", Torch },
        { "StoneSmall", StoneSmall },
        { "SpearStone", SpearStone },
        { "AxeStone", AxeStone },
        { "SpearBone", SpearBone },
        { "AxeBone", AxeBone },
        { "LogFir", LogFir },
        { "CarcassBear", CarcassBear },
        { "PeltBear", PeltBear },
        { "PeltDeer", PeltDeer },
        { "Meat", Meat },
        { "Stone", Stone },
        { "Tree", Tree },
        { "TentBearPelt", TentBearPelt },
        { "TentDeerPelt", TentDeerPelt },
        
        

    };
    public static Item GetItemByName(string _nme){

        //Debug.Log("searching for key: " + _nme);

        return Items[_nme];
       
    }

    public static Item GetRandomItem(ItemType type)
    {
        Item[] itemsOfType = Items.Values.Where(i => i.type.Equals(type)).ToArray();
        Item randomItem = itemsOfType[UnityEngine.Random.Range(0, itemsOfType.Length)];
        //Debug.Log(randomItem.nme);
        return randomItem;
    }




    public int id;
    public string nme;
    public Enum type;
    public bool isRackable;
    public Stats baseStats;
    public Stats wielderStatsModifier;
    public Enum holdStyle;
    public ItemDamageType damageType;
    public ItemCollection drops;
    public CraftingRecipe craftingRecipe;
    public GameObject hitParticlesPrefab;
    public GameObject worldObjectPrefab;
    public Image image;


    public static Item InitiailizeItem(string _nme, Enum _type, bool _isRackable, Stats _stats, Stats _wielderStatsModifier, Enum _holdStyle, ItemDamageType _damageType, ItemCollection _drops, CraftingRecipe _craftingRecipe, GameObject _hitParticlesPrefab, GameObject _worldObjectPrefab, Image _image){
        Item item = ScriptableObject.CreateInstance<Item>();
        item.id = SetItemID(item);
        item.nme = _nme;
        item.type = _type;
        item.isRackable = _isRackable;
        item.baseStats = _stats;
        item.wielderStatsModifier = _wielderStatsModifier;
        item.holdStyle = _holdStyle;
        item.damageType = _damageType;
        item.drops = _drops;
        item.craftingRecipe = _craftingRecipe;
        item.hitParticlesPrefab = _hitParticlesPrefab;
        item.worldObjectPrefab = _worldObjectPrefab;
        item.image = _image;


        //Debug.Log(Drops.ITEM_LOGFIR.ToString());
        

        return item;
    }


}
