using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Item : MonoBehaviour
{




    public static Dictionary<string, Item> Items = new Dictionary<string, Item>
    {
        {
            "Stone",
            new Item("Stone", (int)Type.Misc, .1f, (int)HoldStyle.Hug, true, EmptyItemCollection, EmptyItemCollection, Resources.Load<GameObject>("Items/Stone/Gameobject"), Resources.Load<Image>("Items/Stone/Image"))
        },
        {
            "Spear",
            new Item("Spear", (int)Type.Misc, .1f, (int)HoldStyle.Hug, false, EmptyItemCollection, EmptyItemCollection, Resources.Load<GameObject>("Items/Spear/Gameobject"), Resources.Load<Image>("Items/Spear/Image"))
        },
    };

    public string nme;
    public int type;
    public float weight;
    public int holdStyle;
    public bool pocketable;
    public ItemCollection contents;
    public ItemCollection components;
    public GameObject gameobject;

    enum Type{
        Misc, Weapon, Tool, Container
    }

    enum HoldStyle{
        Hug, UnderArm, Spear, Axe, OverShoulder, Backpack
    }

    public Item(string _nme, int _type, float _weight, int _holdStyle, bool _pocketable, ItemCollection _contents, ItemCollection _components, GameObject _gameobject, Image image){
        nme = _nme;
        type = _type;
        weight = _weight;
        holdStyle = _holdStyle;
        components = _components;
        gameobject = _gameobject;
    }






    void Awake(){

    }




    public static ItemCollection EmptyItemCollection = new ItemCollection(new Dictionary<string, int>());

}

