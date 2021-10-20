using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public bool rangedMode;

    // clothing
    public Transform meshParentT;
    public Item clothing;

    public ItemCollection inventory;


    
    // orientations in space for items
    public Transform orientationParent;
    public Transform orientation_weaponEquipped_spear;
    public Transform orientation_weaponEquipped_axe;
    public Transform orientation_weaponUnequipped;
    public Transform orientation_holding;
    public Transform basePosition_weaponEquipped_spear;
    public Transform basePosition_weaponEquipped_axe;
    public Transform basePosition_holding;

    bool itemOrientationUpdateEnabled;
    bool updateWeaponEquippedOrientation;
    public Animator itemOrientationAnimator;

    public static float FOLLOW_SPEED_HOLDING_TRANSLATION = 30f;
    public static float FOLLOW_SPEED_WEAPON_TRANSLATION = 50f;
    public static float FOLLOW_SPEED_WEAPON_ROTATION = 20f;



    




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

        inventory = new ItemCollection();

        itemOrientationAnimator = orientationParent.GetComponent<Animator>();
    }


    void Start()
    {
        ToggleItemOrientationUpdate(true);
        SetWeaponRangedMode(false);
        SetUpdateWeaponOrientation(true);
    }

    // client method when an object is interacted with
    public void OnObjectTake(GameObject worldObject, object attachedObject){
        Item item = Item.GetItemByName(worldObject.name);
        switch (item.type) {
            case ItemType.Food :
                PickUpHolding(item, worldObject, attachedObject);
                break;
            case ItemType.Weapon :
                PickUpWeapon(item, worldObject, attachedObject);
                break;
            case ItemType.Clothing :
                PickUpHolding(item, worldObject, attachedObject);
                EquipClothing(holding_item);
                ConsumeHolding(holding_item);
                break;
            default:
                PickUpHolding(item, worldObject, attachedObject);
                break;
            
        }

        OnItemsChange();
    }

    public void OnEmptyInteract(){

    }
    public void OnDropInput(){
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


    public void PickUpWeapon(Item item, GameObject worldObject, object attachedObject)
    {

        //Debug.Log("Picking up weapon: " + item.nme);

        GameObject o;

        if (attachedObject is ObjectRack)
        {
            Log("From rack");
            // get rack reference from attached object and add the item to faction items with specified rack
            ObjectRack rack = (ObjectRack)attachedObject;
            rack.camp.faction.RemoveItemOwned(item, 1, rack, false, null);
            o = Utility.InstantiateSameName(item.worldObjectPrefab, worldObject.transform.position, worldObject.transform.rotation);
        }
        else if (attachedObject is EntityItems)
        {
            // picking up from another human
            //EntityItems giverItems = (EntityItems)attachedObject;
            o = worldObject; 
        } 
        else if(attachedObject == null)
        {
            //Log("No attached obj");
            o = worldObject;
        }
        else{
            o = worldObject;
        }

        // handle what to do with currently equipped and unequipped weapons
        if(weaponEquipped_item != null){
            if(weaponUnequipped_item == null){
                ToggleWeaponEquipped();
            }
            else{
                DropEquippedWeapon(attachedObject);
            }
        }

        // finally, equip the weapon
        SetEquippedWeapon(item, o);

    }


    // holding

    public void PickUpHolding(Item item, GameObject worldObject, object attachedObject){

        GameObject o;

        if (attachedObject is ObjectRack)
        {
            // get rack reference from attached object and add the item to faction items with specified rack
            ObjectRack rack = (ObjectRack)attachedObject;
            rack.camp.faction.RemoveItemOwned(item, 1, rack, false, null);
            o = Utility.InstantiateSameName(item.worldObjectPrefab, worldObject.transform.position, worldObject.transform.rotation);
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
        Utility.ToggleObjectPhysics(holding_object, false, false, true, false);

        // update object's ObjectReference
        holding_object.GetComponent<ObjectReference>().SetObjectReference(this);
    }

    public void DropHolding(object targetAttachedObject){
        if(holding_item == null) { return; }

        //Debug.Log("Dropping");

        if (targetAttachedObject is ObjectRack)
        {
            ObjectRack rack = (ObjectRack)targetAttachedObject;
            Enum rackItemType = rack.itemType;

            // if rack is the corresponding type of the item, add it there
            if(rackItemType.Equals(holding_item.type) || rackItemType.Equals(ItemType.Any))
            {
                // get rack reference from attached object and add the item to faction items with specified rack
                //Debug.Log("adding to object rack");
                entityInfo.faction.AddItemOwned(holding_item, 1, rack, transform, 0f);
                GameObject.Destroy(holding_object);
            }
            // if item not not correspond to the rack type
            else
            {
                entityInfo.faction.AddItemOwned(holding_item, 1, null, transform, 0f);
                GameObject.Destroy(holding_object);
            }

            holding_item = null;
            holding_object = null;
        }

        else if (targetAttachedObject == null)
        {
            if(Camp.EntityIsInsideCamp(entityHandle) && holding_item.isRackable){
                //Debug.Log("Adding to rack");
                entityInfo.faction.AddItemOwned(holding_item, 1, null, transform, 0f);
                GameObject.Destroy(holding_object);
                holding_item = null;
                holding_object = null;
            }
            else
            {
                //Debug.Log("Dropping on ground");
                Physics.IgnoreCollision(holding_object.GetComponent<Collider>(), entityPhysics.worldCollider, false);
                holding_object.transform.Find("HoverTrigger").GetComponent<BoxCollider>().enabled = true;
                Utility.ToggleObjectPhysics(holding_object, true, true, true, true);

                // update object's ObjectReference
                holding_object.GetComponent<ObjectReference>().SetObjectReference(this);

                holding_item = null;
                holding_object = null;
            }
        }
        else
        {
            // todo: case human
        }

    }

    public void ConsumeHolding(Item item)
    {
        entityStats.AddStatsModifier(holding_item.baseStats);
        GameObject.Destroy(holding_object);
        holding_item = null;
        holding_object = null;
    }


    // public void OnHoldingUse(){

    //     if(holding_item != null){

    //         switch (holding_item.type) {
    //             case Item.ItemType.Food :
    //                 // todo: eating animation
    //                 ConsumeHolding(holding_item);
    //                 break;
                
    //             case Item.ItemType.Clothing :
    //                 // todo: clothing animation
    //                 Item i = holding_item;
    //                 EquipClothing(holding_item);
    //                 ConsumeHolding(i);
    //                 break;

    //             default:
    //                 break;


    //         }


            
    //     }


    // }



    // weapon

    public void DropEquippedWeapon(object targetAttachedObject){
        
        // if dropping onto an object rack
        if (targetAttachedObject is ObjectRack)
        {
            // get rack reference from attached object and add the item to faction items with specified rack
            ObjectRack rack = (ObjectRack)targetAttachedObject;
            entityInfo.faction.AddItemOwned(weaponEquipped_item, 1, rack, transform, 0f);
            GameObject.Destroy(weaponEquipped_object);
            weaponEquipped_item = null;
            weaponEquipped_object = null;
        }

        // if dropping onto another entity
        else if(targetAttachedObject is EntityItems)
        {
            EntityItems takerItems = (EntityItems)targetAttachedObject;
            Item itemToDrop = weaponEquipped_item;
            GameObject worldObjectToDrop = weaponEquipped_object;
            weaponEquipped_item = null;
            weaponEquipped_object = null;
            takerItems.PickUpWeapon(itemToDrop, worldObjectToDrop, this);
            // if is player and if no equipped weapon (weapon from other entity may have been exchanged), give one from camp
            if(isLocalPlayer)
            {
                if(weaponEquipped_item == null)
                {
                    entityInfo.faction.RemoveItemOwned(itemToDrop, 1, null, true, this);
                }
            }
        }
        else if (targetAttachedObject == null)
        {
            if(Camp.EntityIsInsideCamp(entityHandle)){
                entityInfo.faction.AddItemOwned(weaponEquipped_item, 1,null, transform, 0f);
                GameObject.Destroy(weaponEquipped_object);
                weaponEquipped_item = null;
                weaponEquipped_object = null;
            }
            else
            {
                // Debug.Log("Dropping equipped weapon on ground");
                Physics.IgnoreCollision(weaponEquipped_object.transform.Find("HitZone").GetComponent<Collider>(), entityPhysics.worldCollider, false);
                weaponEquipped_object.transform.Find("HoverTrigger").GetComponent<BoxCollider>().enabled = true;
                Utility.ToggleObjectPhysics(weaponEquipped_object, true, true, true, true);

                // update object's ObjectReference
                weaponEquipped_object.GetComponent<ObjectReference>().SetObjectReference(this);

                weaponEquipped_item = null;
                weaponEquipped_object = null;
            }
        }

        
    }

    public void SetUnequippedWeapon(Item item, GameObject worldObject){

        weaponUnequipped_item = item;
        weaponUnequipped_object = worldObject;

        // toggle physics
        Utility.ToggleObjectPhysics(weaponEquipped_object, false, false, false, false);

        // remove hit detection owner
        weaponUnequipped_object.transform.Find("HitZone").GetComponent<AttackCollisionDetector>().RemoveOwner();
    }
    public void SetEquippedWeapon(Item item, GameObject worldObject)
    {

        //Log("Setting equipped weapon");
        //Log("Weapon name: " + worldObject.name);

        weaponEquipped_item = item;
        weaponEquipped_object = worldObject;

        // add stats
        entityStats.AddStatsModifier(item.wielderStatsModifier);

        // turn off physics
        Utility.ToggleObjectPhysics(weaponEquipped_object, false, false, false, false);

        // update object's ObjectReference
        weaponEquipped_object.GetComponent<ObjectReference>().SetObjectReference(this);

        // set weapon hit detection owner
        AttackCollisionDetector acd = weaponEquipped_object.transform.Find("HitZone").GetComponent<AttackCollisionDetector>();
        acd.SetOwner(entityHandle);
        acd.RemoveFixedJoint();
        acd.SetProjectile(null);
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
            Utility.ToggleObjectPhysics(weaponEquipped_object, false, false, false, false);
            entityStats.AddStatsModifier(weaponEquipped_item.wielderStatsModifier);
            weaponEquipped_object.transform.Find("HitZone").GetComponent<AttackCollisionDetector>().SetOwner(entityHandle);
        }

        if (weaponUnequipped_item != null)
        {
            Utility.ToggleObjectPhysics(weaponUnequipped_object, false, false, false, false);
            entityStats.RemoveStatsModifier(weaponUnequipped_item.wielderStatsModifier);
            if (weaponUnequipped_object != null)
            {
                weaponUnequipped_object.transform.Find("HitZone").GetComponent<AttackCollisionDetector>().RemoveOwner();
            }
        }
        OnItemsChange();
        
    }


    public void ToggleWeaponRanged()
    {
        SetWeaponRangedMode(!rangedMode);
    }
    public void SetWeaponRangedMode(bool rangedStatus){
        if(rangedStatus != rangedMode)
        {
            rangedMode = rangedStatus;
            entityPhysics.UpdateWeaponPoleTarget();
            Debug.Log("Ranged mode: " + rangedMode);

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

            entityInfo.faction.AddItemOwned(clothing, 1, null, transform, 0f);

            // unequip clothing on model
            meshParentT.Find(clothing.nme).gameObject.SetActive(false);

            // remove stats
            entityStats.RemoveStatsModifier(clothing.wielderStatsModifier);

            clothing = null;
        }
    }


    // inventory

    public void AddToInventory(Item item, GameObject worldObject, float delay)
    {
        inventory.AddItem(item, 1);
        StartCoroutine(_MoveObject());

        IEnumerator _MoveObject()
        {

            yield return new WaitForSecondsRealtime(delay);

            // move object to player location before destroying
            Utility.ToggleObjectPhysics(worldObject, false, false, false, false);
            while (Vector3.Distance(worldObject.transform.position, transform.position) > .5f)
            {
                worldObject.transform.position = Vector3.Lerp(worldObject.transform.position, transform.position, ObjectRack.OBJECT_MOVEMENT_ANIMATION_SPEED * Time.deltaTime);
                worldObject.transform.Rotate(Vector3.right * 10f);
                yield return null;
            }
            GameObject.Destroy(worldObject);
        }
    }


    public void ExchangeItemsWithEntity(EntityItems otherEntityItems)
    {

        Debug.Log("EXCHANGING ITEMS");

        bool hasWeaponEquipped_thisEntity = entityItems.weaponEquipped_item != null;
        bool hasWeaponUnequipped_thisEntity = entityItems.weaponUnequipped_item != null;
        bool hasWeaponEquipped_otherEntity = otherEntityItems.weaponEquipped_item != null;
        bool hasWeaponUnequipped_otherEntity = otherEntityItems.weaponUnequipped_item != null;
        bool hasHolding_thisEntity = entityItems.holding_item != null;
        bool hasHolding_otherEntity = otherEntityItems.holding_item != null;

        bool exchangeOccurred = false;
        if (hasWeaponEquipped_thisEntity)
        {
            DropEquippedWeapon(otherEntityItems);
            exchangeOccurred = true;
        }
        else
        {
            if(hasHolding_thisEntity)
            {
                DropHolding(otherEntityItems);
                exchangeOccurred = true;
            }
            else
            {
                if(hasWeaponEquipped_otherEntity)
                {
                    otherEntityItems.DropEquippedWeapon(this);
                    exchangeOccurred = true;
                }
                else if(hasWeaponUnequipped_otherEntity)
                {
                    otherEntityItems.ToggleWeaponEquipped();
                    otherEntityItems.DropEquippedWeapon(this);
                    exchangeOccurred = true;
                }
                else if (hasHolding_otherEntity)
                {
                    otherEntityItems.DropHolding(this);
                    exchangeOccurred = true;
                }
                else
                {
                    // if neither entity has anything, do nothing
                }

            }
        }

        if(exchangeOccurred)
        {
            OnItemsChange();
            otherEntityItems.OnItemsChange();
        }
        
    }


    public void OnItemsChange()
    {
        entityPhysics.UpdateIKForCarryingItems();
        if(weaponEquipped_object != null){
            //Utility.IgnorePhysicsCollisions(transform, weaponEquipped_object.transform);
            Utility.IgnorePhysicsCollisions(weaponEquipped_object.transform, entityInfo.faction.memberHandles.Where(handle => handle != null).Select(handle => handle.transform).ToArray());
            weaponEquipped_object.transform.Find("HoverTrigger").GetComponent<BoxCollider>().enabled = false;
        }
        if(weaponUnequipped_object != null){
             Utility.IgnorePhysicsCollisions(weaponUnequipped_object.transform, weaponUnequipped_object.transform);
            weaponUnequipped_object.transform.Find("HoverTrigger").GetComponent<BoxCollider>().enabled = false;
        }
        if(holding_object != null){
            Physics.IgnoreCollision(holding_object.GetComponent<Collider>(), entityPhysics.worldCollider, true);
            holding_object.transform.Find("HoverTrigger").GetComponent<BoxCollider>().enabled = false;
        }

        if(isLocalPlayer){
            UpdateCameraOffset();
        }
    }

    void UpdateCameraOffset(){
        if(weaponEquipped_item != null){
            CameraController.current.SetTargetOffset(CameraController.current.defaultCameraOffset);
        }
        else{
            CameraController.current.SetTargetOffset(CameraController.current.defaultCameraOffset);
        }
    }


    public void SetUpdateWeaponOrientation(bool value)
    {
        updateWeaponEquippedOrientation = value;
    }

    void UpdateItemOrientations()
    {

        // handle holding orientation
        if (holding_item != null)
        {
            orientation_holding.position = basePosition_holding.position;
            orientation_holding.rotation = basePosition_holding.rotation;
            Vector3 currentPos = holding_object.transform.position;
            Quaternion currentRot = holding_object.transform.rotation;
            holding_object.transform.position = Vector3.Lerp(currentPos, orientation_holding.position, FOLLOW_SPEED_HOLDING_TRANSLATION * Time.deltaTime);
            holding_object.transform.rotation = Quaternion.Slerp(currentRot, orientation_holding.rotation, FOLLOW_SPEED_HOLDING_TRANSLATION * Time.deltaTime);
        }

        // handle equipped weapon orientation
        if (itemOrientationUpdateEnabled)
        {
            if (updateWeaponEquippedOrientation)
            {
                if (weaponEquipped_object != null)
                {
                    Transform orientation_weaponEquipped;
                    Transform basePosition_weaponEquipped;
                    if (weaponEquipped_item.holdStyle.Equals(Item.ItemHoldStyle.Spear))
                    {
                        orientation_weaponEquipped = orientation_weaponEquipped_spear;
                        basePosition_weaponEquipped = basePosition_weaponEquipped_spear;
                    }
                    else if (weaponEquipped_item.holdStyle.Equals(Item.ItemHoldStyle.Axe))
                    {
                        orientation_weaponEquipped = orientation_weaponEquipped_axe;
                        basePosition_weaponEquipped = basePosition_weaponEquipped_axe;
                    }
                    else
                    {
                        orientation_weaponEquipped = basePosition_weaponEquipped = null;
                    }

                    orientation_weaponEquipped.position = basePosition_weaponEquipped.position;
                    orientation_weaponEquipped.rotation = basePosition_weaponEquipped.rotation;
                    Vector3 targetPos = orientation_weaponEquipped.position;
                    Quaternion targetRot = orientation_weaponEquipped.rotation;
                    Vector3 currentPos = weaponEquipped_object.transform.position;
                    Quaternion currentRot = weaponEquipped_object.transform.rotation;
                    weaponEquipped_object.transform.position = Vector3.Lerp(currentPos, targetPos, (FOLLOW_SPEED_WEAPON_TRANSLATION * Mathf.InverseLerp(0f, .8f, Vector3.Distance(currentPos, targetPos)) * Time.deltaTime));
                    weaponEquipped_object.transform.rotation = Quaternion.Slerp(currentRot, targetRot, FOLLOW_SPEED_WEAPON_ROTATION * Time.deltaTime);
                }
            }
        }
        
        // handle unequipped weapon orientation
        if (weaponUnequipped_object != null)
        {
            weaponUnequipped_object.transform.position = Vector3.Lerp(weaponUnequipped_object.transform.position, orientation_weaponUnequipped.position, float.MaxValue);
            weaponUnequipped_object.transform.rotation = Quaternion.Slerp(weaponUnequipped_object.transform.rotation, orientation_weaponUnequipped.rotation, float.MaxValue);
        }
    }

    public void OnCampBorderEnter(){
        //Debug.Log("OnCampBorderCross()");
        entityInfo.faction.AddItemsOwned(inventory, null, transform, 0f);
        inventory = new ItemCollection();
        // todo: command tribe memebrs to line up to orientations
    }

    public void OnCampBorderExit(){
        //Debug.Log("OnCampBorderCross()");
        entityInfo.faction.AddItemsOwned(inventory, null, transform, 0f);
        inventory = new ItemCollection();
        // todo: command tribe memebrs to follow
    }


    // ---

    void Update(){

        UpdateItemOrientations();

    
    }



    public void ToggleItemOrientationUpdate(bool value){
        itemOrientationUpdateEnabled = value;
        itemOrientationAnimator.SetLayerWeight(1, Convert.ToInt32(value));
    }

}