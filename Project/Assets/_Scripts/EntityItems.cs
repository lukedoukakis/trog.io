using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityItems : EntityComponent
{


    // Item the entity is currently holding - can only hold one item at a time - if held item is switched out, move to pockets if pocketable
    public Tuple<Item, GameObject> holding;

    // currently equipped weapon - when not holding, attached to character model
    public Tuple<Item, GameObject> weaponEquipped;
    public Tuple<Item, GameObject> weaponUnequipped;

    // items in the entity's pockets
    public ItemCollection pockets;


    
    // orientations in space for items
    public Transform orientationParent;
    public Transform orientation_weaponEquipped_spear;
    public Transform orientation_weaponEquipped_axe;
    public Transform orientation_weaponUnequipped;
    




    protected override void Awake(){
     
        base.Awake();

        orientationParent = Utility.FindDeepChild(transform, "ItemOrientations");
        orientation_weaponEquipped_spear = orientationParent.Find("WeaponEquippedSpear");
        orientation_weaponEquipped_axe = orientationParent.Find("WeaponEquippedAxe");
        orientation_weaponUnequipped = orientationParent.Find("WeaponUnequipped");
    }


    void Start()
    {
        
    }


    public void SetHolding(Tuple<Item, GameObject> itemObjectPair){
        if(holding != null){
            DropHolding();
        }
        holding = itemObjectPair;
        TogglePhysics(holding.Item2, false);
        
        entityPhysics.OnItemSwitch();
    }
    public void DropHolding(){
        GameObject hold = holding.Item2;
        hold.GetComponent<Rigidbody>().AddForce(transform.forward*900f + transform.up*900f);
        TogglePhysics(hold, true);
        Faction.RemoveItemOwned(holding.Item2, entityInfo.faction);
        
        holding = null;

        entityPhysics.OnItemSwitch();
    }

    public void SetWeapon(Tuple<Item, GameObject> itemObjectPair){
        if(weaponEquipped != null){
            if(weaponUnequipped != null){
                DropUnequippedWeapon();
            }
            SetUnequippedWeapon(itemObjectPair);
        }
        else{
            SetEquippedWeapon(itemObjectPair);
        }

        entityPhysics.OnItemSwitch();
    }
    public void DropUnequippedWeapon(){
        TogglePhysics(weaponEquipped.Item2, true);
        Faction.RemoveItemOwned(weaponEquipped.Item2, entityInfo.faction);
        weaponUnequipped = null;

    }
    public void SetUnequippedWeapon(Tuple<Item, GameObject> itemObjectPair){
        weaponUnequipped = itemObjectPair;
        TogglePhysics(itemObjectPair.Item2, false);
    }
    public void SetEquippedWeapon(Tuple<Item, GameObject> itemObjectPair){
        weaponEquipped = itemObjectPair;
        TogglePhysics(itemObjectPair.Item2, false);
    }

    public void PocketItem(Item i){
        pockets.AddItem(i);
    }


    void TogglePhysics(GameObject o, bool value){
        o.GetComponent<BoxCollider>().enabled = value;
        //o.GetComponent<Rigidbody>().isKinematic = !value;
    }


    void Update(){

        //float objSpeed = .15f;

        // if(holding != null){
        //     GameObject hold = holding.Item2;
        //     hold.transform.position = t_hand_left.position + t_hand_left.forward*hold.GetComponent<BoxCollider>().size.z/4f;
        //     hold.transform.rotation = t_hand_left.rotation;
        // }
        if(weaponEquipped != null){
            GameObject weap_on = weaponEquipped.Item2;
            if(weaponUnequipped.Item1.holdStyle.Equals(Item.HoldStyle.Spear)){
                weap_on.transform.position = orientation_weaponEquipped_spear.position;
                weap_on.transform.rotation = orientation_weaponEquipped_spear.rotation;
            }
            else{
                weap_on.transform.position = orientation_weaponEquipped_axe.position;
                weap_on.transform.rotation = orientation_weaponEquipped_axe.rotation;
            }
        }
        if(weaponUnequipped != null){
            GameObject weap_off = weaponUnequipped.Item2;
            weap_off.transform.position = orientation_weaponUnequipped.position;
            weap_off.transform.rotation = orientation_weaponUnequipped.rotation;

        }

        //weap_on.transform.position = Vector3.Lerp(weap_on.transform.position, , objSpeed);

    }

}