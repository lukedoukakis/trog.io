using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityItems : EntityComponent
{


    // Item the entity is currently holding - can only hold one item at a time - if held item is switched out, move to pockets if pocketable
    public Item holding;

    // currently equipped weapon - when not holding, attached to character model
    public Item weapon_equipped;
    public Item weapon_unequipped;

    // items in the entity's pockets
    public ItemCollection pockets;




    void Awake(){
        handle = GetComponent<EntityHandle>();
        handle.entityInventory = this;

    }


    void Start()
    {
        
    }


    public void PickupItem(Item i){
        SetHolding(i);
    }
    public void SetHolding(Item i){
        if(holding != null){
            DropHolding();
        }
        holding = i;
    }
    public void DropHolding(){
        GameObject.Instantiate(holding.gameobject, transform.position, Quaternion.identity);
        holding = null;
    }

    public void PickupWeapon(Item w){
        if(weapon_equipped != null){
            if(weapon_unequipped != null){
                DropUnequippedWeapon();
            }
            SetUnequippedWeapon(w);
        }
        else{
            SetEquippedWeapon(w);
        }
    }
    public void DropUnequippedWeapon(){
        GameObject.Instantiate(holding.gameobject, transform.position, Quaternion.identity);
        weapon_unequipped = null;
    }
    public void SetUnequippedWeapon(Item w){
        weapon_unequipped = w;
    }
    public void SetEquippedWeapon(Item w){
        weapon_equipped = w;
    }

    public void PocketItem(Item i){
        pockets.AddItem(i);
    }


}
