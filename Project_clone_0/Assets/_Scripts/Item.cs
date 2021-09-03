﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Item : ScriptableObject
{




    // Item defs
    public static Item Torch = InitiailizeItem("Torch", Type.Weapon, StatsHandler.NONE, HoldStyle.Torch, DamageType.Blunt, EmptyItemCollection, EmptyItemCollection, Resources.Load<GameObject>("Items/Torch/Gameobject/Torch"), Resources.Load<Image>("Items/Torch/Image"));
    public static Item Stone = InitiailizeItem("Stone", Type.MiscLarge, StatsHandler.NONE, HoldStyle.UnderArm, DamageType.Blunt, EmptyItemCollection, EmptyItemCollection, Resources.Load<GameObject>("Items/Stone/Gameobject/Stone"), Resources.Load<Image>("Items/Stone/Image"));
    public static Item SmallStone = InitiailizeItem("SmallStone", Type.MiscSmall, StatsHandler.NONE, HoldStyle.Torch, DamageType.Blunt, EmptyItemCollection, EmptyItemCollection, Resources.Load<GameObject>("Items/SmallStone/Gameobject/SmallStone"), Resources.Load<Image>("Items/SmallStone/Image"));
    public static Item Spear = InitiailizeItem("Spear", Type.Weapon, StatsHandler.WEAPON_SPEAR, HoldStyle.Spear, DamageType.Pierce, EmptyItemCollection, EmptyItemCollection, Resources.Load<GameObject>("Items/Spear/Gameobject/Spear"), Resources.Load<Image>("Items/Spear/Image"));
    public static Item Axe = InitiailizeItem("Axe", Type.Weapon, StatsHandler.WEAPON_AXE, HoldStyle.Axe, DamageType.Slash, EmptyItemCollection, EmptyItemCollection, Resources.Load<GameObject>("Items/Axe/Gameobject/Axe"), Resources.Load<Image>("Items/Axe/Image"));
    

    // testing purposes
    public static Item ClothingTest = InitiailizeItem("ClothingTest", Type.Clothing, StatsHandler.CLOTHING_TESTCLOTHING, HoldStyle.UnderArm, DamageType.Blunt, EmptyItemCollection, EmptyItemCollection, Resources.Load<GameObject>("Items/ClothingTest/Gameobject/ClothingTest"), Resources.Load<Image>("Items/ClothingTest/Image"));

    public static Item FoodTest = InitiailizeItem("FoodTest", Type.Food, StatsHandler.FOOD_TESTFOOD, HoldStyle.Torch, DamageType.Blunt, EmptyItemCollection, EmptyItemCollection, Resources.Load<GameObject>("Items/FoodTest/Gameobject/FoodTest"), Resources.Load<Image>("Items/FoodTest/Image"));

    // Not implemented in files yet
    public static Item Wood = InitiailizeItem("Wood", Type.MiscLarge, StatsHandler.NONE, HoldStyle.UnderArm, DamageType.Blunt, EmptyItemCollection, EmptyItemCollection, Resources.Load<GameObject>("Items/Wood/Gameobject/Wood"), Resources.Load<Image>("Items/Wood/Image"));





    public static Dictionary<string, Item> Items = new Dictionary<string, Item>
    {
        { "Torch", Torch },
        { "Stone", Stone },
        { "SmallStone", SmallStone },
        { "Spear", Spear },
        { "Axe", Axe },

    
        { "Wood", Wood },
        { "ClothingTest", ClothingTest },
        { "FoodTest", FoodTest },
    };
    public static Item GetItemByName(string _nme){

        //Debug.Log("searching for key: " + _nme);

        return Items[_nme];
       
    }




    public string nme;
    public Enum type;
    public Stats stats;
    public Enum holdStyle;
    public Enum damageType;
    public ItemCollection contents;
    public ItemCollection components;
    public GameObject gameobject;
    public Image image;

    public enum Type{
        Food, Weapon, Clothing, MiscLarge, MiscSmall
    }

    public enum HoldStyle{
        Hug, UnderArm, Spear, Axe, OverShoulder, Torch
    }

    public enum DamageType{
        Blunt, Pierce, Slash
    };


    public static Item InitiailizeItem(string _nme, Enum _type, Stats _stats, Enum _holdStyle, Enum _damageType, ItemCollection _contents, ItemCollection _components, GameObject _gameobject, Image _image){
        Item i = ScriptableObject.CreateInstance<Item>();
        i.nme = _nme;
        i.type = _type;
        i.stats = _stats;
        i.holdStyle = _holdStyle;
        i.damageType = _damageType;
        i.contents = _contents;
        i.components = _components;
        i.gameobject = _gameobject;
        i.image = _image;
        return i;
    }


    public static ItemCollection EmptyItemCollection = new ItemCollection(new Dictionary<Item, int>());

}