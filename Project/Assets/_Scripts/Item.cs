using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Item : ScriptableObject
{




    // Item defs
    public static Item Torch = InitiailizeItem("Torch", Type.Misc, HoldStyle.Torch, DamageType.Blunt, EmptyItemCollection, EmptyItemCollection, Resources.Load<GameObject>("Items/Torch/Gameobject/Torch"), Resources.Load<Image>("Items/Torch/Image"));
    public static Item Stone = InitiailizeItem("Stone", Type.Misc, HoldStyle.UnderArm, DamageType.Blunt, EmptyItemCollection, EmptyItemCollection, Resources.Load<GameObject>("Items/Stone/Gameobject/Stone"), Resources.Load<Image>("Items/Stone/Image"));
    public static Item SmallStone = InitiailizeItem("SmallStone", Type.Misc, HoldStyle.Torch, DamageType.Blunt, EmptyItemCollection, EmptyItemCollection, Resources.Load<GameObject>("Items/SmallStone/Gameobject/SmallStone"), Resources.Load<Image>("Items/SmallStone/Image"));
    public static Item Spear = InitiailizeItem("Spear", Type.Weapon, HoldStyle.Spear, DamageType.Blunt, EmptyItemCollection, EmptyItemCollection, Resources.Load<GameObject>("Items/Spear/Gameobject/Spear"), Resources.Load<Image>("Items/Spear/Image"));


    public static Dictionary<string, Item> Items = new Dictionary<string, Item>
    {
        { "Torch", Torch },
        { "Stone", Stone },
        { "SmallStone", SmallStone },
        { "Spear", Spear },
    };
    public static Item GetItemByName(string _nme){
        return Items[_nme];
       
    }




    public string nme;
    public Enum type;
    public Enum holdStyle;
    public Enum damageType;
    public ItemCollection contents;
    public ItemCollection components;
    public GameObject gameobject;
    public Image image;

    public enum Type{
        Misc, Weapon, Pocket, Container
    }

    public enum HoldStyle{
        Hug, UnderArm, Spear, Axe, OverShoulder, Torch
    }

    public enum DamageType{
        Blunt, Pierce, Slash
    };


    public static Item InitiailizeItem(string _nme, Enum _type, Enum _holdStyle, Enum _damageType, ItemCollection _contents, ItemCollection _components, GameObject _gameobject, Image _image){
        Item i = ScriptableObject.CreateInstance<Item>();
        i.nme = _nme;
        i.type = _type;
        i.holdStyle = _holdStyle;
        i.contents = _contents;
        i.components = _components;
        i.gameobject = _gameobject;
        i.image = _image;
        return i;
    }


    public static ItemCollection EmptyItemCollection = new ItemCollection(new Dictionary<Item, int>());

}