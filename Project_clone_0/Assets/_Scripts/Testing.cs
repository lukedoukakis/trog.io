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
        //faction.AddItemOwned(Item.ClothingTest, 2, null, playerHandle.transform, 0f);
        faction.AddItemOwned(Item.Meat, 2, null, playerHandle.transform, 0f);
        //faction.AddItemOwned(Item.Spear, 2, null, playerHandle.transform, 0f);
        faction.AddItemOwned(Item.Axe, 8, null, playerHandle.transform, 0f);
        //faction.AddItemOwned(Item.WoodPiece, 7, null, playerHandle.transform, 0f);
        //faction.AddItemOwned(Item.BonePiece, 4, null, playerHandle.transform, 0f);
        //faction.AddItemOwned(Item.StoneSmall, 8, null, playerHandle.transform, 0f);
    }


    void Update()
    {


        if (playerHandle != null)
        {
            if (Input.GetKeyUp(KeyCode.C))
            {
                Camp.TryPlaceCamp(playerHandle.entityInfo.faction, GameManager.current.localPlayer.transform);
            }
            if (Input.GetKeyUp(KeyCode.R))
            {
                playerHandle.gameObject.transform.position = new Vector3(Random.Range(-50000f, 50000f), 4750f, Random.Range(-50000f, 50000f));
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
