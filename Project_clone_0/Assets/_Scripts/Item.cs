using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Item : ScriptableObject
{




    // Item defs
    public static Item Torch = GenerateItem("Torch", (int)Type.Misc, .1f, (int)HoldStyle.Torch, EmptyItemCollection, EmptyItemCollection, Resources.Load<GameObject>("Items/Torch/Gameobject"), Resources.Load<Image>("Items/Torch/Image"));
    public static Item Stone = GenerateItem("Stone", (int)Type.Misc, .1f, (int)HoldStyle.UnderArm, EmptyItemCollection, EmptyItemCollection, Resources.Load<GameObject>("Items/Stone/Gameobject"), Resources.Load<Image>("Items/Stone/Image"));
    public static Item Spear = GenerateItem("Spear", (int)Type.Weapon, .1f, (int)HoldStyle.Spear, EmptyItemCollection, EmptyItemCollection, Resources.Load<GameObject>("Items/Spear/Gameobject"), Resources.Load<Image>("Items/Spear/Image"));


    public static Dictionary<string, Item> Items = new Dictionary<string, Item>
    {
        { "Torch", Torch },
        { "Stone", Stone },
        { "Spear", Spear },
    };
    public static Item GetItemByName(string _nme){
        return Items[_nme];
       
    }




    public string nme;
    public int type;
    public float weight;
    public int holdStyle;
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


    public static Item GenerateItem(string _nme, int _type, float _weight, int _holdStyle, ItemCollection _contents, ItemCollection _components, GameObject _gameobject, Image _image){
        Item i = ScriptableObject.CreateInstance<Item>();
        i.nme = _nme;
        i.type = _type;
        i.weight = _weight;
        i.holdStyle = _holdStyle;
        i.contents = _contents;
        i.components = _components;
        i.gameobject = _gameobject;
        i.image = _image;
        return i;
    }






    void Awake(){

    }




    public static ItemCollection EmptyItemCollection = new ItemCollection(new Dictionary<Item, int>());

}

