﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityItems : EntityComponent
{


    // Item the entity is currently holding - can only hold one item at a time - if held item is switched out, move to pockets if pocketable
    public Item holding_item;
    public GameObject holding_object;

    // currently equipped weapon - when not holding, attached to character model
    public Item weaponEquipped_item;
    public Item weaponUnequipped_item;
    public GameObject weaponEquipped_object, weaponUnequipped_object;

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

        weaponEquipped_item = weaponUnequipped_item = holding_item = null;
        weaponEquipped_object = weaponUnequipped_object = holding_object = null;
    }


    void Start()
    {
        
    }


    public void SetHolding(Item item, GameObject obj){
        if(holding_item != null){
            DropHolding();
        }
        holding_item = item;
        holding_object = obj;
        TogglePhysics(holding_object, false);
    }
    public void DropHolding(){
        if(holding_item == null) { return; }
        holding_object.GetComponent<Rigidbody>().AddForce(transform.forward + Vector3.up);
        TogglePhysics(holding_object, true);
        Faction.RemoveItemOwned(holding_object, entityInfo.faction);
        
        holding_item = null;
        holding_object = null;
    }

    public void PickUpWeapon(Item item, GameObject obj){
        if(weaponEquipped_item == null){
            SetEquippedWeapon(item, obj);
        }
        else{
            if(weaponUnequipped_item != null){
                DropUnequippedWeapon();
            }
            SetUnequippedWeapon(item, obj);
        }
    }
    public void DropUnequippedWeapon(){
        TogglePhysics(weaponEquipped_object, true);
        weaponEquipped_object.GetComponent<Rigidbody>().AddForce(transform.forward * -30f);
        Faction.RemoveItemOwned(weaponEquipped_object, entityInfo.faction);
        weaponUnequipped_item = null;
        weaponUnequipped_object = null;

    }
    public void SetUnequippedWeapon(Item item, GameObject obj){
        weaponUnequipped_item = item;
        weaponUnequipped_object = obj;
        TogglePhysics(weaponEquipped_object, false);
    }
    public void SetEquippedWeapon(Item item, GameObject obj){
        weaponEquipped_item = item;
        weaponEquipped_object = obj;
        TogglePhysics(weaponEquipped_object, false);
    }

    public void PocketItem(Item i){
        pockets.AddItem(i);
    }


    void TogglePhysics(GameObject o, bool value){
        o.GetComponent<BoxCollider>().enabled = value;
        o.GetComponent<Rigidbody>().isKinematic = !value;
    }


    void Update(){

        //float objSpeed = .15f;

        // if(holding != null){
        //     GameObject hold = holding.Item2;
        //     hold.transform.position = t_hand_left.position + t_hand_left.forward*hold.GetComponent<BoxCollider>().size.z/4f;
        //     hold.transform.rotation = t_hand_left.rotation;
        // }
        if(weaponEquipped_object != null){
            Vector3 targetPos;
            Quaternion targetRot;
            if(weaponEquipped_item.holdStyle.Equals(Item.HoldStyle.Spear)){
                targetPos = orientation_weaponEquipped_spear.position;
                targetRot = orientation_weaponEquipped_spear.rotation;
            }
            else{
                targetPos = orientation_weaponEquipped_axe.position;
                targetRot = orientation_weaponEquipped_axe.rotation;
            }
            Vector3 currentPos = weaponEquipped_object.transform.position;
            float x = Mathf.Lerp(currentPos.x, targetPos.x, 30f * Time.deltaTime);
            float y = Mathf.Lerp(currentPos.y, targetPos.y, 25f * Time.deltaTime);
            float z = Mathf.Lerp(currentPos.z, targetPos.z, 30f * Time.deltaTime);
            weaponEquipped_object.transform.position = new Vector3(x, y, z);
            weaponEquipped_object.transform.rotation = Quaternion.Slerp(weaponEquipped_object.transform.rotation, targetRot, 18f * Time.deltaTime);

            // weaponEquipped_object.transform.position = targetPos.position;
            // weaponEquipped_object.transform.rotation = targetRot.rotation;
        }
        if(weaponUnequipped_object != null){
            weaponUnequipped_object.transform.position = Vector3.Lerp(weaponUnequipped_object.transform.position, orientation_weaponUnequipped.position, 30000000f * Time.deltaTime);
            weaponUnequipped_object.transform.rotation = Quaternion.Slerp(weaponUnequipped_object.transform.rotation, orientation_weaponUnequipped.rotation, 18f * Time.deltaTime);
            // weaponUnequipped_object.transform.position = orientation_weaponUnequipped.position;
            // weaponUnequipped_object.transform.rotation = orientation_weaponUnequipped.rotation;

        }

        //weap_on.transform.position = Vector3.Lerp(weap_on.transform.position, , objSpeed);

    }

}