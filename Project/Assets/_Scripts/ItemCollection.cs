using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemCollection
{
    
    public Dictionary<Item, int> items;



    public ItemCollection(Dictionary<Item, int>[] _items){
        this.items = _items[0];
        if(_items.Length > 1){
            for(int i = 1; i < _items.Length; ++i){
                foreach(KeyValuePair<Item, int> kvp in _items[i]){
                    AddItem(kvp.Key, kvp.Value);
                }
            }
        }
    }
    public ItemCollection(Dictionary<Item, int> _items){
        this.items = _items;
    }
    public ItemCollection(){
        this.items = new Dictionary<Item, int>();
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
        }
        else{
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
}
