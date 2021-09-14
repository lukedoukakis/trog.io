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
    public Item clothing;


    
    // orientations in space for items
    public Transform orientationParent;
    public Transform orientation_weaponEquipped_spear;
    public Transform orientation_weaponEquipped_axe;
    public Transform orientation_weaponUnequipped;
    public Transform orientation_holding;
    public Transform basePosition_weaponEquipped_spear;
    public Transform basePosition_weaponEquipped_axe;
    public Transform basePosition_holding;

    public Animator itemOrientationAnimator;



    




    protected override void Awake(){
     
        base.Awake();

        orientationParent = Utility.FindDeepChild(transform, "ItemOrientations");
        orientation_weaponEquipped_spear = orientationParent.Find("WeaponEquippedSpear");
        orientation_weaponEquipped_axe = orientationParent.Find("WeaponEquippedAxe");
        orientation_weaponUnequipped = orientationParent.Find("WeaponUnequipped");
        orientation_holding = orientationParent.Find("Holding");
        basePosition_weaponEquipped_spear = orientationParent.Find("BasePositionAnchorWeaponEquippedSpear");
        basePosition_weaponEquipped_axe = orientationParent.Find("BasePositionAnchorWeaponEquippedAxe");
        basePosition_holding = orientationParent.Find("BasePositionHolding");


        weaponEquipped_item = weaponUnequipped_item = holding_item = null;
        weaponEquipped_object = weaponUnequipped_object = holding_object = null;

        meshParentT = Utility.FindDeepChild(transform, "Human Model 2");
        clothing = null; // TODO: initialize to something

        itemOrientationAnimator = orientationParent.GetComponent<Animator>();
    }


    void Start()
    {

    }

    // client method when an object is interacted with
    public void OnObjectInteract(GameObject worldObject, ScriptableObject attachedObject){
        Item i = Item.GetItemByName(worldObject.name);
        switch (i.type) {
            case Item.Type.Food :
                PickUpHolding(i, worldObject, attachedObject);
                break;
            case Item.Type.Weapon :
                PickUpWeapon(i, worldObject, attachedObject);
                break;
            case Item.Type.Clothing :
                PickUpHolding(i, worldObject, attachedObject);
                break;
            default:
                PickUpHolding(i, worldObject, attachedObject);
                break;
            
        }

        OnItemsChange();
    }

    public void OnEmptyInteract(){
        if(holding_item != null){
            DropHolding(null);
        }
        else{
            if(weaponEquipped_item != null){
                DropEquippedWeapon(null);
            }
        }
        OnItemsChange();
    }


    public void PickUpWeapon(Item item, GameObject worldObject, ScriptableObject attachedObject){

        Debug.Log("Picking up weapon: " + item.nme);

        GameObject o;

        if (attachedObject is ObjectRack)
        {
            Log("From rack");
            // get rack reference from attached object and add the item to faction items with specified rack
            ObjectRack rack = (ObjectRack)attachedObject;
            Faction rackFac = rack.camp.faction;
            Faction.RemoveItemOwned(rackFac, item, 1, rack);
            o = Utility.InstantiatePrefabSameName(item.worldObject);
            o.transform.position = worldObject.transform.position;
            o.transform.rotation = worldObject.transform.rotation;
        }
        // todo: if getting from another human
        else if(attachedObject == null)
        {
            Log("No attached obj");
            o = worldObject;
        }
        else{
            o = worldObject;
        }

        if(weaponEquipped_item != null){
            if(weaponUnequipped_item == null){
                ToggleWeaponEquipped();
            }
            else{
                DropEquippedWeapon(attachedObject);
            }
        }
        SetEquippedWeapon(item, o);

    }


    // holding

    public void PickUpHolding(Item item, GameObject worldObject, ScriptableObject attachedObject){

        GameObject o;

        if (attachedObject is ObjectRack)
        {
            // get rack reference from attached object and add the item to faction items with specified rack
            ObjectRack rack = (ObjectRack)attachedObject;
            Faction rackFac = rack.camp.faction;
            Faction.RemoveItemOwned(rackFac, item, 1, rack);
            o = Utility.InstantiatePrefabSameName(item.worldObject);
            o.transform.position = worldObject.transform.position;
            o.transform.rotation = worldObject.transform.rotation;
        }
        else if(attachedObject == null)
        {
            o = worldObject;
        }
        else{
            o = worldObject;
        }
        // todo: if getting from another human

        if(holding_item != null){
            DropHolding(attachedObject);
        }
        holding_item = item;
        holding_object = o;
        Utility.ToggleObjectPhysics(holding_object, false);
    }

    public void DropHolding(ScriptableObject targetAttachedObject){
        if(holding_item == null) { return; }

        if (targetAttachedObject is ObjectRack)
        {
            // get rack reference from attached object and add the item to faction items with specified rack
            ObjectRack rack = (ObjectRack)targetAttachedObject;
            if (!rack.itemType.Equals(holding_item.type)) { rack = null; }
            Faction.AddItemOwned(entityInfo.faction, weaponUnequipped_item, 1, rack);
            GameObject.Destroy(holding_object);
        }
        else if (targetAttachedObject == null)
        {
            holding_object.GetComponent<ScriptableObjectReference>().SetScriptableObjectReference(null);
            Physics.IgnoreCollision(holding_object.GetComponent<Collider>(), entityPhysics.worldCollider, false);
        }
        else
        {
            // todo: case human
        }


        Utility.ToggleObjectPhysics(holding_object, true);

        holding_item = null;
        holding_object = null;
    }

    public void ConsumeHolding(Item item)
    {
        entityStats.AddStatsModifier(holding_item.stats);
        GameObject.Destroy(holding_object);
        holding_item = null;
        holding_object = null;
    }


    public void OnHoldingUse(){

        if(holding_item != null){

            switch (holding_item.type) {
                case Item.Type.Food :
                    // todo: eating animation
                    ConsumeHolding(holding_item);
                    break;
                
                case Item.Type.Clothing :
                    // todo: clothing animation
                    Item i = holding_item;
                    EquipClothing(holding_item);
                    ConsumeHolding(i);
                    break;

                default:
                    break;


            }


            
        }


    }


    // weapon

    // public void DropUnequippedWeapon(ScriptableObject targetAttachedObject){

    //     if (targetAttachedObject is ObjectRack)
    //     {
    //         // get rack reference from attached object and add the item to faction items with specified rack
    //         ObjectRack rack = (ObjectRack)targetAttachedObject;
    //         Faction.AddItemOwned(entityInfo.faction, weaponUnequipped_item, 1, rack);
    //         GameObject.Destroy(weaponUnequipped_object);
    //     }
    //     else if (targetAttachedObject == null)
    //     {
    //         weaponUnequipped_object.GetComponent<ScriptableObjectReference>().SetScriptableObjectReference(null);
    //         Physics.IgnoreCollision(weaponUnequipped_object.transform.Find("HitZone").GetComponent<Collider>(), entityPhysics.hitbox, false);
    //         Utility.ToggleObjectPhysics(weaponUnequipped_object, true);
    //     }
    //     // todo: case human

    //     weaponUnequipped_item = null;
    //     weaponUnequipped_object = null;

    // }

    public void DropEquippedWeapon(ScriptableObject targetAttachedObject){
        if (targetAttachedObject is ObjectRack)
        {
            // get rack reference from attached object and add the item to faction items with specified rack
            ObjectRack rack = (ObjectRack)targetAttachedObject;
            Faction.AddItemOwned(entityInfo.faction, weaponEquipped_item, 1, rack);
            GameObject.Destroy(weaponEquipped_object);
        }
        else if (targetAttachedObject == null)
        {
            weaponEquipped_object.GetComponent<ScriptableObjectReference>().SetScriptableObjectReference(null);
            Physics.IgnoreCollision(weaponEquipped_object.transform.Find("HitZone").GetComponent<Collider>(), entityPhysics.worldCollider, false);
            Utility.ToggleObjectPhysics(weaponEquipped_object, true);
        }
        // todo: case human

        weaponEquipped_item = null;
        weaponEquipped_object = null;
    }

    public void SetUnequippedWeapon(Item item, GameObject worldObject){

        weaponUnequipped_item = item;
        weaponUnequipped_object = worldObject;

        // toggle physics
        Utility.ToggleObjectPhysics(weaponEquipped_object, false);

        // remove hit detection owner
        weaponUnequipped_object.transform.Find("HitZone").GetComponent<WeaponCollisionDetector>().RemoveOwner();
    }
    public void SetEquippedWeapon(Item item, GameObject worldObject){

        Log("Setting equipped weapon");
        Log("Weapon name: " + worldObject.name);

        weaponEquipped_item = item;
        weaponEquipped_object = worldObject;

        // add stats
        entityStats.AddStatsModifier(item.stats);

        // turn off physics
        Utility.ToggleObjectPhysics(weaponEquipped_object, false);

        // set weapon hit detection owner
        weaponEquipped_object.transform.Find("HitZone").GetComponent<WeaponCollisionDetector>().SetOwner(entityHandle);
    }

    public void ToggleWeaponEquipped(){


        Item tempItem = weaponEquipped_item;
        GameObject tempObject = weaponEquipped_object;

        weaponEquipped_item = weaponUnequipped_item;
        weaponEquipped_object = weaponUnequipped_object;
        weaponUnequipped_item = tempItem;
        weaponUnequipped_object = tempObject;

        if (weaponEquipped_item != null)
        {
            Utility.ToggleObjectPhysics(weaponEquipped_object, false);
            entityStats.AddStatsModifier(weaponEquipped_item.stats);
            weaponEquipped_object.transform.Find("HitZone").GetComponent<WeaponCollisionDetector>().SetOwner(entityHandle);
        }

        if (weaponUnequipped_item != null)
        {
            Utility.ToggleObjectPhysics(weaponUnequipped_object, false);
            entityStats.RemoveStatsModifier(weaponUnequipped_item.stats);
            if (weaponUnequipped_object != null)
            {
                weaponUnequipped_object.transform.Find("HitZone").GetComponent<WeaponCollisionDetector>().RemoveOwner();
            }
        }



        OnItemsChange();
        


        // if(weaponEquipped_item != null && weaponUnequipped_item != null){
        //     Item tempItem = weaponEquipped_item;
        //     GameObject tempObject = weaponEquipped_object;

        //     weaponEquipped_item = weaponUnequipped_item;
        //     weaponEquipped_object = weaponUnequipped_object;
        //     weaponUnequipped_item = tempItem;
        //     weaponUnequipped_object = tempObject;

        //     // turn off physics
        //     Utility.ToggleObjectPhysics(weaponEquipped_object, false);
        //     Utility.ToggleObjectPhysics(weaponUnequipped_object, false);

        //     // update stats
        //     entityStats.RemoveStatsModifier(weaponUnequipped_item.stats);
        //     entityStats.AddStatsModifier(weaponEquipped_item.stats);

        //     // set weapon hit detection owner
        //     weaponEquipped_object.transform.Find("HitZone").GetComponent<WeaponCollisionDetector>().SetOwner(entityHandle);
        //     weaponUnequipped_object.transform.Find("HitZone").GetComponent<WeaponCollisionDetector>().RemoveOwner();
            
        //     OnItemsChange();
        // }
    }

    // ---


    // clothing
    // ---

    public void EquipClothing(Item i){

        // unequip current clothing
        UnequipCurrentClothing();

        // set clothing on model
        Debug.Log("Equipping clothing of name: " + i.nme);
        meshParentT.Find(i.nme).gameObject.SetActive(true);
        this.clothing = i;


    }
    public void UnequipCurrentClothing(){

        // if a clothing is currently equipped, unequip it and add associated item to faction items
        if (clothing != null)
        {

            Faction.AddItemOwned(entityInfo.faction, clothing, 1, null);

            // unequip clothing on model
            meshParentT.Find(clothing.nme).gameObject.SetActive(false);

            // remove stats
            entityStats.RemoveStatsModifier(clothing.stats);

            clothing = null;
        }
    }

    public void OnItemsChange()
    {
        entityPhysics.UpdateIKForCarryingItems();
        if(weaponEquipped_object != null){
            Physics.IgnoreCollision(weaponEquipped_object.transform.Find("HitZone").GetComponent<Collider>(), entityPhysics.worldCollider, true);

        }
        if(weaponUnequipped_object != null){
            Physics.IgnoreCollision(weaponUnequipped_object.transform.Find("HitZone").GetComponent<Collider>(), entityPhysics.worldCollider, true);
        }
        if(holding_object != null){
            Physics.IgnoreCollision(holding_object.GetComponent<Collider>(), entityPhysics.worldCollider, true);
        }
    }

    // ---

    void Update(){

        orientation_weaponEquipped_spear.position = basePosition_weaponEquipped_spear.position;
        orientation_weaponEquipped_spear.rotation = basePosition_weaponEquipped_spear.rotation;
        orientation_weaponEquipped_axe.position = basePosition_weaponEquipped_axe.position;
        orientation_weaponEquipped_axe.rotation = basePosition_weaponEquipped_axe.rotation;
        orientation_holding.position = basePosition_holding.position;
        orientation_holding.rotation = basePosition_holding.rotation;

        float lerpSpeed_weapon = 30f * Time.deltaTime;
        float lerpSpeed_holding = 30f * Time.deltaTime;

        if(holding_item != null){
            Vector3 currentPos = holding_object.transform.position;
            Quaternion currentRot = holding_object.transform.rotation;
            holding_object.transform.position = Vector3.Lerp(currentPos, orientation_holding.position, lerpSpeed_holding);
            holding_object.transform.rotation = Quaternion.Slerp(currentRot, orientation_holding.rotation, lerpSpeed_holding);
        }
        if(weaponEquipped_object != null){
            Vector3 targetPos;
            Quaternion targetRot;
            if(weaponEquipped_item.holdStyle.Equals(Item.HoldStyle.Spear)){
                targetPos = orientation_weaponEquipped_spear.position;
                targetRot = orientation_weaponEquipped_spear.rotation;
            }
            else if(weaponEquipped_item.holdStyle.Equals(Item.HoldStyle.Axe)){
                targetPos = orientation_weaponEquipped_axe.position;
                targetRot = orientation_weaponEquipped_axe.rotation;
            }
            else{
                targetPos = Vector3.zero;
                targetRot = Quaternion.identity;
            }
            Vector3 currentPos = weaponEquipped_object.transform.position;
            Quaternion currentRot = weaponEquipped_object.transform.rotation;
            weaponEquipped_object.transform.position = Vector3.Lerp(currentPos, targetPos, lerpSpeed_weapon);
            weaponEquipped_object.transform.rotation = Quaternion.Slerp(currentRot, targetRot, float.MaxValue);
        }
        if(weaponUnequipped_object != null){
            weaponUnequipped_object.transform.position = Vector3.Lerp(weaponUnequipped_object.transform.position, orientation_weaponUnequipped.position, float.MaxValue);
            weaponUnequipped_object.transform.rotation = Quaternion.Slerp(weaponUnequipped_object.transform.rotation, orientation_weaponUnequipped.rotation, float.MaxValue);

        }


    }

}