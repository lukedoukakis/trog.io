using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public enum ItemType
{
    Any, Food, Weapon, Clothing, Wood, Bone, Stone, Misc
}

public class Item : ScriptableObject
{


    public enum ItemHoldStyle{
        Hug, UnderArm, Spear, Axe, OverShoulder, Torch
    }

    public enum ItemDamageType{
        Blunt, Pierce, Slash
    };


    static int idCounter = 0;
    public static int SetItemID(Item item){
        int id = idCounter;
        ++idCounter;
        return id;
    }


    // Item defs
    public static Item None = InitiailizeItem("None", ItemType.Weapon, false, Stats.NONE, Stats.NONE, ItemHoldStyle.Torch, ItemDamageType.Blunt, ItemCollection.EmptyItemCollection, null, Resources.Load<GameObject>("Items/Torch/Gameobject/Torch"), Resources.Load<Image>("Items/Torch/Image"));
    public static Item Meat = InitiailizeItem("Meat", ItemType.Food, true, Stats.NONE, Stats.ITEM_WIELDERMODIFIER_MEAT, ItemHoldStyle.Torch, ItemDamageType.Blunt, ItemCollection.EmptyItemCollection, null, Resources.Load<GameObject>("Items/Food/Meat/Gameobject/Meat"), Resources.Load<Image>("Items/Food/Meat/Image"));
    public static Item WoodPiece = InitiailizeItem("WoodPiece", ItemType.Wood, true, Stats.NONE, Stats.NONE, ItemHoldStyle.UnderArm, ItemDamageType.Blunt,ItemCollection. EmptyItemCollection, null, Resources.Load<GameObject>("Items/WoodPiece/Gameobject/WoodPiece"), Resources.Load<Image>("Items/WoodPiece/Image"));
    public static Item BonePiece1 = InitiailizeItem("BonePiece1", ItemType.Bone, true, Stats.NONE, Stats.NONE, ItemHoldStyle.UnderArm, ItemDamageType.Blunt, ItemCollection.EmptyItemCollection, null, Resources.Load<GameObject>("Items/Bones/BonePiece1/Gameobject/BonePiece1"), Resources.Load<Image>("Items/Bones/BonePiece1/Image"));
    public static Item BonePiece2 = InitiailizeItem("BonePiece2", ItemType.Bone, true, Stats.NONE, Stats.NONE, ItemHoldStyle.UnderArm, ItemDamageType.Blunt, ItemCollection.EmptyItemCollection, null, Resources.Load<GameObject>("Items/Bones/BonePiece2/Gameobject/BonePiece2"), Resources.Load<Image>("Items/Bones/BonePiece2/Image"));
    public static Item BonePiece3 = InitiailizeItem("BonePiece3", ItemType.Bone, true, Stats.NONE, Stats.NONE, ItemHoldStyle.UnderArm, ItemDamageType.Blunt, ItemCollection.EmptyItemCollection, null, Resources.Load<GameObject>("Items/Bones/BonePiece3/Gameobject/BonePiece3"), Resources.Load<Image>("Items/Bones/BonePiece3/Image"));
    public static Item BonePiece4 = InitiailizeItem("BonePiece4", ItemType.Bone, true, Stats.NONE, Stats.NONE, ItemHoldStyle.UnderArm, ItemDamageType.Blunt, ItemCollection.EmptyItemCollection, null, Resources.Load<GameObject>("Items/Bones/BonePiece4/Gameobject/BonePiece4"), Resources.Load<Image>("Items/Bones/BonePiece4/Image"));
    public static Item StoneSmall = InitiailizeItem("StoneSmall", ItemType.Stone, true, Stats.NONE, Stats.NONE, ItemHoldStyle.Torch, ItemDamageType.Blunt, ItemCollection.EmptyItemCollection, null, Resources.Load<GameObject>("Items/StoneSmall/Gameobject/StoneSmall"), Resources.Load<Image>("Items/StoneSmall/Image"));
    public static Item Torch = InitiailizeItem("Torch", ItemType.Weapon, false, Stats.NONE, Stats.NONE, ItemHoldStyle.Torch, ItemDamageType.Blunt, ItemCollection.EmptyItemCollection, null, Resources.Load<GameObject>("Items/Torch/Gameobject/Torch"), Resources.Load<Image>("Items/Torch/Image"));
    public static Item Stone = InitiailizeItem("Stone", ItemType.Misc, true, Stats.InstantiateStats(3f,0f,0f,0f,0f,0f,0f,0f,float.MaxValue,0f,float.MaxValue,0f), Stats.NONE, ItemHoldStyle.UnderArm, ItemDamageType.Blunt, new ItemCollection(new Dictionary<Item, int>(){{StoneSmall, 5}}), null, Resources.Load<GameObject>("Items/Stone/Gameobject/Stone"), Resources.Load<Image>("Items/Stone/Image"));
    public static Item Spear = InitiailizeItem("Spear", ItemType.Weapon, true, Stats.NONE, Stats.ITEM_WIELDERMODIFIER_SPEAR, ItemHoldStyle.Spear, ItemDamageType.Pierce, ItemCollection.EmptyItemCollection, null, Resources.Load<GameObject>("Items/Weapons/Spear/Gameobject/Spear"), Resources.Load<Image>("Items/Weapons/Spear/Image"));
    public static Item Axe = InitiailizeItem("Axe", ItemType.Weapon, true, Stats.NONE, Stats.ITEM_WIELDERMODIFIER_AXE, ItemHoldStyle.Axe, ItemDamageType.Slash, ItemCollection.EmptyItemCollection, null, Resources.Load<GameObject>("Items/Weapons/Axe/Gameobject/Axe"), Resources.Load<Image>("Items/Weapons/Axe/Image"));
    public static Item CarcassBear = InitiailizeItem("CarcassBear", ItemType.Misc, false, Stats.InstantiateStats(.1f,0f,0f,0f,0f,0f,0f,0f,float.MaxValue,0f,float.MaxValue,0f), Stats.NONE, ItemHoldStyle.UnderArm, ItemDamageType.Blunt, new ItemCollection(new Dictionary<Item, int>(){{BonePiece1, 2}, {BonePiece2, 1}, {BonePiece3, 1}, {Meat, 2}}), ParticleController.instance.BloodSpatter, Resources.Load<GameObject>("Items/Carcasses/CarcassBear/Gameobject/CarcassBear"), Resources.Load<Image>("Items/Carcasses/CarcassBear/Image"));
    public static Item ClothingTest = InitiailizeItem("ClothingTest", ItemType.Clothing, true, Stats.NONE, Stats.ITEM_WIELDERMODIFIER_TESTCLOTHING, ItemHoldStyle.UnderArm, ItemDamageType.Blunt, ItemCollection.EmptyItemCollection, null, Resources.Load<GameObject>("Items/Clothing/ClothingTest/Gameobject/ClothingTest"), Resources.Load<Image>("Items/Clothing/ClothingTest/Image"));
    public static Item LogFir = InitiailizeItem("LogFir", ItemType.Misc, false, Stats.InstantiateStats(.1f,0f,0f,0f,0f,0f,0f,0f,float.MaxValue,0f,float.MaxValue,0f), Stats.NONE, ItemHoldStyle.Hug, ItemDamageType.Blunt, new ItemCollection(new Dictionary<Item, int>(){{WoodPiece, 4},}), ParticleController.instance.TreeDebris, Resources.Load<GameObject>("Items/LogFir/Gameobject/LogFir"), Resources.Load<Image>("Items/LogFir/Image"));
    
