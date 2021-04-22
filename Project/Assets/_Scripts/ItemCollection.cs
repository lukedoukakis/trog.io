using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemCollection
{
    


    public Dictionary<Item, int> items;

    public ItemCollection(Dictionary<Item, int> _items){
        items = _items;
    }


    public void AddItem(Item i){
        if(items.ContainsKey(i)){
            items[i]++;
        }
        else{
            items.Add(i, 1);
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
}
