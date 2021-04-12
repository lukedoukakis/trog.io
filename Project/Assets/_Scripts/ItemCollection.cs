using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemCollection
{
    


    public Dictionary<string, int> items; // <name, number>
    public ItemCollection(Dictionary<string, int> _items){
        items = _items;
    }


    public void AddItem(Item i){
        string nme = i.nme;
        if(items.ContainsKey(nme)){
            items[nme]++;
        }
        else{
            items.Add(nme, 1);
        }
    }


    // returns in format... iron:1_wood:4_leather:4
    public override string ToString(){
        string s = "";
        foreach(KeyValuePair<string, int> kvp in items){
            s += kvp.Key + ":" + kvp.Value + "_" ;
        }
        return s;
    }
}
