using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Item : MonoBehaviour
{




    // Item defs
    public static Item Stone = new Item("Stone", (int)Type.Misc, .1f, (int)HoldStyle.UnderArm, EmptyItemCollection, EmptyItemCollection, Resources.Load<GameObject>("Items/Stone/Gameobject"), Resources.Load<Image>("Items/Stone/Image"));
    public static Item Spear = new Item("Spear", (int)Type.Weapon, .1f, (int)HoldStyle.Spear, EmptyItemCollection, EmptyItemCollection, Resources.Load<GameObject>("Items/Spear/Gameobject"), Resources.Load<Image>("Items/Spear/Image"));


    public static Dictionary<string, Item> Items = new Dictionary<string, Item>
    {
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
        Hug, UnderArm, Spear, OverShoulder
    }

    public Item(string _nme, int _type, float _weight, int _holdStyle, ItemCollection _contents, ItemCollection _components, GameObject _gameobject, Image _image){
        nme = _nme;
        type = _type;
        weight = _weight;
        holdStyle = _holdStyle;
        contents = _contents;
        components = _components;
        gameobject = _gameobject;
        image = _image;
    }






    void Awake(){

    }




    public static ItemCollection EmptyItemCollection = new ItemCollection(new Dictionary<Item, int>());

}

