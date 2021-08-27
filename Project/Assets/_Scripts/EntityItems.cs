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

    // clothing
    public Transform meshParentT;
    public string clothingEquippedName;


    
    // orientations in space for items
    public Transform orientationParent;
    public Transform orientation_weaponEquipped_spear;
    public Transform orientation_weaponEquipped_axe;
    public Transform orientation_weaponUnequipped;

    public Transform anchor_weaponEquipped_spear;
    public Transform anchor_weaponEquipped_axe;
    public Transform basePosition_anchor_weaponEquipped_spear;
    public Transform basePosition_anchor_weaponEquipped_axe;

    public Animator itemOrientationAnimator;



    




    protected override void Awake(){
     
        base.Awake();

        orientationParent = Utility.FindDeepChild(transform, "ItemOrientations");
        orientation_weaponEquipped_spear = orientationParent.Find("WeaponEquippedSpear");
        orientation_weaponEquipped_axe = orientationParent.Find("WeaponEquippedAxe");
        orientation_weaponUnequipped = orientationParent.Find("WeaponUnequipped");

        anchor_weaponEquipped_spear = orientationParent.Find("AnchorWeaponEquippedSpear");
        anchor_weaponEquipped_axe = orientationParent.Find("AnchorWeaponEquippedAxe");
        basePosition_anchor_weaponEquipped_spear = orientationParent.Find("BasePositionAnchorWeaponEquippedSpear");
        basePosition_anchor_weaponEquipped_axe = orientationParent.Find("BasePositionAnchorWeaponEquippedAxe");

        weaponEquipped_item = weaponUnequipped_item = holding_item = null;
        weaponEquipped_object = weaponUnequipped_object = holding_object = null;

        meshParentT = Utility.FindDeepChild(transform, "Human Model 2");
        clothingEquippedName = null; // TODO: initialize to something

        itemOrientationAnimator = orientationParent.GetComponent<Animator>();
    }


    void Start()
    {
        
    }

    // client method when an object is interacted with
    public void OnObjectInteract(GameObject o, GameObject attachedObject){
        Item i = Item.GetItemByName(o.name);
        switch (i.type) {
            case Item.Type.Weapon :
                PickUpWeapon(i, o, attachedObject);
                break;
            case Item.Type.Clothing :
                EquipClothing(i, attachedObject);
                break;
            default:
                PickUpNonFoodOrClothing(i, o, attachedObject);
                break;
            
        }
    }


    public void PickUpWeapon(Item i, GameObject o, GameObject attachedObject){
        if(weaponEquipped_item == null){
            SetEquippedWeapon(i, o);
        }
        else{
            if(weaponUnequipped_item != null){
                DropUnequippedWeapon(attachedObject);
            }
            SetUnequippedWeapon(i, o);
        }
    }

    public void PickUpNonFoodOrClothing(Item i, GameObject o, GameObject attachedObject){
        if(holding_item != null){
            DropHolding();
        }
        holding_item = i;
        holding_object = o;
        Utility.ToggleObjectPhysics(holding_object, false);
    }


    // holding
    public void DropHolding(){
        if(holding_item == null) { return; }
        holding_object.GetComponent<Rigidbody>().AddForce(transform.forward + Vector3.up);
        Utility.ToggleObjectPhysics(holding_object, true);
        
        holding_item = null;
        holding_object = null;
    }

    public void DropUnequippedWeapon(GameObject targetAttachedObject){

        switch (targetAttachedObject.tag) {
            case "ItemRack" :
                // get rack reference from attached object and add the item to faction items with specified rack
                ObjectRack rack = (ObjectRack)targetAttachedObject.GetComponent<ScriptableObjectReference>().GetScriptableObject();
                Faction.AddItemOwned(entityInfo.faction, weaponEquipped_item, 1, rack);
                break;
            case "Human" :
                // todo: give to human
            default :
                Debug.Log("No place to drop object... targetAttachedObject tag: " + targetAttachedObject.tag);
                break;
        }

        // destroy gameobject of unequipped weapon and set references to null
        GameObject.Destroy(weaponUnequipped_object);
        weaponUnequipped_item = null;
        weaponUnequipped_object = null;

        
        // Utility.ToggleObjectPhysics(weaponUnequipped_object, true);
        // weaponEquipped_object.GetComponent<Rigidbody>().AddForce(transform.forward * -30f);
        // weaponUnequipped_item = null;
        // weaponUnequipped_object = null;
    }

    public void SetUnequippedWeapon(Item i, GameObject o){
        weaponUnequipped_item = i;
        weaponUnequipped_object = o;
        Utility.ToggleObjectPhysics(weaponEquipped_object, false);
    }
    public void SetEquippedWeapon(Item i, GameObject o){
        weaponEquipped_item = i;
        weaponEquipped_object = o;
        Utility.ToggleObjectPhysics(weaponEquipped_object, false);
    }

    // ---


    // clothing
    // ---

    public void EquipClothing(Item i, GameObject attachedObject){

        // unequip current clothing
        UnequipCurrentClothing(attachedObject);

        try{

            // set clothing on model
            meshParentT.Find(i.nme).gameObject.SetActive(true);
            this.clothingEquippedName = i.nme;

            // remove clothing from attached object
            ObjectRack rack = (ObjectRack)attachedObject.GetComponent<ScriptableObjectReference>().GetScriptableObject();
            Faction.RemoveItemOwned(entityInfo.faction, i, 1, rack);
        }
        catch(Exception){
            Debug.Log("No clothing found on model for clothing name: " + i.nme);
        }

    }
    public void UnequipCurrentClothing(GameObject targetAttachedObject){

        // if a clothing is currently equipped, unequip it and add associated item to faction items
        if(clothingEquippedName != null){
            ObjectRack rack = (ObjectRack)targetAttachedObject.GetComponent<ScriptableObjectReference>().GetScriptableObject();
            Faction.AddItemOwned(entityInfo.faction, Item.GetItemByName(clothingEquippedName), 1, rack);
            meshParentT.Find(clothingEquippedName).gameObject.SetActive(false);
        }
    }

    // ---

    void Update(){


        anchor_weaponEquipped_spear.position = basePosition_anchor_weaponEquipped_spear.position;
        anchor_weaponEquipped_spear.rotation = basePosition_anchor_weaponEquipped_spear.rotation;
        anchor_weaponEquipped_axe.position = basePosition_anchor_weaponEquipped_axe.position;
        anchor_weaponEquipped_axe.rotation = basePosition_anchor_weaponEquipped_axe.rotation;
        
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
                //targetRot = orientation_weaponEquipped_spear.rotation;
                targetRot = anchor_weaponEquipped_spear.rotation;
            }
            else if(weaponEquipped_item.holdStyle.Equals(Item.HoldStyle.Axe)){
                targetPos = orientation_weaponEquipped_axe.position;
                //targetRot = orientation_weaponEquipped_axe.rotation;
                targetRot = anchor_weaponEquipped_axe.rotation;
            }
            else{
                targetPos = Vector3.zero;
                targetRot = Quaternion.identity;
            }
            Vector3 currentPos = weaponEquipped_object.transform.position;
            // weaponEquipped_object.transform.position = Vector3.Lerp(currentPos, targetPos, 60f * Time.deltaTime);
            // weaponEquipped_object.transform.rotation = Quaternion.Slerp(weaponEquipped_object.transform.rotation, targetRot, 60f * Time.deltaTime);
            weaponEquipped_object.transform.position = targetPos;
            weaponEquipped_object.transform.rotation = targetRot;
        }
        if(weaponUnequipped_object != null){
            weaponUnequipped_object.transform.position = Vector3.Lerp(weaponUnequipped_object.transform.position, orientation_weaponUnequipped.position, float.MaxValue * Time.deltaTime);
            weaponUnequipped_object.transform.rotation = Quaternion.Slerp(weaponUnequipped_object.transform.rotation, orientation_weaponUnequipped.rotation, 18f * Time.deltaTime);
            // weaponUnequipped_object.transform.position = orientation_weaponUnequipped.position;
            // weaponUnequipped_object.transform.rotation = orientation_weaponUnequipped.rotation;

        }

        //weap_on.transform.position = Vector3.Lerp(weap_on.transform.position, , objSpeed);

    }

}