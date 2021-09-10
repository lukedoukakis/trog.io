using System;
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
    public void OnObjectInteract(GameObject o, ScriptableObject attachedObject){
        Item i = Item.GetItemByName(o.name);
        switch (i.type) {
            case Item.Type.Food :
                PickUpHolding(i, o, attachedObject);
                break;
            case Item.Type.Weapon :
                PickUpWeapon(i, attachedObject);
                break;
            case Item.Type.Clothing :
                PickUpHolding(i, o, attachedObject);
                break;
            default:
                PickUpHolding(i, o, attachedObject);
                break;
            
        }

        OnItemsChange();
    }


    public void PickUpWeapon(Item i, ScriptableObject attachedObject){

        Debug.Log("Picking up weapon: " + i.nme);

        if (attachedObject is ObjectRack)
        {
            // get rack reference from attached object and add the item to faction items with specified rack
            ObjectRack rack = (ObjectRack)attachedObject;
            Faction rackFac = rack.camp.faction;
            Faction.RemoveItemOwned(rackFac, i, 1, rack);
        }
        // todo: if getting from another human
        else
        {
            Debug.Log("No attached object match");
        }


        ToggleWeaponEquipped();

        if(weaponEquipped_item == null){

            // if no equipped weapon, set equipped weapon
            SetEquippedWeapon(i);

        }
        else{
            if(weaponUnequipped_item != null){
                DropUnequippedWeapon(attachedObject);
                
            }
            SetUnequippedWeapon(i);
        }
    }


    // holding

    public void PickUpHolding(Item item, GameObject gameobject, ScriptableObject attachedObject){

        if (attachedObject is ObjectRack)
        {
            // get rack reference from attached object and add the item to faction items with specified rack
            ObjectRack rack = (ObjectRack)attachedObject;
            Faction rackFac = rack.camp.faction;
            Faction.RemoveItemOwned(rackFac, item, 1, rack);
        }
        // todo: if getting from another human
        else
        {
            Debug.Log("No attached object match");
        }

        GameObject o;
        if(Item.IsClampedType(item)){
            o = Utility.InstantiatePrefabSameName(item.gameobject);
        }
        else{
            o = gameobject;
        }

        if(holding_item != null){
            DropHolding(attachedObject);
        }
        holding_item = item;
        holding_object = o;
        Utility.ToggleObjectPhysics(holding_object, false);
    }

    public void DropHolding(ScriptableObject targetAttachedObject){
        if(holding_item == null) { return; }

        if (Item.IsClampedType(holding_item))
        {
            if (targetAttachedObject == null){
                Faction.AddItemOwned(entityInfo.faction, weaponUnequipped_item, 1, null);
            }
            else if (targetAttachedObject is ObjectRack)
            {
                // get rack reference from attached object and add the item to faction items with specified rack
                ObjectRack rack = (ObjectRack)targetAttachedObject;
                if (!rack.itemType.Equals(holding_item.type)) { rack = null; }
                Faction.AddItemOwned(entityInfo.faction, weaponUnequipped_item, 1, rack);
            }
            else
            {
                Debug.Log("No attached object match");
            }
            // todo: case human

            GameObject.Destroy(holding_object);
        }
        else{
            Physics.IgnoreCollision(holding_object.GetComponent<Collider>(), entityPhysics.hitbox, false);
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

    public void DropUnequippedWeapon(ScriptableObject targetAttachedObject){


        if(targetAttachedObject is ObjectRack){
            // get rack reference from attached object and add the item to faction items with specified rack
            ObjectRack rack = (ObjectRack)targetAttachedObject;
            Faction.AddItemOwned(entityInfo.faction, weaponUnequipped_item, 1, rack);
        }
        else{
            Debug.Log("No attached object match");
        }
        // todo: case human

        // destroy gameobject of unequipped weapon and set references to null
        GameObject.Destroy(weaponUnequipped_object);
        weaponUnequipped_item = null;
        weaponUnequipped_object = null;

        
        // Utility.ToggleObjectPhysics(weaponUnequipped_object, true);
    }

    public void SetUnequippedWeapon(Item i){

        GameObject o = Utility.InstantiatePrefabSameName(i.gameobject);

        weaponUnequipped_item = i;
        weaponUnequipped_object = o;

        // toggle physics
        Utility.ToggleObjectPhysics(weaponEquipped_object, false);

        // remove hit detection owner
        weaponUnequipped_object.transform.Find("HitZone").GetComponent<WeaponCollisionDetector>().RemoveOwner();
    }
    public void SetEquippedWeapon(Item i){

        GameObject o = Utility.InstantiatePrefabSameName(i.gameobject);

        weaponEquipped_item = i;
        weaponEquipped_object = o;

        // add stats
        entityStats.AddStatsModifier(i.stats);

        // turn off physics
        Utility.ToggleObjectPhysics(weaponEquipped_object, false);

        // set weapon hit detection owner
        weaponEquipped_object.transform.Find("HitZone").GetComponent<WeaponCollisionDetector>().SetOwner(entityHandle);
    }

    public void ToggleWeaponEquipped(){
        if(weaponEquipped_item != null && weaponUnequipped_item != null){
            Item tempItem = weaponEquipped_item;
            GameObject tempObject = weaponEquipped_object;

            weaponEquipped_item = weaponUnequipped_item;
            weaponEquipped_object = weaponUnequipped_object;
            weaponUnequipped_item = tempItem;
            weaponUnequipped_object = tempObject;

            // turn off physics
            Utility.ToggleObjectPhysics(weaponEquipped_object, false);
            Utility.ToggleObjectPhysics(weaponUnequipped_object, false);

            // update stats
            entityStats.RemoveStatsModifier(weaponUnequipped_item.stats);
            entityStats.AddStatsModifier(weaponEquipped_item.stats);

            // set weapon hit detection owner
            weaponEquipped_object.transform.Find("HitZone").GetComponent<WeaponCollisionDetector>().SetOwner(entityHandle);
            weaponUnequipped_object.transform.Find("HitZone").GetComponent<WeaponCollisionDetector>().RemoveOwner();
            
            OnItemsChange();
        }
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
            Physics.IgnoreCollision(weaponEquipped_object.transform.Find("HitZone").GetComponent<Collider>(), entityPhysics.hitbox, true);

        }
        if(weaponUnequipped_object != null){
            Physics.IgnoreCollision(weaponUnequipped_object.transform.Find("HitZone").GetComponent<Collider>(), entityPhysics.hitbox, true);
        }
        if(holding_object != null){
            Physics.IgnoreCollision(holding_object.GetComponent<Collider>(), entityPhysics.hitbox, true);
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