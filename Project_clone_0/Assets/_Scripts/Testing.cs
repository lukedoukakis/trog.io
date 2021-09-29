using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Testing : MonoBehaviour
{


    public bool godMode;


    public EntityHandle playerHandle;
    public Faction faction;


    public Material transparentMat;

    public static Testing instance;

    void Awake(){
        instance = this;
    }


    public void OnFactionCreation(){
        AddItemsToFaction();
    }

    void AddItemsToFaction(){
        faction = GameManager.current.localPlayer.GetComponent<EntityInfo>().faction;
        Faction.AddItemOwned(faction, Item.ClothingTest, 2, null);
        Faction.AddItemOwned(faction, Item.FoodTest, 2, null);
        Faction.AddItemOwned(faction, Item.Spear, 2, null);
        Faction.AddItemOwned(faction, Item.Axe, 2, null);
        Faction.AddItemOwned(faction, Item.WoodPiece, 12, null);
        Faction.AddItemOwned(faction, Item.BonePiece2, 13, null);
        // for(int i = 0; i < 120 / 4; ++i){
        //     Faction.AddItemOwned(faction, Item.BonePiece1, 1, null);
        //     Faction.AddItemOwned(faction, Item.BonePiece2, 1, null);
        //     Faction.AddItemOwned(faction, Item.BonePiece3, 1, null);
        //     Faction.AddItemOwned(faction, Item.BonePiece4, 1, null);
        // }
    }


    void Update()
    {


        if (playerHandle != null)
        {
            if (Input.GetKeyUp(KeyCode.C))
            {
                Camp.TryPlaceCamp(playerHandle.entityInfo.faction, GameManager.current.localPlayer.transform.position);
            }
            if (Input.GetKeyUp(KeyCode.R))
            {
                playerHandle.gameObject.transform.position = new Vector3(Random.Range(-50000f, 50000f), 4500f, Random.Range(-50000f, 50000f));
            }
            if (Input.GetKeyUp(KeyCode.K))
            {
                PlayerCommand.current.SendCommand("Collect Spear");
            }
            if (Input.GetKeyUp(KeyCode.L))
            {
                PlayerCommand.current.SendCommand("Collect Stone");
            }
            if (Input.GetKeyUp(KeyCode.Q))
            {
                PlayerCommand.current.SendCommand("Attack TribeMember");
            }



            //Debug.Log(Camp.EntityIsInsideCamp(playerHandle));
        }



    }
}
