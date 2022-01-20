using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Camp : MonoBehaviour
{

    public static float PLACEMENT_MAXIMUM_TERRAIN_HEIGHT_VARIANCE = 2f;
    public static float BASE_CAMP_RADIUS = 8f;
    public static float CAMP_COMPONENT_PLACING_TIME_GAP = .1f;

    public enum ComponentType{
        Bonfire,
        Workbench, 
        Tent,
        Rack_Food, 
        Rack_Weapons, 
        Rack_Pelt,
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
    public bool placingCampComponents;


    // camp components
    public Bonfire bonfire;
    public Workbench workbench;
    public List<Tent> tents;
    public List<ObjectRack> racks_food;
    public List<ObjectRack> racks_weapons;
    public List<ObjectRack> racks_pelt;
    public List<ObjectRack> racks_wood;
    public List<ObjectRack> racks_bone;
    public List<ObjectRack> racks_stone;
    public List<List<ObjectRack>> racks_all;



    public List<Transform> tribeMemberStandPositions;


    // client method to place a Camp
    public static void TryPlaceCamp(Faction faction, Transform originT)
    {
        bool isPlayerCamp = ReferenceEquals(faction.leaderHandle, GameManager.instance.localPlayerHandle);
        if(CanPlaceCamp(faction, originT.position, isPlayerCamp))
        {
            //faction.leaderHandle.entityItems.EmptyInventory();
            PlaceCamp(faction, originT, isPlayerCamp);
        }
    }
    public static bool CanPlaceCamp(Faction faction, Vector3 position, bool featuresBlockPlacement)
    {

        if (featuresBlockPlacement)
        {
            // check if features in the way
            if (Physics.OverlapSphere(position, BASE_CAMP_RADIUS, LayerMaskController.CLEAR_ON_CAMP_PLACEMENT, QueryTriggerInteraction.Collide).Length > 0)
            {
                Debug.Log("Cannot place camp - features in the way");
                return false;
            }
        }


        // determine if flat enough

        Vector3 placementPos = faction.leaderHandle.transform.position;
        Vector2 placementPosVector2 = new Vector2(placementPos.x, placementPos.z);

        Vector2 placementCoordsInChunk = ChunkGenerator.GetCoordinatesInChunk(placementPos);
        ChunkData cd = ChunkGenerator.GetChunkFromRawPosition(placementPos);
        float[,] heightMap = cd.HeightMap;

        float highestPoint = float.MinValue;
        float lowestPoint = float.MaxValue;
        float height;
        Vector2 sampleVector2 = Vector2.zero;
        for(int z = 0; z < heightMap.GetLength(0); ++z)
        {
            for(int x = 0; x < heightMap.GetLength(1); ++x)
            {
                sampleVector2.x = x;
                sampleVector2.y = z;
                if(Vector2.Distance(sampleVector2, placementCoordsInChunk) <= BASE_CAMP_RADIUS)
                {
                    height = heightMap[x, z];
                    if(height >= lowestPoint)
                    {
                        if(height > highestPoint)
                        {
                            highestPoint = height;
                        }
                    }
                    else
                    {
                        lowestPoint = height;
                    }
                }
            }

        }

        float differenceInMeters = (highestPoint - lowestPoint) / ChunkGenerator.meter;
        bool canPlace = differenceInMeters < PLACEMENT_MAXIMUM_TERRAIN_HEIGHT_VARIANCE;
        if(!canPlace)
        {
            Debug.Log("Cannot place camp - too much terrain height variance");
        }

        return canPlace;

    }
    public static Camp PlaceCamp(Faction faction, Transform originT, bool isPlayerCamp)
    {

        Camp camp = GameManager.instance.gameObject.AddComponent<Camp>();
        faction.camp = camp;
        camp.faction = faction;
        camp.rootT = new GameObject().transform;
        camp.racks_food = new List<ObjectRack>();
        camp.racks_weapons = new List<ObjectRack>();
        camp.racks_pelt = new List<ObjectRack>();
        camp.racks_wood = new List<ObjectRack>();
        camp.racks_bone = new List<ObjectRack>();
        camp.racks_stone = new List<ObjectRack>();
        camp.tents = new List<Tent>();
        if(!isPlayerCamp)
        {
            camp.ClearFeaturesFromCampRadius();
        }
        Vector3 campPlacementPos = originT.position + originT.forward * 2f;
        camp.SetOrigin(campPlacementPos);
        camp.UpdateRadius(faction.memberHandles.Count);
        camp.SetCampLayout(campPlacementPos, originT.rotation);
        camp.PlaceCampComponents(originT);
        camp.SetTribeMemberStandingPositions();
        camp.racks_all = new List<List<ObjectRack>>(){ camp.racks_food, camp.racks_weapons, camp.racks_pelt, camp.racks_wood, camp.racks_bone, camp.racks_stone, new List<ObjectRack>(){camp.workbench} };

        // todo: update all memnbers in camp status from checking distance to origin
        camp.faction.leaderHandle.entityPhysics.isInsideCamp = true;
        camp.faction.UpdateLeaderCampStatus();



        return camp;


    }

    public void PlaceCampComponents(Transform originT){

        StartCoroutine(_PlaceCampComponents());

        IEnumerator _PlaceCampComponents()
        {
            placingCampComponents = true;
    
            PlaceBorderSphere();
            PlaceBonfire();
            yield return new WaitForSecondsRealtime(CAMP_COMPONENT_PLACING_TIME_GAP);
            PlaceWorkbench();
            yield return new WaitForSecondsRealtime(CAMP_COMPONENT_PLACING_TIME_GAP * 3f);

            // separate physical and overflow items and add them to camp
            Item item;
            int count;
            ItemCollection physicalItemsToPlace = new ItemCollection();
            ItemCollection overflowItemsToPlace = new ItemCollection();
            foreach(KeyValuePair<Item, int> kvp in faction.ownedItems.items)
            {
                item = kvp.Key;
                count = kvp.Value;
                //Debug.Log("countOwned for " + item.nme + ": " + countOwned);
                int campTotalCapacity = Camp.GetItemPhysicalCapacity(item);
                int maximumPhysicalCanAdd = campTotalCapacity;
                int countToAddPhysically = Mathf.Min(count, maximumPhysicalCanAdd);
                int countToAddOverflow = count - countToAddPhysically;
                physicalItemsToPlace.AddItem(item, countToAddPhysically);
                overflowItemsToPlace.AddItem(item, countToAddOverflow);

            }
            AddItemsToCamp(physicalItemsToPlace, originT, true);
            AddItemsToCamp(overflowItemsToPlace, originT, false);

            yield return new WaitForSecondsRealtime(CAMP_COMPONENT_PLACING_TIME_GAP);
            // for(int i = 0; i < faction.GetItemCount(Item.TentBearPelt) + faction.GetItemCount(Item.TentDeerPelt); ++i)
            // {
            //     PlaceTent();
            // }


            placingCampComponents = false;

        }
    }

    public void SetTribeMemberStandingPositions()
    {
        tribeMemberStandPositions = layout.transform.GetComponentsInChildren<Transform>().Where(t => t.name.StartsWith("OrientationTribeMemberStandPosition")).ToList();
    }
    public GameObject GetOpenTribeMemberStandPosition()
    {
        foreach(Transform standPosition in Utility.Shuffle(tribeMemberStandPositions))
        {
            if(standPosition.childCount < 1)
            {
                return standPosition.gameObject;
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
            case ComponentType.Rack_Pelt :
                search = "OrientationPeltRack" + racks_pelt.Count;
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

    public CampComponent AddCampComponentItem(Item item)
    {
        if(item == Item.TentBearPelt || item == Item.TentDeerPelt)
        {
            return PlaceTent();
        }


        return null;
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
            case ItemType.Pelt:
                componentType = ComponentType.Rack_Pelt;
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

    public Tent PlaceTent()
    {
        Tent tent = (Tent)AddCampComponent(typeof(Tent));
        tent.SetCampComponent(this);
        Transform targetOrientation = GetCampComponentOrientation(ComponentType.Tent);
        tent.worldObject.transform.position = targetOrientation.position;
        tent.worldObject.transform.rotation = targetOrientation.rotation;
        tent.worldObject.transform.localScale = Vector3.one * UnityEngine.Random.Range(1f, 1.2f);
        tents.Add(tent);

        return tent;
    }

    public Tent GetOpenTent()
    {
        foreach(Tent tent in tents)
        {
            if(tent.IsOpen())
            {
                return tent;
            }
        }
        return null;
    }


    public void AddItemsToCamp(ItemCollection itemsToAdd, Transform originT, bool physical){
        Item item;
        int countToAdd;
        int zeroRacksRef = 0;
        foreach (KeyValuePair<Item, int> kvp in itemsToAdd.items)
        {
            item = kvp.Key;
            countToAdd = kvp.Value;

            if(item.type.Equals(ItemType.CampComponent))
            {
                for(int i = 0; i < countToAdd; ++i)
                {
                    AddCampComponentItem(item);
                }
            }
            else
            {
                AddObjectsAnyRack(item, ref countToAdd, originT, ref zeroRacksRef, physical);
            }
        }

    }
        

    public void RemoveItemsFromCamp(ItemCollection itemsToRemove, bool moveToAnotherRack, object destination)
    {

        //Debug.Log("RemoveItemsFromCamp()");

        Item item;
        int countToRemove;
        foreach(KeyValuePair<Item, int> kvp in itemsToRemove.items)
        {
            //Debug.Log("Going to destroy " + kvp.Key.nme + " x " + kvp.Value);
            item = kvp.Key;
            countToRemove = kvp.Value;
            RemoveObjectsAnyRack(item, ref countToRemove, moveToAnotherRack, destination);
        }
    }

    public GameObject FindObjectInCamp(Item item)
    {

        List<ObjectRack> rackList = GetRackListForItemType(item.type);
        foreach(ObjectRack rack in rackList)
        {
            if (!(rack is Workbench))
            {
                foreach (GameObject rackObject in rack.objectsOnRack)
                {
                    
                }
            }
            
        }



        return null;

    }


    public void AddObjectsAnyRack(Item item, ref int count, Transform originT, ref int newRacksCount, bool physical)
    {

        List<ObjectRack> rackList = GetRackListForItemType(item.type);

        // if adding physical item, find the next rack with room and add
        foreach (ObjectRack rack in rackList)
        {
            if (!rack.IsFull())
            {
                rack.AddObjects(item, ref count, originT, ref newRacksCount, physical);
                break;
            }
        }
        
        // if still objects to add, place a new rack
        if(count > 0){
            ++newRacksCount;
            ObjectRack newRack = PlaceObjectRack(item.type, CAMP_COMPONENT_PLACING_TIME_GAP * newRacksCount);
            newRack.AddObjects(item, ref count, originT, ref newRacksCount, physical);
        }
    }   

    public void RemoveObjectsAnyRack(Item item, ref int count, bool moveToAnotherRack, object destination)
    {
        // get rack list
        List<ObjectRack> rackList = GetRackListForItemType(item.type).Where(rack => !rack.IsEmpty()).ToList();

        // filter rack list to ones that contain at least one of the target item
        rackList = rackList.Where(rack => rack.GetObjectCountsOnRackThatAreItemCount(item) > 0).ToList();

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
            case ItemType.Pelt :
                rackList = racks_pelt;
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
        StartCoroutine(ClientCommand.instance.SpawnNpcFollowerWhenReady(casterHandle.entityInfo.faction.leaderHandle, GetOpenTribeMemberStandPosition().transform.position, false));
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

        //Debug.Log("Dismantle() start");

        // handle inside camp status (set to outside)
        faction.leaderHandle.entityPhysics.OnCampBorderExit();
        borderSphere.SetActive(false);
        Destroy(borderSphere);


        // foreach (KeyValuePair<Item, int> kvp in faction.ownedItems.items)
        // {
        //     faction.RemoveItemOwned(kvp.Key, kvp.Value, null, true, faction.leaderHandle.entityItems);
        // }

        // handle removal of camp components and packing of items
        CampComponent[] allCampComponents = rootT.GetComponents<CampComponent>();
        int delayIterator = 0;
        foreach(CampComponent cp in allCampComponents)
        {
            // if camp component is an object rack, move all its items to leader's inventory
            if(cp is ObjectRack)
            {
                ObjectRack rack = (ObjectRack)cp;

                foreach(GameObject worldObject in rack.objectsOnRack.ToArray())
                {
                    Item item = Item.GetItemByName(worldObject.name);
                    GameObject dummyItem = Utility.InstantiateSameName(worldObject, worldObject.transform.position, worldObject.transform.rotation);
                    Utility.SetGlobalScale(dummyItem.transform, Vector3.one);
                    StartCoroutine(Utility.instance.FlyObjectToPosition(dummyItem, faction.leaderHandle.transform, true, true, delayIterator * (ObjectRack.OBJECT_PLACEMENT_DELAY_TIMESTEP * .25f)));
                    GameObject.Destroy(worldObject);
                    ++delayIterator;
                }
            }

            // destroy and play dismantle animation
            Utility.DestroyInSeconds(cp.worldObject, 5f);
            cp.PlayDismantleAnimation();
        }


        Utility.DestroyInSeconds(rootT.gameObject, 5f);

        Debug.Log("Dismantle() end");
    }


    public static readonly Dictionary<Item, int> ItemPhysicalCapacityDict = new Dictionary<Item, int>()
    {
        { Item.WoodPiece, 30},
        { Item.BonePiece, 10},
        { Item.StoneSmall, 10},
        { Item.Meat, 5},
        { Item.AxeStone, 5},
        { Item.SpearStone, 5}
    };

    public static int GetItemPhysicalCapacity(Item item)
    {
        try
        {
            return ItemPhysicalCapacityDict[item];
        }
        catch(KeyNotFoundException)
        {
            return int.MaxValue;
        }
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


