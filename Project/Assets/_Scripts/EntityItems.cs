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
    public Item clothingEquipped;


    
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
        clothingEquipped = null; // TODO: initialize to something

        itemOrientationAnimator = orientationParent.GetComponent<Animator>();
    }


    void Start()
    {
        
    }

    // client method when an object is interacted with
    public void OnObjectInteract(GameObject o, ScriptableObject attachedObject){
        Item i = Item.GetItemByName(o.name);
        switch (i.type) {
            case Item.Type.Weapon :
                PickUpWeapon(i, attachedObject);
                break;
            case Item.Type.Clothing :
                EquipClothing(i, attachedObject);
                break;
            default:
                PickUpNonFoodOrClothing(i, attachedObject);
                break;
            
        }
        entityPhysics.OnItemSwitch();
    }


    public void PickUpWeapon(Item i, ScriptableObject attachedObject){

        Debug.Log("Picking up weapon: " + i.nme);

        if (attachedObject is ObjectRack)
        {
            // get rack reference from attached object and add the item to faction items with specified rack
            ObjectRack rack = (ObjectRack)attachedObject;
            Faction.RemoveItemOwned(entityInfo.faction, i, 1, rack);
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

    public void PickUpNonFoodOrClothing(Item i, ScriptableObject attachedObject){

        GameObject o = Utility.InstantiatePrefabSameName(i.gameobject);

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

    public void DropUnequippedWeapon(ScriptableObject targetAttachedObject){


        if(targetAttachedObject is ObjectRack){
            // get rack reference from attached object and add the item to faction items with specified rack
            ObjectRack rack = (ObjectRack)targetAttachedObject;
            Faction.AddItemOwned(entityInfo.faction, weaponEquipped_item, 1, rack);
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
        Utility.ToggleObjectPhysics(weaponEquipped_object, false);
    }
    public void SetEquippedWeapon(Item i){

        GameObject o = Utility.InstantiatePrefabSameName(i.gameobject);

        weaponEquipped_item = i;
        weaponEquipped_object = o;
        Utility.ToggleObjectPhysics(weaponEquipped_object, false);
    }

    public void ToggleWeaponEquipped(){
        if(weaponEquipped_item != null && weaponUnequipped_item != null){
            Item tempItem = weaponEquipped_item;
            GameObject tempObject = weaponEquipped_object;

            weaponEquipped_item = weaponUnequipped_item;
            weaponEquipped_object = weaponUnequipped_object;
            weaponUnequipped_item = tempItem;
            weaponUnequipped_object = tempObject;

            entityPhysics.OnItemSwitch();
        }
    }

    // ---


    // clothing
    // ---

    public void EquipClothing(Item i, ScriptableObject attachedObject){

        // unequip current clothing
        UnequipCurrentClothing(attachedObject);

        // set clothing on model
        meshParentT.Find(i.nme).gameObject.SetActive(true);
        this.clothingEquipped = i;

        // remove clothing from attached object
        if (attachedObject is ObjectRack)
        {
            // get rack reference from attached object and add the item to faction items with specified rack
            ObjectRack rack = (ObjectRack)attachedObject;
            Faction.RemoveItemOwned(entityInfo.faction, i, 1, rack);
        }
        // todo: if equipping from another human
        else
        {
            Debug.Log("No clothing attached object match");
        }
    }
    public void UnequipCurrentClothing(ScriptableObject targetAttachedObject){

        // if a clothing is currently equipped, unequip it and add associated item to faction items
        if (clothingEquipped != null)
        {
            if (targetAttachedObject is ObjectRack)
            {

                // if unequipping to object rack, get rack reference from attached object and add the item to faction items with specified rack
                ObjectRack rack = (ObjectRack)targetAttachedObject;
                Faction.AddItemOwned(entityInfo.faction, clothingEquipped, 1, rack);
            }

            // todo: if unequipping onto another human
            else
            {
                Debug.Log("No clothing attached object match");
            }

            // unequip clothing on model
            meshParentT.Find(clothingEquipped.nme).gameObject.SetActive(false);
            clothingEquipped = null;
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