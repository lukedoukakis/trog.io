﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Camp : MonoBehaviour
{

    public static float BASE_CAMP_RADIUS = 6.3f;
    public static float CAMP_COMPONENT_PLACING_TIME_GAP = .1f;

    public enum ComponentType{
        Bonfire,
        Workbench, 
        Tent,
        Rack_Food, 
        Rack_Weapons, 
        Rack_Clothing,
        Rack_Wood,
        Rack_Bone,
        Rack_Stone
    }


    // --
    public Transform rootT;
    public Faction faction;
    public Vector3 origin;
    public float radius;
    public GameObject layout;
    GameObject borderSphere;


    // camp components
    public Bonfire bonfire;
    public Workbench workbench;
    public List<Tent> tents;
    public List<ObjectRack> racks_food;
    public List<ObjectRack> racks_weapons;
    public List<ObjectRack> racks_clothing;
    public List<ObjectRack> racks_wood;
    public List<ObjectRack> racks_bone;
    public List<ObjectRack> racks_stone;
    public List<List<ObjectRack>> racks_all;


    public List<Transform> tribeMemberStandPositions;




    // client method to place a Camp
    public static void TryPlaceCamp(Faction faction, Transform originT)
    {
        if(CanPlaceCamp(originT.position)){
            PlaceCamp(faction, originT);
        }
        else{
            Debug.Log("Can't place camp - doesn't fit requirements");
        }
    }
    public static bool CanPlaceCamp(Vector3 position){
        // determine if flat enough
        return true;
    }
    public static Camp PlaceCamp(Faction faction, Transform originT)
    {        

        Camp camp = GameManager.instance.gameObject.AddComponent<Camp>();
        faction.camp = camp;
        camp.faction = faction;
        camp.rootT = new GameObject().transform;
        camp.racks_food = new List<ObjectRack>();
        camp.racks_weapons = new List<ObjectRack>();
        camp.racks_clothing = new List<ObjectRack>();
        camp.racks_wood = new List<ObjectRack>();
        camp.racks_bone = new List<ObjectRack>();
        camp.racks_stone = new List<ObjectRack>();
        camp.tents = new List<Tent>();
        Vector3 campPlacementPos = originT.position + originT.forward * 2f;
        camp.SetOrigin(campPlacementPos);
        camp.UpdateRadius(faction.memberHandles.Count);
        camp.SetCampLayout(campPlacementPos, originT.rotation);
        camp.PlaceCampComponents(originT);
        camp.SetTribeMemberStandingPositions();
        camp.racks_all = new List<List<ObjectRack>>(){ camp.racks_food, camp.racks_weapons, camp.racks_clothing, camp.racks_wood, camp.racks_bone, camp.racks_stone, new List<ObjectRack>(){camp.workbench} };

        // todo: update all memnbers in camp status from checking distance to origin
        camp.faction.leaderHandle.entityPhysics.isInsideCamp = true;
        camp.faction.UpdateLeaderCampStatus();

        // call the shader controller to update according to new origin position and radius
        ShaderController.instance.UpdateGrassSettings(camp);

        return camp;


    }

    public void PlaceCampComponents(Transform originT){

        StartCoroutine(_PlaceCampComponents());

        IEnumerator _PlaceCampComponents()
        {
            //ClearFeaturesFromCampRadius();
            PlaceBorderSphere();
            PlaceBonfire();
            yield return new WaitForSecondsRealtime(CAMP_COMPONENT_PLACING_TIME_GAP);
            PlaceWorkbench();
            yield return new WaitForSecondsRealtime(CAMP_COMPONENT_PLACING_TIME_GAP);
            AddItemsToCamp(faction.ownedItems, originT);
            yield return new WaitForSecondsRealtime(CAMP_COMPONENT_PLACING_TIME_GAP);
            //UpdateTentCount();
            yield return new WaitForSecondsRealtime(CAMP_COMPONENT_PLACING_TIME_GAP);

        }
    }

    public void SetTribeMemberStandingPositions()
    {
        tribeMemberStandPositions = layout.transform.GetComponentsInChildren<Transform>().Where(t => t.name.StartsWith("OrientationTribeMemberStandPosition")).ToList();
    }
    public Transform GetOpenTribeMemberStandPosition()
    {
        foreach(Transform standPosition in tribeMemberStandPositions)
        {
            if(standPosition.childCount < 1)
            {
                return standPosition;
            }
        }
        Debug.Log("No open standing position");
        return null;
    }


    public void SetOrigin(Vector3 position){
        this.origin = position;
    }

    public void UpdateRadius(int population){

        this.radius = BASE_CAMP_RADIUS;
        
        if(borderSphere != null)
        {
            UpdateBorderSphere();
        }
    
        //Debug.Log("Camp radius: " + radius);
    }

    // place and adjust camp layout for component placement
    public void SetCampLayout(Vector3 position, Quaternion rotation){

        layout = Instantiate(CampResources.PREFAB_CAMPLAYOUT, position, rotation, rootT);
        foreach(Transform orientation in layout.transform){
            //Debug.Log("AdjustCampLayout(): adjusting orientation: " + orientation.name);
            Vector3 pos = orientation.position;
            pos.y = ChunkGenerator.ElevationAmplitude;
            RaycastHit hit;
            if(Physics.Raycast(pos, Vector3.down, out hit, ChunkGenerator.ElevationAmplitude, LayerMaskController.TERRAIN)){
                orientation.position = hit.point + Vector3.up * 0f;
            }
            else{
                orientation.position = Vector3.one * float.MaxValue;
            }
    

            Vector3 toCenterEulers = Quaternion.LookRotation(GetCampComponentOrientation(ComponentType.Bonfire).position - orientation.position, Vector3.up).eulerAngles;
            // Vector3 normalEulers = Quaternion.FromToRotation(Vector3.up, hit.normal).eulerAngles;
            // Vector3 orientationEulers = orientation.rotation.eulerAngles;
            // orientationEulers.z = normalEulers.z;
            // orientationEulers.x = normalEulers.x;
            // orientationEulers.y = toCenterEulers.y;
            // orientation.rotation = Quaternion.Euler(orientationEulers);

            orientation.rotation = Quaternion.Euler(toCenterEulers);


        }
    }

    public void ClearFeaturesFromCampRadius()
    {
        Collider[] featureCollidersInsideCamp = Physics.OverlapSphere(origin, radius, LayerMaskController.CLEAR_ON_CAMP_PLACEMENT, QueryTriggerInteraction.Collide);
        foreach(Collider collider in featureCollidersInsideCamp)
        {
            if(collider != null)
            {
                // ItemHitDetection ihd = collider.gameObject.GetComponent<ItemHitDetection>();
                // if(ihd != null)
                // {
                //     ihd.OnHit(faction.memberHandles[0], collider.transform.position, null);
                // }
                // else
                // {
                //     Destroy(collider.gameObject);
                // }

                Destroy(collider.gameObject);
            }
        }
    }

    public Transform GetCampComponentOrientation(Enum componentType){

        string search;

        switch (componentType) {
            case ComponentType.Bonfire :
                search = "OrientationBonfire";
                break;
            case ComponentType.Workbench :
                search = "OrientationWorkbench";
                break;
            case ComponentType.Rack_Food :
                search = "OrientationFoodRack" + racks_food.Count;
                break;
            case ComponentType.Rack_Weapons :
                search = "OrientationWeaponsRack" + racks_weapons.Count;
                break;
            case ComponentType.Rack_Clothing :
                search = "OrientationClothingRack" + racks_clothing.Count;
                break;
            case ComponentType.Rack_Wood :
                search = "OrientationWoodRack" + racks_wood.Count;
                break;
            case ComponentType.Rack_Bone :
                search = "OrientationBoneRack" + racks_bone.Count;
                break;
            case ComponentType.Rack_Stone :
                search = "OrientationStoneRack" + racks_stone.Count;
                break;
            case ComponentType.Tent :
                search = "OrientationTent" + tents.Count;
                break;
            default:
                search = "OrientationBonfire";
                Debug.Log("Getting component position for unsupported component type");
                break;
        }

        //Debug.Log("GetCampComponentOrientation(): orientation name: " + search);
        return FindOrientationInCampLayout(search);

    }

    public Transform FindOrientationInCampLayout(string search){
        return layout.transform.Find(search);
    }


    public void PlaceBorderSphere(){
        UpdateBorderSphere();
    }

    public void UpdateBorderSphere()
    {
        if(borderSphere == null)
        {
            Transform targetOrientation = GetCampComponentOrientation(ComponentType.Bonfire);
            Rigidbody rb;
            ObjectReference objectReference;

            borderSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            borderSphere.transform.SetParent(rootT);
            borderSphere.layer = LayerMask.NameToLayer("CampBorder");
            borderSphere.transform.position = targetOrientation.position;
            borderSphere.transform.localScale = Vector3.one * radius * 2f;
            Destroy(borderSphere.GetComponent<SphereCollider>());
            SphereCollider collider = borderSphere.AddComponent<SphereCollider>();
            collider.isTrigger = true;
            rb = borderSphere.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            objectReference = borderSphere.AddComponent<ObjectReference>();
            objectReference.SetObjectReference(this);
            Destroy(borderSphere.GetComponent<MeshRenderer>());
        }
    }

    public void PlaceBonfire(){
        Bonfire bonfire = (Bonfire)AddCampComponent(typeof(Bonfire));
        bonfire.SetBonfire(this, faction.ownedItems.GetItemCount(Item.LogFir) > 1f, 1f, 1f);
        Transform targetOrientation = GetCampComponentOrientation(ComponentType.Bonfire);
        bonfire.worldObject.transform.position = targetOrientation.position;
        bonfire.worldObject.transform.rotation = targetOrientation.rotation;
        this.bonfire = bonfire;

        SetOrigin(targetOrientation.position);
    }

    public void PlaceWorkbench()
    {
        Workbench workbench = (Workbench)AddCampComponent(typeof(Workbench));
        workbench.SetCampComponent(this);
        workbench.SetWorkbench();
        Transform targetOrientation = GetCampComponentOrientation(ComponentType.Workbench);
        workbench.worldObject.transform.position = targetOrientation.position;
        workbench.worldObject.transform.rotation = targetOrientation.rotation;
        this.workbench = workbench;
    }

    public CampComponent AddCampComponent(Type type)
    {
        CampComponent campComponent = (CampComponent)rootT.gameObject.AddComponent(type);
        campComponent.SetCampComponent(this);

        return campComponent;
    }


    public ObjectRack PlaceObjectRack(Enum itemType, float delay)
    {

        ObjectRack objectRack = (ObjectRack)AddCampComponent(typeof(ObjectRack));
        objectRack.SetCampComponent(this);
        objectRack.SetObjectRack(itemType);
        List<ObjectRack> rackList = GetRackListForItemType(itemType);
        Enum componentType;
        switch (itemType)
        {
            case ItemType.Food:
                componentType = ComponentType.Rack_Food;
                break;
            case ItemType.Weapon:
                componentType = ComponentType.Rack_Weapons;
                break;
            case ItemType.Clothing:
                componentType = ComponentType.Rack_Clothing;
                break;
            case ItemType.Wood:
                componentType = ComponentType.Rack_Wood;
                break;
            case ItemType.Bone:
                componentType = ComponentType.Rack_Bone;
                break;
            case ItemType.Stone:
                componentType = ComponentType.Rack_Stone;
                break;
            default:
                componentType = ComponentType.Rack_Food;
                Debug.Log("Placing item rack for unsupported item type: " + itemType);
                break;
        }
        Transform targetOrientation = GetCampComponentOrientation(componentType);
        rackList.Add(objectRack);

        StartCoroutine(_SetObjectRackOrientationAfterDelay());

        return objectRack;

        IEnumerator _SetObjectRackOrientationAfterDelay()
        {
            // delay for time
            System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
            timer.Start();
            while (timer.ElapsedMilliseconds / 1000f < delay)
            {
                yield return null;
            }
            timer.Stop();

            // place the rack accordingly and play entry animation
            objectRack.worldObject.transform.position = targetOrientation.position;
            objectRack.worldObject.transform.rotation = targetOrientation.rotation;
            objectRack.PlayEntryAnimation();

        }
  
    }

    public void UpdateTentCount(){
        int properTentCount = faction.memberHandles.Count / 2;
        int currentTentCount = tents.Count;
        int tentDeficit = properTentCount - currentTentCount;

        if(tentDeficit > 0){
            for(int i = 0; i < tentDeficit; ++i){
                PlaceTent();
            }
        }
        else if(tentDeficit < 0){
            for(int i = 0; i < tentDeficit * -1; ++i){
                //RemoveTent();
            }
        }
    }
    public void PlaceTent(){
        Tent tent = (Tent)AddCampComponent(typeof(Tent));
        tent.SetCampComponent(this);
        Transform targetOrientation = GetCampComponentOrientation(ComponentType.Tent);
        tent.worldObject.transform.position = targetOrientation.position;
        tent.worldObject.transform.rotation = targetOrientation.rotation;
        this.tents.Add(tent);
    }


    public void AddItemsToCamp(ItemCollection itemsToAdd, Transform originT){
        Item item;
        int countToAdd;
        int zeroRacksRef = 0;
        foreach (KeyValuePair<Item, int> kvp in itemsToAdd.items)
        {
            item = kvp.Key;
            countToAdd = kvp.Value;
            AddObjectsAnyRack(item, ref countToAdd, originT, ref zeroRacksRef);
        }

    }
        

    public void RemoveItemsFromCamp(ItemCollection itemsToRemove, bool moveToAnotherRack, object destination)
    {

        //Debug.Log("RemoveItemsFromCamp()");

        Item item;
        int countToRemove;
        foreach(KeyValuePair<Item, int> kvp in itemsToRemove.items){
            item = kvp.Key;
            countToRemove = kvp.Value;
            RemoveObjectsAnyRack(item, ref countToRemove, moveToAnotherRack, destination);
        }
    }


    public void AddObjectsAnyRack(Item item, ref int count, Transform originT, ref int newRacksCount){
        List<ObjectRack> rackList = GetRackListForItemType(item.type);
        foreach(ObjectRack rack in rackList){
            if(!rack.IsFull()){
                rack.AddObjects(item, ref count, originT, ref newRacksCount);
                break;
            }
        }

        // if still objects to add, place a new rack
        if(count > 0){
            ++newRacksCount;
            ObjectRack newRack = PlaceObjectRack(item.type, CAMP_COMPONENT_PLACING_TIME_GAP * newRacksCount);
            newRack.AddObjects(item, ref count, originT, ref newRacksCount);
        }
    }   

    public void RemoveObjectsAnyRack(Item item, ref int count, bool moveToAnotherRack, object destination)
    {
        List<ObjectRack> rackList = GetRackListForItemType(item.type).Where(rack => !rack.IsEmpty()).ToList();
        for(int i = rackList.Count - 1; i >= 0; --i){
            if(count > 0){
                rackList[i].RemoveObjects(item, ref count, moveToAnotherRack, destination);
            }
            else{
                break;
            }
        }
    }



    public List<ObjectRack> GetRackListForItemType(Enum itemType){
        List<ObjectRack> rackList;
        rackList = racks_food;
        switch(itemType){
            case ItemType.Food :
                rackList = racks_food;
                break;
            case ItemType.Weapon :
                rackList = racks_weapons;
                break;
            case ItemType.Clothing :
                rackList = racks_clothing;
                break;
            case ItemType.Wood :
                rackList = racks_wood;
                break;
            case ItemType.Bone :
                rackList = racks_bone;
                break;
            case ItemType.Stone :
                rackList = racks_stone;
                break;
            default:
                Debug.Log("Unrecognized item type");
                break;
        }

        return rackList;
    }


    public void CastFoodIntoBonfire(EntityHandle casterHandle)
    {
        EntityItems casterItems = casterHandle.entityItems;
        Item foodItem = casterItems.holding_item;
        if (foodItem == null) {
            // todo: tell how you can make this happen tooltip
            return;
        }
        GameObject foodObject = casterItems.holding_object;
        
        StartCoroutine(_CastFoodIntoBonfire());


        IEnumerator _CastFoodIntoBonfire()
        {

            casterItems.holding_item = null;
            casterItems.holding_object = null;
            casterItems.OnItemsChange();

            Transform foodT = foodObject.transform;
            Vector3 castTargetPosition = bonfire.transform.position;
            float castSpeed = 10000f;
            while(Vector3.Distance(foodT.position, castTargetPosition) > .1f)
            {
                foodT.position = Vector3.Lerp(foodT.position, castTargetPosition, castSpeed * Time.deltaTime);
                yield return null;
            }
            // todo: play particles for food casted into fire
            GameObject.Destroy(foodObject);
            OnFoodCast(casterHandle, foodItem);

        }
    
    }

    void OnFoodCast(EntityHandle casterHandle, Item foodItem)
    {
        //Debug.Log("OnFoodCast()");
        StartCoroutine(ClientCommand.instance.SpawnNpcFollowerWhenReady(casterHandle.entityInfo.faction.leaderHandle, GetOpenTribeMemberStandPosition().position, false));
        if(casterHandle.entityInfo.faction.GetItemCount(foodItem) > 0)
        {
            casterHandle.entityInfo.faction.RemoveItemOwned(foodItem, 1, null, true, casterHandle.entityItems);
        }

    }



    // removes all rack items and adds to (todo: backpack)
    public void PackUp()
    {

    }

    public void Dismantle()
    {

        Debug.Log("Dismantle() start");

        // handle inside camp status (set to outside)
        faction.leaderHandle.entityPhysics.OnCampBorderExit();
        borderSphere.SetActive(false);
        Destroy(borderSphere);

        // handle removal of camp components and packing of items
        CampComponent[] allCampComponents = rootT.GetComponents<CampComponent>();
        foreach(CampComponent cp in allCampComponents)
        {
            // if camp component is an object rack, move all its items to leader's inventory
            if(cp is ObjectRack)
            {
                ObjectRack rack = (ObjectRack)cp;
                rack.EmptyObjects(faction.leaderHandle);
            }

            // destroy and play dismantle animation
            Utility.DestroyInSeconds(cp.worldObject, 5f);
            cp.PlayDismantleAnimation();
        }

        Utility.DestroyInSeconds(rootT.gameObject, 5f);

        Debug.Log("Dismantle() end");
    }

    public static bool EntityIsInsideCamp(EntityHandle handle){

        Camp camp = handle.entityInfo.faction.camp;
        if(camp == null){
            return false;
        }
        else{
            return Vector3.Distance(handle.transform.position, camp.origin) <= camp.radius;
        }
    }


}


