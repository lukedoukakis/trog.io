using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Testing : MonoBehaviour
{
    
    public Faction faction;

    public static Testing instance;

    void Awake(){
        instance = this;
    }

    public void OnFactionCreation(){
        AddItemsToFaction();
    }

    void AddItemsToFaction(){
        faction = GameManager.current.localPlayer.GetComponent<EntityInfo>().faction;
        faction.ownedItems.AddItem(Item.TestClothing, 42);
    }






    void Update(){
        if(Input.GetKeyUp(KeyCode.C)){
            Camp.TryPlaceCamp(GameManager.current.localPlayer.GetComponent<EntityInfo>().faction, GameManager.current.localPlayer.transform.position);
        }
    }
}
