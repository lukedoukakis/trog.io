using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityInventory : EntityComponent
{


    // Item the entity is currently holding - can only hold one item at a time - if held item is switched out, move to pockets if pocketable
    public Item holding;

    // currently equipped weapon - when not holding, attached to character model
    public Item weapon;

    // items in the entity's pockets
    public ItemCollection pockets;




    void Awake(){
        handle = GetComponent<EntityHandle>();
        handle.entityInventory = this;

    }


    // Start is called before the first frame update
    void Start()
    {
        
    }





    public void HoldWeapon(){

    }

    public void SetHoldingItem(Item i){
        Item h = holding;
        if(h != null){
            if(h.pocketable){
                PocketItem(h);
            }
            else{
                DropItem(h);
            }
        }
        holding = i;
    }

    public void PocketItem(Item i){
        pockets.AddItem(i);
    }

    public void DropItem(Item i){
        
    }


}
