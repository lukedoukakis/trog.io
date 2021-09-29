using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        Rack_Bone
    }


    // --
    public Faction faction;
    public Vector3 origin;
    public float radius;
    public GameObject layout;

    public Bonfire bonfire;
    public Workbench workbench;
    public List<Tent> tents;
    public List<ObjectRack> racks_food;
    public List<ObjectRack> racks_weapons;
    public List<ObjectRack> racks_clothing;
    public List<ObjectRack> racks_wood;
    public List<ObjectRack> racks_bone;






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
        camp.tents = new List<Tent>();
        Vector3 campPlacementPos = originT.position + originT.forward * 2f;
        camp.SetOrigin(campPlacementPos);
        camp.SetRadius(faction.members.Count);
        camp.SetCampLayout(campPlacementPos, originT.rotation);
        camp.PlaceCampComponents(originT);
        return camp;


    }

    public void PlaceCampComponents(Transform originT){

        StartCoroutine(_PlaceCampComponents());

        IEnumerator _PlaceCampComponents(){
            PlaceBonfire();
            yield return new WaitForSecondsRealtime(CAMP_COMPONENT_PLACING_TIME_GAP);
            PlaceWorkbench();
            yield return new WaitForSecondsRealtime(CAMP_COMPONENT_PLACING_TIME_GAP);
            AddItemsToCamp(faction.ownedItems, originT);
            yield return new WaitForSecondsRealtime(CAMP_COMPONENT_PLACING_TIME_GAP);
            UpdateTentCount();
        }
    }


    public void SetOrigin(Vector3 position){
        this.origin = position;
    }

    public void SetRadius(int population){
        // this.radius = BASE_CAMP_RADIUS + (BASE_CAMP_RADIUS * population * 05f);

        this.radius = BASE_CAMP_RADIUS;
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


    public void PlaceBonfire(){
        Bonfire bonfire = GameManager.current.gameObject.AddComponent<Bonfire>();
        bonfire.SetBonfire(this, faction.ownedItems.GetItemCount(Item.LogFir) > 1f, 1f, 1f);
        Transform targetOrientation = GetCampComponentOrientation(ComponentType.Bonfire);
        bonfire.worldObject.transform.position = targetOrientation.position;
        bonfire.worldObject.transform.rotation = targetOrientation.rotation;
        this.bonfire = bonfire;

        // test: set sphere to visualize camp radius
        // GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        // sphere.transform.localScale = Vector3.one * radius * 2f;
        // sphere.transform.position = targetOrientation.position;
        // sphere.GetComponent<SphereCollider>().enabled = false;
        // sphere.GetComponent<MeshRenderer>().sharedMaterial = Testing.instance.transparentMat;
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
            case Item.ItemType.Food:
                componentType = ComponentType.Rack_Food;
                break;
            case Item.ItemType.Weapon:
                componentType = ComponentType.Rack_Weapons;
                break;
            case Item.ItemType.Clothing:
                componentType = ComponentType.Rack_Clothing;
                break;
            case Item.ItemType.Wood:
                componentType = ComponentType.Rack_Wood;
                break;
            case Item.ItemType.Bone:
                componentType = ComponentType.Rack_Bone;
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
        int properTentCount = faction.members.Count / 2;
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
            case Item.ItemType.Food :
                rackList = racks_food;
                break;
            case Item.ItemType.Weapon :
                rackList = racks_weapons;
                break;
            case Item.ItemType.Clothing :
                rackList = racks_clothing;
                break;
            case Item.ItemType.Wood :
                rackList = racks_wood;
                break;
            case Item.ItemType.Bone :
                rackList = racks_bone;
                break;
            default:
                Debug.Log("Unrecognized item type");
                break;
        }

        return rackList;
    }

    public static bool EntityIsInsideCamp(EntityHandle handle){

        Camp camp = handle.entityInfo.faction.camp;
        if(camp == null){
            return false;
        }
        else{
            return Vector3.Distance(handle.transform.position, camp.layout.transform.position) <= camp.radius;
        }
    }


}


