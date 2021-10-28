﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemCollection
{
    
    public Dictionary<Item, int> items;


    public ItemCollection(){
        this.items = new Dictionary<Item, int>();
    }
    public ItemCollection(Dictionary<Item, int> _items){
        this.items = _items;
    }
    public ItemCollection(ItemCollection ic){
        this.items = new Dictionary<Item, int>();
        foreach(KeyValuePair<Item, int> kvp in ic.items){
            this.AddItem(kvp.Key, kvp.Value);
        }
    }


    public void AddItem(Item i, int count){
        if(items.ContainsKey(i)){
            items[i] += count;
            // ++items[i]
        }
        else{
            items.Add(i, count);
        }
    }

    public void RemoveItem(Item i, int count){
        if(items.ContainsKey(i)){
            items[i] -= count;
            items[i] = Mathf.Max(items[i], 0);
        }
        else
        {
            Debug.Log("Removing item that isn't present in this item collection");
        }
    }

    public int GetItemCount(Item i){
        if(items.ContainsKey(i)){
            return items[i];
        }
        else{
            return 0;
        }
    }


    // returns in format... iron:1_wood:4_leather:4
    public override string ToString(){
        string s = "";
        foreach(KeyValuePair<Item, int> kvp in items){
            s += kvp.Key.nme + ":" + kvp.Value + "_" ;
        }
        return s;
    }


    public static ItemCollection EmptyItemCollection = new ItemCollection(new Dictionary<Item, int>());
}
