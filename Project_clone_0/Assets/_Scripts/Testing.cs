using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Testing : MonoBehaviour
{


    EntityHandle playerHandle;
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
        Faction.AddItemOwned(faction, Item.ClothingTest, 3, null);
        Faction.AddItemOwned(faction, Item.FoodTest, 1, null);
        Faction.AddItemOwned(faction, Item.Spear, 1, null);
        Faction.AddItemOwned(faction, Item.Axe, 1, null);
    }


    void Update(){


        playerHandle = GameManager.current.localPlayer.GetComponent<EntityHandle>();


        if(Input.GetKeyUp(KeyCode.C)){
            Camp.TryPlaceCamp(playerHandle.entityInfo.faction, GameManager.current.localPlayer.transform.position);
        }

        // if(Input.GetKeyUp(KeyCode.V)){
        //     GameObject o = new GameObject();
        //     playerHandle.entityItems.EquipClothing("ClothingTest", o);
        // }

        // if(Input.GetKeyUp(KeyCode.B)){
        //     playerHandle.entityItems.UnequipCurrentClothing();
        // }
    }
}