    public static Dictionary<string, Item> Items = new Dictionary<string, Item>
    {

        { "WoodPiece", WoodPiece },
        { "BonePiece1", BonePiece1 },
        { "BonePiece2", BonePiece2 },
        { "BonePiece3", BonePiece3 },
        { "BonePiece4", BonePiece4 },
        { "Torch", Torch },
        { "Stone", Stone },
        { "StoneSmall", StoneSmall },
        { "Spear", Spear },
        { "Axe", Axe },
        { "LogFir", LogFir },
        { "CarcassBear", CarcassBear },
        { "ClothingTest", ClothingTest },
        { "Meat", Meat },
        
        

    };
    public static Item GetItemByName(string _nme){

        //Debug.Log("searching for key: " + _nme);

        return Items[_nme];
       
    }




    public int id;
    public string nme;
    public Enum type;
    public bool isRackable;
    public Stats baseStats;
    public Stats wielderStatsModifier;
    public Enum holdStyle;
    public Enum damageType;
    public ItemCollection drops;
    public GameObject hitParticlesPrefab;
    public GameObject worldObjectPrefab;
    public Image image;


    public static Item InitiailizeItem(string _nme, Enum _type, bool _isRackable, Stats _stats, Stats _wielderStatsModifier, Enum _holdStyle, Enum _damageType, ItemCollection _drops, GameObject _hitParticlesPrefab, GameObject _worldObjectPrefab, Image _image){
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
        item.hitParticlesPrefab = _hitParticlesPrefab;
        item.worldObjectPrefab = _worldObjectPrefab;
        item.image = _image;


        //Debug.Log(Drops.ITEM_LOGFIR.ToString());
        

        return item;
    }

}
