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
        faction.ownedItems.AddItem(Item.ClothingTest, 42);
    }


    void Update(){


        playerHandle = GameManager.current.localPlayer.GetComponent<EntityHandle>();


        if(Input.GetKeyUp(KeyCode.C)){
            Camp.TryPlaceCamp(playerHandle.entityInfo.faction, GameManager.current.localPlayer.transform.position);
        }

        if(Input.GetKeyUp(KeyCode.V)){
            playerHandle.entityItems.EquipClothing("ClothingTest");
        }

        if(Input.GetKeyUp(KeyCode.B)){
            playerHandle.entityItems.UnequipClothing();
        }
    }
}
