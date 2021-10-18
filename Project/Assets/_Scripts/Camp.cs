using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Camp : MonoBehaviour
{

    public static float BASE_CAMP_RADIUS = 8f;
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


    public List<Transform> tribeMemberStandPositions;


    // client method to place a Camp
    public static void TryPlaceCamp(Faction faction, Transform originT){
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
    public static Camp PlaceCamp(Faction faction, Transform originT){        
        Camp camp = GameManager.current.gameObject.AddComponent<Camp>();
        faction.camp = camp;
        camp.faction = faction;
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
        return camp;


    }

    public void PlaceCampComponents(Transform originT){

        StartCoroutine(_PlaceCampComponents());

        IEnumerator _PlaceCampComponents(){
            PlaceBorderSphere();
            PlaceBonfire();
            yield return new WaitForSecondsRealtime(CAMP_COMPONENT_PLACING_TIME_GAP);
            PlaceWorkbench();
            yield return new WaitForSecondsRealtime(CAMP_COMPONENT_PLACING_TIME_GAP);
            AddItemsToCamp(faction.ownedItems, originT);
            yield return new WaitForSecondsRealtime(CAMP_COMPONENT_PLACING_TIME_GAP);
            UpdateTentCount();
            yield return new WaitForSecondsRealtime(CAMP_COMPONENT_PLACING_TIME_GAP);
            ClearFeaturesFromCampRadius();
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

        layout = Instantiate(CampResources.PREFAB_CAMPLAYOUT, position, rotation);
        foreach(Transform orientation in layout.transform){
            //Debug.Log("AdjustCampLayout(): adjusting orientation: " + orientation.name);
            Vector3 pos = orientation.position;
            pos.y = ChunkGenerator.ElevationAmplitude;
            RaycastHit hit;
            if(Physics.Raycast(pos, Vector3.down, out hit, ChunkGenerator.ElevationAmplitude, CampResources.LayerMask_Terrain)){
                orientation.position = hit.point + Vector3.up * .15f;
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
        Collider[] featureCollidersInsideCamp = Physics.OverlapSphere(origin, radius, LayerMask.GetMask("Feature"));
        foreach(Collider collider in featureCollidersInsideCamp)
        {
            if(collider != null)
            {
                EntityHitDetection ehd = collider.gameObject.GetComponent<EntityHitDetection>();
                if(ehd != null && false)
                {
                    ehd.OnHit(faction.memberHandles[0], collider.transform.position, null, true);
                }
                else
                {
                    Destroy(collider.gameObject);
                }
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
        Bonfire bonfire = GameManager.current.gameObject.AddComponent<Bonfire>();
        bonfire.SetBonfire(this, faction.ownedItems.GetItemCount(Item.LogFir) > 1f, 1f, 1f);
        Transform targetOrientation = GetCampComponentOrientation(ComponentType.Bonfire);
        bonfire.worldObject.transform.position = targetOrientation.position;
        bonfire.worldObject.transform.rotation = targetOrientation.rotation;
        this.bonfire = bonfire;

        SetOrigin(targetOrientation.position);
    }

    public void PlaceWorkbench(){
        Workbench workbench = GameManager.current.gameObject.AddComponent<Workbench>();
        workbench.SetWorkbench(this);
        Transform targetOrientation = GetCampComponentOrientation(ComponentType.Workbench);
        workbench.worldObject.transform.position = targetOrientation.position;
        workbench.worldObject.transform.rotation = targetOrientation.rotation;
        this.workbench = workbench;
    }


    public ObjectRack PlaceObjectRack(Enum itemType, float delay){


        ObjectRack objectRack = GameManager.current.gameObject.AddComponent<ObjectRack>();
        objectRack.SetObjectRack(this, itemType);
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
                RemoveTent();
            }
        }
    }
    public void PlaceTent(){
        Tent tent = GameManager.current.gameObject.AddComponent<Tent>();
        tent.SetTent(this);
        Transform targetOrientation = GetCampComponentOrientation(ComponentType.Tent);
        tent.worldObject.transform.position = targetOrientation.position;
        tent.worldObject.transform.rotation = targetOrientation.rotation;
        this.tents.Add(tent);
    }
    public void RemoveTent(){
        Tent tent = tents[tents.Count - 1];
        tents.Remove(tent);
        tent.DeleteSelf();
    }


    public void AddItemsToCamp(ItemCollection itemsToAdd, Transform originT){
        Item item;
        int councountToAdd;
        int zeroRacksRef = 0;
        foreach (KeyValuePair<Item, int> kvp in itemsToAdd.items)
        {
            item = kvp.Key;
            councountToAdd = kvp.Value;
            AddObjectsAnyRack(item, ref councountToAdd, originT, ref zeroRacksRef);
        }

    }
        

    public void RemoveItemsFromCamp(ItemCollection itemsToRemove){
        Item item;
        int countToRemove;
        foreach(KeyValuePair<Item, int> kvp in itemsToRemove.items){
            item = kvp.Key;
            countToRemove = kvp.Value;
            RemoveObjectsAnyRack(item, ref countToRemove);
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


    public void RemoveObjectsAnyRack(Item item, ref int count){
        List<ObjectRack> rackList = GetRackListForItemType(item.type);
        for(int i = rackList.Count - 1; i >= 0; --i){
            if(count > 0){
                rackList[i].RemoveObjects(item, ref count);
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
        GameObject foodObject = casterItems.holding_object;

        if (foodItem == null) {
            // todo: tell how you can make this happen tooltip
            return;
        }
        
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
        StartCoroutine(casterHandle.entityCommandServer.SpawnNpcWhenReady(casterHandle));

        // todo: play particles for new tribe member birth
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


